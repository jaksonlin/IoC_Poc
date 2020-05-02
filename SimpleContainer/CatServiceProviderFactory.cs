using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleContainer
{
    public class CatServiceProviderFactory : IServiceProviderFactory<CatBuilder>
    {
        //将用户定义的IServiceCollection导入
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
        //采用参数依赖的方式形成业务链，令用户必须先调用CreateBuilder，才能获得CatBuilder，才能调用此函数。别无他法。
        public IServiceProvider CreateServiceProvider(CatBuilder containerBuilder)
        {
            return containerBuilder.BuildServiceProvider();
        }
    }
}
