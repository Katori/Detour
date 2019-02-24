using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DetourClient;

namespace DetourExamples
{
    public class DetourExampleConnection : MonoBehaviour
    {
        private DetourConnection conn;

        void Start()
        {
            var uri = new System.Uri("wss://localhost:27416/");
            conn.RegisterHandler(1, typeof(TestMessage), OnServerSentTestMessage);
            conn.Connect(uri);
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