using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PowerPlantCodingChallenge.API.Services.Notifiers;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerPlantCodingChallenge.API.Middlewares
{
    public class WebSocketsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IProductionPlanCalculatedNotifier _notifier;
        private readonly ILogger<WebSocketsMiddleware> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public WebSocketsMiddleware(RequestDelegate next, 
                                    IProductionPlanCalculatedNotifier notifier, 
                                    ILogger<WebSocketsMiddleware> logger,
                                    IHostApplicationLifetime applicationLifetime)
        {
            this._next = next;
            this._notifier = notifier;
            this._logger = logger;
            this._logger = logger;
            this._applicationLifetime = applicationLifetime;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                    {
                        await HandleSocket(webSocket);
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }
            else
            {
                await _next(context);
            }
        }

        private async Task HandleSocket(WebSocket webSocket)
        {
            ProductionPlanCalculatedEventHandler eventHandler = new ProductionPlanCalculatedEventHandler(async (e) => {
                await SendPlanToSocket(e, webSocket);
            });
            _notifier.ProductionPlanCalculated += eventHandler;
            _logger.LogInformation("A client has connected to the WebSocket");

            await WaitUntilClosed(webSocket);

            _notifier.ProductionPlanCalculated -= eventHandler;
            webSocket.Dispose();
            _logger.LogInformation("A client has disconnected from the WebSocket");
        }

        private async Task SendPlanToSocket(ProductionPlanCalculatedEventArgs e, WebSocket webSocket)
        {
            string jsonString = JsonConvert.SerializeObject(e);
            var buffer = Encoding.ASCII.GetBytes(jsonString);

            if (webSocket.State != WebSocketState.Closed)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        private async Task WaitUntilClosed(WebSocket webSocket)
        {
            try
            {
                while (webSocket.State != WebSocketState.Closed && !_applicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    await Task.Delay(2000, _applicationLifetime.ApplicationStopping);
                }
            }
            catch (TaskCanceledException)
            {
                // the application is closing
            }
        }
    }
}
