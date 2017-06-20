using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepeAuto
{
    public class DefaultUriProcessor
    {

        public static Tuple<IList<Uri>, IList<Uri>> UriProcessor(Uri toProc)
        {
            return new Tuple<IList<Uri>, IList<Uri>>(new List<Uri>() { toProc }, new List<Uri>() { toProc } );
        }

    }
}
