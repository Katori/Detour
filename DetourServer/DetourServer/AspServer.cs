#if ASP
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace DetourServer.Middleware
{
    public class AspServer
    {
        private readonly RequestDelegate _next;

        public AspServer(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    var newClient = new AspClientContainer(context, webSocket);
                    Server.AddClient(newClient);
                    await newClient.ClientProcess();
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await _next(context);
            }
        }
    }

    public static class DetourServerMiddlewareExtensions
    {
        public static IApplicationBuilder UseDetourServer(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AspServer>();
        }
    }
}
#endif