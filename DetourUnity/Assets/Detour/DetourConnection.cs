using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using UnityEngine;

namespace DetourClient
{
    public class DetourConnection
    {
        public event System.Action Connected;
        public event System.Action Disconnected;
        public event System.Action<System.Exception> ReceivedError;

        public bool Connecting { get { return Client.Connecting; } }
        public bool ConnectionActive = false;

        public Dictionary<int, MessageDefinition> MessageTypeToMessageDefinition = new Dictionary<int, MessageDefinition>();

        public List<DetourMessage> EnqueuedMessagesToSend = new List<DetourMessage>();

        private string JSONBuffer;
        private byte[] MessageBuffer;
        
        private JsonSerializerSettings JSONSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        private DetourClient Client = new DetourClient();

        public void Connect(System.Uri uri)
        {
            Client.Connected += Client_Connected;
            Client.ReceivedData += Client_ReceivedData;
            Client.Disconnected += Client_Disconnected;
            Client.ReceivedError += Client_ReceivedError;
            Client.Connect(uri);
        }

        private void Client_ReceivedError(System.Exception obj)
        {
            ReceivedError.Invoke(obj);
        }

        private void Client_Disconnected()
        {
            ConnectionActive = false;
            Disconnected.Invoke();
        }

        private void Client_Connected()
        {
            ConnectionActive = true;
            Connected.Invoke();
        }

        private void Client_ReceivedData(byte[] obj)
        {
            try
            {
                var msg = ProcessJsonToDetourMessage(obj, obj.Length);
                if (msg != null)
                {
                    ReceivedMessage(msg);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        public void SendMessage(DetourMessage Message)
        {
            EnqueuedMessagesToSend.Add(Message);
        }

        public void RegisterHandler(int MessageType, System.Type MessageClass, MessageEventHandler Handler)
        {
            MessageTypeToMessageDefinition.Add(MessageType, new MessageDefinition { Type = MessageClass, EventHandler = Handler });
        }

        public void Send()
        {
            if (Client.ConnectionActive == false)
            {
                ReceivedError?.Invoke(new System.Net.Sockets.SocketException((int)System.Net.Sockets.SocketError.NotConnected));
                return;
            }

            if (EnqueuedMessagesToSend.Count > 0)
            {
                SendEnqueuedMessages(EnqueuedMessagesToSend);
                EnqueuedMessagesToSend = new List<DetourMessage>();
            }
        }

        private void ReceivedMessage(DetourMessage msg)
        {
            if (MessageTypeToMessageDefinition.ContainsKey(msg.MessageType))
            {
                MessageTypeToMessageDefinition[msg.MessageType].EventHandler(msg);
            }
        }

        private void SendEnqueuedMessages(List<DetourMessage> enqueuedMessagesToSend)
        {
            foreach (var item in enqueuedMessagesToSend)
            {
                if (ConnectionActive)
                {
                    try
                    {
                        JSONBuffer = JsonConvert.SerializeObject(item, JSONSettings);
                        MessageBuffer = Encoding.UTF8.GetBytes(JSONBuffer);
                        Client.Send(MessageBuffer);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError(ex);
                        Client.Disconnect();
                        ReceivedError?.Invoke(ex);
                    }
                }
            }
        }

        private DetourMessage ProcessJsonToDetourMessage(byte[] JsonBuffer, int ByteLength)
        {
            JSONBuffer = Encoding.UTF8.GetString(JsonBuffer, 0, ByteLength);
            Debug.Log(JSONBuffer);
            JObject jObject = JObject.Parse(JSONBuffer);
            if (jObject.HasValues && jObject.Property("MessageType")!=null)
            {
                return (DetourMessage)jObject.ToObject(TypeExtensions.FindType(MessageTypeToMessageDefinition[jObject.Value<int>("MessageType")].Type.FullName));
            }
            else
            {
                return null;
            }
        }

        public void Disconnect()
        {
            Client.Disconnect();
        }
    }
}