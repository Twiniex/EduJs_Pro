using System;
using ModbusLib;

namespace ModbusMasterConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ModbusClient modbusMaster = new ModbusClient();
            modbusMaster.Connect();

            for (int i = 0; i < 90; i++)
                PlcData.toPlc[i] = (ushort)i;

            modbusMaster.CommAction();
            for (int i = 0; i < 100; i++)
                Console.WriteLine(PlcData.fromPlc[i].ToString());
        }
    }
}
