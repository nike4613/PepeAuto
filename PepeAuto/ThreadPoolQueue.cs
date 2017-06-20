using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PepeAuto
{
    public static class ThreadPoolQueue
    {
        public delegate void QWaitCallback<T>(T state);

        public static bool QueueUserWorkItem<T>(QWaitCallback<T> kuwait, T obj)
        {
            return ThreadPool.QueueUserWorkItem((object o) => kuwait((T)o), obj);
        }

        public class ThreadPoolResult<R>
        {
            private WorkerStructure<R> ws;

            internal ThreadPoolResult(WorkerStructure<R> wos)
            {
                ws = wos;
            }

            public R Result
            {
                get
                {
                    if (IsDone)
                    {
                        return ws.ret;
                    }
                    else
                        throw new InvalidOperationException("The job is not done!");
                }
            }

            public bool IsDone
            {
                get
                {
                    return ws.mrs.WaitOne(TimeSpan.Zero);
                }
            }
        }

        public delegate R ThreadPoolWorker<R>();
        public delegate R ThreadPoolWorker<R, A1>(A1 arg1);
        public delegate R ThreadPoolWorker<R, A1, A2>(A1 arg1, A2 arg2);

        protected internal class WorkerStructure<R>
        {
            public ManualResetEvent mrs = new ManualResetEvent(false);
            public R ret;
        }
        private class WorkerStructure<R, A1> : WorkerStructure<R>
        {
            public A1 arg1;
        }
        private class WorkerStructure<R, A1, A2> : WorkerStructure<R, A1>
        {
            public A2 arg2;
        }

        public static bool QueueUserWorkItem<R>(out ThreadPoolResult<R> result, ThreadPoolWorker<R> worker)
        {
            var wstr = new WorkerStructure<R>();
            QWaitCallback<WorkerStructure<R>> cb = (WorkerStructure<R> str) =>
            {
                str.ret = worker();
                str.mrs.Set();
            };
            result = new ThreadPoolResult<R>(wstr);
            return QueueUserWorkItem(cb, wstr);
        }
        public static bool QueueUserWorkItem<R, A1>(out ThreadPoolResult<R> result, ThreadPoolWorker<R, A1> worker, A1 arg1)
        {
            var wstr = new WorkerStructure<R, A1>()
            {
                arg1 = arg1
            };
            QWaitCallback<WorkerStructure<R, A1>> cb = (WorkerStructure<R, A1> str) =>
            {
                str.ret = worker(str.arg1);
                str.mrs.Set();
            };
            result = new ThreadPoolResult<R>(wstr);
            return QueueUserWorkItem(cb, wstr);
        }
        public static bool QueueUserWorkItem<R, A1, A2>(out ThreadPoolResult<R> result, ThreadPoolWorker<R, A1, A2> worker, 
            A1 arg1, A2 arg2)
        {
            var wstr = new WorkerStructure<R, A1, A2>()
            {
                arg1 = arg1,
                arg2 = arg2
            };
            QWaitCallback<WorkerStructure<R, A1, A2>> cb = (WorkerStructure<R, A1, A2> str) =>
            {
                str.ret = worker(str.arg1, str.arg2);
                str.mrs.Set();
            };
            result = new ThreadPoolResult<R>(wstr);
            return QueueUserWorkItem(cb, wstr);
        }
    }
}
