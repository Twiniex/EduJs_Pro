using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Fleck;

namespace PlcConsole
{
    class WebSock
    {
        ushort[] fromPlc = new ushort[100];
        ushort[] toPlc = new ushort[100];
        bool[] bFromPlc = new bool[160];
        bool[] bToPlc = new bool[160];
        static List<IWebSocketConnection> allSockets = new List<IWebSocketConnection>();
        static System.Timers.Timer timer = new System.Timers.Timer(50);
        public WebSock()
        {
            FleckLog.Level = LogLevel.Warn;
            var server = new WebSocketServer("ws://127.0.0.1:8080");

            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("웹소켓 클라이언트 연결됨");
                    allSockets.Add(socket);
                };

                socket.OnClose = () =>
                {
                    Console.WriteLine("웹소켓 클라이언트 연결 종료");
                    allSockets.Remove(socket);
                };

                socket.OnMessage = message =>
                {
                    Console.WriteLine("클라이언트로부터 메시지: " + message);
                };
            });

            timer.Elapsed += BroadcastPlcData;
            timer.AutoReset = true;
            timer.Start();
        }

        static void BroadcastPlcData(object? sender, ElapsedEventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                ushort word = PlcData.FromPlc[90 + i];
                for (int j = 0; j < 16; j++)
                    PlcData.BFromPlc[i * 16 + j] = (word & (1 << j)) != 0;
            }

            for (int i = 0; i < 10; i++)
            {
                ushort word = PlcData.ToPlc[90 + i];
                for (int j = 0; j < 16; j++)
                    PlcData.BToPlc[i * 16 + j] = (word & (1 << j)) != 0;
            }

            var json = System.Text.Json.JsonSerializer.Serialize(new
            {
                fromPlc = PlcData.FromPlc,
                toPlc = PlcData.ToPlc,
                bFromPlc = PlcData.BFromPlc,
                bToPlc = PlcData.BToPlc
            });

            foreach (var socket in allSockets.ToArray())
            {
                if (socket.IsAvailable)
                    socket.Send(json);
            }
        }
    }
}
