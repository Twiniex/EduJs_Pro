using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using XIMUTILLib;

namespace WinFormsApp3
{
    public class PlcComm
    {
        DeviceInterface deviceInterface = new DeviceInterface();
        bool bIsConnected = false;

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        public PlcComm()
        {
            timer.Interval = 50;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Enabled = true;
        }

        int nStep = 0;
        void timer_Tick(object sender, EventArgs e)
        {
            if (bIsConnected)
            {
                if (nStep == 0)
                {
                    ReadWord();
                    nStep = 1;
                }
                else
                {
                    WriteWord();
                    nStep = 0;
                }
            }
        }
        public void Connect()
        {
            deviceInterface.Connect();
            bIsConnected = true;
        }
        public void Disconnect()
        {
            bIsConnected = false;
        }

        Logger logger;
        public void ReadWord()
        {
            byte[] buf = new byte[200];     
            deviceInterface.ReadDevice("D", 65200, 200, ref buf[0]);
            for(int i = 0; i < 100; i++)
                PlcData.FromPlc[i] = (ushort)((buf[(i * 2) + 1] << 8) | buf[i * 2]);
        }
        public void WriteWord()
        {
            byte[] buf = new byte[200];

            for (int i = 0; i < 100; i++)
            {
                buf[(i * 2) + 0] = (byte)(PlcData.ToPlc[i] & 0x00FF);
                buf[(i * 2) + 1] = (byte)((PlcData.ToPlc[i] & 0xFF00) >> 8);
            }
            deviceInterface.WriteDevice("D", 65000, 200, ref buf[0]);
        }
    }
}
