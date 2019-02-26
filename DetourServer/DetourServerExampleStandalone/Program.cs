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
            Server.UseRoomHandling(new RoomDefinition {RoomType = "Default", RoomCapacity = 2, OnRoomJoined = OnClientJoinedRoom });
            Task task = StandaloneServer.StartDetourServer();
            task.Wait();
        }

        private static void OnClientJoinedRoom(string Address, string RoomId, DetourMessage RoomMessage)
        {
            var p = RoomMessage as ClientRequestingRoomJoin;
            Server.StoreClientData(Address, "Name", p.Name);
            Server.SendToRoomExcept(RoomId, new List<string>(new string[] { Address }), new ClientJoinedRoomMessage() {MessageType = 16, Name = p.Name });
            var c = new List<string>();
            foreach (var item in Server.Rooms[RoomId].RoomClients.Values)
            {
                c.Add(item.StoredData["Name"] as string);
            }
            Server.SendMessage(Address, new ClientRoomDataCatchUp { Names = c });
        }

        private static void OnClientSentTestMessage(string Address, DetourMessage msg)
        {
            var testMsg = msg as ClientSentTestMessage;
            System.Console.WriteLine("Received TestString: " + testMsg.TestString);
        }
    }
}
