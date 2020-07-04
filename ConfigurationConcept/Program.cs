using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace ConfigurationConcept
{
    class VersionOptions
    {
        public int Main { get; set; }
        public int SubVersion { get; set; }
    }
    class FormatOptions
    {
        public DateTimeFormatOptions DateTime { get; set; }
        public CurrencyFormatOptions Currency { get; set; }

    }
    class CurrencyFormatOptions
    {
        public String Digits { get; set; }
        public String Symbol { get; set; }

    }
    class DateTimeFormatOptions
    {
        public String LongDatePattern { get; set; }
        public String LongTimePattern { get; set; }

    }
    class Program
    {
        static void MemorySource()
        {

            // prepare data for the Factory: IConfigurationBuilder
            var source = new Dictionary<String, String>()
            {
                ["Format:DateTime:LongDatePattern"] = "ddd, MMMM d,yyyy",
                ["Format:DateTime:LongTimePattern"] = "h:mm:ss tt",

                ["Format:Currency:Digits"] = "22222",
                ["Format:Currency:Symbol"] = "$",

            };
            // Create factory, manage the factory's configuration source
            FormatOptions option = new ConfigurationBuilder()
                .Add(new MemoryConfigurationSource() { InitialData = source }).Build()
                .GetSection("Format")
                .Get<FormatOptions>();
            // Create Container 
            var container = new ServiceCollection()
                .AddSingleton<FormatOptions>(option)
                .BuildServiceProvider();
            using (var p = container.CreateScope())
            {
                var result = p.ServiceProvider.GetRequiredService<FormatOptions>();
                Console.WriteLine(result.DateTime.LongDatePattern);
                Console.WriteLine(result.Currency.Digits);
            }

        }
        // normal json source with hot-modification-reload
        static void JsonSource()
        {

            var configStr = "stage";
            // Create factory, manage the factory's configuration source
            var optionFactory = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, reloadOnChange: true)
                .AddJsonFile($@"appsettings.{configStr}.json", true).Build();
            ChangeToken.OnChange(() => optionFactory.GetReloadToken(), () =>
              {
                  var newVersion = optionFactory.GetSection("Version").Get<VersionOptions>();
                  Console.WriteLine($@"{newVersion.Main}.{newVersion.SubVersion}");
              });
            var formatOption = optionFactory.GetSection("Format").Get<FormatOptions>();
            var versionOption = optionFactory.GetSection("Version").Get<VersionOptions>();
            // Create Container 
            var container = new ServiceCollection()
                .AddSingleton<FormatOptions>(formatOption)
                .AddSingleton<VersionOptions>(versionOption)
                .BuildServiceProvider();
            using (var p = container.CreateScope())
            {
                var result = p.ServiceProvider.GetRequiredService<FormatOptions>();
                Console.WriteLine(result.DateTime.LongDatePattern);
                Console.WriteLine(result.Currency.Digits);
                var verInfo = p.ServiceProvider.GetRequiredService<VersionOptions>();
                Console.WriteLine($@"{verInfo.Main}.{verInfo.SubVersion}");
            }

        }
        static void Main(string[] args)
        {
            DiffOfIOptionsAndSnapshots();
            Console.ReadLine();
        }

        // Option-Pattern Programming
        // IOptions will help to map the user defined options with the IConfiguration
        // Use IOptions<T> to contain the options
        static void OptionPattern()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.stage.json")
                .Build();
            // IOptions is for application and is always signleton
            var profile = new ServiceCollection()
                .AddOptions()
                .Configure<FormatOptions>(configuration.GetSection("Format"))
                .BuildServiceProvider()
                .GetRequiredService<IOptions<FormatOptions>>().Value;
            Console.WriteLine($@"{profile.Currency.Digits}#{profile.DateTime.LongDatePattern}");
        }
        // Use IoC Container to load multiple options based on the key for creating the runtime instance 
        // Use IOptionsSnapshot to distinguish the differnt configs for the same Option POCO
        static void OptionPatternMultiple()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.multiple.json")
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddOptions()
                .Configure<FormatOptions>("production", configuration.GetSection("FormatProduction"))
                .Configure<FormatOptions>("debug", configuration.GetSection("FormatDebug"))
                .BuildServiceProvider();
            //IOptionsSnapshot is always Scope, for processing of current request
            var optionAccessor = serviceProvider.GetRequiredService<IOptionsSnapshot<FormatOptions>>();
            var debugInfo = optionAccessor.Get("debug");
            var productInfo = optionAccessor.Get("production");
            Console.WriteLine($@"{debugInfo.Currency.Digits}#{debugInfo.DateTime.LongDatePattern}");
            Console.WriteLine($@"{productInfo.Currency.Digits}#{productInfo.DateTime.LongDatePattern}");
        }

        // Use IOptionMonitor to mon the changes on the file.
        static void OptionPatternWithMonitor()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.production.json", optional: false, reloadOnChange: true)
                .Build();
            var serviceProvider = new ServiceCollection()
                 .AddOptions()
                 .Configure<FormatOptions>(configuration.GetSection("Format"))
                 .BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var sp = scope.ServiceProvider;
                sp.GetRequiredService<IOptionsMonitor<FormatOptions>>().OnChange(options =>
                {
                    Console.WriteLine($@"{options.DateTime.LongDatePattern}");
                    Console.WriteLine($@"{options.DateTime.LongTimePattern}");
                    Console.WriteLine($@"{options.Currency.Digits}");
                    Console.WriteLine($@"{options.Currency.Symbol}");
                    Console.WriteLine("========================");
                });
            }



        }

        static void DiffOfIOptionsAndSnapshots()
        {
            var random = new Random();
            var serviceProvider = new ServiceCollection()
                .AddOptions()
                .Configure<FoobarOptions>(foobar => {
                    foobar.Foo = random.Next(1, 100);
                    foobar.Bar = random.Next(1, 100);
                }).BuildServiceProvider();
            Print(serviceProvider);
            Print(serviceProvider);
        }
        static void Print(ServiceProvider provider)
        {
            var scopedProvider = provider.GetRequiredService<IServiceScopeFactory>().CreateScope().ServiceProvider;
            var options = scopedProvider.GetRequiredService<IOptions<FoobarOptions>>();
            var optionsSnapshot1 = scopedProvider.GetRequiredService<IOptionsSnapshot<FoobarOptions>>();

            var optionsSnapshot2 = scopedProvider.GetRequiredService<IOptionsSnapshot<FoobarOptions>>();
            Console.WriteLine($"options:{options.Value.Bar}");
            Console.WriteLine($"optionsSnapshot1:{optionsSnapshot1.Value.Bar}");
            Console.WriteLine($"optionsSnapshot2:{optionsSnapshot2.Value.Bar}");
            Console.WriteLine("==============================");
        }
    }
    public class FoobarOptions
    {
        public int Foo { get; set; }
        public int Bar { get; set; }
        public override string ToString()
        {
            return $"FOO:{Foo};BAR:{Bar}";
        }
    }
}
