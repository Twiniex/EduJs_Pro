using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WinFormsApp3
{
    public partial class Form1 : Form
    {
        PlcComm plcComm;
        Modbus modbus;                                          
        private DataTable dtToPlc;
        private DataTable dtFromPlc;

        Logger logger;
        private void InitializeDataTables()
        {
            dtToPlc = new DataTable();
            dtToPlc.Columns.Add("Address", typeof(string));
            dtToPlc.Columns.Add("Value", typeof(ushort));

            for (int i = 0; i < 100; i++)
                dtToPlc.Rows.Add($"D{(i + 32500):D5}", PlcData.ToPlc[i]);

            dtFromPlc = new DataTable();
            dtFromPlc.Columns.Add("Address", typeof(string));
            dtFromPlc.Columns.Add("Value", typeof(ushort));

            for (int i = 0; i < 100; i++)
                dtFromPlc.Rows.Add($"D{(i+32600):D5}", PlcData.FromPlc[i]);

            dataGridView1.DataSource = dtToPlc;
            dataGridView2.DataSource = dtFromPlc;
        }

        public Form1(PlcComm _plcComm, Modbus _modbus)
        {// *******************************************
            InitializeComponent();
            plcComm = _plcComm;
            modbus = _modbus;

            connectToolStripMenuItem.Enabled = true;
            disconnectToolStripMenuItem.Enabled = false;

            toolStripStatusLabel1.Text = "연결안됨";

            InitializeDataTables();

            modbus.SetText = SetLabel;

            logger = Logger.GetInstance();
        }

        void SetLabel(string txt)
        {
            if (InvokeRequired)
            {
                // UI 스레드로 위임
                BeginInvoke(new Action(() => SetLabel(txt)));
            }
            else
            {
                toolStripStatusLabel1.Text = txt;
            }
        }

        void PlcConnect()
        {
            plcComm.Connect();
            connectToolStripMenuItem.Enabled = false;
            disconnectToolStripMenuItem.Enabled = true;
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e) => PlcConnect();

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            plcComm.Disconnect();
            connectToolStripMenuItem.Enabled = true;
            disconnectToolStripMenuItem.Enabled = false;
        }

        int nStep;
        private void timer1_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < 100; i++) dtFromPlc.Rows[i]["Value"] = PlcData.FromPlc[i];
            for (int i = 0; i < 100; i++) dtToPlc.Rows[i]["Value"] = PlcData.ToPlc[i];
        }
    }
}
