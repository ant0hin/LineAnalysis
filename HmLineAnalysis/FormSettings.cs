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
    public partial class FormSettings : Form
    {
        public MainForm mainForm;
        
        public FormSettings()
        {
            InitializeComponent();
            checkBoxStopTimerWhenShowing.Checked = Properties.Settings.Default.StopTimerWhenShowing;
            checkBoxAlwaysOnTop.Checked = Properties.Settings.Default.TopMost;
            dateTimePicker1.Value = Properties.Settings.Default.LastDateLoad;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.StopTimerWhenShowing = checkBoxStopTimerWhenShowing.Checked;
            Properties.Settings.Default.DoNotCloseNotesWindow = checkBoxDoNotCloseNotesWindow.Checked;
            Properties.Settings.Default.LastDateLoad = dateTimePicker1.Value;
            Properties.Settings.Default.TopMost = checkBoxAlwaysOnTop.Checked;
            Properties.Settings.Default.Save();
            mainForm.TopMost = checkBoxAlwaysOnTop.Checked;
            mainForm.log = checkBoxLog.Checked;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
