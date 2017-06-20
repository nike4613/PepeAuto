using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PepeAuto
{
    public class AwesomiumLinkProc
    {
        static AwesomiumLinkProc()
        {
            
        }

        public AwesomiumLinkProc(LinkProcessor lp, int priority=0): this()
        {
            RegisterUriProcessor(lp, priority);
        }
        public AwesomiumLinkProc()
        {
            WebCore.Initialize(new WebConfig() { LogLevel = LogLevel.Verbose });
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
            List<Uri> uriOut = new List<Uri>();
            List<Uri> imgOut = new List<Uri>();

            ManualResetEvent revent = new ManualResetEvent(false);

            using (var webv = WebCore.CreateWebView(1280, 720))
            {

                webv.Source = toProc;

                using (JSObject jsobj = webv.CreateGlobalJavascriptObject("UriProcessor"))
                {

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
                    jsobj.Bind("MarkDone", (sender, e) =>
                    {
                        revent.Set();

                        return JSValue.Undefined;
                    });
                }

                string JScript =
    @"

document.addEventHandler(""load"", function(){
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
    UriProcessor.ReturnUris(linkarray);
    UriProcessor.ReturnImages(imarray);
    UriProcessor.MarkDone();
});

";

                webv.ExecuteJavascriptWithResult(JScript);

                

            }

            revent.WaitOne();

            return new Tuple<IList<Uri>, IList<Uri>>(new List<Uri>() { toProc }, new List<Uri>() { toProc });
        }
    }
}
