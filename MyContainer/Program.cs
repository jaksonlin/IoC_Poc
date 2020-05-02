using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleContainer;
//using SimpleContainer;

namespace MyContainer
{
    class Program
    {
        static void Main(string[] args)
        {

            UsingContainerDotNetWay();
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
        
        static void TryAddEnumerableCheckAndReplace()
        {
            var services = new ServiceCollection();
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IFooBar<IFoo, IBar>), typeof(FooBar<IFoo,IBar>)));
            Debug.Assert(services.Count == 1);
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IFooBar<IFoo, IBar>), typeof(FooBar<IFoo, IBar>)));
            Debug.Assert(services.Count == 1);
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IFooBar<IFoo, IBar>), new FooBar<IFoo,IBar>(new Foo(), new Bar())));
            Debug.Assert(services.Count == 1);
            Func<IServiceProvider, FooBar<IFoo,IBar>> factory4FooBar = _ => new FooBar<IFoo, IBar>(new Foo(), new Bar());
            Func<IServiceProvider, IFooBar<IFoo, IBar>> factory4IFooBar = _ => new FooBar<IFoo, IBar>(new Foo(), new Bar());
            var sd = ServiceDescriptor.Singleton(typeof(IFooBar<IFoo, IBar>), factory4FooBar);
            services.TryAddEnumerable(sd);
            Debug.Assert(services.Count == 1);
            //此处会抛出ArgumentException,因为这里是定义了IFooBar<IFoo,IBar> 的工厂，但工厂的func里解析为IFooBar<IFoo, IBar>映射为IFooBar<IFoo, IBar>,与services里的IFooBar<IFoo, IBar>映射为FooBar<IFoo, IBar>是不可区分的。
            //services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IFooBar<IFoo, IBar>), factory4IFooBar));
            //因此，可以用Replace重新定义
            services.Replace(ServiceDescriptor.Singleton(typeof(IFooBar<IFoo, IBar>), factory4IFooBar));
            Debug.Assert(services.Count == 1);
        }

        static void SelectionOfConstructor()
        {
            var provider = new ServiceCollection()
                .AddTransient<IFoo, Foo>()
                .AddTransient<IBar, Bar>()
                .AddTransient<IQux, Qux>()
                .BuildServiceProvider();
            //select second，原因是，第二个构造函数的参数组成的集合，是所有其他构造函数的参数的集合的超集。比如第一个构造函数的参数集合是（IFoo)，是(IFoo, IBar)的子集。
            //不选第三个的原因是容器不知道如何构造IBaz。
            //选择构造函数的原则就是：这个构造函数的参数集合"最全，最多"并且任何一个其他构造函数的参数的集合，都是它的子集。
            provider.GetService<IQux>();

        }
    
        static void WhichServiceProvider()
        {
            //注意，BuildServiceProvider是用此处的ServiceCollection来创建一个新的ServiceProviderEngine
            var serviceProvider = new ServiceCollection()
                .AddSingleton<SingletonService>()
                .AddScoped<ScopedService>()
                .BuildServiceProvider();
            //而这个serviceProvider是ServiceProviderEngine的实例，这个实例实现的IServiceProvider接口时依赖于内部的rootScope，因此此处的ReferencesEquals是不成立的。
            var rootScope = serviceProvider.GetService<IServiceProvider>();
            //它实现这个接口是一种设计模型，以扩展自身更多的功能。它并不仅仅是为了做IServiceProvider。真正仅仅做IServiceProvider的是它内部的rootScope(IServiceProvider)。
            Debug.Assert(!ReferenceEquals(rootScope, serviceProvider));
            //根容器创建Scope都使用IServiceScopeFactory.
            var rootScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
            using (var scope = serviceProvider.CreateScope())
            {
                var childContainer = scope.ServiceProvider;
                var singletonService = childContainer.GetRequiredService<SingletonService>();
                var scopedService = childContainer.GetRequiredService<ScopedService>();

                Debug.Assert(ReferenceEquals(childContainer, childContainer.GetRequiredService<IServiceProvider>()));
                Debug.Assert(ReferenceEquals(childContainer, scopedService.RequestServices));
                Debug.Assert(ReferenceEquals(rootScope, singletonService.ApplicationServices));
                //注意，从root创建的scope里拿IServiceScopeFactory，它依然会是rootScopeFactory。也就是说所有的子容器之间不存在父子关系。
                //所有子容器的父容器都是root。注意：所谓子容器，本身是被Scope包裹的。
                var childScopeFactory = childContainer.GetRequiredService<IServiceScopeFactory>();
                Debug.Assert(ReferenceEquals(rootScopeFactory, childScopeFactory));
            }
        }

        static void UsingContainerDotNetWay()
        {
            var serviceCollection = new ServiceCollection()
                .AddTransient<IFoo, Foo>()
                .AddScoped<IBar>(_ => new Bar())
                .AddSingleton<IBaz>(new Baz());

            var factory = new CatServiceProviderFactory();
            var builder = factory.CreateBuilder(serviceCollection).Register(Assembly.GetEntryAssembly());
            var container = factory.CreateServiceProvider(builder);
            GetServices();
            GetServices();
            Console.WriteLine("\n Root container is disposed");
            (container as IDisposable)?.Dispose();
            void GetServices()
            {
                using(var scope = container.CreateScope())
                {
                    Console.WriteLine("\nService Scope is created");
                    var child = scope.ServiceProvider;
                    child.GetService<IFoo>();
                    child.GetService<IBar>();
                    child.GetService<IBaz>();
                    child.GetService<IQux>();

                    child.GetService<IFoo>();
                    child.GetService<IBar>();
                    child.GetService<IBaz>();
                    child.GetService<IQux>();

                    Console.WriteLine("Service Scope is disposed");
                }
            }
        }
    }

    class SingletonService
    {
        public IServiceProvider ApplicationServices { get; }
        public SingletonService(IServiceProvider serviceProvider) => ApplicationServices = serviceProvider;
    }
    class ScopedService
    {
        public IServiceProvider RequestServices { get; }
        public ScopedService(IServiceProvider serviceProvider) => RequestServices = serviceProvider;
    }

}
