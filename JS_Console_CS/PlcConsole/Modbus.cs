using PlcConsole;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PlcConsole
{
    public class Modbus
    {
        public bool bIsConnected;

        TcpListener tcpListener;
        List<TcpClient> connectedClients = new List<TcpClient>();

        public Modbus()
        {
            StartServerAsync();
        }

        async void StartServerAsync()
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            int port = 505;
            tcpListener = new TcpListener(ip, port);
            tcpListener.Start();

            Console.WriteLine("서버 대기 중...");
            bIsConnected = false;

            while (true)
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();
                connectedClients.Add(client);

                bIsConnected = true;
                Console.WriteLine($"클라이언트 {connectedClients.Count}개 연결됨");

                _ = Task.Run(() => HandleClientAsync(client));
            }
        }

        async Task HandleClientAsync(TcpClient client)
        {
            byte[] recv = new byte[256];
            byte[] send = new byte[256];

            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                {
                    while (true)
                    {
                        int bytesRead = await stream.ReadAsync(recv, 0, recv.Length);
                        if (bytesRead == 0) break;

                        Array.Copy(recv, send, 6); // header
                        send[6] = recv[6];
                        send[7] = recv[7];
                        switch (recv[7])
                        {
                            case 4:
                                send[9] = 200;

                                if (((recv[8] << 8) | (recv[9] & 0xFF)) == 0)
                                {
                                    for (int i = 0; i < 100; i++)
                                    {
                                        send[9 + (i * 2)] = (byte)(PlcData.ToPlc[i] / 256);
                                        send[10 + (i * 2)] = (byte)(PlcData.ToPlc[i] % 256);
                                    }
                                }
                                else if (((recv[8] << 8) | (recv[9] & 0xFF)) == 100)
                                {
                                    for (int i = 0; i < 100; i++)
                                    {
                                        send[9 + (i * 2)] = (byte)(PlcData.FromPlc[i] / 256);
                                        send[10 + (i * 2)] = (byte)(PlcData.FromPlc[i] % 256);
                                    }
                                }

                                await stream.WriteAsync(send, 0, 209);
                                break;

                            case 23:
                                send[9] = 200;
                                for (int i = 0; i < 100; i++)
                                {
                                    send[9 + (i * 2)] = (byte)(PlcData.FromPlc[i] / 256);
                                    send[10 + (i * 2)] = (byte)(PlcData.FromPlc[i] % 256);
                                }

                                await stream.WriteAsync(send, 0, 209);

                                for (int i = 0; i < 100; i++)
                                {
                                    PlcData.ToPlc[i] = (ushort)((recv[17 + (i * 2)] << 8) | recv[18 + (i * 2)]);
                                }

                                for (int i = 0; i < 10; i++)
                                {
                                    ushort word = PlcData.ToPlc[90 + i];
                                    for (int j = 0; j < 16; j++)
                                    {
                                        PlcData.BToPlc[i * 16 + j] = (word & (1 << j)) != 0;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            catch
            {
                // 예외 무시
            }

            connectedClients.Remove(client);
            Console.WriteLine($"클라이언트 연결 해제됨. 현재: {connectedClients.Count}개");

            if (connectedClients.Count == 0)
                bIsConnected = false;
        }
    }
}
