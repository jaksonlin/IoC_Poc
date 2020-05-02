using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleContainer
{
    internal static class Extensions
    {
        public static LifeTime AsCatLifetime(this ServiceLifetime serviceLifetime)
        {
            return serviceLifetime switch
            {
                ServiceLifetime.Scoped => LifeTime.Self,
                ServiceLifetime.Singleton => LifeTime.Root,
                ServiceLifetime.Transient => LifeTime.Transient,
                _ => LifeTime.Transient,
            };
        }
    }
}
