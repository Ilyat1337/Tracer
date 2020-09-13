using System.Collections.Generic;

namespace Tracer
{
    public class TraceResult
    {
        public TraceResult()
        {
            Threads = new List<ThreadTraceResult>();
        }

        public List<ThreadTraceResult> Threads
        { get; }
    }
    
    public class ThreadTraceResult
    {
        public ThreadTraceResult(int threadId)
        {
            Id = threadId;
            Methods = new List<MethodTraceResult>();
        }

        public int Id
        { get; }

        public int Time
        { get; set; }

        public List<MethodTraceResult> Methods
        { get; }

    }

    public class MethodTraceResult
    {
        public MethodTraceResult(string methodName, string className)
        {
            Name = methodName;
            Class = className;
        }

        public string Name
        { get; }

        public string Class
        { get; }

        public int Time
        { get; set; }

        public List<MethodTraceResult> Methods
        { get; set; }
    }
}
