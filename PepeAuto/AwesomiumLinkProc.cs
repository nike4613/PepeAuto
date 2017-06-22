using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PepeAuto
{
    public class AwesomiumLinkProc
    {
        public static Thread AwesomiumThread;

        static AwesomiumLinkProc()
        {
            StartAweThread();
        }

        public static void StartAweThread()
        {
            ManualResetEvent mre = new ManualResetEvent(false);

            AwesomiumThread = new Thread(() =>
            {
                WebCore.Initialize(new WebConfig() { LogLevel = LogLevel.Verbose });
                WebCore.Run((send, e) =>
                {
                    Console.WriteLine("Awesomium Running");
                    mre.Set();
                });
            });
            AwesomiumThread.Start();

            mre.WaitOne();
        }

        public static void KillAweThread()
        {
            WebCore.Shutdown();
            AwesomiumThread.Join();
            Console.WriteLine("Awesomium Shutdown");
        }

        public AwesomiumLinkProc(LinkProcessor lp, int priority=0): this()
        {
            RegisterUriProcessor(lp, priority);
        }

        public AwesomiumLinkProc()
        {
        }

        public void RegisterUriProcessor(LinkProcessor lp, int priority=0)
        {
            RegisterUriProcessor(lp, (uri) => true, priority);
        }
        public void RegisterUriProcessor(LinkProcessor lp, Filter<Uri> filter, int priority=0)
        {
            lp.RegisterUriProcessor(filter, UriProcessor, priority);
        }

        protected Tuple<IList<Uri>, IList<Uri>> UriProcessor(Uri toProc)
        {
            Console.WriteLine("Queueing work for '" + toProc.ToString() + "'");

            List<Uri> uriOut = new List<Uri>();
            List<Uri> imgOut = new List<Uri>();

            ManualResetEvent MethodDone = new ManualResetEvent(false);

            WebView webv = null;

            WebCore.QueueWork(() =>
            {
                webv = WebCore.CreateWebView(1280, 720);

                webv.Source = toProc;

                webv.DocumentReady += (send, arg) =>
                {
                    if (!webv.HasTitle)
                        return;
                    Console.WriteLine("Processing '" + toProc.ToString() + "'");

                    // For some reason this doesnt work
                    /*JSObject jsobj = webv.ExecuteJavascriptWithResult("window");

                    jsobj.Bind("ReturnImages", (sender, e) =>
                    {
                        var args = e.Arguments;
                        if (!(args.Length > 0 && args[0].IsArray))
                            throw new ArgumentException("Not enough arguments or incorrect type!");
                        JSValue list = args[0];
                        JSValue[] listt = (JSValue[])list;
                        foreach (JSValue v in listt)
                        {
                            if (!v.IsString)
                                throw new ArgumentException("Array contains non-string!");
                            string str = v;
                            Uri uri = new Uri(str);
                            imgOut.Add(uri);
                        }

                        return JSValue.Undefined;
                    });
                    jsobj.Bind("ReturnUris", (sender, e) =>
                    {
                        var args = e.Arguments;
                        if (!(args.Length > 0 && args[0].IsArray))
                            throw new ArgumentException("Not enough arguments or incorrect type!");
                        JSValue list = args[0];
                        JSValue[] listt = (JSValue[])list;
                        foreach (JSValue v in listt)
                        {
                            if (!v.IsString)
                                throw new ArgumentException("Array contains non-string!");
                            string str = v;
                            Uri uri = new Uri(str);
                            uriOut.Add(uri);
                        }

                        return JSValue.Undefined;
                    });

                    string JScript =
@"
window.RunLinkproc = function() {
    // Load all links
    var atags = document.querySelectorAll(""a"");
    var linkarray = [];
    atags.forEach(function(e,i,a) {
        linkarray.push(e.href);
    });
    // Load all images
    var imgtags = document.querySelectorAll(""img"");
    var imarray = [];
    imgtags.forEach(function(e,i,a) {
        imarray.push(e.src);
    });
    // Push to C#
    window.ReturnUris(linkarray);
    window.ReturnImages(imarray);

    return [linkarray, imarray];
};
";

                    var outp = webv.ExecuteJavascriptWithResult(JScript);
                    var out2 = jsobj.Invoke("RunLinkproc");
                    var out3 = webv.ExecuteJavascriptWithResult("window.RunLinkproc()");*/
                    JSObject out4 = webv.ExecuteJavascriptWithResult(@"document.querySelectorAll(""a"")");

                    foreach (var v in out4)
                    {
                        if (!int.TryParse(v, out int n)) continue;

                        var v2 = ((JSObject)out4[v]);
                        var href = v2.GetPropertyDescriptor("href");

                        string v3 = href.Value;
                        if (v3 == "") continue;
                        Uri uri = new Uri(v3);
                        uriOut.Add(uri);
                    }

                    JSObject out5 = webv.ExecuteJavascriptWithResult(@"document.querySelectorAll(""img"")");

                    foreach (var v in out5)
                    {
                        if (!int.TryParse(v, out int n)) continue;

                        var v2 = ((JSObject)out5[v]);
                        var src = v2.GetPropertyDescriptor("src");

                        string v3 = src.Value;
                        if (v3 == "") continue;
                        Uri uri = new Uri(v3);
                        imgOut.Add(uri);
                    }

                    MethodDone.Set();
                };
            });
            
            MethodDone.WaitOne();
            MethodDone.Reset();

            WebCore.QueueWork(() =>
            {
                webv.Dispose();
                MethodDone.Set();
            });
            MethodDone.WaitOne();

            return new Tuple<IList<Uri>, IList<Uri>>(uriOut, imgOut);
        }
    }
}
