using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace msbuild_lesschat
{
    public class LesschatIncomingMessageResponse
    {
        public HttpStatusCode Code { get; set; }

        public string Message { get; set; }
    }
}
