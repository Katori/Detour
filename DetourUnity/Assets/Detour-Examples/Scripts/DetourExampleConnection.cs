using System.Collections.Generic;
using UnityEngine;
using DetourClient;

namespace Detour.Examples.Client
{
    public class DetourExampleConnection : MonoBehaviour
    {
        internal static DetourExampleConnection Instance { get; private set; }

        private DetourConnection conn = new DetourConnection();

        public List<PlayerDefinition> PlayersList;

        internal Dictionary<string, PlayerDefinition> Players = new Dictionary<string, PlayerDefinition>();

        private string _Name;

        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            conn.RegisterHandler(1, typeof(TestMessage), OnServerSentTestMessage);
            conn.RegisterHandler((int)MessageTypes.ClientJoinedRoomMessage, typeof(ClientJoinedRoomMessage), OnClientJoinedRoom);
            conn.RegisterHandler((int)MessageTypes.ClientRoomDataCatchUp, typeof(ClientRoomDataCatchUp), RoomDataCatchUpReceived);
            conn.Connected += Conn_Connected;
            conn.Disconnected += Conn_Disconnected;
        }

        private void Update()
        {
            if (conn.ConnectionActive)
            {
                conn.Send();
            }
        }

        private void Conn_Disconnected()
        {
            Debug.Log("disconn");
            UIController.Instance.ShowConnectionUI();
        }

        internal void Connect(string Name)
        {
            _Name = Name;
            System.UriBuilder TestServerUri = new System.UriBuilder("ws", "localhost", 27416, "game");
            Debug.Log(TestServerUri.Uri.ToString());
            conn.Connect(TestServerUri.Uri);
        }

        private void RoomDataCatchUpReceived(DetourMessage netMsg)
        {
            var c = netMsg as ClientRoomDataCatchUp;
            Debug.Log("received room catchup data");
            foreach (var item in c.Players)
            {
                AddPlayer(item);
            }
        }

        private void OnClientJoinedRoom(DetourMessage netMsg)
        {
            Debug.Log("Received RoomJoin");
            var c = netMsg as ClientJoinedRoomMessage;
            AddPlayer(c.Player);
        }

        private void Conn_Connected()
        {
            conn.SendMessage(new DetourMessage {MessageType = 0 });
            conn.SendMessage(new TestMessage { MessageType = 1, TestString = "TestData" });
            conn.SendMessage(new ClientRequestingRoomJoin {MessageType = (int)MessageTypes.RoomRequestMessage, RequestedRoomType = "Default", Name = _Name });
            Debug.Log("connected");
            UIController.Instance.HideConnectionUI();
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

        private void AddPlayer(PlayerDefinition PlayerToAdd)
        {
            PlayersList.Add(PlayerToAdd);
            Players.Add(PlayerToAdd.Id, PlayerToAdd);
        }
    }
}