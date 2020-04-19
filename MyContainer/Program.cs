using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
//using SimpleContainer;

namespace MyContainer
{
    class Program
    {
        static void Main(string[] args)
        {
            var provider = new ServiceCollection()
                .AddTransient<IFoo, Foo>()
                .AddScoped<IBar>(_ => new Bar())
                .AddSingleton<IBaz, Baz>()
                .AddSingleton<IQux, Qux>()
                .AddTransient<Base, Foo>()
                .AddTransient<Base, Bar>()
                .BuildServiceProvider();
            var cat1 = provider.CreateScope().ServiceProvider;
            var cat2 = provider.CreateScope().ServiceProvider;

/*            var root = new Cat().
                Register<IFoo, Foo>(LifeTime.Transient).
                Register<IBar>(_ => new Bar(), LifeTime.Self).
                Register<IBaz, Baz>(LifeTime.Root).
                Register(Assembly.GetEntryAssembly());

            var cat1 = root.CreateChild();
            var cat2 = root.CreateChild();*/

            void GetServiceTwice<TService>(IServiceProvider cat)
            {
                cat.GetService<TService>();
                cat.GetService<TService>();
            }
            GetServiceTwice<IFoo>(cat1);
            GetServiceTwice<IBar>(cat1);
            GetServiceTwice<IBaz>(cat1);
            GetServiceTwice<IQux>(cat1);

            Console.WriteLine();
            GetServiceTwice<IFoo>(cat2);
            GetServiceTwice<IBar>(cat2);
            GetServiceTwice<IBaz>(cat2);
            GetServiceTwice<IQux>(cat2);

            Console.WriteLine();
            var bases = provider.GetServices<Base>();
            foreach(var item in bases){
                Console.WriteLine($@"{item.GetType().Name}");
            }
            Console.WriteLine();
            
            var sp = provider.GetService<IServiceProvider>();
            GetServiceTwice<IFoo>(sp);
            GetServiceTwice<IBar>(sp);
            GetServiceTwice<IBaz>(sp);
            GetServiceTwice<IQux>(sp);
            Console.ReadLine();
        }
    }
}
