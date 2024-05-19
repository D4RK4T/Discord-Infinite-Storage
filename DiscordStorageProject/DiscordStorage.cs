using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Runtime.Remoting.Channels;
using System.Net.Http.Headers;
using System.Security.Policy;
using static DiscordStorageProject.DiscordStorage;
using System.Net;
using System.Threading;

namespace DiscordStorageProject
{
    public class DiscordStorage
    {
        public string Token { get; set; }

        public string ServerID { get; set; }
        public string ChannelID { get; set; }
        public string Webhook { get; set; }


        //DISCORD MESSAGES ATTACHMENTS
        public class DiscordMessage
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("attachments")]
            public List<AttachmentFile> Attachments { get; set; }
        }
        public class AttachmentFile
        {
            [JsonProperty("filename")]
            public string FileName { get; set; }

            [JsonProperty("size")]
            public long FileSize { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            public string RealFileName { get; set; }
            public int FileID { get; set; }
            public string MessageID { get; set; }
            public int FilePart {  get; set; }
        }


        public List<AttachmentFile> _files = new List<AttachmentFile>();


        //SEND FILE REQUESTS
        public void UploadFile(string filePath)
        {
            CutFileAndSend(filePath);
        }
        public bool UploadFilePart(string fileName, byte[] fileBytes)
        {
            using (HttpClient client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    var byteContent = new ByteArrayContent(fileBytes);
                    byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    content.Add(byteContent, "file", fileName);

                    var response = client.PostAsync(Webhook, content).Result;
                    if (response.IsSuccessStatusCode)
                        return true;
                    return false;
                }
            }
        }
        public void CutFileAndSend(string filePath)
        {
            long chunkSize = 24 * 1024 * 1024;
            long fileSize = new FileInfo(filePath).Length;
            Random random = new Random();
            int randomId = random.Next(100000, 999999);
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                long _bytesRead = 0;
                int fileID = 0;

                while (_bytesRead < fileSize)
                {
                    long bytesToRead = Math.Min(chunkSize, fileSize - _bytesRead);
                    byte[] chunk = new byte[bytesToRead];
                    int bytesRead = fs.Read(chunk, 0, (int)bytesToRead);

                    UploadFilePart(Path.GetFileName(filePath) + "_" + randomId + "_" + fileID, chunk);

                    _bytesRead += bytesRead;
                    fileID++;

                    if (_bytesRead == fileSize)
                        break;
                }
            }
        }
        
        
        
        //GET FILES
        public string GetMessagesRequest()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("authorization", Token);
                client.DefaultRequestHeaders.Add("dnt", "1");
                client.DefaultRequestHeaders.Add("origin", "https://discord.com");
                client.DefaultRequestHeaders.Add("priority", "u=1, i");
                client.DefaultRequestHeaders.Add("referer", $"https://discord.com/channels/{ServerID}");
                client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"124\", \"Microsoft Edge\";v=\"124\", \"Not-A.Brand\";v=\"99\"");
                client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
                client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
                client.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36 Edg/124.0.0.0");
                client.DefaultRequestHeaders.Add("x-debug-options", "bugReporterEnabled");
                client.DefaultRequestHeaders.Add("x-discord-locale", "fr");
                client.DefaultRequestHeaders.Add("x-discord-timezone", "Europe/Paris");

                var response = client.GetAsync($"https://discord.com/api/v9/channels/{ChannelID}/messages?limit=100").Result;
                var responseBody = response.Content.ReadAsStringAsync().Result;
                return responseBody;
            }
        }
        public string GetMessagesBefore(string messageId)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("authorization", Token);
                client.DefaultRequestHeaders.Add("dnt", "1");
                client.DefaultRequestHeaders.Add("origin", "https://discord.com");
                client.DefaultRequestHeaders.Add("priority", "u=1, i");
                client.DefaultRequestHeaders.Add("referer", $"https://discord.com/channels/{ServerID}");
                client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"124\", \"Microsoft Edge\";v=\"124\", \"Not-A.Brand\";v=\"99\"");
                client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
                client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
                client.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36 Edg/124.0.0.0");
                client.DefaultRequestHeaders.Add("x-debug-options", "bugReporterEnabled");
                client.DefaultRequestHeaders.Add("x-discord-locale", "fr");
                client.DefaultRequestHeaders.Add("x-discord-timezone", "Europe/Paris");

                var response = client.GetAsync($"https://discord.com/api/v9/channels/{ChannelID}/messages?before={messageId}&limit=100").Result;
                var responseBody = response.Content.ReadAsStringAsync().Result;
                return responseBody;
            }
        }
        public List<AttachmentFile> DeserialiseMessages()
        {
            _files.Clear();
            List<AttachmentFile> files = new List<AttachmentFile>();

            string jsonMessages = GetMessagesRequest();
            var msgs = JsonConvert.DeserializeObject<List<DiscordMessage>>(jsonMessages);

            Console.WriteLine(msgs.Count);

            if (msgs.Count >= 100)
            {
                while (true)
                {
                    string jsonMessagesBefore = GetMessagesBefore(msgs[msgs.Count - 1].Id);
                    var beforeMsgs = JsonConvert.DeserializeObject<List<DiscordMessage>>(jsonMessagesBefore);

                    foreach (var befmsg in beforeMsgs)
                        msgs.Add(befmsg);

                    if (!(beforeMsgs.Count >= 100))
                        break;
                }
            }
            

            foreach (var msg in msgs)
            {
                foreach (var attachment in msg.Attachments)
                {
                    attachment.RealFileName = attachment.FileName.Split('_')[0];
                    attachment.FileID = Convert.ToInt32(attachment.FileName.Split('_')[1]);
                    attachment.FilePart = Convert.ToInt32(attachment.FileName.Split('_')[2]);
                    attachment.MessageID = msg.Id;
                    files.Add(attachment);

                }

            }

            return files;

        }
        public void RefreshFilesList()
        {
            _files = DeserialiseMessages();
        }


        //DOWNLOAD FILES
        public void DownloadFile(string fileName, string downloadPath)
        {
            foreach (var file in _files)
            {
                if (file.RealFileName == fileName)
                {
                    List<AttachmentFile> files2Download = GetSortFiles(file.FileID);

                    string fullPath = Path.Combine(downloadPath, file.RealFileName);
                    

                    foreach (var attachment in files2Download)
                    {
                        Download(attachment, downloadPath);
                    }
                    break;
                }
            }
        }
        public void Download(AttachmentFile file, string downloadPath)
        {
            string fullPath = Path.Combine(downloadPath, file.RealFileName);

            byte[] fileBytes;
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(file.Url).Result;
                fileBytes = response.Content.ReadAsByteArrayAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    AddBytesToFile(fullPath, fileBytes);
                }
            }

            
        }
        public List<AttachmentFile> GetSortFiles(int fileId)
        {
            List<AttachmentFile> files = new List<AttachmentFile>();

            foreach (var file in _files)
            {
                if (file.FileID == fileId)
                {
                    files.Add(file);
                }
            }

            files = files.OrderBy(af => af.FilePart).ToList();
            return files;
        }
        static void AddBytesToFile(string filePath, byte[] bytesToAdd)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            {
                fs.Write(bytesToAdd, 0, bytesToAdd.Length);
            }
        }

        //DELETE FILES
        public void DeleteFile(string fileName)
        {
            foreach (var file in _files)
            {
                if (file.RealFileName == fileName) 
                { 
                    DeleteMessage(file.MessageID);
                    Thread.Sleep(1500); //AVOID RATE LIMIT
                }
            }
        }
        public void DeleteMessage(string msgId)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("authorization", Token);
                client.DefaultRequestHeaders.Add("dnt", "1");
                client.DefaultRequestHeaders.Add("origin", "https://discord.com");
                client.DefaultRequestHeaders.Add("priority", "u=1, i");
                client.DefaultRequestHeaders.Add("referer", $"https://discord.com/channels/{ServerID}");
                client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"124\", \"Microsoft Edge\";v=\"124\", \"Not-A.Brand\";v=\"99\"");
                client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
                client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
                client.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36 Edg/124.0.0.0");
                client.DefaultRequestHeaders.Add("x-debug-options", "bugReporterEnabled");
                client.DefaultRequestHeaders.Add("x-discord-locale", "fr");
                client.DefaultRequestHeaders.Add("x-discord-timezone", "Europe/Paris");

                var response = client.DeleteAsync($"https://discord.com/api/v9/channels/{ChannelID}/messages/{msgId}").Result;
            }
        }



    }
}
