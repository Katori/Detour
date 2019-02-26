using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DetourClient;

namespace DetourExamples
{
    public class DetourExampleConnection : MonoBehaviour
    {
        private DetourConnection conn = new DetourConnection();

        async void Start()
        {
            System.UriBuilder TestServerUri = new System.UriBuilder("ws", "localhost", 27416, "game");
            Debug.Log(TestServerUri.Uri.ToString());
            conn.RegisterHandler(1, typeof(TestMessage), OnServerSentTestMessage);
            conn.Connected += Conn_Connected;
            await conn.Connect(TestServerUri.Uri);
        }

        private void Conn_Connected()
        {
            conn.EnqueuedMessagesToSend.Add(new TestMessage {MessageType = 1, TestString = "TestData" });
            conn.EnqueuedMessagesToSend.Add(new TestMessage { MessageType = 1, TestString = "TestData" });
        }

        private void OnServerSentTestMessage(DetourMessage msg)
        {
            var testMsg = msg as TestMessage;
            Debug.Log("received some stuff: " + testMsg.TestString);
        }

        [System.Serializable]
        public class TestMessage : DetourMessage
        {
            public string TestString;
        }
    }
}