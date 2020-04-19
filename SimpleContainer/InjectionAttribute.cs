using System;

namespace SimpleContainer
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public sealed class InjectionAttribute:Attribute
    {
    }
}