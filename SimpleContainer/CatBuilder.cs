using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimpleContainer
{
    //Builder的核心要义在于对外提供IServiceScopeFactory！使得使用容器构建对象处理服务时，能够管理好Scope。
    //事实上，IoC容器的重点在于：定义每一个服务处理时的业务对象的构建。他们都在一次业务处理的过程中由Scope内的子容器进行管理。
    public class CatBuilder
    {
        private readonly Cat _cat;
        public CatBuilder(Cat cat)
        {
            _cat = cat;
            //create builder的时候，主要是要告诉外面的使用者，我们的容器的Scope是如何定义的。这里就需要注意，注入Scope的肯定是当前root容器的子容器，不然Scope销毁的时候会销毁掉root
            //Transient的原因是，Scope是服务于每一个服务调用的，Scope必须而且只能是Transient。
            _cat.Register<IServiceScopeFactory>(c => new ServiceScopeFactory(c.CreateChild()), LifeTime.Transient);
        }

        public IServiceProvider BuildServiceProvider()//ServiceProviderOptions options)
        {
            //此处还要补充Option
            return _cat;
        }

        public IServiceProvider BuildServiceProvider(ServiceProviderOptions options)
        {
            //此处还要补充Option
            if (options.ValidateOnBuild)
            {
                CheckTypeCanConstruct();
            }
            if (options.ValidateScopes)
            {
                CheckSingletonLeak();
            }
            return _cat;
        }

        private void CheckSingletonLeak()
        {
            throw new NotImplementedException();
        }

        private void CheckTypeCanConstruct()
        {
            var allTypes = _cat._registries.Keys.ToList();
            var constructors = from x in allTypes select x.GetConstructors();
            var hset = new HashSet<Type>();
            foreach(var constructorDetails in constructors)
            {
                var parameterList = from x in constructorDetails select x.GetParameters();
                 
                foreach (var param in parameterList)
                {
                    var tmp = from item in param select item.ParameterType;
                    hset.AddRange(tmp);
                }
            }
            foreach(var typeInConstructor in hset)
            {
                if (!_cat._registries.ContainsKey(typeInConstructor))
                {
                    throw new InvalidOperationException($@"type {typeInConstructor.FullName} is not registered");
                }
            }
        }

        public CatBuilder Register(Assembly assembly)
        {
            _cat.Register(assembly);
            return this;
        }

        private class ServiceScope : IServiceScope
        {
            public IServiceProvider ServiceProvider { get; }

            public ServiceScope(IServiceProvider serviceProvider) => this.ServiceProvider = serviceProvider;

            public void Dispose()
            {
                (ServiceProvider as IDisposable)?.Dispose();
            }
        }

        private class ServiceScopeFactory : IServiceScopeFactory
        {
            private readonly Cat _cat;
            public ServiceScopeFactory(Cat cat) => _cat = cat;
            public IServiceScope CreateScope()
            {
                return new ServiceScope(_cat);
            }
        }
    }
}
