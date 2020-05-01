﻿using System;
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

            PreventLeak();
            Console.ReadLine();



        }

        static void BasicConcept()
        {
            using (var provider = new ServiceCollection()
                .AddTransient<IFoo, Foo>() // 这个是非容器管理生命周期的
                .AddScoped<IBar>(_ => new Bar()) //这个是scope级别的
                .AddSingleton<IBaz, Baz>() //这个在root上的单例
                .AddSingleton<IQux, Qux>()
                .AddTransient<Base, Foo>()
                .AddTransient<Base, Bar>()
                .BuildServiceProvider())
            {
                using (var sp1 = provider.CreateScope())
                {
                    var cat1 = sp1.ServiceProvider;
                    GetServiceTwice<IBar>(cat1);
                    GetServiceTwice<IBaz>(cat1);
                    GetServiceTwice<IQux>(cat1);

                    Console.WriteLine();
                }
                using (var sp2 = provider.CreateScope())
                {
                    var cat2 = sp2.ServiceProvider;
                    GetServiceTwice<IFoo>(cat2);
                    GetServiceTwice<IBar>(cat2);
                    GetServiceTwice<IBaz>(cat2);
                    GetServiceTwice<IQux>(cat2);

                    Console.WriteLine();
                }
                var bases = provider.GetServices<Base>();
                foreach (var item in bases)
                {
                    Console.WriteLine($@"{item.GetType().Name}");
                }
                Console.WriteLine();

                var sp = provider.GetService<IServiceProvider>();
                GetServiceTwice<IFoo>(sp);
                GetServiceTwice<IBar>(sp);
                GetServiceTwice<IBaz>(sp);
                GetServiceTwice<IQux>(sp);


            }
            Console.ReadLine();
            // ServiceCollection就是一个依赖注入容器。

            // scope就时我们的容器里的子容器的概念。子容器内管理各自创建的对象的生命周期。



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
        }

        //容器单例与Scoped实例的依赖需要管理，防止内存泄漏
        static void PreventLeak()
        {
            var options = new ServiceProviderOptions()
            {
                ValidateOnBuild = true,//build的时候就做检查，而不是运行时才报错
                ValidateScopes = true,//true防止singleton 里引用Scope的对象。

            };
            var root = new ServiceCollection()
                .AddSingleton(typeof(IFooBar<,>), typeof(FooBar<,>))
                .AddScoped<IBar, Bar>()
                .AddScoped<IFoo, Foo>()
                .BuildServiceProvider(options); 
            var sp = root.CreateScope().ServiceProvider;
            void ResoleService<T>(IServiceProvider serviceProvider)
            {
                var isRootContainer = root == serviceProvider ? "Yes" : "No";
                try
                {
                    serviceProvider.GetService<T>();
                    Console.WriteLine($@"Ok, {typeof(T).Name}; Root:{isRootContainer}");
                }catch(Exception ex)
                {
                    Console.WriteLine($@"fail, {typeof(T).Name}; Root:{isRootContainer}");
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            void ResoleServiceG(IServiceProvider serviceProvider, Type serviceType)
            {
                var isRootContainer = root == serviceProvider ? "Yes" : "No";
                try
                {
                    serviceProvider.GetService(serviceType);
                    Console.WriteLine($@"Ok, {serviceType.Name}; Root:{isRootContainer}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($@"fail, {serviceType.Name}; Root:{isRootContainer}");
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            ResoleService<IFoo>(sp);
            //失败，是因为IFooBar是root容器上的单例，按上面的注册，它依赖Scoped的IFoo和IBar， 如此会引发内存泄漏
            ResoleServiceG(sp, typeof(IFooBar<IFoo,IBar>));
            //失败，是因为IFoo是Scoped的，而root是根容器，根容器不解析Scoped注册的对象。
            ResoleService<IFoo>(root);
            //失败，是因为IFooBar是root容器上的单例，按上面的注册，它依赖Scoped的IFoo和IBar， 如此会引发内存泄漏
            ResoleServiceG(root, typeof(IFooBar<IFoo,IBar>));

        }
    }
}
