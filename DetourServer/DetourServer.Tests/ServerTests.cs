using Moq;
using NUnit.Framework;
using System.Text;

namespace DetourServer.Tests
{
    [TestFixture]
    public class ServerTests
    {
        ClientContainer TestClientContainer;

        bool TestMessageReceived = false;

        private const string TestData = "{\"MessageType\": 1,\"DetourVersion\": null,\"ApplicationVersion\": null,\"TestString\": \"TestData\"}";

        [SetUp]
        public void Setup()
        {
            Server.ClearHandlers();
            Server.RegisterHandler(1, typeof(ClientSentTestMessage), OnClientSentTestMessage);
            TestClientContainer = new ClientContainer("TestClient", null);
        }

        [Test]
        public void ClientContainer_ProcessJson_TestData()
        {
            var buffer = Encoding.UTF8.GetBytes(TestData);
            var realMessage = (ClientSentTestMessage)TestClientContainer.ProcessJsonIntoDetourMessage(buffer, buffer.Length);
            Assert.That(realMessage.TestString.Equals("TestData"), Is.True);
        }

        [Test]
        public void Server_EventHandler_RaiseEvent()
        {
            Assert.That(TestMessageReceived, Is.False);
            var buffer = Encoding.UTF8.GetBytes(TestData);
            var realMessage = (ClientSentTestMessage)TestClientContainer.ProcessJsonIntoDetourMessage(buffer, buffer.Length);
            Server.ReceivedMessage("vvvv", realMessage);
            Assert.That(realMessage.TestString == "TestData", Is.True);
            Assert.That(TestMessageReceived, Is.True);
        }

        [Test]
        public void Server_Handlers_AddEvent()
        {
            Assert.That(Server.MessageTypeToMessageDefinition[1] != null, Is.True);
        }

        [Test]
        public void Server_Clients_AddClient()
        {
            Server.AddClient(TestClientContainer);
            Assert.That(Server.Clients[TestClientContainer.Id] == TestClientContainer, Is.True);
        }

        private void OnClientSentTestMessage(DetourMessage msg)
        {
            var testMsg = msg as ClientSentTestMessage;
            TestMessageReceived = true;
            Assert.That(testMsg.TestString == "TestData");
        }

        [System.Serializable]
        public class ClientSentTestMessage : DetourMessage
        {
            public string TestString;
        }
    }

    
}