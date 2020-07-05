using Microsoft.Extensions.DiagnosticAdapter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace LogConcept
{
    class Program
    {
        static void Main(string[] args)
        {
            LogSourceCollector();
            Console.ReadLine();
        }

        //Write to the debug listener's output 
        static void BasicDebugWrite()
        {
            int a = 1111;
            Debug.WriteLineIf(a > 10, "a larger than 10");
            Debug.WriteLine("Hello World");
            a -= 100;
            Debug.WriteLineIf(a > 10, "a larger than 10");
            Debug.WriteLine("End");
        }
        //Write to the debug listener's output 
        static void TraceSourceTest()
        {
            //Lowest level to log:
            //var source = new TraceSource("zhislin-debug-info", SourceLevels.All);
            var source = new TraceSource("zhislin-debug-info", SourceLevels.Warning);
            var eventType = (TraceEventType[])Enum.GetValues(typeof(TraceEventType));
            var eventId = 1;
            Array.ForEach(eventType, it => source.TraceEvent(it, eventId++, $"this is a {it} message"));
        }
        //Write to the user trace listener
        static void TraceSourceTest2()
        {
            //Lowest level to log:
            //var source = new TraceSource("zhislin-debug-info", SourceLevels.All);
            var source = new TraceSource("zhislin-debug-info", SourceLevels.Warning);
            source.Listeners.Add(new ConsoleTraceListener());
            var eventType = (TraceEventType[])Enum.GetValues(typeof(TraceEventType));
            var eventId = 1;
            Array.ForEach(eventType, it => source.TraceEvent(it, eventId++, $"this is a {it} message"));
        }

        static void EventSourceTest()
        {
            DatabaseSource.Instance.OnCommandExecute(CommandType.Text, "SELECT * FROM T_USER");
        }
        //将日志对象进行转发，并交由观察者决定如何处置
        static void LogObserver()
        {
            DiagnosticListener.AllListeners.Subscribe(new Observer<DiagnosticListener>(listener=> {
            if (listener.Name == "my-register-listener")
            {
                listener.Subscribe(new Observer<KeyValuePair<string, object>>(kv => {
                    Console.WriteLine($"Event name: {kv.Key}");
                    dynamic payload = kv.Value;
                    Console.WriteLine($"Payload name: {payload.CommandType}");

                    Console.WriteLine($"Payload name: {payload.CommandText}");
                }));
                }
            }));
            var source = new DiagnosticListener("my-register-listener");
            if (source.IsEnabled("CommandExecution"))
            {
                source.Write("CommandExecution", new { CommandType = CommandType.Text, CommandText = "Hello World" });
            }
        }

        static void LogSourceCollector()
        {
            DiagnosticListener.AllListeners.Subscribe(new Observer<DiagnosticListener>(listener => {
                if (listener.Name == "my-register-listener")
                {
                    listener.SubscribeWithAdapter(new DatabseSourceCollector());
                }
            }));
            var source = new DiagnosticListener("my-register-listener");
            if (source.IsEnabled("CommandExecution"))
            {
                source.Write("CommandExecution", new { CommandType = CommandType.Text, CommandText = "Hello World" });
            }
        }

        
    }
    [EventSource(Name ="abdcefg")]
    public sealed class DatabaseSource : EventSource
    {
        public static readonly DatabaseSource Instance = new DatabaseSource();
        private DatabaseSource() { }
        [Event(1)]
        public void OnCommandExecute(CommandType commandType, string commandText) => WriteEvent(1, commandType, commandText);
    }
    public class ConsoleTraceListener : TraceListener
    {
        public override void Write(string message)
        {
            Console.Write(message);
        }

        public override void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }

    public class Observer<T> : IObserver<T>
    {
        private Action<T> _onNext;
        
        public Observer(Action<T> next)
        {
            this._onNext = next;
        }

        public void OnCompleted()
        {
            
        }

        public void OnError(Exception error)
        {
            
        }

        public void OnNext(T value)
        {
            _onNext(value);
        }
    }

    public class DatabseSourceCollector
    {
        [DiagnosticName("CommandExecution")]
        public void OnCommandExecute(CommandType commandType, string commandText)
        {
            Console.WriteLine($"Event Name: CommandExecution");
            Console.WriteLine($"Event Name: {commandType}");
            Console.WriteLine($"Event Name: {commandText}");
        }

    }
}
