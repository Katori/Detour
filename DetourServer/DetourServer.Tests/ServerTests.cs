using NUnit.Framework;
using System.Text;

namespace DetourServer.Tests
{
    [TestFixture]
    public class ServerTests
    {
        ClientContainer TestClientContainer;

        [SetUp]
        public void Setup()
        {
            Server.RegisterHandler(1, typeof(ClientSentTestMessage), OnClientSentTestMessage);
            TestClientContainer = new ClientContainer("TestClient", null);
        }

        [Test]
        public void ClientContainer_ProcessJson_TestData()
        {
            var buffer = Encoding.UTF8.GetBytes("{\"MessageType\": 1,\"DetourVersion\": null,\"ApplicationVersion\": null,\"TestString\": \"TestData\"}");
            var realMessage = (ClientSentTestMessage)TestClientContainer.ProcessJsonIntoDetourMessage(buffer, buffer.Length);
            Assert.That(realMessage.TestString.Equals("TestData"), Is.True);
        }

        private void OnClientSentTestMessage(DetourMessage msg)
        {
            var testMsg = msg as ClientSentTestMessage;
            Assert.That(testMsg.TestString == "TestData");
        }

        [System.Serializable]
        public class ClientSentTestMessage : DetourMessage
        {
            public string TestString;
        }
    }

    
}