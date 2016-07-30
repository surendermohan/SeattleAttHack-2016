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
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Gmail API service.
            GmailService service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            UsersResource.MessagesResource.ListRequest messagesRequest = service.Users.Messages.List("me");
            messagesRequest.Q = "matthewcalligaro@hotmail.com";
            IList<Message> messages = messagesRequest.Execute().Messages;

            for (int i = 0; i < messages.Count; i++)
            {
                UsersResource.MessagesResource.GetRequest messageRequest = service.Users.Messages.Get("me", messages[i].Id);
                messageRequest.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Minimal;
                Message message = messageRequest.Execute();

                Console.WriteLine(message.Snippet);
                Console.WriteLine();
            }


            Console.WriteLine("Finished");
            WalmartApi("");
            Console.Read();
        }

        //Take theproduct name and find the item based on the 
        public static void WalmartApi(String name)
        {
            WebRequest request = WebRequest.Create(
              "http://api.walmartlabs.com/v1/search?apiKey=28npz4h9tt2pmgmkh6fse5tr&query=" + name);

            WebResponse response = request.GetResponse();

            Stream dataStream = response.GetResponseStream();
            //testing to see if the data was responding correctly
            Console.WriteLine(dataStream.ToString());


            response.Close();
        }
    }
}
