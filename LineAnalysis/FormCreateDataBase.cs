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
    public partial class FormCreateDataBase : Form
    {
        public MainForm mainForm;
        
        public FormCreateDataBase()
        {
            InitializeComponent();
        }

        private void buttonCreate_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.PostgrePassword = textBoxPassword.Text;
            Properties.Settings.Default.Save();
            NpgsqlConn classConn = new NpgsqlConn();
            classConn.mainForm = mainForm;
            classConn.CreateDatabase();
            this.Close();

        }
    }
}
