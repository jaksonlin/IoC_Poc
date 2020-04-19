using System;
using SimpleContainer;

namespace MyContainer
{
    public interface IFoo { }
    public interface IBar { }
    public interface IBaz { }
    public interface IQux { }
    public interface IFooBar<T1, T2> { }
    public class Base : IDisposable
    {
        public Base() => Console.WriteLine($"Instance of {GetType().Name} is Created.");
        public void Dispose() => Console.WriteLine($"Instance of {GetType().Name} is disposed!");
    }

    public class Foo : Base, IFoo { }
    public class Bar:Base, IBar { }
    public class Baz:Base, IBaz { }

    [MapTo(typeof(IQux), LifeTime.Root)]
    public class Qux:Base, IQux { }

    public class FooBar<T1, T2> : IFooBar<T1, T2> {
        public FooBar(IFoo foo, IBar bar)
        {
            this.foo = foo;
            this.bar = bar;
        }

        public IFoo foo { get; }
        public IBar bar { get; }
    }
    public class DummyInfo
    {
        public DummyInfo()
        {
        }
    }
}
