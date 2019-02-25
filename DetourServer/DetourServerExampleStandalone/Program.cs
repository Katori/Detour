using System;
using System.Threading.Tasks;
using DetourServer;

namespace DetourServerExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Server.RegisterHandler(1, typeof(ClientSentTestMessage), OnClientSentTestMessage);
            Task task = StandaloneServer.StartDetourServer();
            task.Wait();
        }

        private static void OnClientSentTestMessage(DetourMessage msg)
        {
            var testMsg = msg as ClientSentTestMessage;
            System.Console.WriteLine("Received TestString: " + testMsg.TestString);
        }

        [System.Serializable]
        public class ClientSentTestMessage : DetourMessage
        {
            public string TestString;
        }
    }
}
