using System.Collections.Generic;
using UnityEngine;
using DetourClient;
using System.Linq;

namespace Detour.Examples.Client
{
    public class DetourExampleConnection : MonoBehaviour
    {
        internal static DetourExampleConnection Instance { get; private set; }

        private DetourConnection conn = new DetourConnection();

        public List<PlayerDefinition> PlayersList;

        internal Dictionary<string, PlayerDefinition> Players = new Dictionary<string, PlayerDefinition>();

        internal Dictionary<string, GameObject> PlayerObjects = new Dictionary<string, GameObject>();

        public GameObject PlayerPrefab;

        public List<Transform> StartPoints;

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
            conn.RegisterHandler<TestMessage>(1, typeof(TestMessage), OnServerSentTestMessage);
            conn.RegisterHandler<ClientJoinedRoomMessage>((int)MessageTypes.ClientJoinedRoomMessage, typeof(ClientJoinedRoomMessage), OnClientJoinedRoom);
            conn.RegisterHandler<ClientRoomDataCatchUp>((int)MessageTypes.ClientRoomDataCatchUp, typeof(ClientRoomDataCatchUp), RoomDataCatchUpReceived);
            conn.RegisterHandler<PlayerRemovedMessage>(5, typeof(PlayerRemovedMessage), PlayerRemoved);
            conn.Connected += Conn_Connected;
            conn.Disconnected += Conn_Disconnected;
        }

        private void PlayerRemoved(PlayerRemovedMessage netMsg)
        {
            Debug.Log("removing player: " + netMsg.Id);
            RemovePlayer(netMsg.Id);
        }

        private void RemovePlayer(string id)
        {
            Players.Remove(id);
            var p = PlayersList.Where(x => x.Id == id).ToList();
            if (p.Count > 0)
            {
                PlayersList.Remove(p[0]);
            }
            if (PlayerObjects.ContainsKey(id))
            {
                var c = PlayerObjects[id];
                PlayerObjects.Remove(id);
                Destroy(c);
            }
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
                Debug.Log("adding player: " + item.Name);
                AddPlayer(item);
            }
            AddPlayer(new PlayerDefinition
            {
                Id = "0",
                Name = _Name,
                HasMoved = false,
                StartPosition = c.ClientStartPosition
            });
            foreach (var Q in c.MapTiles)
            {
                Debug.Log("got a " + Q.terrainType);
            }
            ExampleGameController.Instance.SetTiles(c.MapTiles);
            Debug.Log("Map size detected: " + c.MapSize);
            MapControllerComponent.Instance.RenderMap(c.MapTiles, c.MapSize);
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
            var c = Instantiate(PlayerPrefab);
            PlayerObjects.Add(PlayerToAdd.Id, c);
            if (!PlayerToAdd.HasMoved)
            {
                var b = StartPoints[PlayerToAdd.StartPosition];
                PlayerToAdd.Position = new Vector2(b.position.x, b.position.z);
            }
            PlayerObjects[PlayerToAdd.Id].transform.position = new Vector3(PlayerToAdd.Position.x, 0, PlayerToAdd.Position.y);
        }
    }
}