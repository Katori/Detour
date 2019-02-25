using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DetourServer.Asp
{
    public class AspClientContainer : ClientContainer
    {
        public HttpContext Context;

        public AspClientContainer(HttpContext _Context, WebSocket _Socket) : base(_Context.Connection.Id, _Socket)
        {
            Id = _Context.Connection.Id;
            Context = _Context;
            Socket = _Socket;
        }

    }
}