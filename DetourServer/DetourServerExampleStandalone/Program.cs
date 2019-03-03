using System.Collections.Generic;
using System.Threading.Tasks;
using DetourServer;

namespace DetourServerExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Server.RegisterHandler((int)MessageTypes.ClientSentTestMessage, typeof(ClientSentTestMessage), OnClientSentTestMessage);
            Server.UseRoomHandling(typeof(ClientRequestingRoomJoin), new RoomDefinition {RoomType = "Default", RoomCapacity = 2, OnRoomJoined = OnClientJoinedRoom });
            Server.ApplicationVersion = 0.1f;
            Server.DetourVersion = 0.2f;
            Task task = StandaloneServer.StartDetourServer();
            task.Wait();
        }

        private static void OnClientJoinedRoom(string Address, string RoomId, DetourMessage RoomMessage)
        {
            System.Console.WriteLine("made it to roomjoined ");
            var p = RoomMessage as ClientRequestingRoomJoin;
            Server.StoreClientData(Address, "Name", p.Name);
            Server.StoreClientData(Address, "Position", new Vector2(1, 1));
            Server.SendToRoomExcept(RoomId, new List<string>(new string[] { Address }), new ClientJoinedRoomMessage() {ApplicationVersion = Server.ApplicationVersion, DetourVersion = Server.DetourVersion, RoomId = RoomId, MessageType = (int)MessageTypes.ClientJoinedRoomMessage, Name = p.Name });
            var c = new List<string>();
            //var count = Server.Rooms[RoomId].RoomClientCount;
            //var lV = new Vector2[count-1];
            //var x = new ClientContainer[count-1];

            //Server.Rooms[RoomId].RoomClients.Values.CopyTo(x, 0);
            //for (int i = 0; i < count; i++)
            //{
            //    var item = x[i];
            //    if (item.Id != Address)
            //    {
            //        c.Add(item.StoredData["Name"] as string);
            //        lV[i] = item.StoredData["Position"] as Vector2;
            //    }
            //}

            var lV = new List<Vector2>();
            foreach (var item in Server.Rooms[RoomId].RoomClients.Values)
            {
                if (item.Id != Address)
                {
                    c.Add(item.StoredData["Name"] as string);
                    lV.Add(item.StoredData["Position"] as Vector2);
                }
            }
            Server.SendMessage(Address, new ClientRoomDataCatchUp { DetourVersion = Server.DetourVersion, ApplicationVersion = Server.ApplicationVersion, RoomId = RoomId, MessageType = (int)MessageTypes.ClientRoomDataCatchUp, Names = c, Positions = lV });
            System.Console.WriteLine("sent roomdatacatchup message");
        }

        private static void OnClientSentTestMessage(string Address, DetourMessage msg)
        {
            var testMsg = msg as ClientSentTestMessage;
            System.Console.WriteLine("Received TestString: " + testMsg.TestString);
        }
    }
}
