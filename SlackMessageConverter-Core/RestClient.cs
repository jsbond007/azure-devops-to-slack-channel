using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SlackMessageConverter.Core
{
    public class RestClient
    {
        public async static Task SendToSlack(string strMessage, List<AttachmentField> fields, string url)
        {

           
            SlackMessage message = new SlackMessage()
            {
                attachments = new List<Attachment>()
            };

            message.attachments.Add(new Attachment
            {
                fields = fields,
                pretext = strMessage
            });


            var d = JsonConvert.SerializeObject(message);

            await PostRequest(url, message);
        }

        private static HttpRequestMessage GetPostMessage(string url, dynamic data)
        {
            string json = JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = content,
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
            };



            //request.Headers.Add("contentType", "application/json");
            return request;

        }
        private static HttpClient _defaultClient;
        private static HttpClient GetHttpClient()
        {
            if (_defaultClient == null)
            {
                _defaultClient = new HttpClient();
                _defaultClient.Timeout = new TimeSpan(2, 0, 5000);
            }

            return _defaultClient;
        }

        private static async Task<string> PostRequest(string url, dynamic data)
        {
            try
            {
                HttpClient client = GetHttpClient();
                var request = GetPostMessage(url, data);

              
                var response = await client.SendAsync(request);

                var responseJson = await response.Content.ReadAsStringAsync();
                return responseJson;
				
            }
            catch 
            {
                throw;                
            }

        }
    }
}
