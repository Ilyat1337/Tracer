using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Tracer
{
    public class MethodTracer : ITracer
    {
        private static readonly int METHOD_DEPTH = 2;

        private TraceResult traceResult;
        private readonly ConcurrentDictionary<int, TracingThreadInfo> threadsMap;
        private bool isThreadResultCreated;

        public MethodTracer()
        {
            threadsMap = new ConcurrentDictionary<int, TracingThreadInfo>();
        }

        public TraceResult GetTraceResult()
        {
            if (!isThreadResultCreated)
            {
                traceResult = new TraceResult(threadsMap.Values.Where(tracingInfo => !tracingInfo.IsEmptyThread).
                    Select(tracingInfo => tracingInfo.ThreadTraceResult).ToList());
                CountThreadTimes();
                isThreadResultCreated = true;
            }
            return traceResult;
        }

        private void CountThreadTimes()
        {
            foreach (var threadTraceResult in traceResult.Threads)
                threadTraceResult.Time = threadTraceResult.Methods.Select(method => method.Time).Sum();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void StartTrace()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            TracingThreadInfo tracingThreadInfo = GetOrCreateTracingTreadInfo(threadId);
            MethodBase parentMethod = GetParentMethod();
            long startTime = GetTimeInMs();
            AddStartTraceData(tracingThreadInfo, parentMethod, startTime);
        }

        private TracingThreadInfo GetOrCreateTracingTreadInfo(int threadId)
        {
            if (threadsMap.ContainsKey(threadId))
                return threadsMap[threadId];

            ThreadTraceResult threadTraceResult = new ThreadTraceResult(threadId);
            TracingThreadInfo tracingThreadInfo = new TracingThreadInfo(threadTraceResult);
            return threadsMap[threadId] = tracingThreadInfo;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private MethodBase GetParentMethod()
        {
            StackTrace stackTrace = new StackTrace();
            return stackTrace.GetFrame(METHOD_DEPTH).GetMethod();
        }

        private void AddStartTraceData(TracingThreadInfo tracingThreadInfo, MethodBase parentMethod, long startTime)
        {
            MethodTraceResult methodTraceResult = new MethodTraceResult(parentMethod.Name, parentMethod.ReflectedType.Name);
            TracingMethodInfo tracingMethodInfo = new TracingMethodInfo(startTime, parentMethod, methodTraceResult);
            if (tracingThreadInfo.TracingStack.Count != 0)
            {
                TracingMethodInfo parentTracingMethodInfo = tracingThreadInfo.TracingStack.Peek();
                if (parentTracingMethodInfo.MethodTraceResult.Methods == null)
                    parentTracingMethodInfo.MethodTraceResult.Methods = new List<MethodTraceResult>();
                parentTracingMethodInfo.MethodTraceResult.Methods.Add(methodTraceResult);
            }
            tracingThreadInfo.TracingStack.Push(tracingMethodInfo);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void StopTrace()
        {
            long endTime = GetTimeInMs();
            int threadId = Thread.CurrentThread.ManagedThreadId;
            TracingThreadInfo tracingThreadInfo = TryGetTracingThreadInfo(threadId);
            MethodBase parentMethod = GetParentMethod();
            TrySaveTraceResults(tracingThreadInfo, parentMethod, endTime);
        }

        private void TrySaveTraceResults(TracingThreadInfo tracingThreadInfo, MethodBase parentMethod, long endTime)
        {
            if ((tracingThreadInfo.TracingStack.Count == 0) || (tracingThreadInfo.TracingStack.Peek().MethodBase != parentMethod))
                throw new OrderViolationException();

            TracingMethodInfo tracingMethodInfo = tracingThreadInfo.TracingStack.Pop();
            tracingMethodInfo.MethodTraceResult.Time = (int)(endTime - tracingMethodInfo.StartTime);
            if (tracingThreadInfo.TracingStack.Count == 0)
            {
                tracingThreadInfo.ThreadTraceResult.Methods.Add(tracingMethodInfo.MethodTraceResult);
                tracingThreadInfo.IsEmptyThread = false;

                isThreadResultCreated = false;
            }
        }

        private TracingThreadInfo TryGetTracingThreadInfo(int threadId)
        {
            if (!threadsMap.ContainsKey(threadId))
                throw new OrderViolationException();

            return threadsMap[threadId];
        }

        private long GetTimeInMs()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }

    class TracingThreadInfo
    {
        public TracingThreadInfo(ThreadTraceResult threadTraceResult)
        {
            ThreadTraceResult = threadTraceResult;
            TracingStack = new Stack<TracingMethodInfo>();
            IsEmptyThread = true;
        }

        public Stack<TracingMethodInfo> TracingStack
        { get; }

        public ThreadTraceResult ThreadTraceResult
        { get; }

        public bool IsEmptyThread
        { get; set; }
    }

    class TracingMethodInfo
    {
        public TracingMethodInfo(long startTime, MethodBase methodBase, MethodTraceResult methodTraceResult)
        {
            StartTime = startTime;
            MethodBase = methodBase;
            MethodTraceResult = methodTraceResult;
        }

        public long StartTime
        { get; }

        public MethodBase MethodBase
        { get; }

        public MethodTraceResult MethodTraceResult
        { get; }
    }
}
