using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SimpleContainer
{
    public class Cat : IServiceProvider, IDisposable
    {
        internal readonly Cat _root;
        // 服务注册表
        internal readonly ConcurrentDictionary<Type, ServiceRegistry> _registries;
        // 管理已经创建对self/root类型对服务实例。需要使用Key进行严格的类型信息管理。
        internal readonly ConcurrentDictionary<Key, object> _services;
        // 管理对象生命周期
        private readonly ConcurrentStack<IDisposable> _disposables;
        private volatile bool _disposed = false;

        // root container
        public Cat()
        {
            this._root = this;
            this._registries = new ConcurrentDictionary<Type, ServiceRegistry>();
            this._services = new ConcurrentDictionary<Key, object>();
            this._disposables = new ConcurrentStack<IDisposable>();

        }

        public Cat(Cat parent)
        {
            this._root = parent;
            //share the registry with parent
            this._registries = parent._registries;
            // in child's scope
            this._services = new ConcurrentDictionary<Key, object>();
            this._disposables = new ConcurrentStack<IDisposable>();
        }

        // to prevent creating new service when the container is disposing.
        private void EnsureNotDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("Cat");
            }
        }
        //return this是为了可以连续点号操作
        public Cat Register(ServiceRegistry registry)
        {
            this.EnsureNotDisposed();
            if (this._registries.TryGetValue(registry.ServiceType, out var existingItem))
            {
                //更新Registry同类引用链表
                this._registries[registry.ServiceType] = registry;
                registry.Next = existingItem;
            }
            else
            {
                this._registries[registry.ServiceType] = registry;
            }
            return this;
        }
        // 结合：类型的生命周期，以及范型构造的时候的情况，真正调用registry中的工厂完成构建，并管理他们的生命周期。
        private object GetServiceCore(ServiceRegistry registry, Type[] genericArguments)
        {
            var key = new Key(registry, genericArguments);
            var serviceType = registry.ServiceType;
            switch (registry.LifeTime)
            {
                case LifeTime.Root:
                    return GetOrCreate(this._root._services, this._root._disposables);
                case LifeTime.Self:
                    return GetOrCreate(this._services, this._disposables);
                case LifeTime.Transient:
                    var service = registry.Factory(this, genericArguments);
                    if(service is IDisposable disposable && disposable != this)
                    {
                        this._disposables.Push(disposable);
                    }
                    return service;
                default:
                    throw new Exception($@"LifeTIme not valid: {registry.LifeTime}");
            }
            // 需要容器托管生命周期的对象。（容器级别单例）
            object GetOrCreate(ConcurrentDictionary<Key, object>services, ConcurrentStack<IDisposable> disposables)
            {
                object service;
                if(services.TryGetValue(key, out service))
                {
                    return service;
                }
                // registry里对factory在这里调用。并传递container与范型参数。这里封锁类型时候要使用到的类型再继续由container负责构造。
                service = registry.Factory(this, genericArguments);
                services[key] = service;
                // 托管生命周期
                if(service is IDisposable disposable)
                {
                    disposables.Push(disposable);
                }
                return service;
            }

        }
        
        public object GetService(Type serviceType)
        {
            this.EnsureNotDisposed();
            if (serviceType==typeof(Cat) || serviceType == typeof(IServiceProvider))
            {
                return this;
            }

            ServiceRegistry registry;
            // 列表类型范型 IEnumerable<T> type
            if(serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var elementType = serviceType.GetGenericArguments()[0];
                // 如果我们不知道T要如何构造对话，就返回一个空的array；
                if (!this._registries.TryGetValue(elementType, out registry)){
                    return Array.CreateInstance(elementType, 0);
                }
                // T 如何构造的信息都在registry里，我们知道的话，就用GetServiceCore将它从registry中构造出来
                var registries = registry.AsEnumerable();
                var services = registries.Select(item => this.GetServiceCore(item, Type.EmptyTypes)).ToArray();// from item in registries select this.GetServiceCore(item, Type.EmptyTypes);
                Array result = Array.CreateInstance(elementType, services.Length);
                services.CopyTo(result, 0);
                return result;
            }

            //非列表类型范型
            // type是范型的时候，如何构造？1. 获取范型的定义；2. 获取其范型的封锁类型，用于构造该范型
            if(serviceType.IsGenericType && !this._registries.ContainsKey(serviceType)){
                // 获取范型定义 https://docs.microsoft.com/en-us/dotnet/api/system.type.getgenerictypedefinition?view=netframework-4.8
                var definition = serviceType.GetGenericTypeDefinition();
                // 查看这个范型我们是否已经注册，是的话，就根据它的封锁类型构造object
                return this._registries.TryGetValue(definition, out registry) ? this.GetServiceCore(registry, serviceType.GetGenericArguments()) : null;
            }

            //非范型
            return this._registries.TryGetValue(serviceType, out registry) ? this.GetServiceCore(registry, new Type[0]) : null;


        }

        public object GetService<TService>()
        {
            return this.GetService(typeof(TService));
        }

        public Cat CreateChild()
        {
            return new Cat(this);
        }

        public void Dispose()
        {
            this._disposed = true;
            foreach(var item in this._disposables)
            {
                item.Dispose();
            }
            this._disposables.Clear();
            this._services.Clear();
        }


    }
}