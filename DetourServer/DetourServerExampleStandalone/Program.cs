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
            Task task = StandaloneServer.StartDetourServer();
            task.Wait();
        }

        private static void OnClientJoinedRoom(string Address, string RoomId, DetourMessage RoomMessage)
        {
            System.Console.WriteLine("made it to roomjoined ");
            var p = RoomMessage as ClientRequestingRoomJoin;
            Server.StoreClientData(Address, "Name", p.Name);
            Server.SendToRoomExcept(RoomId, new List<string>(new string[] { Address }), new ClientJoinedRoomMessage() {MessageType = (int)MessageTypes.ClientJoinedRoomMessage, Name = p.Name });
            var c = new List<string>();
            foreach (var item in Server.Rooms[RoomId].RoomClients.Values)
            {
                c.Add(item.StoredData["Name"] as string);
            }
            Server.SendMessage(Address, new ClientRoomDataCatchUp { MessageType = (int)MessageTypes.ClientRoomDataCatchUp, Names = c });
            System.Console.WriteLine("sent roomdatacatchup message");
        }

        private static void OnClientSentTestMessage(string Address, DetourMessage msg)
        {
            var testMsg = msg as ClientSentTestMessage;
            System.Console.WriteLine("Received TestString: " + testMsg.TestString);
        }
    }
}
