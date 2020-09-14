using Demonstration.Serialization;
using Demonstration.Writing;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tracer;

namespace Demonstration
{
    class Program
    {
        static void Main(string[] args)
        {
            ITracer methodTracer = new MethodTracer();
            BackgroundTestStarter testStarter = new BackgroundTestStarter(methodTracer);
            testStarter.StartTest();

            TraceResult traceResult = methodTracer.GetTraceResult();

            IWriter consoleWriter = new ConsoleWriter();
            IWriter fileWriter = new FileWriter("out.txt");

            ISerializer serializer = new JSONSerializer();
            string serializedResult = serializer.Serizlize(traceResult);

            consoleWriter.Write(serializedResult);
            fileWriter.Write(serializedResult);
        }
    }

    class BackgroundTestStarter
    {
        private readonly ITracer tracer;

        public BackgroundTestStarter(ITracer tracer)
        {
            this.tracer = tracer;
        }

        public void StartTest()
        {
            tracer.StartTrace();

            BackgroundClassTest test = new BackgroundClassTest(tracer);

            List<Thread> threadList = new List<Thread>();
            for (int i = 0; i < 3; i++)
            {
                threadList.Add(new Thread(test.StartMethod));
                threadList.Last().Start();
            }

            foreach (var thread in threadList)
            {
                thread.Join();
            }

            tracer.StopTrace();
        }
    }

    class BackgroundClassTest
    {
        private readonly ITracer tracer;

        public BackgroundClassTest(ITracer tracer)
        {
            this.tracer = tracer;
        }

        public void StartMethod()
        {
            InnerMethod1();
            InnerMethod2();
        }

        public void InnerMethod1()
        {
            tracer.StartTrace();
            Thread.Sleep(500);
            tracer.StopTrace();
        }

        private void InnerMethod2()
        {
            tracer.StartTrace();
            Thread.Sleep(200);
            DeepMethod();
            tracer.StopTrace();
        }

        private void DeepMethod()
        {
            tracer.StartTrace();
            Thread.Sleep(100);
            tracer.StopTrace();
        }
    }
}
