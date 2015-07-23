using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace msbuild_lesschat
{
    public class LesschatClient
    {
        private readonly Uri _webhook;
        private readonly JsonSerializerSettings _settings;

        public LesschatClient(Uri webhook)
        {
            _webhook = webhook;
            _settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
            };
        }

        private async Task<LesschatIncomingMessageResponse> SendAsycImpl(string json)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = _webhook;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(string.Empty, content);

                response.EnsureSuccessStatusCode();
                var rspn = JsonConvert.DeserializeObject<LesschatIncomingMessageResponse>(await response.Content.ReadAsStringAsync());
                if (rspn == null || rspn.Code != HttpStatusCode.OK)
                {
                    throw new Exception(string.Format("Failed to send message to lesschat. Message = {0}, webhook address = {1}.", json, _webhook.AbsoluteUri));
                }
                else
                {
                    return rspn;
                }
            }
        }

        public async Task<LesschatIncomingMessageResponse> SendAsync(string message)
        {
            var json = JsonConvert.SerializeObject(new LesschatIncomingMessageLite(message), Formatting.None, _settings);
            return await SendAsycImpl(json);
        }

        public async Task<LesschatIncomingMessageResponse> SendAsync(LesschatIncomingMessage message)
        {
            if (message.Attachment.Fields != null && message.Attachment.Fields.Count <= 0)
            {
                message.Attachment.Fields = null;
            }
            var json = JsonConvert.SerializeObject(message, Formatting.None, _settings);
            return await SendAsycImpl(json);
        }

        public LesschatIncomingMessageResponse Send(LesschatIncomingMessage message)
        {
            return SendAsync(message).Result;
        }
    }
}
