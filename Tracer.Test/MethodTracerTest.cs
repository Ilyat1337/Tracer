using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace Tracer.Test
{
    public class MethodTracerTest
    {
        [Fact]
        public void TracerShouldTraceTwoMethodsOnHighestLevel()
        {
            ITracer tracer = new MethodTracer();
            ClassWithSeveralMethods classWithSeveralMethods = new ClassWithSeveralMethods(tracer);

            classWithSeveralMethods.MethodWithInnerMethods();
            TraceResult traceResult = tracer.GetTraceResult();

            Assert.Equal(2, traceResult.Threads[0].Methods.Count);
        }

        [Fact]
        public void ThreadTimeShouldEqualSumOfHighestLevelMethodsTimes()
        {
            ITracer tracer = new MethodTracer();
            ClassWithSeveralMethods classWithSeveralMethods = new ClassWithSeveralMethods(tracer);

            classWithSeveralMethods.MethodWithInnerMethods();
            TraceResult traceResult = tracer.GetTraceResult();

            Assert.Equal(traceResult.Threads[0].Time, traceResult.Threads[0].Methods.Select(method => method.Time).Sum());
        }

        [Fact]
        public void ShouldReturnCorrectThreadId()
        {
            ITracer tracer = new MethodTracer();
            ClassWithSleepMethod classWithSleepMethod = new ClassWithSleepMethod(tracer);
            int threadId = Thread.CurrentThread.ManagedThreadId;

            classWithSleepMethod.MethodWithSleep();
            TraceResult traceResult = tracer.GetTraceResult();

            Assert.Equal(threadId, traceResult.Threads[0].Id);
        }

        [Fact]
        public void FirstMethodResultShouldContainOneInnerMethod()
        {
            ITracer tracer = new MethodTracer();
            ClassWithSeveralMethods classWithSeveralMethods = new ClassWithSeveralMethods(tracer);

            classWithSeveralMethods.MethodWithInnerMethods();
            TraceResult traceResult = tracer.GetTraceResult();

            Assert.Single(traceResult.Threads[0].Methods[0].Methods);
        }

        [Fact]
        public void MethodTimeShouldBeMoreOrEqualThanSleepTime()
        {
            ITracer tracer = new MethodTracer();
            ClassWithSleepMethod classWithSleepMethod = new ClassWithSleepMethod(tracer);

            classWithSleepMethod.MethodWithSleep();
            TraceResult traceResult = tracer.GetTraceResult();

            Assert.True(traceResult.Threads[0].Methods[0].Time >= 100);
        }

        [Fact]
        public void TraceResultShouldContainThreeThreads()
        {
            ITracer tracer = new MethodTracer();
            MultiThreadingClass multiThreadingClass = new MultiThreadingClass(tracer);

            multiThreadingClass.RunThreeThreads();
            TraceResult traceResult = tracer.GetTraceResult();

            Assert.Equal(3, traceResult.Threads.Count());
        }

        [Fact]
        public void WrongTraceOrderShouldThrowException()
        {
            ITracer tracer = new MethodTracer();
            WrongTraceOrder wrongTraceOrder = new WrongTraceOrder(tracer);

            Assert.Throws<OrderViolationException>(wrongTraceOrder.WrongTraceOrderMethod);
        }
    }

    class MultiThreadingClass
    {
        private readonly ITracer tracer;

        public MultiThreadingClass(ITracer tracer)
        {
            this.tracer = tracer;
        }

        public void RunThreeThreads()
        {
            ClassWithSleepMethod test = new ClassWithSleepMethod(tracer);

            List<Thread> threadList = new List<Thread>();
            for (int i = 0; i < 3; i++)
            {
                threadList.Add(new Thread(test.MethodWithSleep));
                threadList.Last().Start();
            }

            foreach (var thread in threadList)
            {
                thread.Join();
            }
        }
    }

    class WrongTraceOrder
    {
        private readonly ITracer tracer;

        public WrongTraceOrder(ITracer tracer)
        {
            this.tracer = tracer;
        }

        public void WrongTraceOrderMethod()
        {
            tracer.StartTrace();
            tracer.StopTrace();
            tracer.StopTrace();
        }
    }

    class ClassWithSleepMethod
    {
        private readonly ITracer tracer;

        public ClassWithSleepMethod(ITracer tracer)
        {
            this.tracer = tracer;
        }

        public void MethodWithSleep()
        {
            tracer.StartTrace();
            Thread.Sleep(100);
            tracer.StopTrace();
        }
    }

    class ClassWithSeveralMethods
    {
        private readonly ITracer tracer;

        public ClassWithSeveralMethods(ITracer tracer)
        {
            this.tracer = tracer;
        }

        public void MethodWithInnerMethods()
        {
            InnerMethod1();
            InnerMethod2();
        }

        public void InnerMethod1()
        {
            tracer.StartTrace();
            Thread.Sleep(50);
            DeepMethod();
            tracer.StopTrace();
        }

        public void InnerMethod2()
        {
            tracer.StartTrace();
            Thread.Sleep(100);
            tracer.StopTrace();
        }

        public void DeepMethod()
        {
            tracer.StartTrace();
            Thread.Sleep(50);
            tracer.StopTrace();
        }
    }
}

