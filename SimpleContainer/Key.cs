using System;

namespace SimpleContainer
{
    // key：主要负责范型管理（非范型的时候GenericArgment为空。
    // 范型涉及两个部分：1. 范型的类型；2. 范型类型当中的（封锁)类型。
    // 当我们需要管理单例的时候，就需要严谨的类型信息管理。
    // GenericArguments其实是从范型对应的Type推导的，也不需要用户提供。type.GetGenricArguments。
    internal class Key : IEquatable<Key>
    {
        public ServiceRegistry Registry {get;}
        public Type[] GenericArguments { get; }

        public Key(ServiceRegistry registry, Type[] genericArg)
        {
            this.Registry = registry;
            this.GenericArguments = genericArg;
        }

    
        public bool Equals(Key other)
        {
            if (Registry != other.Registry)
            {
                return false;
            }
            if (this.GenericArguments.Length != other.GenericArguments.Length)
            {
                return false;
            }
            for(int index =0;index < this.GenericArguments.Length; index++)
            {
                if(this.GenericArguments[index] != other.GenericArguments[index])
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = this.Registry.GetHashCode();
            for (int index = 0; index < this.GenericArguments.Length; index++)
            {
                hashCode ^= this.GenericArguments[index].GetHashCode();
            }
            return hashCode;

        }

        public override bool Equals(object obj)
        {
            return obj is Key key ? this.Equals(key) : false;
        }
    }
}