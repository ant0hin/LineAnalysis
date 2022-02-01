using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Timers;
using Npgsql;
using  System.Threading;
using System.IO;

namespace HmLineAnalysis
{
    public partial class MainForm : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr handle, UInt32 message, IntPtr w, IntPtr l);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [StructLayout(LayoutKind.Sequential)]
        public struct Rectangle
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetWindowRect(IntPtr hwnd, ref Rectangle rc);

        System.Timers.Timer timer = new System.Timers.Timer();

        public NpgsqlConn classConn = new NpgsqlConn();

        string name;

        SolidBrush redBrush = new SolidBrush(Color.Red);
        SolidBrush greenBrush = new SolidBrush(Color.Green);
        SolidBrush blueBrush = new SolidBrush(Color.Blue);
        SolidBrush blackBrush = new SolidBrush(Color.Black);

        System.Collections.Hashtable pocketCardsDefaultColor = new System.Collections.Hashtable();

        DataTable handsTable = new DataTable();
        DataTable newHandsTable = new DataTable();

        ContextMenuStrip contextMenuStrip1 = new ContextMenuStrip();

        System.Diagnostics.Process proc;

        public string tracker = "";
        Tables classTables = new Tables();

        FormHand formHand = new FormHand();
        FormOverall formOverall = new FormOverall();
        DataTable statsTable = new DataTable();

        public bool log = false;

        public bool overall = false;
        
        public MainForm()
        {
            InitializeComponent();

            formHand.Owner = this;

            classConn.mainForm = this;

            proc = System.Diagnostics.Process.GetProcessesByName("HoldemManager").FirstOrDefault();
            if (proc == null)
            {
                proc = System.Diagnostics.Process.GetProcessesByName("PokerTracker4").FirstOrDefault();
                if (proc == null)
                    MessageBox.Show("HoldemManeger2 or PokerTracker4 not found");
                else
                {
                    tracker = "pt4";
     
                }
            }
            else
                tracker = "hm2";

            if (tracker != "")
            {
                timer.Elapsed += new ElapsedEventHandler(TimerEvent);
                timer.Interval = 500;
                timer.Start();
            }

            //tableLayoutPanel1.RowStyles[0].SizeType = SizeType.Percent;
            tableLayoutPanel4.Visible = false;
            tableLayoutPanel3.ColumnStyles[0].Width = 95;
            tableLayoutPanel3.ColumnStyles[1].Width = 5;

            //comboBoxPreflopAct.Text = "all actions";

            foreach (var control in tableLayoutPanel4.Controls.OfType<Label>())
            {
                pocketCardsDefaultColor.Add(control.Name, control.BackColor);
            }

            this.TopMost = Properties.Settings.Default.TopMost;

            contextMenuStrip1.ItemClicked += new ToolStripItemClickedEventHandler(contextMenuStrip1_ItemClicked);
            contextMenuStrip1.Items.Add("Settings");
            contextMenuStrip1.Items.Add("Import");
            contextMenuStrip1.Items.Add("Overall");
            contextMenuStrip1.Items.Add("Notes");
        }

        public void TimerEvent(object source, ElapsedEventArgs e)
        {
            timer.Stop();

            bool windowIsFound = false;
            if (tracker == "hm2")
            {
                proc = System.Diagnostics.Process.GetProcessesByName("HoldemManager").FirstOrDefault();
                if (proc.MainWindowTitle.Contains("Line Analysis"))
                {
                    windowIsFound = true;

                    panel1.BackgroundImage = Properties.Resources.chart;
                    classConn.formText = proc.MainWindowTitle;
                    name = classConn.formText.Substring(0, proc.MainWindowTitle.IndexOf(" -"));
                    Rectangle r = new Rectangle();
                    GetWindowRect(proc.MainWindowHandle, ref r);
                    const UInt32 WM_CLOSE = 0x0010;
                    IntPtr windowPtr = FindWindowByCaption(IntPtr.Zero, proc.MainWindowTitle);
                    SendMessage(windowPtr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                    BeginInvoke(new Action(delegate()
                    {
                        Left = r.Left;
                        Top = r.Top;
                    }));

                }
            }
            else if (tracker == "pt4")
            {
                string newName = classTables.FindWindow();
                if (newName != "")
                {
                    windowIsFound = true;
                    name = newName;
                    panel1.BackgroundImage = Properties.Resources.chart;
                    classConn.formText = "Line Analyis - " + name;
                    //Rectangle r = new Rectangle();
                    if (!Properties.Settings.Default.DoNotCloseNotesWindow)
                    {
                        const UInt32 WM_CLOSE = 0x0010;
                        SendMessage(classTables.windowPtr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                    }
                }
            }

            if (windowIsFound)
            {
                if (classConn.conn.State == ConnectionState.Closed)
                    return;

                BeginInvoke(new Action(delegate()
                {

                    if (labelMenu.Visible)
                        labelMenu.Visible = false;

                    DataTable handsTable = new DataTable();
                    dataGridView1.DataSource = handsTable;

                    if (tableLayoutPanel4.Visible)
                        foreach (var control in tableLayoutPanel4.Controls.OfType<Label>())
                            if (control.BackColor != (System.Drawing.Color)pocketCardsDefaultColor[control.Name])
                            {
                                control.BackColor = (System.Drawing.Color)pocketCardsDefaultColor[control.Name];
                                control.Text = control.Name.Replace("label", "");
                            }

                    if (checkBoxBB.Enabled)
                        checkBoxBB.Enabled = false;
                    if (checkBoxSB.Enabled)
                        checkBoxSB.Enabled = false;
                    if (checkBoxBU.Enabled)
                        checkBoxBU.Enabled = false;
                    if (checkBoxCO.Enabled)
                        checkBoxCO.Enabled = false;
                    if (checkBoxMP.Enabled)
                        checkBoxMP.Enabled = false;
                    if (checkBoxUTG.Enabled)
                        checkBoxUTG.Enabled = false;

                    if (checkBoxBB.Checked)
                        checkBoxBB.Checked = false;
                    if (checkBoxSB.Checked)
                        checkBoxSB.Checked = false;
                    if (checkBoxBU.Checked)
                        checkBoxBU.Checked = false;
                    if (checkBoxCO.Checked)
                        checkBoxCO.Checked = false;
                    if (checkBoxMP.Checked)
                        checkBoxMP.Checked = false;
                    if (checkBoxUTG.Checked)
                        checkBoxUTG.Checked = false;

                    if (radioButtonIP.Enabled)
                        radioButtonIP.Enabled = false;
                    if (radioButtonOOP.Enabled)
                        radioButtonOOP.Enabled = false;
                    if (radioButtonIP.Checked)
                        radioButtonIP.Checked = false;
                    if (radioButtonOOP.Checked)
                        radioButtonOOP.Checked = false;

                    if (checkBoxStack.Enabled)
                        checkBoxStack.Enabled = false;
                    if (comboBoxPreflopAct.Enabled)
                        comboBoxPreflopAct.Enabled = false;
                    if (numericUpDownMinStack.Enabled)
                        numericUpDownMinStack.Enabled = false;
                    if (numericUpDownMaxStack.Enabled)
                        numericUpDownMaxStack.Enabled = false;


                    WindowState = FormWindowState.Normal;
                    Text = classConn.formText;

                }));
                string query = @"SELECT acts FROM results WHERE name = @name";
                NpgsqlCommand cmd = new NpgsqlCommand(query, classConn.conn);
                cmd.Parameters.AddWithValue("@Name", name);
                //cmd.Parameters.AddWithValue("@Name", "H0merSimps0n"); //отладка
                NpgsqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string acts = reader["acts"].ToString();

                    BeginInvoke(new Action(delegate()
                    {
                        if (acts.Contains("por_"))
                            buttonOpenRaise.Enabled = true;
                        else if (buttonOpenRaise.Enabled)
                            buttonOpenRaise.Enabled = false;
                        if (acts.Contains("pc_"))
                            buttonColdCall.Enabled = true;
                        else if (buttonColdCall.Enabled)
                            buttonColdCall.Enabled = false;
                        if (acts.Contains("pr_"))
                            buttonRaise.Enabled = true;
                        else if (buttonRaise.Enabled)
                            buttonRaise.Enabled = false;
                        if (acts.Contains("pl_"))
                            buttonLimp.Enabled = true;
                        else if (buttonLimp.Enabled)
                            buttonLimp.Enabled = false;
                        if (acts.Contains("p3_"))
                            button3bet.Enabled = true;
                        else if (button3bet.Enabled)
                            button3bet.Enabled = false;
                        if (acts.Contains("pc3_"))
                            buttonCall3bet.Enabled = true;
                        else if (buttonCall3bet.Enabled)
                            buttonCall3bet.Enabled = false;
                        if (acts.Contains("p4_"))
                            button4bet.Enabled = true;
                        else if (button4bet.Enabled)
                            button4bet.Enabled = false;
                        if (acts.Contains("pc4_"))
                            buttonCall4bet.Enabled = true;
                        else if (buttonCall4bet.Enabled)
                            buttonCall4bet.Enabled = false;

                        if (acts.Contains("ac_") || acts.Contains("ao_") || acts.Contains("ar_") || acts.Contains("arr_"))
                            buttonPush.Enabled = true;
                        else if (buttonPush.Enabled)
                            buttonPush.Enabled = false;

                        if (acts.Contains("fco_"))
                            buttonFlopCBet.Enabled = true;
                        else if (buttonFlopCBet.Enabled)
                            buttonFlopCBet.Enabled = false;
                        if (acts.Contains("fbm_"))
                            buttonFlopBetVsMissCbet.Enabled = true;
                        else if (buttonFlopBetVsMissCbet.Enabled)
                            buttonFlopBetVsMissCbet.Enabled = false;
                        if (acts.Contains("fm_"))
                            buttonFlopMissCbet.Enabled = true;
                        else if (buttonFlopMissCbet.Enabled)
                            buttonFlopMissCbet.Enabled = false;
                        if (acts.Contains("fcd_"))
                            buttonFlopCallDonk.Enabled = true;
                        else if (buttonFlopCallDonk.Enabled)
                            buttonFlopCallDonk.Enabled = false;
                        if (acts.Contains("frd_"))
                            buttonFlopRaiseDonk.Enabled = true;
                        else if (buttonFlopRaiseDonk.Enabled)
                            buttonFlopRaiseDonk.Enabled = false;
                        if (acts.Contains("fd_"))
                            buttonFlopDonk.Enabled = true;
                        else if (buttonFlopDonk.Enabled)
                            buttonFlopDonk.Enabled = false;
                        if (acts.Contains("fcco_"))
                            buttonFlopCallConBet.Enabled = true;
                        else if (buttonFlopCallConBet.Enabled)
                            buttonFlopCallConBet.Enabled = false;
                        if (acts.Contains("frco_"))
                            buttonFlopRaiseCbet.Enabled = true;
                        else if (buttonFlopRaiseCbet.Enabled)
                            buttonFlopRaiseCbet.Enabled = false;
                        if (acts.Contains("fb_"))
                            buttonFlopBet.Enabled = true;
                        else if (buttonFlopBet.Enabled)
                            buttonFlopBet.Enabled = false;
                        if (acts.Contains("fc_"))
                            buttonFlopCall.Enabled = true;
                        else if (buttonFlopCall.Enabled)
                            buttonFlopCall.Enabled = false;
                        if (acts.Contains("fx_"))
                            buttonFlopCheck.Enabled = true;
                        else if (buttonFlopCheck.Enabled)
                            buttonFlopCheck.Enabled = false;
                        if (acts.Contains("fr_"))
                            buttonFlopRaise.Enabled = true;
                        else if (buttonFlopRaise.Enabled)
                            buttonFlopRaise.Enabled = false;
                        if (acts.Contains("fcr_"))
                            buttonFlopCallRaise.Enabled = true;
                        else if (buttonFlopCallRaise.Enabled)
                            buttonFlopCallRaise.Enabled = false;
                        if (acts.Contains("f3_"))
                            buttonFlop3bet.Enabled = true;
                        else if (buttonFlop3bet.Enabled)
                            buttonFlop3bet.Enabled = false;

                        if (acts.Contains("tco_"))
                            buttonTurnCBet.Enabled = true;
                        else if (buttonTurnCBet.Enabled)
                            buttonTurnCBet.Enabled = false;
                        if (acts.Contains("tbm_"))
                            buttonTurnBetVsMissCbet.Enabled = true;
                        else if (buttonTurnBetVsMissCbet.Enabled)
                            buttonTurnBetVsMissCbet.Enabled = false;
                        if (acts.Contains("tm_"))
                            buttonTurnMissCbet.Enabled = true;
                        else if (buttonTurnMissCbet.Enabled)
                            buttonTurnMissCbet.Enabled = false;
                        if (acts.Contains("tcco_"))
                            buttonTurnCallCbet.Enabled = true;
                        else if (buttonTurnCallCbet.Enabled)
                            buttonTurnCallCbet.Enabled = false;
                        if (acts.Contains("tbb_"))
                            buttonTurnBB.Enabled = true;
                        else if (buttonTurnBB.Enabled)
                            buttonTurnBB.Enabled = false;
                        if (acts.Contains("tcc_"))
                            buttonTurnCC.Enabled = true;
                        else if (buttonTurnCC.Enabled)
                            buttonTurnCC.Enabled = false;
                        if (acts.Contains("tx_"))
                            buttonTurnCheck.Enabled = true;
                        else if (buttonTurnCheck.Enabled)
                            buttonTurnCheck.Enabled = false;
                        if (acts.Contains("tcx_"))
                            buttonTurnCX.Enabled = true;
                        else if (buttonTurnCX.Enabled)
                            buttonTurnCX.Enabled = false;
                        if (acts.Contains("tcb_"))
                            buttonTurnCB.Enabled = true;
                        else if (buttonTurnCB.Enabled)
                            buttonTurnCB.Enabled = false;
                        if (acts.Contains("txc_"))
                            buttonTurnXC.Enabled = true;
                        else if (buttonTurnXC.Enabled)
                            buttonTurnXC.Enabled = false;
                        if (acts.Contains("txb_"))
                            buttonTurnXB.Enabled = true;
                        else if (buttonTurnXB.Enabled)
                            buttonTurnXB.Enabled = false;
                        if (acts.Contains("txx_"))
                            buttonTurnXX.Enabled = true;
                        else if (buttonTurnXX.Enabled)
                            buttonTurnXX.Enabled = false;
                        if (acts.Contains("tr_"))
                            buttonTurnRaise.Enabled = true;
                        else if (buttonTurnRaise.Enabled)
                            buttonTurnRaise.Enabled = false;
                        if (acts.Contains("tcr_"))
                            buttonTurnCallRaise.Enabled = true;
                        else if (buttonTurnCallRaise.Enabled)
                            buttonTurnCallRaise.Enabled = false;

                        if (acts.Contains("rco_"))
                            buttonRiverCBet.Enabled = true;
                        else if (buttonRiverCBet.Enabled)
                            buttonRiverCBet.Enabled = false;
                        if (acts.Contains("rbm_"))
                            buttonRiverBetVsMissCbet.Enabled = true;
                        else if (buttonRiverBetVsMissCbet.Enabled)
                            buttonRiverBetVsMissCbet.Enabled = false;
                        if (acts.Contains("rm_"))
                            buttonRiverMissCbet.Enabled = true;
                        else if (buttonRiverMissCbet.Enabled)
                            buttonRiverMissCbet.Enabled = false;
                        if (acts.Contains("rcco_"))
                            buttonRiverCallCbet.Enabled = true;
                        else if (buttonRiverCallCbet.Enabled)
                            buttonRiverCallCbet.Enabled = false;
                        if (acts.Contains("rb_"))
                            buttonRiverBet.Enabled = true;
                        else if (buttonRiverBet.Enabled)
                            buttonRiverBet.Enabled = false;
                        if (acts.Contains("rc_"))
                            buttonRiverCall.Enabled = true;
                        else if (buttonRiverCall.Enabled)
                            buttonRiverCall.Enabled = false;
                        if (acts.Contains("rbb_"))
                            buttonRiverBB.Enabled = true;
                        else if (buttonRiverBB.Enabled)
                            buttonRiverBB.Enabled = false;
                        if (acts.Contains("rcc_"))
                            buttonRiverCC.Enabled = true;
                        else if (buttonRiverCC.Enabled)
                            buttonRiverCC.Enabled = false;
                        if (acts.Contains("rx_"))
                            buttonRiverCheck.Enabled = true;
                        else if (buttonRiverCheck.Enabled)
                            buttonRiverCheck.Enabled = false;
                        if (acts.Contains("rcx_"))
                            buttonRiverCX.Enabled = true;
                        else if (buttonRiverCX.Enabled)
                            buttonRiverCX.Enabled = false;
                        if (acts.Contains("rcb_"))
                            buttonRiverCB.Enabled = true;
                        else if (buttonRiverCB.Enabled)
                            buttonRiverCB.Enabled = false;
                        if (acts.Contains("rxc_"))
                            buttonRiverXC.Enabled = true;
                        else if (buttonRiverXC.Enabled)
                            buttonRiverXC.Enabled = false;
                        if (acts.Contains("rxb_"))
                            buttonRiverXB.Enabled = true;
                        else if (buttonRiverXB.Enabled)
                            buttonRiverXB.Enabled = false;
                        if (acts.Contains("rxx_"))
                            buttonRiverXX.Enabled = true;
                        else if (buttonRiverXX.Enabled)
                            buttonRiverXX.Enabled = false;
                        if (acts.Contains("_rr_"))
                            buttonRiverRaise.Enabled = true;
                        else if (buttonRiverRaise.Enabled)
                            buttonRiverRaise.Enabled = false;
                        if (acts.Contains("rcr_"))
                            buttonRiverCalRaise.Enabled = true;
                        else if (buttonRiverCalRaise.Enabled)
                            buttonRiverCalRaise.Enabled = false;
                    }));
                }
                else
                {
                    BeginInvoke(new Action(delegate()
                    {
                        if (buttonOpenRaise.Enabled)
                            buttonOpenRaise.Enabled = false;
                        if (buttonColdCall.Enabled)
                            buttonColdCall.Enabled = false;
                        if (buttonRaise.Enabled)
                            buttonRaise.Enabled = false;
                        if (buttonLimp.Enabled)
                            buttonLimp.Enabled = false;
                        if (button3bet.Enabled)
                            button3bet.Enabled = false;
                        if (buttonCall3bet.Enabled)
                            buttonCall3bet.Enabled = false;
                        if (button4bet.Enabled)
                            button4bet.Enabled = false;
                        if (buttonCall4bet.Enabled)
                            buttonCall4bet.Enabled = false;

                        if (buttonPush.Enabled)
                            buttonPush.Enabled = false;

                        if (buttonFlopCBet.Enabled)
                            buttonFlopCBet.Enabled = false;
                        if (buttonFlopMissCbet.Enabled)
                            buttonFlopMissCbet.Enabled = false;
                        if (buttonFlopBetVsMissCbet.Enabled)
                            buttonFlopBetVsMissCbet.Enabled = false;
                        if (buttonFlopCallDonk.Enabled)
                            buttonFlopCallDonk.Enabled = false;
                        if (buttonFlopRaiseDonk.Enabled)
                            buttonFlopRaiseDonk.Enabled = false;
                        if (buttonFlopDonk.Enabled)
                            buttonFlopDonk.Enabled = false;
                        if (buttonFlopCallConBet.Enabled)
                            buttonFlopCallConBet.Enabled = false;
                        if (buttonFlopRaiseCbet.Enabled)
                            buttonFlopRaiseCbet.Enabled = false;
                        if (buttonFlopBet.Enabled)
                            buttonFlopBet.Enabled = false;
                        if (buttonFlopCall.Enabled)
                            buttonFlopCall.Enabled = false;
                        if (buttonFlopCheck.Enabled)
                            buttonFlopCheck.Enabled = false;
                        if (buttonFlopRaise.Enabled)
                            buttonFlopRaise.Enabled = false;
                        if (buttonFlopCallRaise.Enabled)
                            buttonFlopCallRaise.Enabled = false;
                        if (buttonFlop3bet.Enabled)
                            buttonFlop3bet.Enabled = false;

                        if (buttonTurnCBet.Enabled)
                            buttonTurnCBet.Enabled = false;
                        if (buttonTurnMissCbet.Enabled)
                            buttonTurnMissCbet.Enabled = false;
                        if (buttonTurnBetVsMissCbet.Enabled)
                            buttonTurnBetVsMissCbet.Enabled = false;
                        if (buttonTurnCallCbet.Enabled)
                            buttonTurnCallCbet.Enabled = false;
                        if (buttonTurnBB.Enabled)
                            buttonTurnBB.Enabled = false;
                        if (buttonTurnCC.Enabled)
                            buttonTurnCC.Enabled = false;
                        if (buttonTurnCheck.Enabled)
                            buttonTurnCheck.Enabled = false;
                        if (buttonTurnCX.Enabled)
                            buttonTurnCX.Enabled = false;
                        if (buttonTurnCB.Enabled)
                            buttonTurnCB.Enabled = false;
                        if (buttonTurnXC.Enabled)
                            buttonTurnXC.Enabled = false;
                        if (buttonTurnXB.Enabled)
                            buttonTurnXB.Enabled = false;
                        if (buttonTurnXX.Enabled)
                            buttonTurnXX.Enabled = false;
                        if (buttonTurnRaise.Enabled)
                            buttonTurnRaise.Enabled = false;
                        if (buttonTurnCallRaise.Enabled)
                            buttonTurnCallRaise.Enabled = false;

                        if (buttonRiverCBet.Enabled)
                            buttonRiverCBet.Enabled = false;
                        if (buttonRiverMissCbet.Enabled)
                            buttonRiverMissCbet.Enabled = false;
                        if (buttonRiverBetVsMissCbet.Enabled)
                            buttonRiverBetVsMissCbet.Enabled = false;
                        if (buttonRiverCallCbet.Enabled)
                            buttonRiverCallCbet.Enabled = false;
                        if (buttonRiverBet.Enabled)
                            buttonRiverBet.Enabled = false;
                        if (buttonRiverCall.Enabled)
                            buttonRiverCall.Enabled = false;
                        if (buttonRiverBB.Enabled)
                            buttonRiverBB.Enabled = false;
                        if (buttonRiverCC.Enabled)
                            buttonRiverCC.Enabled = false;
                        if (buttonRiverCheck.Enabled)
                            buttonRiverCheck.Enabled = false;
                        if (buttonRiverCX.Enabled)
                            buttonRiverCX.Enabled = false;
                        if (buttonRiverCB.Enabled)
                            buttonRiverCB.Enabled = false;
                        if (buttonRiverXC.Enabled)
                            buttonRiverXC.Enabled = false;
                        if (buttonRiverXB.Enabled)
                            buttonRiverXB.Enabled = false;
                        if (buttonRiverXX.Enabled)
                            buttonRiverXX.Enabled = false;
                        if (buttonRiverRaise.Enabled)
                            buttonRiverRaise.Enabled = false;
                        if (buttonRiverCalRaise.Enabled)
                            buttonRiverCalRaise.Enabled = false;
                    }));
                }

                reader.Close();

                if (!Properties.Settings.Default.StopTimerWhenShowing && !Properties.Settings.Default.DoNotCloseNotesWindow)
                {
                    if (tracker == "hm2")
                    {
                        if (!proc.MainWindowTitle.Contains("Line Analysis"))
                            timer.Start();
                    }
                }

            }
            else
                timer.Start();

            
          
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Text.Contains(" - "))
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
                Text = "Line Analysis";
                classConn.formText = Text;

                //timer.Start();
            }
            else
            {
                //System.Environment.Exit(1);
                if (classConn.StaticCaller != null)  
                    if (classConn.StaticCaller.IsAlive)
                        classConn.StaticCaller.Abort();
                //Thread.Sleep(500);
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            //classConn.startDateTime = Properties.Settings.Default.LastDateLoad;
            classConn.endDateTime = DateTime.Now;
            classConn.OpenDataBase();
            WindowState = FormWindowState.Minimized;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized && !timer.Enabled)
            {
                timer.Start();
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Columns["ID"] != null && classConn.connHM2 != null)
            {
                string id = (string)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;
                id = id.Substring(0, id.IndexOf(" "));

                string query = "";
                if (tracker == "hm2")
                {
                    query = @"
                    SELECT 
                    handhistories.handhistory 
                    FROM 
                    handhistories
                    WHERE handhistories.gamenumber = '" + id + "'";
                }
                else
                {
                    query = @"            (SELECT 
                      cash_hand_histories.history AS handhistory
                    FROM 
                        cash_hand_histories
                    JOIN   
                        cash_hand_summary
                    ON
                        cash_hand_histories.id_hand = cash_hand_summary.id_hand   
                    WHERE 
                        cash_hand_summary.hand_no = '" + id + @"' ) 

                    UNION

                    (SELECT 
                        tourney_hand_histories.history 
                    FROM 
                        tourney_hand_histories
                    JOIN   
                        tourney_hand_summary
                    ON
                        tourney_hand_histories.id_hand = tourney_hand_summary.id_hand  
                    WHERE 
                       tourney_hand_summary.hand_no = '" + id + @"' ) ";//hand_no
                }
                NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, classConn.connHM2);
                NpgsqlDataReader reader = npgSqlCommand.ExecuteReader();
                if (reader.Read()) 
                {
                    string handhistory = reader["handhistory"].ToString();
                    reader.Close();
                    MessageBox.Show(handhistory);
                }
   
            }
        }
               
        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dataGridView1.Columns["ID"] != null && e.RowIndex >= 0)
            {
                string id = (string)dataGridView1.Rows[e.RowIndex].Cells["id"].Value;
                string query = @"SELECT PreflopPos, Hand4, PreflopAct, Pos, Board, FlopAct, FlopBet, TurnAct, TurnBet, RiverAct, RiverBet FROM hands WHERE ID = @ID";
                NpgsqlCommand cmd = new NpgsqlCommand(query, classConn.conn);
                cmd.Parameters.AddWithValue("@ID", id);
                NpgsqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    formHand.Show();
                    formHand.Left = Cursor.Position.X + 10 ;
                    formHand.Top = Cursor.Position.Y + 10;
                    formHand.ShowHandText(reader);
                    reader.Close();
                }
            }
        }

        private void dataGridView1_MouseLeave(object sender, EventArgs e)
        {
            if (formHand.Visible)
                formHand.Visible = false;
        }

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            //if (((sender as DataGridView).DataSource as DataTable).TableName != "")
            //    return;
            DataTable table = (sender as DataGridView).DataSource as DataTable;

            if ((table.Columns[e.ColumnIndex].ColumnName == "hand" || table.Columns[e.ColumnIndex].ColumnName == "board") && e.RowIndex >= 0)
            {
                e.PaintBackground(e.ClipBounds, true); // фон отрисовывается стандартный

                Font font = e.CellStyle.Font;

                string text = (string)e.FormattedValue;
                if (text == "" || text.Length < 4)
                    return;
                string card1 = text.Substring(0, 1);
                string card2 = text.Substring(3, 1);

                string suit = text.Substring(1, 1);
                SolidBrush myBrush = blackBrush;
                if (suit == "h")
                    myBrush = redBrush;
                else if (suit == "d")
                    myBrush = blueBrush;
                else if (suit == "c")
                    myBrush = greenBrush;

                e.Graphics.DrawString(card1, font, myBrush, e.CellBounds);

                SizeF size = e.Graphics.MeasureString(card1, font, e.CellBounds.Location, StringFormat.GenericTypographic); // размер первой половины текста
                //e.Graphics.FillRectangle(myBrush, new RectangleF(new Point(e.CellBounds.X, e.CellBounds.Y), size));
                var rect = new RectangleF(e.CellBounds.X + size.Width, e.CellBounds.Y, e.CellBounds.Width - size.Width, e.CellBounds.Height); // ограничивающий прямоугольник для оставшейся части текста

                suit = text.Substring(4, 1);
                if (suit == "h")
                    myBrush = redBrush;
                else if (suit == "d")
                    myBrush = blueBrush;
                else if (suit == "c")
                    myBrush = greenBrush;
                else
                    myBrush = blackBrush;

                e.Graphics.DrawString(card2, font, myBrush, rect);

                if (text.Length >= 8)
                {
                    string card3 = text.Substring(6, 1);
                    suit = text.Substring(7, 1);
                    myBrush = blackBrush;
                    if (suit == "h")
                        myBrush = redBrush;
                    else if (suit == "d")
                        myBrush = blueBrush;
                    else if (suit == "c")
                        myBrush = greenBrush;
                    size = e.Graphics.MeasureString(card1+card2, font, e.CellBounds.Location, StringFormat.GenericTypographic); 
                    rect = new RectangleF(e.CellBounds.X + size.Width, e.CellBounds.Y, e.CellBounds.Width - size.Width, e.CellBounds.Height);
                    e.Graphics.DrawString(card3, font, myBrush, rect);

                    if (text.Length >= 11)
                    {
                        string card4 = text.Substring(9, 1);
                        suit = text.Substring(10, 1);
                        myBrush = blackBrush;
                        if (suit == "h")
                            myBrush = redBrush;
                        else if (suit == "d")
                            myBrush = blueBrush;
                        else if (suit == "c")
                            myBrush = greenBrush;
                        size = e.Graphics.MeasureString(card1 + card2 + card3, font, e.CellBounds.Location, StringFormat.GenericTypographic);
                        rect = new RectangleF(e.CellBounds.X + size.Width, e.CellBounds.Y, e.CellBounds.Width - size.Width, e.CellBounds.Height);
                        e.Graphics.DrawString(card4, font, myBrush, rect);

                        if (text.Length >= 14)
                        {
                            string card5 = text.Substring(12, 1);
                            suit = text.Substring(13, 1);
                            myBrush = blackBrush;
                            if (suit == "h")
                                myBrush = redBrush;
                            else if (suit == "d")
                                myBrush = blueBrush;
                            else if (suit == "c")
                                myBrush = greenBrush;
                            size = e.Graphics.MeasureString(card1 + card2 + card3 + card4, font, e.CellBounds.Location, StringFormat.GenericTypographic);
                            rect = new RectangleF(e.CellBounds.X + size.Width, e.CellBounds.Y, e.CellBounds.Width - size.Width, e.CellBounds.Height);
                            e.Graphics.DrawString(card5, font, myBrush, rect);
                        }
                    }
                                    
                }

              

                //using (var sf = new StringFormat())
                //using (var firstBrush = new SolidBrush(e.CellStyle.ForeColor)) // кисть для первой половины текста
                //using (var lastBrush = new SolidBrush(Color.Red)) // кисть для последней половины текста
                //using (var boldFont = new Font(font, FontStyle.Bold)) // жирный шрифт, используется для последней половины текста
                //{
                //    sf.LineAlignment = StringAlignment.Center; // выравнивание по центру, как в других ячейках (стандартно)
                //    sf.FormatFlags = StringFormatFlags.NoWrap; // чтобы при сжатии колонки текст не переносился на вторую строку
                //    sf.Trimming = StringTrimming.EllipsisWord; // будет отрисовываться многоточие, если текст не влезает в колонку

                //    string text = (string)e.FormattedValue; // полное (форматированное) значение ячейки
                //    string firstHalf = text.Substring(0, 1); // первая часть текста
                //    string lastHalf = text.Substring(3, 1); // оставшаяся часть текста

                //    e.Graphics.DrawString(firstHalf, font, firstBrush, e.CellBounds, sf); // первую половину текста рисуем стандартным шрифтом и цветом

                //    SizeF size = e.Graphics.MeasureString(firstHalf, font, e.CellBounds.Location, StringFormat.GenericTypographic); // размер первой половины текста
                //    var rect = new RectangleF(e.CellBounds.X + size.Width, e.CellBounds.Y, e.CellBounds.Width - size.Width, e.CellBounds.Height); // ограничивающий прямоугольник для оставшейся части текста

                //    e.Graphics.DrawString(lastHalf, boldFont, lastBrush, rect, sf); // выводим вторую часть текста жирным шрифтом и другим цветом
                //}
                e.Handled = true; // сигнализируем, что закончили обработку
            }
            else if (table.Columns[e.ColumnIndex].ColumnName.Contains("comb") && e.RowIndex >= 0)
            {
                var datagridview = (sender as DataGridView);
                var cell = datagridview.Rows[e.RowIndex].Cells[e.ColumnIndex];
                string comb = cell.Value.ToString();
                if (comb == "TwoPair" || comb == "Three" || comb == "Straight" || comb == "Flush" || comb == "FullHouse" || comb == "Four")
                    cell.Style.BackColor = Color.DeepPink;
                else if (comb == "Overcards" || comb == "")
                    cell.Style.BackColor = Color.LightGreen;
                else if (comb.Substring(0, 4) == "Pair")
                    cell.Style.BackColor = Color.Yellow;
                else if (comb.Contains("TopPair") || comb.Contains("OverPair"))
                    cell.Style.BackColor = Color.Orange;
                else
                    cell.Style.BackColor = Color.DodgerBlue;


                //dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;
                //if (cell.Value != DBNull.Value)
                //{
                //    e.PaintBackground(e.ClipBounds, cell.Selected);

                //    //  center of the cell
                //    var x = e.CellBounds.X + e.CellBounds.Width / 2 - size / 2;
                //    var y = e.CellBounds.Y + e.CellBounds.Height / 2 - size / 2;

                //    RectangleF rectangle = new RectangleF(x, y, size, size);
                //    e.Graphics.FillRectangle(Brushes.Yellow, rectangle);

                //    e.PaintContent(e.ClipBounds);

                //    e.Handled = true;
                //}

            }
        }

        //*******************************************************************************************

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string clikedItem = e.ClickedItem.Text;
            if (clikedItem == "Settings")
            {
                FormSettings formSettings = new FormSettings();
                formSettings.mainForm = this;
                formSettings.Show();
            }
            else if (clikedItem == "Import")
            {
                FormImport formImport = new FormImport();
                formImport.mainForm = this;
                formImport.Show();
            }
            else if (clikedItem == "Overall")
            {
                Cursor = Cursors.WaitCursor;
                
                buttonOpenRaise.Enabled = true;
                buttonColdCall.Enabled = true;
                buttonRaise.Enabled = true;
                buttonLimp.Enabled = true;
                button3bet.Enabled = true;
                buttonCall3bet.Enabled = true;
                button4bet.Enabled = true;
                buttonCall4bet.Enabled = true;

                checkBoxBB.Enabled = true;
                checkBoxSB.Enabled = true;
                checkBoxBU.Enabled = true;
                checkBoxCO.Enabled = true;
                checkBoxMP.Enabled = true;
                checkBoxUTG.Enabled = true;

                radioButtonIP.Enabled = true;
                radioButtonOOP.Enabled = true;

                buttonFlopCBet.Enabled = true;
                buttonFlopBetVsMissCbet.Enabled = true;
                buttonFlopMissCbet.Enabled = true;
                buttonFlopCallDonk.Enabled = true;
                buttonFlopRaiseDonk.Enabled = true;
                buttonFlopDonk.Enabled = true;
                buttonFlopCallConBet.Enabled = true;
                buttonFlopRaiseCbet.Enabled = true;
                buttonFlopBet.Enabled = true;
                buttonFlopCall.Enabled = true;
                buttonFlopCheck.Enabled = true;
                buttonFlopRaise.Enabled = true;
                buttonFlopCallRaise.Enabled = true;
                buttonFlop3bet.Enabled = true;

                buttonTurnCBet.Enabled = true;
                buttonTurnBetVsMissCbet.Enabled = true;
                buttonTurnMissCbet.Enabled = true;
                buttonTurnCallCbet.Enabled = true;
                buttonTurnBB.Enabled = true;
                buttonTurnCC.Enabled = true;
                buttonTurnCheck.Enabled = true;
                buttonTurnCX.Enabled = true;
                buttonTurnCB.Enabled = true;
                buttonTurnXC.Enabled = true;
                buttonTurnXB.Enabled = true;
                buttonTurnXX.Enabled = true;
                buttonTurnRaise.Enabled = true;
                buttonTurnCallRaise.Enabled = true;

                buttonRiverCBet.Enabled = true;
                buttonRiverBetVsMissCbet.Enabled = true;
                buttonRiverMissCbet.Enabled = true;
                buttonRiverCallCbet.Enabled = true;
                buttonRiverBet.Enabled = true;
                buttonRiverCall.Enabled = true;
                buttonRiverBB.Enabled = true;
                buttonRiverCC.Enabled = true;
                buttonRiverCheck.Enabled = true;
                buttonRiverCX.Enabled = true;
                buttonRiverCB.Enabled = true;
                buttonRiverXC.Enabled = true;
                buttonRiverXB.Enabled = true;
                buttonRiverXX.Enabled = true;
                buttonRiverRaise.Enabled = true;
                buttonRiverCalRaise.Enabled = true;

                checkBoxStack.Enabled = true;
                comboBoxPreflopAct.Enabled = true;
                buttonPush.Enabled = true;

                statsTable = classConn.GetPlayerStats();
                if (formOverall.IsDisposed)
                    formOverall = new FormOverall();
                formOverall.mainForm = this;
                formOverall.Show();
                overall = true;

                Cursor = Cursors.Default;
            }
            else if (clikedItem == "Notes")
            {
                FormNotes formNotes = new FormNotes();
                formNotes.mainForm = this;
                formNotes.classConn = classConn;
                formNotes.Show();
            }

     
        }

        //*******************************************************************************************
        private void buttonOpenRaise_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            string line = (sender as Button).Name;
            line = line.Replace("button", "");
            string query = "";

            if (line == "OpenRaise")
                query = @"SELECT ID, Hand3 AS Hand, PreflopPos AS pos, PreflopBet FROM hands WHERE PreflopAct = 'openraise' AND Name = @name";
            else if (line == "ColdCall")
                query = @"SELECT ID, Hand3 AS Hand, PreflopPos AS pos, PreflopBet FROM hands WHERE PreflopAct = 'cold call' AND Name = @name";
            else if (line == "Raise")
                query = @"SELECT ID, Hand3 AS Hand, PreflopPos AS pos, PreflopBet FROM hands WHERE PreflopAct = 'raise' AND Name = @name";
            else if (line == "Limp")
                query = @"SELECT ID, Hand3 AS Hand, PreflopPos AS pos, PreflopBet FROM hands WHERE (PreflopAct = 'limp' OR PreflopAct = 'limp, call') AND Name = @name";
            else if (line == "3bet")
                query = @"SELECT ID, Hand3 AS Hand, PreflopPos AS pos, PreflopBet FROM hands WHERE PreflopAct = '3bet' AND Name = @name";
            else if (line == "Call3bet")
                query = @"SELECT ID, Hand3 AS Hand, PreflopPos AS pos, PreflopBet FROM hands WHERE PreflopAct = 'call 3bet' AND Name = @name";
            else if (line == "4bet")
                query = @"SELECT ID, Hand3 AS Hand, PreflopPos AS pos, PreflopBet FROM hands WHERE PreflopAct = '4bet' AND Name = @name";
            else if (line == "Call4bet")
                query = @"SELECT ID, Hand3 AS Hand, PreflopPos AS pos, PreflopBet FROM hands WHERE PreflopAct = 'call 4bet' AND Name = @name";

            else if (line == "Push")
                query = @"SELECT ID, Hand3 AS Hand, PreflopPos AS pos, PreflopAct as act, Stack FROM prefloppush WHERE Name = @name";

            else if (line == "FlopBet")
                query = @"SELECT ID, PreflopAct, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 9) AS board, FlopComb, FlopBet, FlopPlayersCount AS Players FROM hands WHERE FlopAct LIKE 'bet%' AND Name = @name";
            else if (line == "FlopCall")
                query = @"SELECT ID, PreflopAct, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 9) AS board, FlopComb, FlopBet, FlopPlayersCount AS Players FROM hands WHERE (FlopAct = 'call' OR FlopAct = 'check, call') AND Name = @name";
            else if (line == "FlopCBet")
                query = @"SELECT ID, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 9) AS board, FlopComb, FlopBet, FlopPlayersCount AS Players FROM hands WHERE PreflopAct IN ('openraise', 'raise', '3bet', '4bet') AND FlopAct LIKE 'bet%' AND Name = @name";
            else if (line == "FlopCallConBet")
                query = @"SELECT ID, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 9) AS board, FlopComb, FlopBet, FlopPlayersCount AS Players FROM hands WHERE PreflopAct LIKE '%call' AND FlopAct IN ('call', 'check, call') AND Name = @name";
            else if (line == "FlopMissCbet")
                query = @"SELECT ID, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 9) AS board, FlopComb, FlopAct, FlopBet, FlopPlayersCount AS Players FROM hands WHERE PreflopAct IN ('openraise', 'raise', '3bet', '4bet') AND FlopAct IN ('check', 'check, call', 'check, raise') AND Name = @name";
            else if (line == "FlopBetVsMissCbet")
                query = @"SELECT ID, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 9) AS board, FlopComb, FlopBet, FlopPlayersCount AS Players FROM hands WHERE PreflopAct LIKE '%call' AND FlopAct LIKE 'bet%' AND Pos = 'IP' AND Name = @name";
            else if (line == "FlopDonk")
                query = @"SELECT ID, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 9) AS board, FlopComb, FlopBet, FlopPlayersCount AS Players FROM hands WHERE PreflopAct LIKE '%call' AND Pos = 'OOP' AND FlopAct LIKE 'bet%' AND Name = @name";
            else if (line == "FlopCallDonk")
                query = @"SELECT ID, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 9) AS board, FlopComb, FlopAct, FlopBet, FlopPlayersCount AS Players FROM hands WHERE PreflopAct IN ('openraise', 'raise', '3bet', '4bet') AND FlopAct = 'call' AND Name = @name";
            else if (line == "FlopCheck")
                query = @"SELECT ID, PreflopAct, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 9) AS board, FlopComb, FlopAct, FlopBet, FlopPlayersCount AS Players FROM hands WHERE FlopAct IN ('check', 'check, call') AND Name = @name";
            else if (line == "FlopRaise")
                query = @"SELECT ID, PreflopAct, Pos, Hand4 AS Hand, Board, FlopComb, FlopAct, FlopBet, FlopPlayersCount AS Players, TurnAct, TurnBet, RiverAct, RiverBet FROM hands WHERE FlopAct IN ('raise', 'check, raise') AND Name = @name";
            else if (line == "FlopRaiseCbet")
                query = @"SELECT ID, Pos, Hand4 AS Hand, Board, FlopComb, FlopBet, FlopPlayersCount, TurnAct, TurnBet, RiverAct, RiverBet AS Players FROM hands WHERE PreflopAct LIKE '%call' AND FlopAct IN ('raise', 'check, raise') AND Name = @name";
            else if (line == "FlopRaiseDonk")
                query = @"SELECT ID, Pos, Hand4 AS Hand, Board, FlopComb, FlopBet, FlopPlayersCount, TurnAct, TurnBet, RiverAct, RiverBet AS Players FROM hands WHERE PreflopAct IN ('openraise', 'raise', '3bet', '4bet') AND FlopAct IN ('raise', 'check, raise') AND Name = @name";
            else if (line == "FlopCallRaise")
                query = @"SELECT ID, PreflopAct, Hand4 AS Hand, Board, FlopComb, FlopBet, FlopPlayersCount AS Players, TurnAct, TurnBet, RiverAct, RiverBet FROM hands WHERE FlopAct = 'call raise' AND Name = @name";
            else if (line == "Flop3bet")
                query = @"SELECT ID, PreflopAct, Hand4 AS Hand, Board, FlopComb, FlopBet, FlopPlayersCount AS Players, TurnAct, TurnBet, RiverAct, RiverBet FROM hands WHERE FlopAct = '3bet' AND Name = @name";

            else if (line == "TurnBB")
                query = @"SELECT ID, PreflopAct, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 12) AS board, TurnComb, FlopBet, TurnBet, TurnPlayersCount AS Players FROM hands WHERE FlopAct = 'bet' AND TurnAct LIKE 'bet%' AND Name = @name";
            else if (line == "TurnCC")
                query = @"SELECT ID, PreflopAct, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 12) AS board, TurnComb, FlopBet, TurnBet, TurnPlayersCount AS Players FROM hands WHERE FlopAct IN ('call', 'check, call') AND TurnAct IN ('call', 'check, call') AND Name = @name";
            else if (line == "TurnCBet")
                query = @"SELECT ID, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 12) AS board, TurnComb, FlopBet, TurnBet, TurnPlayersCount AS Players FROM hands WHERE PreflopAct IN ('openraise', 'raise', '3bet', '4bet') AND FlopAct = 'bet' AND TurnAct LIKE 'bet%' AND Name = @name";
            else if (line == "TurnCallCbet")
                query = @"SELECT ID, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 12) AS board, TurnComb, FlopBet, TurnBet, TurnPlayersCount AS Players FROM hands WHERE PreflopAct LIKE '%call' AND FlopAct IN ('call', 'check, call') AND TurnAct IN ('call', 'check, call') AND Name = @name";
            else if (line == "TurnMissCbet")
                query = @"SELECT ID, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 12) AS board, TurnComb, FlopBet, TurnPlayersCount AS Players FROM hands WHERE PreflopAct IN ('openraise', 'raise', '3bet', '4bet') AND FlopAct = 'bet' AND TurnAct IN ('check', 'check, call', 'check, raise') AND Name = @name";
            else if (line == "TurnBetVsMissCbet")
                query = @"SELECT ID, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 12) AS board, TurnComb, FlopBet, TurnPlayersCount AS Players FROM hands WHERE PreflopAct LIKE '%call' AND FlopAct = 'call' AND TurnAct LIKE 'bet%' AND Name = @name";
            else if (line == "TurnCheck")
                query = @"SELECT ID, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 12) AS board, TurnComb, FlopAct, FlopBet, TurnPlayersCount AS Players FROM hands WHERE TurnAct LIKE 'check%' AND Name = @name";
            else if (line == "TurnCX")
                query = @"SELECT ID, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 12) AS board, TurnComb, FlopBet, TurnPlayersCount AS Players FROM hands WHERE FlopAct IN ('call', 'check, call') AND TurnAct LIKE 'check%' AND Name = @name";
            else if (line == "TurnCB")
                query = @"SELECT ID, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 12) AS board, TurnComb, FlopBet, TurnPlayersCount AS Players FROM hands WHERE FlopAct IN ('call', 'check, call') AND TurnAct LIKE 'bet%' AND Name = @name";
            else if (line == "TurnXC")
                query = @"SELECT ID, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 12) AS board, TurnComb, FlopBet, TurnPlayersCount AS Players FROM hands WHERE FlopAct = 'check' AND TurnAct IN ('call', 'check, call') AND Name = @name";
            else if (line == "TurnXB")
                query = @"SELECT ID, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 12) AS board, TurnComb, TurnBet, TurnPlayersCount AS Players FROM hands WHERE FlopAct = 'check' AND TurnAct LIKE 'bet%' AND Name = @name";
            else if (line == "TurnXX")
                query = @"SELECT ID, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 12) AS board, TurnComb, TurnPlayersCount AS Players FROM hands WHERE FlopAct = 'check' AND TurnAct LIKE 'check%' AND Name = @name";
            else if (line == "TurnRaise")
                query = @"SELECT ID, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 12) AS board, FlopAct, TurnComb, TurnBet, TurnPlayersCount AS Players FROM hands WHERE TurnAct IN ('raise', 'check, raise', '3bet') AND Name = @name";
            else if (line == "TurnCallRaise")
                query = @"SELECT ID, Pos, Hand4 AS Hand, SUBSTRING(Board, 0, 12) AS board, FlopAct, TurnComb, TurnBet, TurnPlayersCount AS Players FROM hands WHERE TurnAct = 'call raise' AND Name = @name";

            else if (line == "RiverBet")
                query = @"SELECT ID, Pos, Hand4 AS Hand, Board, RiverComb, FlopAct, TurnAct, RiverBet, RiverPlayersCount AS Players FROM hands WHERE RiverAct LIKE 'bet%' AND Name = @name";
            else if (line == "RiverCall")
                query = @"SELECT ID, Pos, Hand4 AS Hand, Board, RiverComb, FlopAct, TurnAct, RiverBet, RiverPlayersCount AS Players FROM hands WHERE RiverAct IN ('call', 'check, call') AND Name = @name";
            else if (line == "RiverBB")
                query = @"SELECT ID, Pos, Hand4 AS Hand, Board, RiverComb, FlopAct, TurnBet, RiverBet, RiverPlayersCount AS Players FROM hands WHERE TurnAct = 'bet' AND RiverAct LIKE 'bet%' AND Name = @name";
            else if (line == "RiverCC")
                query = @"SELECT ID, Pos, Hand4 AS Hand, Board, RiverComb, FlopAct, TurnBet, RiverBet, RiverPlayersCount AS Players FROM hands WHERE TurnAct IN ('call', 'check, call') AND RiverAct IN ('call', 'check, call') AND Name = @name";
            else if (line == "RiverCBet")
                query = @"SELECT ID, Pos, Hand4 AS Hand, Board, RiverComb, RiverBet, RiverPlayersCount AS Players FROM hands WHERE PreflopAct IN ('openraise', 'raise', '3bet', '4bet') AND FlopAct = 'bet' AND TurnAct = 'bet' AND RiverAct LIKE 'bet%' AND Name = @name";
            else if (line == "RiverCallCbet")
                query = @"SELECT ID, Pos, Hand4 AS Hand, Board, RiverComb, RiverBet, RiverPlayersCount AS Players FROM hands WHERE PreflopAct LIKE '%call' AND FlopAct IN ('call', 'check, call') AND TurnAct IN ('call', 'check, call') AND RiverAct IN ('call', 'check, call') AND Name = @name";
            else if (line == "RiverMissCbet")
                query = @"SELECT ID, Pos, Hand4 AS Hand, Board, RiverComb, RiverPlayersCount AS Players FROM hands WHERE PreflopAct IN ('openraise', 'raise', '3bet', '4bet') AND FlopAct = 'bet' AND TurnAct = 'bet' AND RiverAct LIKE 'check%' AND Name = @name";
            else if (line == "RiverBetVsMissCbet")
                query = @"SELECT ID, Pos, Hand4 AS Hand, Board, RiverComb, RiverPlayersCount AS Players FROM hands WHERE PreflopAct LIKE '%call' AND FlopAct = 'call' AND TurnAct = 'call' AND RiverAct LIKE 'bet%' AND Name = @name";
            else if (line == "RiverCheck")
                query = @"SELECT ID, Pos, Hand4 AS Hand, Board, RiverComb, FlopAct, TurnAct, RiverPlayersCount AS Players FROM hands WHERE RiverAct LIKE 'check%' AND Name = @name";
            else if (line == "RiverCX")
                query = @"SELECT ID, Pos, Hand4 AS Hand, Board, RiverComb, RiverPlayersCount AS Players FROM hands WHERE TurnAct IN ('call', 'check, call') AND RiverAct LIKE 'check%' AND Name = @name";
            else if (line == "RiverCB")
                query = @"SELECT ID, Pos, Hand4 AS Hand, Board, RiverComb, RiverBet, RiverPlayersCount AS Players FROM hands WHERE TurnAct IN ('call', 'check, call') AND RiverAct LIKE 'bet%' AND Name = @name";
            else if (line == "RiverXC")
                query = @"SELECT ID, Pos, Hand4 AS Hand, Board, RiverComb, RiverBet, RiverPlayersCount AS Players FROM hands WHERE TurnAct = 'check' AND RiverAct IN ('call', 'check, call') AND Name = @name";
            else if (line == "RiverXB")
                query = @"SELECT ID, Pos, Hand4 AS Hand, Board, RiverComb, RiverBet, RiverPlayersCount AS Players FROM hands WHERE TurnAct = 'check' AND RiverAct LIKE 'bet%' AND Name = @name";
            else if (line == "RiverXX")
                query = @"SELECT ID, Pos, Hand4 AS Hand, Board, RiverComb, RiverPlayersCount AS Players FROM hands WHERE TurnAct = 'check' AND RiverAct LIKE 'check%' AND Name = @name";
            else if (line == "RiverRaise")
                query = @"SELECT ID, Pos, Hand4 AS Hand, Board, RiverComb, FlopAct, TurnAct, RiverBet, RiverPlayersCount AS Players FROM hands WHERE RiverAct IN ('raise', 'check, raise', '3bet') AND Name = @name";
            else if (line == "RiverCalRaise")
                query = @"SELECT ID, Pos, Hand4 AS Hand, Board, RiverComb, FlopAct, TurnAct, RiverBet, RiverPlayersCount AS Players FROM hands WHERE RiverAct = 'call raise' AND Name = @name";

            else
                return;

            if (overall)
            {
                query = query.Replace("AND Name = @name", "");
                query = query.Replace("ID, ", "ID, Name, ");
            }

            NpgsqlCommand cmd = new NpgsqlCommand(query, classConn.conn);
            cmd.Parameters.AddWithValue("@Name", name);

            handsTable.Columns.Clear();
            handsTable.Rows.Clear();
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                try
                {
                    handsTable.Load(reader);
                }
                catch { }
            }

            if (log)
                File.AppendAllText("log.txt", DateTime.Now + "\r\n " + query + "\r\n rows=" + handsTable.Rows.Count + "\r\n");

            if (handsTable.Rows.Count == 0)
            {
                Cursor = Cursors.Default;
                return;
            }

            if (overall)
            {
                string names = " ";
                DataTable newStatsTable = statsTable.Copy();
                if (formOverall.checkedStats.Contains(" hands "))
                {
                    string stat = "totalhands";
                    NumericUpDown control = formOverall.Controls["numericUpDownMinHands"] as NumericUpDown;
                    decimal minStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            long statValue = (long)newStatsTable.Rows[i][stat];
                            if (statValue < minStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }
                if (formOverall.checkedStats.Contains(" vpip "))
                {
                    string stat = "vpip";
                    NumericUpDown control = formOverall.Controls["groupBoxPreflop"].Controls["numericUpDownMinVpip"] as NumericUpDown;
                    decimal minStat = control.Value;
                    control = formOverall.Controls["groupBoxPreflop"].Controls["numericUpDownMaxVpip"] as NumericUpDown;
                    decimal maxStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            decimal statValue = (decimal)newStatsTable.Rows[i][stat];
                            if (statValue < minStat || statValue > maxStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }
                if (formOverall.checkedStats.Contains(" pfr "))
                {
                    string stat = "pfr";
                    NumericUpDown control = formOverall.Controls["groupBoxPreflop"].Controls["numericUpDownMinPfr"] as NumericUpDown;
                    decimal minStat = control.Value;
                    control = formOverall.Controls["groupBoxPreflop"].Controls["numericUpDownMaxPfr"] as NumericUpDown;
                    decimal maxStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            decimal statValue = (decimal)newStatsTable.Rows[i][stat];
                            if (statValue < minStat || statValue > maxStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }
                if (formOverall.checkedStats.Contains(" call "))
                {
                    string stat = "coldcall";
                    NumericUpDown control = formOverall.Controls["groupBoxPreflop"].Controls["numericUpDownMinCall"] as NumericUpDown;
                    decimal minStat = control.Value;
                    control = formOverall.Controls["groupBoxPreflop"].Controls["numericUpDownMaxCall"] as NumericUpDown;
                    decimal maxStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            decimal statValue = (decimal)newStatsTable.Rows[i][stat];
                            if (statValue < minStat || statValue > maxStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }
                if (formOverall.checkedStats.Contains(" 3bet "))
                {
                    string stat = "threebet";
                    NumericUpDown control = formOverall.Controls["groupBoxPreflop"].Controls["numericUpDownMin3bet"] as NumericUpDown;
                    decimal minStat = control.Value;
                    control = formOverall.Controls["groupBoxPreflop"].Controls["numericUpDownMax3bet"] as NumericUpDown;
                    decimal maxStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            decimal statValue = (decimal)newStatsTable.Rows[i][stat];
                            if (statValue < minStat || statValue > maxStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }
                if (formOverall.checkedStats.Contains(" foldTo3bet "))
                {
                    string stat = "foldto3bet";
                    NumericUpDown control = formOverall.Controls["groupBoxPreflop"].Controls["numericUpDownMinFoldTo3bet"] as NumericUpDown;
                    decimal minStat = control.Value;
                    control = formOverall.Controls["groupBoxPreflop"].Controls["numericUpDownMaxFoldTo3bet"] as NumericUpDown;
                    decimal maxStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            decimal statValue = (decimal)newStatsTable.Rows[i][stat];
                            if (statValue < minStat || statValue > maxStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }
                if (formOverall.checkedStats.Contains(" 4bet "))
                {
                    string stat = "fourbet";
                    NumericUpDown control = formOverall.Controls["groupBoxPreflop"].Controls["numericUpDownMin4bet"] as NumericUpDown;
                    decimal minStat = control.Value;
                    control = formOverall.Controls["groupBoxPreflop"].Controls["numericUpDownMax4bet"] as NumericUpDown;
                    decimal maxStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            decimal statValue = (decimal)newStatsTable.Rows[i][stat];
                            if (statValue < minStat || statValue > maxStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }
                if (formOverall.checkedStats.Contains(" foldTo4bet "))
                {
                    string stat = "foldto4bet";
                    NumericUpDown control = formOverall.Controls["groupBoxPreflop"].Controls["numericUpDownMinFoldTo4bet"] as NumericUpDown;
                    decimal minStat = control.Value;
                    control = formOverall.Controls["groupBoxPreflop"].Controls["numericUpDownMaxFoldTo4bet"] as NumericUpDown;
                    decimal maxStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            decimal statValue = (decimal)newStatsTable.Rows[i][stat];
                            if (statValue < minStat || statValue > maxStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }
                if (formOverall.checkedStats.Contains(" cBFlop "))
                {
                    string stat = "cBFlop";
                    NumericUpDown control = formOverall.Controls["groupBoxFlop"].Controls["numericUpDownMinFlopCBet"] as NumericUpDown;
                    decimal minStat = control.Value;
                    control = formOverall.Controls["groupBoxFlop"].Controls["numericUpDownMaxFlopCBet"] as NumericUpDown;
                    decimal maxStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            decimal statValue = (decimal)newStatsTable.Rows[i][stat];
                            if (statValue < minStat || statValue > maxStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }
                if (formOverall.checkedStats.Contains(" foldToCBFlop "))
                {
                    string stat = "foldToCBFlop";
                    NumericUpDown control = formOverall.Controls["groupBoxFlop"].Controls["numericUpDownMinFlopFoldToCB"] as NumericUpDown;
                    decimal minStat = control.Value;
                    control = formOverall.Controls["groupBoxFlop"].Controls["numericUpDownMaxFlopFoldToCB"] as NumericUpDown;
                    decimal maxStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            decimal statValue = (decimal)newStatsTable.Rows[i][stat];
                            if (statValue < minStat || statValue > maxStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }
                if (formOverall.checkedStats.Contains(" raiseCBFlop "))
                {
                    string stat = "raiseCBFlop";
                    NumericUpDown control = formOverall.Controls["groupBoxFlop"].Controls["numericUpDownMinRaiseCBFlop"] as NumericUpDown;
                    decimal minStat = control.Value;
                    control = formOverall.Controls["groupBoxFlop"].Controls["numericUpDownMaxRaiseCBFlop"] as NumericUpDown;
                    decimal maxStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            decimal statValue = (decimal)newStatsTable.Rows[i][stat];
                            if (statValue < minStat || statValue > maxStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }
                if (formOverall.checkedStats.Contains(" cBTurn "))
                {
                    string stat = "cBTurn";
                    NumericUpDown control = formOverall.Controls["groupBoxTurn"].Controls["numericUpDownMinTurnCBet"] as NumericUpDown;
                    decimal minStat = control.Value;
                    control = formOverall.Controls["groupBoxTurn"].Controls["numericUpDownMaxTurnCBet"] as NumericUpDown;
                    decimal maxStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            decimal statValue = (decimal)newStatsTable.Rows[i][stat];
                            if (statValue < minStat || statValue > maxStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }
                if (formOverall.checkedStats.Contains(" foldToCBTurn "))
                {
                    string stat = "foldToCBTurn";
                    NumericUpDown control = formOverall.Controls["groupBoxTurn"].Controls["numericUpDownMinTurnFoldToCB"] as NumericUpDown;
                    decimal minStat = control.Value;
                    control = formOverall.Controls["groupBoxTurn"].Controls["numericUpDownMaxTurnFoldToCB"] as NumericUpDown;
                    decimal maxStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            decimal statValue = (decimal)newStatsTable.Rows[i][stat];
                            if (statValue < minStat || statValue > maxStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }
                if (formOverall.checkedStats.Contains(" raiseCBTurn "))
                {
                    string stat = "raiseCBTurn";
                    NumericUpDown control = formOverall.Controls["groupBoxTurn"].Controls["numericUpDownMinRaiseCBTurn"] as NumericUpDown;
                    decimal minStat = control.Value;
                    control = formOverall.Controls["groupBoxTurn"].Controls["numericUpDownMaxRaiseCBTurn"] as NumericUpDown;
                    decimal maxStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            decimal statValue = (decimal)newStatsTable.Rows[i][stat];
                            if (statValue < minStat || statValue > maxStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }
                if (formOverall.checkedStats.Contains(" cBRiver "))
                {
                    string stat = "cBRiver";
                    NumericUpDown control = formOverall.Controls["groupBoxRiver"].Controls["numericUpDownMinRiverCBet"] as NumericUpDown;
                    decimal minStat = control.Value;
                    control = formOverall.Controls["groupBoxRiver"].Controls["numericUpDownMaxRiverCBet"] as NumericUpDown;
                    decimal maxStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            decimal statValue = (decimal)newStatsTable.Rows[i][stat];
                            if (statValue < minStat || statValue > maxStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }
                if (formOverall.checkedStats.Contains(" foldToCBRiver "))
                {
                    string stat = "foldToCBRiver";
                    NumericUpDown control = formOverall.Controls["groupBoxRiver"].Controls["numericUpDownMinRiverFoldToCB"] as NumericUpDown;
                    decimal minStat = control.Value;
                    control = formOverall.Controls["groupBoxRiver"].Controls["numericUpDownMaxRiverFoldToCB"] as NumericUpDown;
                    decimal maxStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            decimal statValue = (decimal)newStatsTable.Rows[i][stat];
                            if (statValue < minStat || statValue > maxStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }
                if (formOverall.checkedStats.Contains(" raiseCBRiver "))
                {
                    string stat = "raiseCBRiver";
                    NumericUpDown control = formOverall.Controls["groupBoxRiver"].Controls["numericUpDownMinRaiseCBRiver"] as NumericUpDown;
                    decimal minStat = control.Value;
                    control = formOverall.Controls["groupBoxRiver"].Controls["numericUpDownMaxRaiseCBRiver"] as NumericUpDown;
                    decimal maxStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            decimal statValue = (decimal)newStatsTable.Rows[i][stat];
                            if (statValue < minStat || statValue > maxStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }
                if (formOverall.checkedStats.Contains(" wentToSD "))
                {
                    string stat = "wentToSD";
                    NumericUpDown control = formOverall.Controls["numericUpDownMinWentToSD"] as NumericUpDown;
                    decimal minStat = control.Value;
                    control = formOverall.Controls["numericUpDownMaxWentToSD"] as NumericUpDown;
                    decimal maxStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            decimal statValue = (decimal)newStatsTable.Rows[i][stat];
                            if (statValue < minStat || statValue > maxStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }
                if (formOverall.checkedStats.Contains(" wonAtSD "))
                {
                    string stat = "wonAtSD";
                    NumericUpDown control = formOverall.Controls["numericUpDownMinWonAtSD"] as NumericUpDown;
                    decimal minStat = control.Value;
                    control = formOverall.Controls["numericUpDownMaxWonAtSD"] as NumericUpDown;
                    decimal maxStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            decimal statValue = (decimal)newStatsTable.Rows[i][stat];
                            if (statValue < minStat || statValue > maxStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }
                if (formOverall.checkedStats.Contains(" agg "))
                {
                    string stat = "agg";
                    NumericUpDown control = formOverall.Controls["numericUpDownMinAgg"] as NumericUpDown;
                    decimal minStat = control.Value;
                    control = formOverall.Controls["numericUpDownMaxAgg"] as NumericUpDown;
                    decimal maxStat = control.Value;
                    for (int i = newStatsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (newStatsTable.Rows[i][stat].ToString() != "")
                        {
                            decimal statValue = (decimal)newStatsTable.Rows[i][stat];
                            if (statValue < minStat || statValue > maxStat)
                                newStatsTable.Rows.RemoveAt(i);
                        }
                        else
                            newStatsTable.Rows.RemoveAt(i);
                    }
                }

                if (formOverall.checkedStats != " ")
                {
                    for (int i = 0; i < newStatsTable.Rows.Count; i++)
                    {
                        names += (string)newStatsTable.Rows[i]["name"] + " ";
                    }

                    //DataView dataView = new DataView(handsTable); // отладка
                    //dataView.Sort = "name";

                    for (int i = handsTable.Rows.Count - 1; i >= 0; i--)
                    {
                        if (!names.Contains(" " + handsTable.Rows[i]["name"].ToString() + " "))
                            handsTable.Rows.RemoveAt(i);
                    }
                }
            }

            dataGridView1.DataSource = handsTable;
            dataGridView1.Columns["id"].Visible = false;
            dataGridView1.Columns["hand"].DefaultCellStyle.Font = new Font("Serif", 11, FontStyle.Bold);

            if (overall)
                dataGridView1.Columns["name"].Visible = false;

            if (checkBoxBB.Enabled)
                checkBoxBB.Enabled = false;
            if (checkBoxSB.Enabled)
                checkBoxSB.Enabled = false;
            if (checkBoxBU.Enabled)
                checkBoxBU.Enabled = false;
            if (checkBoxCO.Enabled)
                checkBoxCO.Enabled = false;
            if (checkBoxMP.Enabled)
                checkBoxMP.Enabled = false;
            if (checkBoxUTG.Enabled)
                checkBoxUTG.Enabled = false;

            if (checkBoxBB.Checked)
                checkBoxBB.Checked = false;
            if (checkBoxSB.Checked)
                checkBoxSB.Checked = false;
            if (checkBoxBU.Checked)
                checkBoxBU.Checked = false;
            if (checkBoxCO.Checked)
                checkBoxCO.Checked = false;
            if (checkBoxMP.Checked)
                checkBoxMP.Checked = false;
            if (checkBoxUTG.Checked)
                checkBoxUTG.Checked = false;

            if (radioButtonIP.Enabled)
                radioButtonIP.Enabled = false;
            if (radioButtonOOP.Enabled)
                radioButtonOOP.Enabled = false;
            if (radioButtonIP.Checked)
                radioButtonIP.Checked = false;
            if (radioButtonOOP.Checked)
                radioButtonOOP.Checked = false;

            if (line == "Push" || line == "OpenRaise" || line == "ColdCall" || line == "Raise" || line == "Limp" || line == "3bet" || line == "Call3bet" || line == "4bet" || line == "Call4bet")
            {
                tableLayoutPanel3.ColumnStyles[0].Width = 25;
                tableLayoutPanel3.ColumnStyles[1].Width = 75;
                tableLayoutPanel4.Visible = true;

                for (var i = 0; i < handsTable.Rows.Count; i++)
                {
                    string pos = (string)handsTable.Rows[i]["pos"];
                    if (pos == "BB" && !checkBoxBB.Enabled)
                        checkBoxBB.Enabled = true;
                    else if (pos == "SB" && !checkBoxSB.Enabled)
                        checkBoxSB.Enabled = true;
                    else if (pos == "BU" && !checkBoxBU.Enabled)
                        checkBoxBU.Enabled = true;
                    else if (pos == "CO" && !checkBoxCO.Enabled)
                        checkBoxCO.Enabled = true;
                    else if (pos.Contains("MP") && !checkBoxMP.Enabled)
                        checkBoxMP.Enabled = true;
                    else if (pos.Contains("UTG") && !checkBoxUTG.Enabled)
                        checkBoxUTG.Enabled = true;
                }

                if (line == "Push")
                {
                    if (!checkBoxStack.Enabled)
                        checkBoxStack.Enabled = true;
                    if (!comboBoxPreflopAct.Enabled)
                        comboBoxPreflopAct.Enabled = true;
                    if (!numericUpDownMinStack.Enabled && checkBoxStack.Checked)
                        numericUpDownMinStack.Enabled = true;
                    if (!numericUpDownMaxStack.Enabled && checkBoxStack.Checked)
                        numericUpDownMaxStack.Enabled = true;
                    dataGridView1.Columns[1].Width = 42;
                    dataGridView1.Columns[2].Width = 32;
                    dataGridView1.Columns[4].Width = 21;
                    PreflopPosFilters();
                    PreflopPushFilters();
                }
                else
                {
                    if (checkBoxStack.Enabled)
                        checkBoxStack.Enabled = false;
                    if (comboBoxPreflopAct.Enabled)
                        comboBoxPreflopAct.Enabled = false;
                    if (numericUpDownMinStack.Enabled && checkBoxStack.Checked)
                        numericUpDownMinStack.Enabled = false;
                    if (numericUpDownMaxStack.Enabled && checkBoxStack.Checked)
                        numericUpDownMaxStack.Enabled = false;
                    ShowMatrix(handsTable);
                }
            }
            else
            {
                tableLayoutPanel3.ColumnStyles[0].Width = 95;
                tableLayoutPanel3.ColumnStyles[1].Width = 5;
                tableLayoutPanel4.Visible = false;

                if (dataGridView1.Columns["board"] != null)
                    dataGridView1.Columns["board"].DefaultCellStyle.Font = new Font("Serif", 11, FontStyle.Bold);

                string c = "";
                if (dataGridView1.Columns["flopcomb"] != null)
                    c = "flopcomb";
                else if (dataGridView1.Columns["turncomb"] != null)
                    c = "turncomb";
                else if (dataGridView1.Columns["rivercomb"] != null)
                    c = "rivercomb";
                int p = 0;
                int g = 0;
                int y = 0;
                int o = 0;
                int b = 0;
                if (c != "")
                {
                    dataGridView1.Columns[c].DefaultCellStyle.Font = new Font("Serif", 9, FontStyle.Bold);
                    dataGridView1.Columns[c].Width = 120;
                    for (int j = 0; j < dataGridView1.RowCount; j++)
                    {
                        if (dataGridView1.Columns["pos"] != null)
                        {
                            if (!radioButtonIP.Enabled && dataGridView1.Rows[j].Cells["pos"].Value.ToString() == "IP")
                                radioButtonIP.Enabled = true;
                            else if (!radioButtonOOP.Enabled && dataGridView1.Rows[j].Cells["pos"].Value.ToString() == "OOP")
                                radioButtonOOP.Enabled = true;
                        }

                        string comb = dataGridView1.Rows[j].Cells[c].Value.ToString();
                        if (comb == "TwoPair" || comb == "Three" || comb == "Straight" || comb == "Flush" || comb == "FullHouse" || comb == "Four")
                            p += 1;
                        else if (comb == "Overcards" || comb == "")
                            g += 1;
                        else if (comb.Substring(0, 4) == "Pair")
                            y += 1;
                        else if (comb.Contains("TopPair") || comb.Contains("OverPair"))
                            o += 1;
                        else
                            b += 1;
                    }
                }

                Bitmap image = (Bitmap)Properties.Resources.chart.Clone();
                decimal a = p + g + y + o + b;
                if (a == 0)
                {
                    panel1.BackgroundImage = image;
                    Cursor = Cursors.Default;
                    return;
                }
                a = 238 / a;
                p = Convert.ToInt32(p * a);
                g = Convert.ToInt32(g * a);
                y = Convert.ToInt32(y * a);
                o = Convert.ToInt32(o * a);
                b = Convert.ToInt32(b * a);
                Color clr = Color.White;
                for (int i = 0; i < image.Height; i++)
                {
                    if (p > i)
                        clr = Color.DeepPink;
                    else if (p + o > i)
                        clr = Color.Orange;
                    else if (p + o + y > i)
                        clr = Color.Yellow;
                    else if (p + o + y + b > i)
                        clr = Color.DodgerBlue;
                    else
                        clr = Color.LightGreen;
                    for (int j = 0; j < image.Width; j++)
                    {
                        Color color = image.GetPixel(j, i);
                        if (color.R > 220)
                        {
                            image.SetPixel(j, i, clr);
                        }
                    }
                }
                panel1.BackgroundImage = image;
            }

            Cursor = Cursors.Default;
        }

        private void checkBoxBB_Click(object sender, EventArgs e)
        {
            PreflopPosFilters();
            if (comboBoxPreflopAct.Enabled && (checkBoxStack.Checked || comboBoxPreflopAct.Text != "" || comboBoxPreflopAct.Text != "all actions"))
                PreflopPushFilters();
            else
            {
                dataGridView1.DataSource = newHandsTable;
                ShowMatrix(newHandsTable);
            }
        }

        private void radioButtonIP_Click(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            rb.Checked = !rb.Checked;
            if (radioButtonIP.Checked && radioButtonOOP.Checked)
            {
                if (rb.Name == "radioButtonOOP")
                    radioButtonIP.Checked = false;
                else
                    radioButtonOOP.Checked = false;
            }

            string c = "";
            if (handsTable.Columns["flopcomb"] != null)
                c = "flopcomb";
            else if (handsTable.Columns["turncomb"] != null)
                c = "turncomb";
            else if (handsTable.Columns["rivercomb"] != null)
                c = "rivercomb";
            int p = 0;
            int g = 0;
            int y = 0;
            int o = 0;
            int b = 0;

            DataTable newHandsTable = handsTable.Copy();
            for (int i = newHandsTable.Rows.Count - 1; i >= 0; i--)
            {
                string pos = newHandsTable.Rows[i]["pos"].ToString();
                if ((radioButtonIP.Checked || radioButtonOOP.Checked) && ((pos == "IP" && !radioButtonIP.Checked) || (pos == "OOP" && !radioButtonOOP.Checked)))
                    newHandsTable.Rows.RemoveAt(i);
                else
                {
                    string comb = newHandsTable.Rows[i][c].ToString();
                    if (comb == "TwoPair" || comb == "Three" || comb == "Straight" || comb == "Flush" || comb == "FullHouse" || comb == "Four")
                        p += 1;
                    else if (comb == "Overcards" || comb == "")
                        g += 1;
                    else if (comb.Substring(0, 4) == "Pair")
                        y += 1;
                    else if (comb.Contains("TopPair") || comb.Contains("OverPair"))
                        o += 1;
                    else
                        b += 1;

 
                }
            }
            dataGridView1.DataSource = newHandsTable;

            Bitmap image = (Bitmap)Properties.Resources.chart.Clone();
            decimal a = p + g + y + o + b;
            a = 238 / a;
            p = Convert.ToInt32(p * a);
            g = Convert.ToInt32(g * a);
            y = Convert.ToInt32(y * a);
            o = Convert.ToInt32(o * a);
            b = Convert.ToInt32(b * a);
            Color clr = Color.White;
            for (int i = 0; i < image.Height; i++)
            {
                if (p > i)
                    clr = Color.DeepPink;
                else if (p + o > i)
                    clr = Color.Orange;
                else if (p + o + y > i)
                    clr = Color.Yellow;
                else if (p + o + y + b > i)
                    clr = Color.DodgerBlue;
                else
                    clr = Color.LightGreen;
                for (int j = 0; j < image.Width; j++)
                {
                    Color color = image.GetPixel(j, i);
                    if (color.R > 220)
                    {
                        image.SetPixel(j, i, clr);
                    }
                }
            }
            panel1.BackgroundImage = image;
        }
          
        private void comboBoxPreflopAct_TextChanged(object sender, EventArgs e)
        {
            if (checkBoxBB.Enabled)
                checkBoxBB.Enabled = false;
            if (checkBoxSB.Enabled)
                checkBoxSB.Enabled = false;
            if (checkBoxBU.Enabled)
                checkBoxBU.Enabled = false;
            if (checkBoxCO.Enabled)
                checkBoxCO.Enabled = false;
            if (checkBoxMP.Enabled)
                checkBoxMP.Enabled = false;
            if (checkBoxUTG.Enabled)
                checkBoxUTG.Enabled = false;
            for (var i = 0; i < handsTable.Rows.Count; i++)
            {
                string pos = (string)handsTable.Rows[i]["pos"];
                string act = (string)handsTable.Rows[i]["act"];
                if (act == comboBoxPreflopAct.Text || comboBoxPreflopAct.Text == "all actions")
                {
                    if (!checkBoxBB.Enabled && pos == "BB")
                        checkBoxBB.Enabled = true;
                    else if (!checkBoxSB.Enabled && pos == "SB")
                        checkBoxSB.Enabled = true;
                    else if (!checkBoxBU.Enabled && pos == "BU")
                        checkBoxBU.Enabled = true;
                    else if (!checkBoxCO.Enabled && pos == "CO")
                        checkBoxCO.Enabled = true;
                    else if (!checkBoxMP.Enabled && pos.Contains("MP"))
                        checkBoxMP.Enabled = true;
                    else if (!checkBoxUTG.Enabled && pos.Contains("UTG"))
                        checkBoxUTG.Enabled = true;
                }
            }
            
            PreflopPosFilters();
            PreflopPushFilters();
        }

        private void checkBoxStack_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxStack.Checked)
            {
                if (!numericUpDownMinStack.Enabled)
                    numericUpDownMinStack.Enabled = true;
                if (!numericUpDownMaxStack.Enabled)
                    numericUpDownMaxStack.Enabled = true;
            }
            else
            {
                if (numericUpDownMinStack.Enabled)
                    numericUpDownMinStack.Enabled = false;
                if (numericUpDownMaxStack.Enabled)
                    numericUpDownMaxStack.Enabled = false;
            }
            PreflopPosFilters();
            PreflopPushFilters();
        }

        private void numericUpDownMinStack_ValueChanged(object sender, EventArgs e)
        {
            PreflopPosFilters();
            PreflopPushFilters();
        }
        //*******************************************************************************************

        private void PreflopPosFilters()
        {
            string poss = "";
            if (checkBoxBB.Checked)
                poss += "BB_";
            if (checkBoxSB.Checked)
                poss += "SB_";
            if (checkBoxBU.Checked)
                poss += "BU_";
            if (checkBoxCO.Checked)
                poss += "CO_";
            if (checkBoxMP.Checked)
                poss += "MP_";
            if (checkBoxUTG.Checked)
                poss += "UTG_";

            newHandsTable = handsTable.Copy();
            if (poss != "")
            {
                //DataTable newHandsTable;
                //if (checkBoxStack.Checked || comboBoxPreflopAct.Text != "" || comboBoxPreflopAct.Text != "all actions")
                //    newHandsTable = pushTable;
                //else
                //    newHandsTable = handsTable.Copy();
                for (int i = newHandsTable.Rows.Count - 1; i >= 0; i--)
                {
                    string pos = newHandsTable.Rows[i]["pos"].ToString();
                    if (pos.Contains("MP"))
                        pos = "MP";
                    else if (pos.Contains("UTG"))
                        pos = "UTG";
                    if (!poss.Contains(pos) && poss != "")
                        newHandsTable.Rows.RemoveAt(i);
                }
            }
        }

        private void PreflopPushFilters()
        {
            if (comboBoxPreflopAct.Text != "" && comboBoxPreflopAct.Text != "all actions")
            {
                dataGridView1.Columns[3].Visible = false;
                for (int i = newHandsTable.Rows.Count - 1; i >= 0; i--)
                {
                    if (comboBoxPreflopAct.Text != newHandsTable.Rows[i]["act"].ToString())
                        newHandsTable.Rows.RemoveAt(i);
                }
            }
            else if (!dataGridView1.Columns[3].Visible)
                dataGridView1.Columns[3].Visible = true;

            if (checkBoxStack.Checked)
            {
                for (int i = newHandsTable.Rows.Count - 1; i >= 0; i--)
                {
                    int stack = (int)newHandsTable.Rows[i]["Stack"];
                    if (stack < numericUpDownMinStack.Value || stack > numericUpDownMaxStack.Value)
                        newHandsTable.Rows.RemoveAt(i);
                }
            }

            dataGridView1.DataSource = newHandsTable;
            ShowMatrix(newHandsTable);

        }
       
        private void ShowMatrix(DataTable table)
        {
            foreach (var control in tableLayoutPanel4.Controls.OfType<Label>())
            {
                if (control.BackColor != (System.Drawing.Color)pocketCardsDefaultColor[control.Name] || control.Text != control.Name.Replace("label", ""))
                {
                    control.BackColor = (System.Drawing.Color)pocketCardsDefaultColor[control.Name];
                    control.Text = control.Name.Replace("label", "");
                }
            }

            if (table.Rows.Count == 0)
                return;
            
            DataTable handsGroup = new DataTable();
            handsGroup.Columns.Add("Hand");
            handsGroup.Columns.Add("Count", typeof(int));

            for (int i = table.Rows.Count - 1; i >= 0; i--)
            {
                DataRow[] foundRows = handsGroup.Select("Hand = '" + (string)table.Rows[i]["hand"] + "'");
                if (foundRows.Count() == 0)
                {
                    DataRow newRow = handsGroup.Rows.Add();
                    newRow["Hand"] = (string)table.Rows[i]["hand"];
                    newRow["Count"] = 1;
                }
                else
                    foundRows[0]["Count"] = (int)foundRows[0]["Count"] + 1;
            }
            
            string hand = "";
            int count = 0;
            int allCount = Convert.ToInt32(handsGroup.Compute("Sum(Count)", ""));
            if (allCount > 90)
            {
                for (var i = 0; i < handsGroup.Rows.Count; i++)
                {
                    hand = (string)handsGroup.Rows[i]["Hand"];
                    count = (int)handsGroup.Rows[i]["Count"];
                    if (count * 100 / allCount > 2)
                        (tableLayoutPanel4.Controls["label" + hand] as Label).BackColor = Color.SaddleBrown;
                    else if (count * 100 / allCount > 1)
                        (tableLayoutPanel4.Controls["label" + hand] as Label).BackColor = Color.DarkOrange;
                    else if (count * 100 / allCount > 0.5)
                        (tableLayoutPanel4.Controls["label" + hand] as Label).BackColor = Color.BurlyWood;
                    else
                        (tableLayoutPanel4.Controls["label" + hand] as Label).BackColor = Color.Wheat;
                    (tableLayoutPanel4.Controls["label" + hand] as Label).Text = hand + "\n" + count;
                }
            }
            else if (allCount > 40)
            {
                for (var i = 0; i < handsGroup.Rows.Count; i++)
                {
                    hand = (string)handsGroup.Rows[i]["Hand"];
                    count = (int)handsGroup.Rows[i]["Count"];
                    if (count * 100 / allCount > 5)
                        (tableLayoutPanel4.Controls["label" + hand] as Label).BackColor = Color.SaddleBrown;
                    else if (count * 100 / allCount > 3)
                        (tableLayoutPanel4.Controls["label" + hand] as Label).BackColor = Color.DarkOrange;
                    else if (count * 100 / allCount > 1.5)
                        (tableLayoutPanel4.Controls["label" + hand] as Label).BackColor = Color.BurlyWood;
                    else
                        (tableLayoutPanel4.Controls["label" + hand] as Label).BackColor = Color.Wheat;
                    (tableLayoutPanel4.Controls["label" + hand] as Label).Text = hand + "\n" + count;
                }
            }
            else if (allCount > 20)
            {
                for (var i = 0; i < handsGroup.Rows.Count; i++)
                {
                    hand = (string)handsGroup.Rows[i]["Hand"];
                    count = (int)handsGroup.Rows[i]["Count"];
                    if (count * 100 / allCount > 3)
                        (tableLayoutPanel4.Controls["label" + hand] as Label).BackColor = Color.SaddleBrown;
                    else if (count * 100 / allCount > 2)
                        (tableLayoutPanel4.Controls["label" + hand] as Label).BackColor = Color.DarkOrange;
                    else if (count * 100 / allCount > 1)
                        (tableLayoutPanel4.Controls["label" + hand] as Label).BackColor = Color.BurlyWood;
                    else
                        (tableLayoutPanel4.Controls["label" + hand] as Label).BackColor = Color.Wheat;
                    (tableLayoutPanel4.Controls["label" + hand] as Label).Text = hand + "\n" + count;
                }
            }
            else
            {
                for (var i = 0; i < handsGroup.Rows.Count; i++)
                {
                    hand = (string)handsGroup.Rows[i]["Hand"];
                    count = (int)handsGroup.Rows[i]["Count"];
                    if (count * 100 / allCount > 3)
                        (tableLayoutPanel4.Controls["label" + hand] as Label).BackColor = Color.SaddleBrown;
                    else if (count * 100 / allCount > 2)
                        (tableLayoutPanel4.Controls["label" + hand] as Label).BackColor = Color.DarkOrange;
                    else if (count * 100 / allCount > 1)
                        (tableLayoutPanel4.Controls["label" + hand] as Label).BackColor = Color.BurlyWood;
                    else
                        (tableLayoutPanel4.Controls["label" + hand] as Label).BackColor = Color.Wheat;
                    (tableLayoutPanel4.Controls["label" + hand] as Label).Text = hand + "\n" + count;
                }
            }
        }

        private void labelMenu_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(labelMenu, new Point(0, labelMenu.Height));
        }

        //*******************************************************************************************

        public void ShowHandsNotes(string queryText, string name)
        {
            Cursor = Cursors.WaitCursor;

            NpgsqlCommand cmd = new NpgsqlCommand(queryText, classConn.conn);
            cmd.Parameters.AddWithValue("@Name", name);
            handsTable.Columns.Clear();
            handsTable.Rows.Clear();
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                try
                {
                    handsTable.Load(reader);
                }
                catch
                {
                    MessageBox.Show("Query Error");
                }
            }
            dataGridView1.DataSource = handsTable;
            dataGridView1.Columns["ID"].Visible = false;
            dataGridView1.Columns["hand"].DefaultCellStyle.Font = new Font("Serif", 11, FontStyle.Bold);
            if (dataGridView1.Columns["board"] != null)
                dataGridView1.Columns["board"].DefaultCellStyle.Font = new Font("Serif", 11, FontStyle.Bold);

            Cursor = Cursors.Default;
        }

        public DataTable GetHandsNotes(string queryText)
        {
            Cursor = Cursors.WaitCursor;
            DataTable table = new DataTable();
            NpgsqlCommand cmd = new NpgsqlCommand(queryText, classConn.conn);
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                try
                {
                    table.Load(reader);
                }
                catch
                {
                    MessageBox.Show("Query Error");
                }
            }

            Cursor = Cursors.Default;

            return table;
        }

        //*******************************************************************************************

        

 


       
    }
}
