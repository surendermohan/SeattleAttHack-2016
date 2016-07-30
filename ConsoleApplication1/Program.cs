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
using System.Net.Mail;
using Newtonsoft.Json;

namespace ConsoleApplication1
{
    class Program
    {
        static string[] Scopes = { GmailService.Scope.GmailReadonly };
        static string ApplicationName = "Gmail API .NET Quickstart";
        static GmailService service;

        static List<ItemWatch> itemWatches = new List<ItemWatch>();

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
                Console.WriteLine("Credential file saved to: " + credPath + "\n");
            }

            // Create Gmail API service.
            service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            //SendMessage();

            Console.WriteLine("...Awaiting Input (load, update, or exit)");
            string input = Console.ReadLine();
            while (input != "exit")
            {
                if (input.Equals("load"))
                {
                    LoadNewPurchases();
                }
                else if (input.Equals("update"))
                {
                    UpdateSavings();
                }
                else
                {
                    Console.WriteLine("Invalid input");
                }

                Console.WriteLine("\n...Awaiting Input (load, update, or exit)");
                input = Console.ReadLine();
            }

        }

        public static void LoadNewPurchases()
        {
            UsersResource.MessagesResource.ListRequest messagesRequest = service.Users.Messages.List("me");
            messagesRequest.Q = "from:matthewcalligaro@hotmail.com";
            IList<Message> messages = messagesRequest.Execute().Messages;

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

                string upc = GetUPC(itemName);
                Console.WriteLine("UPC: " + upc);

                try
                {
                    itemWatches.Add(new ItemWatch(upc, Double.Parse(price), itemName));
                    Console.WriteLine("Load Sucessful");
                }
                catch
                {
                    Console.WriteLine("An error occured in reading the price");
                }
            }
        }

        public static void UpdateSavings()
        {
            if (itemWatches.Count == 0)
            {
                Console.WriteLine("You currently have no items to watch");
            }
            for (int i = 0; i < itemWatches.Count; i++)
            {
                double newPrice = GetPrice(itemWatches[i].upc);

                if (newPrice != Double.NaN)
                {
                    if (newPrice < itemWatches[i].price)
                    {
                        Console.WriteLine("Product Name: " + itemWatches[i].name);
                        Console.WriteLine("Purchase Price: $" + itemWatches[i].price);
                        Console.WriteLine("Current Price: $" + newPrice);
                        Console.WriteLine("Amount to be earned: $" + (itemWatches[i].price - newPrice));
                        Console.WriteLine("% to be earned: " + ((int)((itemWatches[i].price - newPrice)/itemWatches[i].price * 10000) / 100.0) + "%");
                    }
                }
            }
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

        public static string GetUPC(String productName)
        {
            WebClient webclient = new WebClient();
            string formatedName = productName.Replace(" ", "%20");

            string result = webclient.DownloadString("http://api.walmartlabs.com/v1/search?apiKey=28npz4h9tt2pmgmkh6fse5tr&query=" + formatedName);

            string upc = result.Substring(result.IndexOf("upc\":\"") + 6, result.IndexOf("\"", result.IndexOf("upc\":\"") + 6) - result.IndexOf("upc\":\"") - 6);
            return upc;

            //dynamic jsonResult = JsonConvert.DeserializeObject<dynamic>(result);
            //return jsonResult.upc;            
        }

        public static double GetPrice(string upc)
        {
            WebClient webclient = new WebClient();

            string result = webclient.DownloadString("http://api.walmartlabs.com/v1/search?apiKey=28npz4h9tt2pmgmkh6fse5tr&query=" + upc);
            string strPrice = result.Substring(result.IndexOf("salePrice\":") + 11, result.IndexOf(",", result.IndexOf("salePrice\":") + 11) - result.IndexOf("salePrice\":") - 11);
            double price;
            try
            {
                price = Double.Parse(strPrice);
            }
            catch
            {
                price = Double.NaN;
            }

            return price;
        }

        public static void SendMessage()
        {
            MailMessage message = new AE.Net.Mail.MailMessage
            {
                Subject = "Your Subject",
                Body = "Hello, World, from Gmail API!",
                From = new MailAddress("[you]@gmail.com")
            };
            message.To.Add(new MailAddress("me"));
            message.ReplyToList.Add(message.From); 
            StringWriter msgStr = new StringWriter();
            //message.Save(msgStr);

            var result = service.Users.Messages.Send(new Message
            {
                Raw = Base64UrlEncode(msgStr.ToString())
            }, "me").Execute();

            Console.WriteLine("Message ID {0} sent.", result.Id);
        }

        private static string Base64UrlEncode(string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            // Special "url-safe" base64 encode.
            return Convert.ToBase64String(inputBytes)
              .Replace('+', '-')
              .Replace('/', '_')
              .Replace("=", "");
        }
    }
}
