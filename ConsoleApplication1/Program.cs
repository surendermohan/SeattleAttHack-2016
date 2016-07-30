using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace ConsoleApplication1
{
    class Program
    {
        static string[] Scopes = { GmailService.Scope.GmailReadonly };
        static string ApplicationName = "Gmail API .NET Quickstart";

        static void Main(string[] args)
        {
            UserCredential credential;

            using (FileStream stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/gmail-dotnet-quickstart2.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                //Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Gmail API service.
            GmailService service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            UsersResource.MessagesResource.ListRequest messagesRequest = service.Users.Messages.List("me");
            messagesRequest.Q = "from:matthewcalligaro@hotmail.com";
            IList<Message> messages = messagesRequest.Execute().Messages;

            //for (int i = 0; i < messages.Count; i++)
            //{
            //    UsersResource.MessagesResource.GetRequest messageRequest = service.Users.Messages.Get("me", messages[i].Id);
            //    messageRequest.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Minimal;
            //    Message message = messageRequest.Execute();

            //    //Console.WriteLine(i + ": " + message.Snippet);
            //}

            for (int i = 0; i < messages.Count; i++)
            {
                UsersResource.MessagesResource.GetRequest messageRequest = service.Users.Messages.Get("me", messages[i].Id);
                Message message = messageRequest.Execute();

                string messageBody = GetMimeString(message.Payload);
                messageBody = messageBody.Replace("\r", "");
                messageBody = messageBody.Replace("\n", "");
                for (int j = 0; j < 9; j++)
                {
                    messageBody = messageBody.Replace("  ", " ");
                }

                Console.WriteLine(messageBody);
                Console.WriteLine();
                Console.WriteLine();

                string price = messageBody.Substring(messageBody.IndexOf("Order total: $") + 14, messageBody.IndexOf("Billing information") - messageBody.IndexOf("Order total: $") - 14);
                Console.WriteLine("Price: " + price);

                string itemName = messageBody.Substring(messageBody.IndexOf("Price Total ") + 12, messageBody.IndexOf("$", messageBody.IndexOf("Price Total ")) - messageBody.IndexOf("Price Total ") - 15);
                Console.WriteLine("Item Name: " + itemName);

                Console.WriteLine("UPC: " + GetUPC(itemName));
            }


            Console.WriteLine("Finished");
            Console.Read();
        }

        public static String GetMimeString(MessagePart Parts)
        {
            String Body = "";

            if (Parts.Parts != null)
            {
                foreach (MessagePart part in Parts.Parts)
                {
                    Body = String.Format("{0}\n{1}", Body, GetMimeString(part));
                }
            }
            else if (Parts.Body.Data != null && Parts.Body.AttachmentId == null && Parts.MimeType == "text/plain")
            {
                String codedBody = Parts.Body.Data.Replace("-", "+");
                codedBody = codedBody.Replace("_", "/");
                byte[] data = Convert.FromBase64String(codedBody);
                Body = Encoding.UTF8.GetString(data);
            }

            return Body;
        }

        //Take the product name and find the item's upc
        public static string GetUPC(String productName)
        {
            WebClient webclient = new WebClient();
            string formatedName = productName.Replace(" ", "%20");

            string result = webclient.DownloadString("http://api.walmartlabs.com/v1/search?apiKey=28npz4h9tt2pmgmkh6fse5tr&query=" + formatedName);

            string upc = result.Substring(result.IndexOf("upc\":\"") + 6, result.IndexOf("\"", result.IndexOf("upc\":\"") + 6) - result.IndexOf("upc\":\"") - 6);
            return upc;
        }
    }
}