using System.Collections.Generic;
using UnityEngine;
using DetourClient;

namespace Detour.Examples.Client
{
    public class DetourExampleConnection : MonoBehaviour
    {
        private DetourConnection conn = new DetourConnection();

        public List<string> RoomClientNames;

        async void Start()
        {
            System.UriBuilder TestServerUri = new System.UriBuilder("ws", "localhost", 27416, "game");
            Debug.Log(TestServerUri.Uri.ToString());
            conn.RegisterHandler(1, typeof(TestMessage), OnServerSentTestMessage);
            conn.RegisterHandler((int)MessageTypes.ClientJoinedRoomMessage, typeof(ClientJoinedRoomMessage), OnClientJoinedRoom);
            conn.RegisterHandler((int)MessageTypes.ClientRoomDataCatchUp, typeof(ClientRoomDataCatchUp), RoomDataCatchUpReceived);
            conn.Connected += Conn_Connected;
            await conn.Connect(TestServerUri.Uri);
        }

        private void RoomDataCatchUpReceived(DetourMessage netMsg)
        {
            var c = netMsg as ClientRoomDataCatchUp;
            Debug.Log("received room catchup data");
            RoomClientNames = c.Names;
        }

        private void OnClientJoinedRoom(DetourMessage netMsg)
        {
            var c = netMsg as ClientJoinedRoomMessage;
            RoomClientNames.Add(c.Name);
        }

        private void Conn_Connected()
        {
            conn.SendMessage(new DetourMessage());
            conn.SendMessage(new TestMessage { MessageType = 1, TestString = "TestData" });
            conn.SendMessage(new ClientRequestingRoomJoin {MessageType = (int)MessageTypes.RoomRequestMessage, RequestedRoomType = "Default", Name = "blep" });
            Debug.Log("connected");
        }

        private void OnServerSentTestMessage(DetourMessage msg)
        {
            var testMsg = msg as TestMessage;
            Debug.Log("Received TestString: " + testMsg.TestString);
        }

        [System.Serializable]
        public class TestMessage : DetourMessage
        {
            public string TestString;
        }

        private void OnDisable()
        {
            conn.Disconnect();
        }
    }
}