using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lesschat.Plugins.MSBuildLogger.IncomingMessage
{
    public class LesschatIncomingMessageResponse
    {
        public HttpStatusCode Code { get; set; }

        public string Message { get; set; }
    }
}
