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
    public partial class FormOverall : Form
    {
        public MainForm mainForm;
        public string checkedStats = " ";
        
        public FormOverall()
        {
            InitializeComponent();
        }

        private void FormOverall_FormClosed(object sender, FormClosedEventArgs e)
        {
            mainForm.overall = false;
        }

        private void checkBoxVpip_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            string stat = cb.AccessibleName;
            if (cb.Checked)
                checkedStats += stat + " ";
            else
                checkedStats = checkedStats.Replace(" " + stat + " ", " ");
   
        }
    }
}
