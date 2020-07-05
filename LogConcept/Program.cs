using System;
using System.Diagnostics;

namespace LogConcept
{
    class Program
    {
        static void Main(string[] args)
        {
            TraceSourceTest2();
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
}
