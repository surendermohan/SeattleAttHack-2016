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
            messagesRequest.Q = "from:tkhando@pioneer-inc.com";
            IList<Message> messages = messagesRequest.Execute().Messages;

            UsersResource.MessagesResource.GetRequest messageRequest = service.Users.Messages.Get("me", messages[0].Id);
            messageRequest.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Minimal;
            Message message = messageRequest.Execute();

            Console.WriteLine(message.Snippet);

            Console.WriteLine("Finished");
            Console.Read();
        }

    }
}
