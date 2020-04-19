using System;
using System.Linq;
using System.Reflection;

namespace SimpleContainer
{
    public static class CatExtensions
    {
        public static Cat Register(this Cat cat, Type from, Type to, LifeTime lifeTime)
        {
            // args为范型的封锁类型，非范型上层判定之后会传递new Type[0]
            Func<Cat, Type[], object> factory = (container, args) => Create(container, to, args);
            // ServiceRegistry将类型，生命周期，与工厂绑定
            cat.Register(new ServiceRegistry(from, lifeTime, factory));
            return cat;
        }

        public static Cat Register<TFrom, TTo>(this Cat cat, LifeTime lifeTime) where TTo : TFrom
        {
            return cat.Register(typeof(TFrom), typeof(TTo), lifeTime);            
        }

        // 全局单例注册
        public static Cat Register(this Cat cat, Type serviceType, object instance)
        {
            Func<Cat, Type[], object> factory = (container, args) => instance;
            cat.Register(new ServiceRegistry(serviceType, LifeTime.Root, factory));
            return cat;
        }

        public static Cat Register<TService>(this Cat cat, TService instance)
        {
            Func<Cat, Type[], object> factory = (container, args) => instance;
            cat.Register(new ServiceRegistry(typeof(TService), LifeTime.Root, factory));
            return cat;
        }
        // 自己提供工厂
        public static Cat Register(this Cat cat, Type serviceType, Func<Cat, object> factory, LifeTime lifeTime)
        {
            cat.Register(new ServiceRegistry(serviceType, lifeTime, (container, args) => factory(container)));
            return cat;
        }

        public static Cat Register<TService>(this Cat cat, Func<Cat, object> factory, LifeTime lifeTime)
        {
            cat.Register(new ServiceRegistry(typeof(TService), lifeTime, (container, args) => factory(container)));
            return cat;
        }

        //批量注册
        public static Cat Register(this Cat cat, Assembly assembly)
        {
            var typedAttributes = from type in assembly.GetExportedTypes()
                                  let attribute = type.GetCustomAttribute<MapToAttribute>()
                                  where attribute != null
                                  select new { ServiceType = type, Attribute = attribute };//第一个是assembly里真实的类型，第二个是那个类型的Attribute的声明
            foreach(var item in typedAttributes)
            {
                cat.Register(item.Attribute.ServiceType, item.ServiceType, item.Attribute.LifeTime);
            }
            return cat;
        }

        // 反射并构造
        private static object Create(Cat cat, Type type, Type[] genericArguments)
        {
            // 是范型
            if(genericArguments.Length > 0)
            {
                // 构造范型，注意type是传递进来的。
                type = type.MakeGenericType(genericArguments);
            }
            var constructors = type.GetConstructors();
            if(constructors.Length == 0)
            {
                throw new InvalidOperationException($@"Cannot create an instance of type {type} which has no public constructor");
            }

            var constructor = constructors.FirstOrDefault(item => item.GetCustomAttributes(false).OfType<InjectionAttribute>().Any());
            constructor ??= constructors.First();
            var parameters = constructor.GetParameters();
            if (parameters.Length == 0)
            {
                return Activator.CreateInstance(type);
            }
            var arguments = new object[parameters.Length];
            for (int index = 0; index < arguments.Length; index++)
            {
                arguments[index] = cat.GetService(parameters[index].ParameterType);
            }
            return constructor.Invoke(arguments);
        }
    }
}
