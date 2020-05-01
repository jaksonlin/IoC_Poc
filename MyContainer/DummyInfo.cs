using System;
//using SimpleContainer;

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

//    [MapTo(typeof(IQux), LifeTime.Root)]
    public class Qux:Base, IQux {
        public Qux(IFoo foo) => Console.WriteLine("Select first constructor");

        public Qux(IFoo foo, IBar bar) => Console.WriteLine("Select second constructor");

        public Qux(IFoo foo, IBar bar, IBaz baz) => Console.WriteLine("Select third constructor");
    }

    public class FooBar<T1, T2> : IFooBar<T1, T2> {
        public FooBar(T1 foo, T2 bar)
        {
            this.foo = foo;
            this.bar = bar;
        }

        public T1 foo { get; }
        public T2 bar { get; }
    }
    public class DummyInfo
    {
        public DummyInfo()
        {
        }
    }
}
