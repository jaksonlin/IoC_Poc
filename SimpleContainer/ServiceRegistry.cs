using System;
using System.Collections.Generic;

namespace SimpleContainer
{
    public enum LifeTime
    {
        Transient,
        Self,
        Root,
    }
    //他只是用来管理类型的注册，以及对应的工厂，本身并不参与服务构建。只是登记这个类型在容器中构建用的工厂
    // 这个是根据用户提交的需求，容器进行该类型构建管理用的：要构建什么，工厂是谁，生命周期。
    public class ServiceRegistry
    {
        public Type ServiceType { get; }
        public LifeTime LifeTime { get; }
        //注意这里的工厂，通过容器解析构造函数的Type[]。
        public Func<Cat, Type[], object> Factory { get; }
        // 用单向链表保存多个接口对同一个ServiceRegistry对映射。1对多对关系管理。
        internal ServiceRegistry Next { get; set; }

        public ServiceRegistry(Type serviceType, LifeTime lifeTime, Func<Cat, Type[], object> factory)
        {
            this.ServiceType = serviceType;
            this.LifeTime = lifeTime;
            this.Factory = factory;
        }

        internal IEnumerable<ServiceRegistry> AsEnumerable()
        {
            var result = new List<ServiceRegistry>();
            for(var item=this; item.Next!=null;item = item.Next)
            {
                result.Add(item);
            }
            return result;
        }
    }
}
