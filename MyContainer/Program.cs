using System;
using System.Reflection;
using SimpleContainer;

namespace MyContainer
{
    class Program
    {
        static void Main(string[] args)
        {
            var root = new Cat().
                Register<IFoo, Foo>(LifeTime.Transient).
                Register<IBar>(_ => new Bar(), LifeTime.Self).
                Register<IBaz, Baz>(LifeTime.Root).
                Register(Assembly.GetEntryAssembly());

            var cat1 = root.CreateChild();
            var cat2 = root.CreateChild();

            void GetServiceTwice<TService>(Cat cat)
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

            Console.ReadLine();
        }
    }
}
