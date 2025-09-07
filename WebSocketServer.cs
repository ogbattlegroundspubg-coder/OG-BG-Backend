using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace PUBGLiteBackendWV
{
    public static class WSServer
    {
        public class WSHandler_Message : WebSocketBehavior
        {
            protected override void OnMessage(MessageEventArgs e)
            {
                PUBGLiteBackendWV.Log.WriteLine("WSS", "received on /api/WebSocketService/message: " + e.Data);
                string response = "";
                switch (e.Data)
                {
                    case "MSG_PING":
                        response = "MSG_PONG";
                        break;
                }
                Send(response);
            }
        }
        public class WSHandler_UserProxy : WebSocketBehavior
        {
            protected override void OnMessage(MessageEventArgs e)
            {
                PUBGLiteBackendWV.Log.WriteLine("WSS", "received on /userproxy: " + e.Data);
                string response = "";
                switch (e.Data)
                {
                    case "MSG_PING":
                        response = "MSG_PONG";
                        break;
                }
                Send(response);
            }
        }

        public static WebSocketServer ws;
        public static bool _exit = false;
        public static readonly object _sync = new object();
        public static int clientCounter = 0;

        public static void Start()
        {
            Log.WriteLine("WSS", "Starting server...");
            ws = new WebSocketServer("ws://localhost:2000");
            ws.AddWebSocketService<WSHandler_Message>("/api/WebSocketService/message");
            ws.AddWebSocketService<WSHandler_UserProxy>("/userproxy");
            ws.Start();
            Log.WriteLine("WSS", "Server is listening...");
        }


        public static void Stop()
        {
            if (ws != null)
                ws.Stop();
            lock (_sync)
            {
                _exit = true;
            }
        }
    }
}
