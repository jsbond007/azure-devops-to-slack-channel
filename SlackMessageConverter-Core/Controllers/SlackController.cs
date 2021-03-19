using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace SlackMessageConverter.Core
{
    [Route("api/[controller]")]
    [ApiController]
    public class SlackController : ControllerBase
    {
        [HttpPost]
        public async void ConvertAndSend([FromBody] JObject data)
        {


            string user = this.Request.Query["user"].ToString();
            string cid = this.Request.Query["channel"].ToString();
            string token = this.Request.Query["token"].ToString();

            string url = $"https://hooks.slack.com/services/{user}/{cid}/{token}";

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            //var data = JObject.Parse(body);


            var oest = data.SelectToken("resource.fields.['Microsoft.VSTS.Scheduling.OriginalEstimate']");
            var allFields = data.SelectToken("resource.fields").Where(p => p.Path.Contains("Critical"));

            var attachFields = new List<AttachmentField>();


            string postMessage = "";
            postMessage = data.SelectToken("message.html")?.ToString();

            if (oest != null)
            {

                attachFields.Add(new AttachmentField
                {
                    title = "Old Original Estimation",
                    value = oest.SelectToken("oldValue")?.ToString()
                });

                attachFields.Add(new AttachmentField
                {
                    title = "New Original Estimation",
                    value = oest.SelectToken("newValue")?.ToString()
                });
            }


            foreach (var field in allFields)
            {
                if (data.SelectToken(field.Path + ".oldValue") != null)
                {
                    attachFields.Add(new AttachmentField
                    {
                        title = "Old " + GetFieldTitle(field.Path),
                        value = data.SelectToken(field.Path + ".oldValue")?.ToString() ?? ""
                    });
                }

                if (data.SelectToken(field.Path + ".newValue") != null)
                {
                    attachFields.Add(new AttachmentField
                    {
                        title = "New " + GetFieldTitle(field.Path),
                        value = data.SelectToken(field.Path + ".newValue")?.ToString() ?? ""
                    });
                }
            }


            if (attachFields.Count > 0)
            {
                var html = data.SelectToken("message.markdown").ToString();
                var taskBugIndex = html.IndexOf("]", 1);
                var taskBug = html.Substring(1, taskBugIndex - 1);
                var linkEndIndex = html.IndexOf(")", taskBugIndex + 2);
                var link = html.Substring(taskBugIndex + 2, linkEndIndex - taskBugIndex - 2);
                var text = html.Substring(linkEndIndex + 2);
                var finalText = $"<{link}|{taskBug}>{text}";
                await RestClient.SendToSlack(finalText, attachFields, url);
            }


        }

        private string GetFieldTitle(string path)
        {
            string returnValue = path.Replace("resource.fields['", "").Replace("Custom.", "").Replace("Critical_", "").Replace("']", "");
            return returnValue;
        }


    }

    public class AttachmentField
    {
        public AttachmentField()
        {
            @short = true;
        }

        public string title { get; set; }
        public string value { get; set; }
        public bool @short { get; set; }
    }

    public class SlackMessage
    {
        public List<Attachment> attachments { get; set; }
    }

    public class Attachment
    {
        public List<AttachmentField> fields { get; set; }
        public string pretext { get; set; }
        
    }

}