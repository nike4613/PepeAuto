using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepeAuto
{
    class Program
    {
        static void Main(string[] args)
        {
            LinkProcessor lp = new LinkProcessor(null);

            new AwesomiumLinkProc(lp);

            lp.SortProcList();

            ConcurrentQueue<Uri> qu = new ConcurrentQueue<Uri>();

            lp.RunProcessing(new List<Uri>()
                {
                    new Uri("https://www.google.com")
                },
                qu
            );

        }


    }
}
