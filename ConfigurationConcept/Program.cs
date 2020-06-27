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
    class FormatOptions
    {
        public DateTimeFormatOptions DateTime { get; private set; }
        public CurrencyFormatOptions Currency { get; private set; }
        public FormatOptions(IConfiguration config)
        {
            //Format:DateTime的原因是：原本传入来的IConfiguration应该是config.GetSection("Format")
            //但在容器当中，我们如果将IConfiguration映射为config.GetSeciont("Format")才可以。
            //this.DateTime = new DateTimeFormatOptions(config.GetSection("Format:DateTime"));
            //this.Currency = new CurrencyFormatOptions(config.GetSection("Format:Currency"));

            this.DateTime = new DateTimeFormatOptions(config.GetSection("DateTime"));
            this.Currency = new CurrencyFormatOptions(config.GetSection("Currency"));
        }
    }
    class CurrencyFormatOptions
    {
        public String Digits { get; private set; }
        public String Symbol { get; private set; }
        public CurrencyFormatOptions(IConfiguration config)
        {
            this.Digits = config["Digits"];
            this.Symbol = config["Symbol"];
        }
    }
    class DateTimeFormatOptions
    {
        public String LongDatePattern { get; private set; }
        public String LongTimePattern { get; private set; }
        public DateTimeFormatOptions(IConfiguration config)
        {
            this.LongDatePattern = config["LongDatePattern"];
            this.LongTimePattern = config["LongTimePattern"];
        }
    }
    class Program
    {
        static void Main(string[] args)
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
            var config = new ConfigurationBuilder().Add(new MemoryConfigurationSource() { InitialData = source }).Build();
            // Create Container 
            var container = new ServiceCollection()
                .AddSingleton<IConfiguration>(config.GetSection("Format"))
                .AddScoped<FormatOptions, FormatOptions>()
                .BuildServiceProvider();
            using(var p = container.CreateScope())
            {
                var result = p.ServiceProvider.GetRequiredService<FormatOptions>();
                Console.WriteLine(result.DateTime.LongDatePattern);
                Console.WriteLine(result.Currency.Digits);
            }

            Console.ReadLine();
        }
    }
}
