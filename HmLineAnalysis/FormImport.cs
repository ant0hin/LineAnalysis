using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HmLineAnalysis
{
    public partial class FormImport : Form
    {

        public MainForm mainForm;

        public FormImport()
        {
            InitializeComponent();
            
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            
            if (textBoxServer.Text != Properties.Settings.Default.Server)
                Properties.Settings.Default.Server = textBoxServer.Text;
            if (textBoxPort.Text != Properties.Settings.Default.Port)
                Properties.Settings.Default.Port = textBoxPort.Text;
            if (textBoxUserId.Text != Properties.Settings.Default.UserId)
                Properties.Settings.Default.UserId = textBoxUserId.Text;
            if (textBoxPassword.Text != Properties.Settings.Default.Password)
                Properties.Settings.Default.Password = textBoxPassword.Text;
            if (textBoxDatabase.Text != Properties.Settings.Default.Database)
                Properties.Settings.Default.Database = textBoxDatabase.Text;

            NpgsqlConn classConn = mainForm.classConn;
            classConn.endDateTime = dateTimePickerMax.Value.EndOfDay();
            Properties.Settings.Default.LastDateLoad = dateTimePickerMin.Value.StartOfDay();
            Properties.Settings.Default.Save();

            classConn.connHM2String = @"Server=" + Properties.Settings.Default.Server + ";";
            classConn.connHM2String += "Port=" + Properties.Settings.Default.Port + ";";
            classConn.connHM2String += "User Id=" + Properties.Settings.Default.UserId + ";";
            classConn.connHM2String += "Password=" + Properties.Settings.Default.Password + ";";
            classConn.connHM2String += "Database=" + Properties.Settings.Default.Database + ";";
            classConn.connHM2String += "Encoding=UNICODE;CommandTimeout=200";

            this.Close();
            classConn.LoadHands();

        }

        private void FormImport_Load(object sender, EventArgs e)
        {
            textBoxServer.Text = Properties.Settings.Default.Server;
            textBoxPort.Text = Properties.Settings.Default.Port;
            textBoxUserId.Text = Properties.Settings.Default.UserId;
            textBoxPassword.Text = Properties.Settings.Default.Password;
            textBoxDatabase.Text = Properties.Settings.Default.Database;
            if (textBoxDatabase.Text == "")
            {
                if (mainForm.tracker == "pt4")
                    textBoxDatabase.Text = "PT4 DB";
                else
                    textBoxDatabase.Text = "HoldemManager2";
            }

        }
    }
}
