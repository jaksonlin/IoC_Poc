using System;
namespace SimpleContainer
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple =true)]
    public sealed class MapToAttribute:Attribute
    {
        public Type ServiceType { get; }
        public LifeTime LifeTime { get; }
        public MapToAttribute(Type type, LifeTime lifeTime)
        {
            this.ServiceType = type;
            this.LifeTime = lifeTime;
        }
    }
}
