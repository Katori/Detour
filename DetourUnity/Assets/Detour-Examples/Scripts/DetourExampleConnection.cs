using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DetourClient;

namespace DetourExamples
{
    public class DetourExampleConnection : MonoBehaviour
    {
        private DetourConnection conn = new DetourConnection();

        void Start()
        {
            System.UriBuilder blep = new System.UriBuilder("wss", "localhost", 44350, "ws");
            Debug.Log(blep.Uri.ToString());
            conn.RegisterHandler(1, typeof(TestMessage), OnServerSentTestMessage);
            conn.Connect(blep.Uri);
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