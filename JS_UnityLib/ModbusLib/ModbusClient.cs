using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusLib
{
    public class ModbusClient
    {
        public bool bIsConnected;
        private TcpClient tcpClient;
        private NetworkStream stream;
        private ushort nTransaction = 0;
        private CancellationTokenSource cancellationTokenSource;
        private Task communicationTask;

        public void Connect()
        {
            if (bIsConnected) return;

            tcpClient = new TcpClient("127.0.0.1", 505);
            stream = tcpClient.GetStream();
            bIsConnected = true;

            StartCommunicationLoop(); // ✅ 연결 시 자동으로 통신 시작
        }

        public void Disconnect()
        {
            bIsConnected = false;

            cancellationTokenSource?.Cancel(); // ✅ 통신 루프 종료
            stream?.Close();
            tcpClient?.Close();
        }

        private void StartCommunicationLoop()
        {
            cancellationTokenSource = new CancellationTokenSource();
            communicationTask = Task.Run(async () =>
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    CommAction();
                    await Task.Delay(100); // ✅ 0.1초마다 실행
                }
            }, cancellationTokenSource.Token);
        }

        public void CommAction()
        {
            if (!bIsConnected) return;

            byte[] send = new byte[256];
            byte[] recv = new byte[256];

            send[0] = (byte)(nTransaction >> 8);
            send[1] = (byte)(nTransaction & 0xFF);
            send[2] = 0;
            send[3] = 0;
            send[4] = 0;
            send[5] = 211; // Data length
            send[6] = 1;   // Slave ID
            send[7] = 23;  // Function Code 23 (Read/Write Multiple Registers)
            send[8] = 0;
            send[9] = 0;
            send[10] = 0;
            send[11] = 100;
            send[12] = 0;
            send[13] = 100;
            send[14] = 0;
            send[15] = 100;
            send[16] = 200;

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    if (PlcData.bToPlc[(i * 16) + j] == true)
                        PlcData.toPlc[i + 90] |= (ushort)(0x0001 << j);
                    else
                        PlcData.toPlc[i + 90] &= (ushort)(~(0x0001 << j));
                }
            }

            for (int i = 0; i < 100; i++)
            {
                send[17 + (i * 2)] = (byte)(PlcData.toPlc[i] >> 8);
                send[18 + (i * 2)] = (byte)(PlcData.toPlc[i] & 0xff);
            }

            stream.Write(send, 0, send.Length);
            stream.Read(recv, 0, recv.Length);

            for (int i = 0; i < 100; i++)
            {
                PlcData.fromPlc[i] = (ushort)((recv[9 + (i * 2)] << 8) | recv[10 + (i * 2)]);
            }

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    PlcData.bFromPlc[(i * 16) + j] = (PlcData.fromPlc[i + 90] & (0x0001 << j)) != 0;
                }
            }

            //nTransaction++;
        }
    }
}
