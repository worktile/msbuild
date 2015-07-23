using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msbuild_lesschat
{
    public class LesschatIncomingMessage
    {
        public LesschatIncomingMessageAttachment Attachment { get; set; }

        public LesschatIncomingMessage()
        {
            Attachment = new LesschatIncomingMessageAttachment();
        }

        public LesschatIncomingMessage(string text) : this()
        {
            Attachment.Text = text;
        }

        public class LesschatIncomingMessageAttachmentField
        {
            public string Title { get; set; }

            public string Value { get; set; }

            public int Short { get; set; }

            public LesschatIncomingMessageAttachmentField(string title, string value, bool isShort)
            {
                Title = title;
                Value = value;
                Short = isShort ? 1 : 0;
            }

            public LesschatIncomingMessageAttachmentField() : this(string.Empty, string.Empty, true)
            {
            }
        }

        public class LesschatIncomingMessageAttachment
        {
            public string Fallback { get; set; }

            public string Color { get; set; }

            public string Pretext { get; set; }

            [JsonProperty("author_name")]
            public string AuthorName { get; set; }

            [JsonProperty("author_link")]
            public string AuthorLink { get; set; }

            [JsonProperty("author_icon")]
            public string AuthorIcon { get; set; }

            public string Title { get; set; }

            [JsonProperty("title_link")]
            public string TitleLink { get; set; }

            public string Text { get; set; }

            public IList<LesschatIncomingMessageAttachmentField> Fields { get; set; }

            public LesschatIncomingMessageAttachment()
            {
                Fields = new List<LesschatIncomingMessageAttachmentField>();
            }
        }

        public static class LesschatIncomingMessageAttachmentColors
        {
            public static readonly string Error = "#ff0000";

            public static readonly string Warning = "#ffff00";

            public static readonly string Message = "#cccccc";

            public static readonly string Success = "#00ff00";
        }
    }
}
