using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
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
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($@"appsettings.{configStr}.json", true).Build();
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
            JsonSource();
            Console.ReadLine();
        }
    }
}
