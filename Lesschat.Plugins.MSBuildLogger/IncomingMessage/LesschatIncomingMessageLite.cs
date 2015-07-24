using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lesschat.Plugins.MSBuildLogger.IncomingMessage
{
    public class LesschatIncomingMessageLite
    {
        public string Text { get; set; }

        public LesschatIncomingMessageLite(string text)
        {
            Text = text;
        }

        public LesschatIncomingMessageLite() : this(string.Empty)
        {
        }
    }
}
