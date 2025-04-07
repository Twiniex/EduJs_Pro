using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WinFormsApp3
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            PlcComm plcComm = new PlcComm();
            Modbus modbus = new Modbus();
            Application.Run(new Form1(plcComm, modbus));
        }
    }
}