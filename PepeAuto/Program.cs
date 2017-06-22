using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PepeAuto
{
    class Program
    {
        static void Main(string[] args)
        {
            LinkProcessor lp = new LinkProcessor(null);

            new AwesomiumLinkProc(lp);
            ConcurrentQueue<Uri> qu = new ConcurrentQueue<Uri>();
            bool run = true;

            Thread lProc = new Thread(() =>
            {
                lp.RunProcessing(new List<Uri>()
                    {
                        new Uri("https://www.google.com")
                    },
                    qu
                );
            })
            {
                Name = "LinkProcessor"
            };
            Thread iProc = new Thread(() =>
            {
                while (run)
                {
                    bool b = qu.TryDequeue(out Uri uri);
                    if (!b) continue;

                    Console.WriteLine("Image Found: " + uri.ToString());
                }
            })
            {
                Name = "ImageProcessor"
            };

            lProc.Start();
            iProc.Start();

            lProc.Join();
            Console.WriteLine("lProc exited");
            iProc.Join();
            Console.WriteLine("iProc exited");

            AwesomiumLinkProc.KillAweThread();

        }


    }
}
