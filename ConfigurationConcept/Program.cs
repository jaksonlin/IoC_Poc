using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
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
        public DateTimeFormatOptions DateTime { get;  set; }
        public CurrencyFormatOptions Currency { get;  set; }

    }
    class CurrencyFormatOptions
    {
        public String Digits { get;  set; }
        public String Symbol { get;  set; }

    }
    class DateTimeFormatOptions
    {
        public String LongDatePattern { get;  set; }
        public String LongTimePattern { get;  set; }

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

        static void JsonSource()
        {

            var configStr = "stage";
            // Create factory, manage the factory's configuration source
            var optionFactory = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, reloadOnChange : true)
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
            OptionPatternMultiple();
            Console.ReadLine();
        }

        static void OptionPattern()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.stage.json")
                .Build();

            var profile = new ServiceCollection()
                .AddOptions()
                .Configure<FormatOptions>(configuration.GetSection("Format"))
                .BuildServiceProvider()
                .GetRequiredService<IOptions<FormatOptions>>().Value;
            Console.WriteLine($@"{profile.Currency.Digits}#{profile.DateTime.LongDatePattern}");
        }
        static void OptionPatternMultiple ()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.multiple.json")
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddOptions()
                .Configure<FormatOptions>("production", configuration.GetSection("FormatProduction"))
                .Configure<FormatOptions>("debug", configuration.GetSection("FormatDebug"))
                .BuildServiceProvider();
            var optionAccessor = serviceProvider.GetRequiredService<IOptionsSnapshot<FormatOptions>>();
            var debugInfo = optionAccessor.Get("debug");
            var productInfo = optionAccessor.Get("production");
            Console.WriteLine($@"{debugInfo.Currency.Digits}#{debugInfo.DateTime.LongDatePattern}");
            Console.WriteLine($@"{productInfo.Currency.Digits}#{productInfo.DateTime.LongDatePattern}");
        }
    }
}
