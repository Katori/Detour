#if UNITY_WEBGL && !UNITY_EDITOR

using AOT;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace DetourClient
{
    class DetourClient
    {
        private static int idGenerator = 0;
        private static readonly Dictionary<int, DetourClient> clients = new Dictionary<int, DetourClient>();

        public bool NoDelay = true;

        public event System.Action Connected;
        public event System.Action<byte[]> ReceivedData;
        public event System.Action Disconnected;
        public event System.Action<System.Exception> ReceivedError;

        public bool Connecting { get; set; }
        public bool ConnectionActive
        {
            get
            {
                return SocketState(m_NativeRef) != 0;
            }
        }

        int m_NativeRef = 0;
        readonly int id;

        public DetourClient()
        {
            id = Interlocked.Increment(ref idGenerator);
        }

        public void Connect(System.Uri uri)
        {
            clients[id] = this;

            Connecting = true;

            m_NativeRef = SocketCreate(uri.ToString(), id, OnOpen, OnData, OnClose);
        }

        public void Disconnect()
        {
            SocketClose(m_NativeRef);
        }

        // send the data or throw exception
        public void Send(byte[] data)
        {
            SocketSend(m_NativeRef, data, data.Length);
        }


#region Javascript native functions
        [DllImport("__Internal")]
        private static extern int SocketCreate(
            string url,
            int id,
            System.Action<int> onpen,
            System.Action<int, System.IntPtr, int> ondata,
            System.Action<int> onclose);

        [DllImport("__Internal")]
        private static extern int SocketState(int socketInstance);

        [DllImport("__Internal")]
        private static extern void SocketSend(int socketInstance, byte[] ptr, int length);

        [DllImport("__Internal")]
        private static extern void SocketClose(int socketInstance);

#endregion

#region Javascript callbacks

        [MonoPInvokeCallback(typeof(System.Action))]
        public static void OnOpen(int id)
        {
            clients[id].Connecting = false;
            clients[id].Connected?.Invoke();
        }

        [MonoPInvokeCallback(typeof(System.Action))]
        public static void OnClose(int id)
        {
            clients[id].Connecting = false;
            clients[id].Disconnected?.Invoke();
            clients.Remove(id);
        }

        [MonoPInvokeCallback(typeof(System.Action))]
        public static void OnData(int id, System.IntPtr ptr, int length)
        {
            byte[] data = new byte[length];
            Marshal.Copy(ptr, data, 0, length);

            clients[id].ReceivedData(data);
        }
#endregion
    }
}
#endif