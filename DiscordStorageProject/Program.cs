using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace DiscordStorageProject
{
    internal class Program
    {
        static void Main(string[] args)
        {

            DiscordStorage storage = new DiscordStorage();
            storage.Token = "Discord Token";
            storage.ServerID = "Storage server ID";
            storage.ChannelID = "Storage Channel ID";
            storage.Webhook = "Storage Webhook";

            storage.UploadFile(@"Path\to\file.txt");
            storage.DownloadFile(@"Download\path", @"FileName.txt");
            storage.DeleteFile(@"FileName.txt");
            

            Console.ReadKey();

            

        }
    }
}
