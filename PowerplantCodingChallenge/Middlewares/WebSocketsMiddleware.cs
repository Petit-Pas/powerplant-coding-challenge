using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PowerplantCodingChallenge.API.Services.Notifiers;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.API.Middlewares
{
    public class WebSocketsMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IProductionPlanCalculatedNotifier notifier;
        private readonly ILogger<WebSocketsMiddleware> logger;
        private readonly IHostApplicationLifetime applicationLifetime;

        public WebSocketsMiddleware(RequestDelegate next, 
                                    IProductionPlanCalculatedNotifier notifier, 
                                    ILogger<WebSocketsMiddleware> logger,
                                    IHostApplicationLifetime applicationLifetime)
        {
            this.next = next;
            this.notifier = notifier;
            this.logger = logger;
            this.applicationLifetime = applicationLifetime;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                    {
                        await handleSocket(webSocket);
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }
            else
            {
                await next(context);
            }
        }

        private async Task handleSocket(WebSocket webSocket)
        {
            ProductionPlanCalculatedEventHandler eventHandler = new ProductionPlanCalculatedEventHandler(async (e) => {
                await sendPlanToSocket(e, webSocket);
            });
            notifier.ProductionPlanCalculated += eventHandler;
            logger.LogInformation("A client has connected to the WebSocket");

            await waitUntilClosed(webSocket);

            notifier.ProductionPlanCalculated -= eventHandler;
            webSocket.Dispose();
            logger.LogInformation("A client has disconnected from the WebSocket");
        }

        private async Task sendPlanToSocket(ProductionPlanCalculatedEventArgs e, WebSocket webSocket)
        {
            string jsonString = JsonConvert.SerializeObject(e);
            var buffer = Encoding.ASCII.GetBytes(jsonString);

            if (webSocket.State != WebSocketState.Closed)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        private async Task waitUntilClosed(WebSocket webSocket)
        {
            try
            {
                while (webSocket.State != WebSocketState.Closed && !applicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    await Task.Delay(2000, applicationLifetime.ApplicationStopping);
                }
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}
