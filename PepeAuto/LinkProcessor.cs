using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PepeAuto
{
    public static class MiscStaticMethods
    {
        public static void Add<T>(this IDictionary<T, object> th, T obj)
        {
            th.Add(obj, null);
        }
    }
    public delegate bool Filter<T>(T value);
    public class LinkProcessor
    {
        private ImageProcessor imageProc;

        /**
         * 
         * Returns a tuple: A list of links to follow, and a list of images to process.
         * 
         */
        public delegate Tuple<IList<Uri>, IList<Uri>> UriProcessor(Uri toProc);

        private List<Tuple<int, Filter<Uri>, UriProcessor>> processors = new List<Tuple<int, Filter<Uri>, UriProcessor>>();
        private bool acceptingProcs = true;

        public LinkProcessor(ImageProcessor imprc)
        {
            imageProc = imprc;

            RegisterUriProcessor((Uri u) => true, DefaultUriProcessor.UriProcessor, int.MinValue);
        }
        
        public void RegisterUriProcessor(Filter<Uri> filter, UriProcessor proc, int prec = 0)
        {
            if (acceptingProcs)
                processors.Add(new Tuple<int, Filter<Uri>, UriProcessor>(prec, filter, proc));
            else
                throw new InvalidOperationException("The processing has started! You cannot add processors now!");
        }

        internal void SortProcList()
        {
            if (acceptingProcs)
            {
                processors.Sort(
                (Tuple<int, Filter<Uri>, UriProcessor> a, Tuple<int, Filter<Uri>, UriProcessor> b)
                    => a.Item1 < b.Item1 ? 1 : (a.Item1 > b.Item1 ? -1 : 0));
                acceptingProcs = false;
            } 
            else
                throw new InvalidOperationException("The processing has started!");
        }

        protected Tuple<IList<Uri>, IList<Uri>> HandleUri(Uri tproc)
        {
            UriProcessor proc = (Uri u) => new Tuple<IList<Uri>, IList<Uri>>(new List<Uri>(), new List<Uri>());

            foreach (var proct in processors)
            {
                if (proct.Item2(tproc))
                {
                    proc = proct.Item3;
                    break;
                }
            }

            return proc(tproc);
        }

        public void RunProcessing(IList<Uri> startUris, ConcurrentQueue<Uri> imgoutl)
        {
            if (acceptingProcs)
                SortProcList();

            List<ThreadPoolQueue.ThreadPoolResult<Tuple<IList<Uri>, IList<Uri>>>>
                procQueue = new List<ThreadPoolQueue.ThreadPoolResult<Tuple<IList<Uri>, IList<Uri>>>>();

            // Replace with better method (eats less memory)
            Dictionary<string, object> prevlyused = new Dictionary<string, object>();

            Action<IEnumerable<Uri>> queueEnum = (IEnumerable<Uri> uris) =>
            {
                foreach (var u in uris)
                {
                    if (prevlyused.ContainsKey(u.ToString())) continue;

                    prevlyused.Add(u.ToString());

                    ThreadPoolQueue.QueueUserWorkItem(out var v, HandleUri, u);

                    procQueue.Add(v);
                }
            };

            queueEnum(startUris);

            while (procQueue.Count > 0) {
                var v = procQueue.Where((res) => res.IsDone).ToList();

                foreach (var res in v)
                {
                    procQueue.Remove(res);

                    var links = res.Result.Item1;
                    var imgs = res.Result.Item2;

                    queueEnum(links);
                    foreach (var u in imgs)
                        imgoutl.Enqueue(u);
                }

                
            }

        }
    }
}
