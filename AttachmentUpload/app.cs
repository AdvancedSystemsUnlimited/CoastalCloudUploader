using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;
using System.Text.RegularExpressions;

namespace AttachmentUpload

{
	public class Attachment
    {
        public long Id;
        public string pathName;
        public string authorName;
        public string bodyText;
        public long contactId;
        public long accountId;
        public long contentTypeId;
        public long entryTypeId;
        public string contentType;
    }

    class App
    {
        private readonly ILogger<App> _logger;
        private readonly IArgService _iArgs;
        private readonly IConfigurationRoot _config;
        private readonly string[] args;
        public Attachment[] attachments = null;
        private const int MAXBUFFER = 10000;
        private ConnectAPI apiConfig { get; }
        private long Id { get; set; }
        private long contactId { get; set; }
        private long accountId { get; set; }
        private long contentTypeId { get; set; }
        private string MappedId { get; set; }
        private string FilePath { get; set; }
        private long entryTypeId { get; set; }
        private string Uri { get; set; }
        public App(ILogger<App> logger, ConnectAPI _configAPI)
        {
            _logger = logger;
            apiConfig = _configAPI;
        }
        public void Run(string[] args = null)
        {
            using (var reader = new StreamReader(args[0]))
            {

                List<Attachment> attachments = new List<Attachment>();
                long Id;
                long contactId;
                long accountId;
                //  long contentTypeId;
                long entryTypeId;
                Attachment attachmentSave = new Attachment();
                int lineNo = 0;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var valuesArr = line.Split('|');
                    var hasline = false;

                    if (valuesArr.Length > 2)
                    // Array has 2 parameters, which expects to be the incident ID. (Possibly new row and possibly a row that has wrapped.)
                    {
                        if (hasline == true)
                        {
                            attachments.Add(attachmentSave);
                        }
                        else if (hasline == false)
                        {
                            attachmentSave = new Attachment();
                        }

                        Attachment attachment = new Attachment();
                        if (Int64.TryParse(valuesArr[2], out Id))
                        {
                            Int64.TryParse(valuesArr[6], out contactId);
                            Int64.TryParse(valuesArr[7], out entryTypeId);
                            Int64.TryParse(valuesArr[5], out accountId);
                            // If contentTypeId is passed through the datasource leverage this line and set to the appropriate VaulesArr[x]
                            //Int64.TryParse(valuesArr[7], out contentTypeId);
                            attachment.Id = Id;
                            attachment.bodyText = valuesArr[9].Replace("\\", "\\\\") + "\\n"; // Add line break
                            if (contactId > 0)
                            {
                                attachment.contactId = contactId;
                            }
                            attachment.accountId = accountId;
                            attachment.contentTypeId = 1; // contentTypeId;
                            attachment.entryTypeId = entryTypeId;
                            hasline = true;
                            attachmentSave = attachment;
                        }
                    }
                    else if (valuesArr.Length < 2)
                    {
                        // Found additional body text. Add it to the last file and read the next row.
                        attachmentSave.bodyText = attachmentSave.bodyText + valuesArr[0].Replace("\\", "\\\\") + "\\n"; // Add line break
                    }
                    if (hasline == true)
                    {
                        attachments.Add(attachmentSave);
                    }

                }
                foreach (Attachment att in attachments)
                {
                    ++lineNo; // Row Number
                    // Logging Begins
                    _logger.LogInformation("****Thread Loop and Pass to format JSON ****");
                    _logger.LogInformation($" Line Number: {lineNo}");
                    StringContent requestBody = null;
                    Uri = apiConfig.uri.Replace("{Id}", att.Id.ToString());  // Endpoint and ID (IncidentId)
                    FilePath = att.bodyText; // Thread comment details
                    requestBody = GetUploadFileRequestBody(att.bodyText, att.contentTypeId, att.contactId, att.accountId, att.entryTypeId); // Pass contents to format JSON
                    _logger.LogInformation($" Thread Endpoint: {Uri}");
                    _logger.LogInformation($" INCIDENTID: { att.Id.ToString()}");
                    if (requestBody != null)
                    {
                        OracleServiceCloudAttachmentUpload(requestBody).Wait(); // Post to API
                    }
                    else if (requestBody == null)
                    {
                        _logger.LogInformation("RequestBodyNull");
                    }
                }
            }
        }
        static string EncodeNonAsciiCharacters(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();

        }
        StringContent GetUploadFileRequestBody(string bodyText, long contentId, long contactId, long accountId, long entryTypeId)
        {
            // Clean contents of comment body
            
            bodyText = bodyText.Replace("/\u2013|\u2014/ g", "-");  //Remove em dash
            bodyText = Regex.Replace(bodyText, @"\p{C}+", string.Empty); // Successfully parsed 3 failed rows.
            string bodycontent = bodyText.Replace("\\:", " "); //Correct c:\:
           // bodycontent = bodycontent.Replace("\n", "\\n"); // Remove quotation
           // bodycontent = bodycontent.Replace("\\", "\\\\"); // Escape backslash
            bodycontent = bodycontent.Replace('"', ' '); // Remove quotations
            bodycontent = bodycontent.Replace(" / ", " "); // Escape backslash
            bodycontent = bodycontent.Replace("./ ", " "); // Escape backslash
            string content =
                 "{"
                  + $@"""threads"":[{{";

            if (entryTypeId > 0)
            {
                content = content + $@"""entryType"":{{""id"":{entryTypeId}}},";
            }
            //if (contentId > 0)  // ContentTypeID is the same for all records. If this changes, leverage the contentId variable.
            //  {
            content = content + $@"""contentType"":{{""id"":1}},";
            //}
            if (contactId > 0)
            {
                content = content + $@"""contact"""
                   + $@":{{""id"":{contactId}}},";
            }
            if (accountId > 0)
            {
                content = content + $@"""account"""
             + $@":{{""id"":{accountId}}},";
            }
            content = content + $@"""text"":""{bodycontent}""}}"
         + "]}";
            _logger.LogInformation($" JSON {content}");
            StringContent stringContent = new StringContent(content, Encoding.UTF8, "text/json");
            return stringContent;
        }
        private async Task OracleServiceCloudAttachmentUpload(StringContent content)
        {
            string userName = apiConfig.username, password = apiConfig.password;
            var credentials = new NetworkCredential(userName, password);

            //  Create HTTP Handler with Credentials
            using (var handler = new HttpClientHandler { Credentials = credentials })
            using (var client = new HttpClient(handler))
            {
                // string json = null;
                try
                {
                    //  Invoke Oracle Service Cloud Thread Upload Web API
                    _logger.LogInformation($@"-----------------------TRY PROCESS JSON ----------------------");
                    _logger.LogInformation($@" Access Oracle Service Cloud Thread Web API {Environment.NewLine}uri: {Uri} {Environment.NewLine}UserName: {userName} ");
                    var stringTask = client.PatchAsync(Uri, content).Result;
                    _logger.LogInformation($@" Content: {content.ToString()}");
                    _logger.LogInformation($@" Header Length: {content.Headers.ContentLength}");
                    _logger.LogInformation($@" StatusCode: {stringTask.StatusCode}");
                    _logger.LogInformation($@" Reason Phrase: {stringTask.ReasonPhrase}");
                    _logger.LogInformation($@" Request Msg: {stringTask.RequestMessage}");
                }
                catch (Exception ex)
                {

                    _logger.LogError($@"-----------------------CATCH EXCEPTION ----------------------");
                    _logger.LogError($@"JSON: {content}");
                    _logger.LogError($@"Exception Result: {ex.HResult}");
                    _logger.LogError($@"Exception Message: {ex.Message}");
                    _logger.LogError($@" Credentials {Environment.NewLine}uri: {Uri} {Environment.NewLine}UserName: {userName} ");
                }
                _logger.LogInformation($@"---------------------- END THREAD -------------------");
            }
        }
    }
}
