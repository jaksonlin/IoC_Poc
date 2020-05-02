using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleContainer
{
    public class CatServiceProviderFactory : IServiceProviderFactory<CatBuilder>
    {
        public CatBuilder CreateBuilder(IServiceCollection services)
        {
            var cat = new Cat();
            foreach(var service in services)
            {
                //针对三个构造途径分别注册。
                if(service.ImplementationFactory != null)
                {
                    cat.Register(service.ServiceType, provider => service.ImplementationFactory(provider), service.Lifetime.AsCatLifetime());
                }else if(service.ImplementationInstance != null)
                {
                    // 这个只能是root里的单例
                    cat.Register(service.ServiceType, service.ImplementationInstance);
                }else if(service.ImplementationType != null)
                {
                    cat.Register(service.ServiceType, service.ImplementationType, service.Lifetime.AsCatLifetime());
                }
            }
            return new CatBuilder(cat);
        }

        public IServiceProvider CreateServiceProvider(CatBuilder containerBuilder)
        {
            return containerBuilder.BuildServiceProvider();
        }
    }
}
