using System.Collections.Generic;
using System.Xml.Serialization;

namespace Tracer
{
    [XmlRoot("root")]
    public class TraceResult
    {
        public TraceResult()
        {
            Threads = new List<ThreadTraceResult>();
        }

        [XmlElement("thread")]
        public List<ThreadTraceResult> Threads
        { get; set; }
    }

    public class ThreadTraceResult
    {
        public ThreadTraceResult()
        {

        }

        public ThreadTraceResult(int threadId)
        {
            Id = threadId;
            Methods = new List<MethodTraceResult>();
        }

        [XmlAttribute("id")]
        public int Id
        { get; set; }

        [XmlAttribute("time")]
        public int Time
        { get; set; }

        [XmlElement("method")]
        public List<MethodTraceResult> Methods
        { get; set; }

    }

    public class MethodTraceResult
    {
        public MethodTraceResult()
        {

        }

        public MethodTraceResult(string methodName, string className)
        {
            Name = methodName;
            Class = className;
        }

        [XmlAttribute("name")]
        public string Name
        { get; set; }

        [XmlAttribute("class")]
        public string Class
        { get; set; }

        [XmlAttribute("time")]
        public int Time
        { get; set; }

        [XmlElement("method")]
        public List<MethodTraceResult> Methods
        { get; set; }
    }
}
