using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SimpleContainer
{
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

        public IServiceProvider BuildServiceProvider() => _cat;

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
