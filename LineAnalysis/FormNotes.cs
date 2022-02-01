using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using System.Collections;

namespace HmLineAnalysis
{
    public partial class FormNotes : Form
    {
        public MainForm mainForm;
        public NpgsqlConn classConn;
        DataTable statsTable = new DataTable();
        DataTable namesTable = new DataTable();
        string handsQueryText = "";
        Font boldFont = new Font("Serif", 8, FontStyle.Bold);
        Font regFont = new Font("Serif", 8, FontStyle.Regular);
        XElement root; // PokerStars
        string notesFileText = ""; //PartyPoker
        string labelsFileText = ""; //PartyPoker
        
        public FormNotes()
        {
            InitializeComponent();

            //System.Globalization.CultureInfo myCulture = new System.Globalization.CultureInfo("en-US") { /* Тут можно доопределить специфику культуры */ };
            //System.Threading.Thread.CurrentThread.CurrentCulture = myCulture;
            //System.Threading.Thread.CurrentThread.CurrentUICulture = myCulture;

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn() { Name = "Name"});
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn() { Name = "Checked" });
            dataGridView1.Columns[1].Visible = false;
            dataGridView1.Columns["Name"].DefaultCellStyle.Font = boldFont;

            comboBoxClr.Items.Add("No Label");
            //comboBoxClr.Items.Add("Yellow");
            comboBoxClr.Items.Add("Green");
            comboBoxClr.Items.Add("DeepSkyBlue");
            comboBoxClr.Items.Add("Blue");
            comboBoxClr.Items.Add("DarkBlue");
            comboBoxClr.Items.Add("BlueViolet");
            comboBoxClr.Items.Add("Red");
            comboBoxClr.Items.Add("Orange");
            //0-желтый, зеленый, голубой, синий, темносиний, фиолетовый, красный, оранжевый 

            comboBoxNotesType.Text = Properties.Settings.Default.NotesType;
            if (comboBoxNotesType.Text == "")
                comboBoxNotesType.Text = "PokerStars xml file";
            textBoxNotesFile.Text = Properties.Settings.Default.NotesFile;

            toolTip1.SetToolTip(buttonDel, "delete");
            toolTip1.SetToolTip(buttonOnOff, "on/off");
            toolTip1.SetToolTip(buttonRename, "rename");
            toolTip1.SetToolTip(buttonAdd, "add");
 
            LoadNotes();

        }

        private void LoadNotes()
        {
            dataGridView1.Rows.Clear();
            DirectoryInfo dir = new DirectoryInfo(@"Settings\Notes");
            int i = 0;
            foreach (var item in dir.GetFiles())
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0].Value = item.Name.Replace(".bin", "");

                //string[] lines = System.Text.RegularExpressions.Regex.Split(s, "\t");
                //dataGridView1.Rows[i].Cells[1].Value = lines[0];
                //dataGridView1.Rows[i].Cells[2].Value = lines[1];
                //dataGridView1.Rows[i].Cells[3].Value = lines[2];
                //dataGridView1.Rows[i].Cells[3].Value = Convert.ToInt32(lines[2]);
                //dataGridView1.Rows[i].Cells[4].Value = lines[3];
                i += 1;
            }
            if (dataGridView1.Rows.Count > 0)
            {
                ShowNote(dataGridView1.Rows[0].Cells[0].Value.ToString());
                
            }
        }

        private void buttonOnOff_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow.Cells[0].Style.Font == null || dataGridView1.CurrentRow.Cells[0].Style.Font == boldFont)
                dataGridView1.CurrentRow.Cells[0].Style.Font = regFont;
            else
                dataGridView1.CurrentRow.Cells[0].Style.Font = boldFont;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
        
            var binformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            string fileName = (string)dataGridView1.CurrentRow.Cells[0].Value;
            Hashtable notesHt = new Hashtable();
            notesHt.Add("textBoxDescription", textBoxDescription.Text);
            notesHt.Add("textBoxNote", textBoxNote.Text);

            if (comboBoxClr.Text != "" && comboBoxClr.Text != "No Label")
                notesHt.Add("comboBoxClr", comboBoxClr.Text);

            if (comboBoxPreflopAct.Text != "")
                notesHt.Add("comboBoxPreflopAct", comboBoxPreflopAct.Text);
            if (comboBoxFlopAct.Text != "")
                notesHt.Add("comboBoxFlopAct", comboBoxFlopAct.Text);
            if (comboBoxTurnAct.Text != "")
                notesHt.Add("comboBoxTurnAct", comboBoxTurnAct.Text);
            if (comboBoxRiverAct.Text != "")
                notesHt.Add("comboBoxRiverAct", comboBoxRiverAct.Text);

            if (numericUpDownMinPreflopBet.Value > 0)
                notesHt.Add("numericUpDownMinPreflopBet", numericUpDownMinPreflopBet.Value);
            if (numericUpDownMaxPreflopBet.Value > 0)
                notesHt.Add("numericUpDownMaxPreflopBet", numericUpDownMaxPreflopBet.Value);
            if (numericUpDownMinFlopBet.Value > 0)
                notesHt.Add("numericUpDownMinFlopBet", numericUpDownMinFlopBet.Value);
            if (numericUpDownMaxFlopBet.Value > 0)
                notesHt.Add("numericUpDownMaxFlopBet", numericUpDownMaxFlopBet.Value);
            if (numericUpDownMinTurnBet.Value > 0)
                notesHt.Add("numericUpDownMinTurnBet", numericUpDownMinTurnBet.Value);
            if (numericUpDownMaxTurnBet.Value > 0)
                notesHt.Add("numericUpDownMaxTurnBet", numericUpDownMaxTurnBet.Value);
            if (numericUpDownMinRiverBet.Value > 0)
                notesHt.Add("numericUpDownMinRiverBet", numericUpDownMinRiverBet.Value);
            if (numericUpDownMaxRiverBet.Value > 0)
                notesHt.Add("numericUpDownMaxRiverBet", numericUpDownMaxRiverBet.Value);

            if (comboBoxFlopComb.Text != "")
                notesHt.Add("comboBoxFlopComb", comboBoxFlopComb.Text);
            if (comboBoxTurnComb.Text != "")
                notesHt.Add("comboBoxTurnComb", comboBoxTurnComb.Text);
            if (comboBoxRiverComb.Text != "")
                notesHt.Add("comboBoxRiverComb", comboBoxRiverComb.Text);
            if (comboBoxFlopCombCompare.Text != "")
                notesHt.Add("comboBoxFlopCombCompare", comboBoxFlopCombCompare.Text);
            if (comboBoxTurnCombCompare.Text != "")
                notesHt.Add("comboBoxTurnCombCompare", comboBoxTurnCombCompare.Text);
            if (comboBoxRiverCombCompare.Text != "")
                notesHt.Add("comboBoxRiverCombCompare", comboBoxRiverCombCompare.Text);
            if (numericUpDownFlopCombPercent.Value > 0)
                notesHt.Add("numericUpDownFlopCombPercent", numericUpDownFlopCombPercent.Value);
            if (numericUpDownTurnCombPercent.Value > 0)
                notesHt.Add("numericUpDownTurnCombPercent", numericUpDownTurnCombPercent.Value);
            if (numericUpDownRiverCombPercent.Value > 0)
                notesHt.Add("numericUpDownRiverCombPercent", numericUpDownRiverCombPercent.Value);

            if (numericUpDownFlopPlayers.Value > 0)
                notesHt.Add("numericUpDownFlopPlayers", numericUpDownFlopPlayers.Value);
            if (numericUpDownTurnPlayers.Value > 0)
                notesHt.Add("numericUpDownTurnPlayers", numericUpDownTurnPlayers.Value);
            if (numericUpDownRiverPlayers.Value > 0)
                notesHt.Add("numericUpDownRiverPlayers", numericUpDownRiverPlayers.Value);

            if (radioButtonIP.Checked)
                notesHt.Add("radioButtonIP", true);
            else if (radioButtonOOP.Checked)
                notesHt.Add("radioButtonOOP", true);

            if (checkBoxVpip.Checked)
            {
                notesHt.Add("checkBoxVpip", true);
                notesHt.Add("numericUpDownMinVpip", numericUpDownMinVpip.Value);
                notesHt.Add("numericUpDownMaxVpip", numericUpDownMaxVpip.Value);
            }
            if (checkBoxPfr.Checked)
            {
                notesHt.Add("checkBoxPfr", true);
                notesHt.Add("numericUpDownMinPfr", numericUpDownMinPfr.Value);
                notesHt.Add("numericUpDownMaxPfr", numericUpDownMaxPfr.Value);
            }
            if (checkBoxCall.Checked)
            {
                notesHt.Add("checkBoxCall", true);
                notesHt.Add("numericUpDownMinCall", numericUpDownMinCall.Value);
                notesHt.Add("numericUpDownMaxCall", numericUpDownMaxCall.Value);
            }
            if (checkBox3bet.Checked)
            {
                notesHt.Add("checkBox3bet", true);
                notesHt.Add("numericUpDownMin3bet", numericUpDownMin3bet.Value);
                notesHt.Add("numericUpDownMax3bet", numericUpDownMax3bet.Value);
            }
            if (checkBoxFoldTo3bet.Checked)
            {
                notesHt.Add("checkBoxFoldTo3bet", true);
                notesHt.Add("numericUpDownMinFoldTo3bet", numericUpDownMinFoldTo3bet.Value);
                notesHt.Add("numericUpDownMaxFoldTo3bet", numericUpDownMaxFoldTo3bet.Value);
            }
            if (checkBox4bet.Checked)
            {
                notesHt.Add("checkBox4bet", true);
                notesHt.Add("numericUpDownMin4bet", numericUpDownMin4bet.Value);
                notesHt.Add("numericUpDownMax4bet", numericUpDownMax4bet.Value);
            }
            if (checkBoxFoldTo4bet.Checked)
            {
                notesHt.Add("checkBoxFoldTo4bet", true);
                notesHt.Add("numericUpDownMinFoldTo4bet", numericUpDownMinFoldTo4bet.Value);
                notesHt.Add("numericUpDownMaxFoldTo4bet", numericUpDownMaxFoldTo4bet.Value);
            }

            if (checkBoxFlopCBet.Checked)
            {
                notesHt.Add("checkBoxFlopCBet", true);
                notesHt.Add("numericUpDownMinFlopCBet", numericUpDownMinFlopCBet.Value);
                notesHt.Add("numericUpDownMaxFlopCBet", numericUpDownMaxFlopCBet.Value);
            }
            if (checkBoxFlopFoldToCB.Checked)
            {
                notesHt.Add("checkBoxFlopFoldToCB", true);
                notesHt.Add("numericUpDownMinFlopFoldToCB", numericUpDownMinFlopFoldToCB.Value);
                notesHt.Add("numericUpDownMaxFlopFoldToCB", numericUpDownMaxFlopFoldToCB.Value);
            }
            if (checkBoxRaiseCBFlop.Checked)
            {
                notesHt.Add("checkBoxRaiseCBFlop", true);
                notesHt.Add("numericUpDownMinRaiseCBFlop", numericUpDownMinRaiseCBFlop.Value);
                notesHt.Add("numericUpDownMaxRaiseCBFlop", numericUpDownMaxRaiseCBFlop.Value);
            }

            if (checkBoxTurnCBet.Checked)
            {
                notesHt.Add("checkBoxTurnCBet", true);
                notesHt.Add("numericUpDownMinTurnCBet", numericUpDownMinTurnCBet.Value);
                notesHt.Add("numericUpDownMaxTurnCBet", numericUpDownMaxTurnCBet.Value);
            }
            if (checkBoxTurnFoldToCB.Checked)
            {
                notesHt.Add("checkBoxTurnFoldToCB", true);
                notesHt.Add("numericUpDownMinTurnFoldToCB", numericUpDownMinTurnFoldToCB.Value);
                notesHt.Add("numericUpDownMaxTurnFoldToCB", numericUpDownMaxTurnFoldToCB.Value);
            }
            if (checkBoxRaiseCBTurn.Checked)
            {
                notesHt.Add("checkBoxRaiseCBTurn", true);
                notesHt.Add("numericUpDownMinRaiseCBTurn", numericUpDownMinRaiseCBTurn.Value);
                notesHt.Add("numericUpDownMaxRaiseCBTurn", numericUpDownMaxRaiseCBTurn.Value);
            }

            if (checkBoxRiverCBet.Checked)
            {
                notesHt.Add("checkBoxRiverCBet", true);
                notesHt.Add("numericUpDownMinRiverCBet", numericUpDownMinRiverCBet.Value);
                notesHt.Add("numericUpDownMaxRiverCBet", numericUpDownMaxRiverCBet.Value);
            }
            if (checkBoxRiverFoldToCB.Checked)
            {
                notesHt.Add("checkBoxRiverFoldToCB", true);
                notesHt.Add("numericUpDownMinRiverFoldToCB", numericUpDownMinRiverFoldToCB.Value);
                notesHt.Add("numericUpDownMaxRiverFoldToCB", numericUpDownMaxRiverFoldToCB.Value);
            }
            if (checkBoxRaiseCBRiver.Checked)
            {
                notesHt.Add("checkBoxRaiseCBRiver", true);
                notesHt.Add("numericUpDownMinRaiseCBRiver", numericUpDownMinRaiseCBRiver.Value);
                notesHt.Add("numericUpDownMaxRaiseCBRiver", numericUpDownMaxRaiseCBRiver.Value);
            }

            if (checkBoxHands.Checked)
            {
                notesHt.Add("checkBoxHands", true);
                notesHt.Add("numericUpDownMinHands", numericUpDownMinHands.Value);
            }
            if (checkBoxWentToSD.Checked)
            {
                notesHt.Add("checkBoxWentToSD", true);
                notesHt.Add("numericUpDownMinWentToSD", numericUpDownMinWentToSD.Value);
                notesHt.Add("numericUpDownMaxWentToSD", numericUpDownMaxWentToSD.Value);
            }
            if (checkBoxWonAtSD.Checked)
            {
                notesHt.Add("checkBoxWonAtSD", true);
                notesHt.Add("numericUpDownMinWonAtSD", numericUpDownMinWonAtSD.Value);
                notesHt.Add("numericUpDownMaxWonAtSD", numericUpDownMaxWonAtSD.Value);
            }
            if (checkBoxAgg.Checked)
            {
                notesHt.Add("checkBoxAgg", true);
                notesHt.Add("numericUpDownMinAgg", numericUpDownMinAgg.Value);
                notesHt.Add("numericUpDownMaxAgg", numericUpDownMaxAgg.Value);
            }
            
            using (var fs = System.IO.File.Create(@"Settings\Notes\" + fileName + ".bin"))
            {
                binformatter.Serialize(fs, notesHt);
            }
    
            //File.WriteAllText(@"Settings\Notes\" + fileName + ".txt", textBoxQuery.Text + "\t" + textBoxDescription.Text + "\t" + textBoxNote.Text);

            //LoadNotes();

            Cursor = Cursors.Default;
        }

        private void buttonDel_Click(object sender, EventArgs e)
        {
          
            System.IO.File.Delete(@"Settings\Notes\"+ dataGridView1.CurrentRow.Cells[0].Value.ToString() + ".bin");
            LoadNotes();
        }
                
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            textBoxName.Text = "";
            textBoxName.Visible = true;
            buttonNewNote.Visible = true;
        }

        private void buttonNewNote_Click(object sender, EventArgs e)
        {
            if (textBoxName.Text != "")
                dataGridView1.Rows.Insert(0);
            dataGridView1.Rows[0].Cells[0].Value = textBoxName.Text;
            textBoxName.Visible = false;
            buttonNewNote.Visible = false;
        }

        private void buttonRename_Click(object sender, EventArgs e)
        {
            DataGridViewCellCollection cells = dataGridView1.CurrentRow.Cells;
            textBoxName.Text = cells[0].Value.ToString(); 
            textBoxName.Visible = true;
            buttonRenameNote.Visible = true;
        }

        private void buttonRenameNote_Click(object sender, EventArgs e)
        {
            if (textBoxName.Text == "")
                return;
            File.Delete(@"Settings\Notes\" + dataGridView1.CurrentRow.Cells[0].Value.ToString() + ".bin");
            dataGridView1.CurrentRow.Cells[0].Value = textBoxName.Text;
            buttonSave_Click(null, null);
            textBoxName.Visible = false;
            buttonRenameNote.Visible = false;
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
        }

        private void cmbboxClr_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle rect = e.Bounds;
            if (e.Index >= 0)
            {
                string n = ((ComboBox)sender).Items[e.Index].ToString();
                Font f = new Font("Arial", 9, FontStyle.Regular);
                Color c = Color.FromName(n);
                Brush b = new SolidBrush(c);
                g.DrawString(n, f, Brushes.Black, rect.X, rect.Top);
                g.FillRectangle(b, rect.X + 110, rect.Y + 5,
                                rect.Width - 10, rect.Height - 10);
            }
        }

        private void textBoxNotesFile_Leave(object sender, EventArgs e)
        {
            Properties.Settings.Default.NotesFile = textBoxNotesFile.Text;
            Properties.Settings.Default.Save();
        }

        private void comboBoxNotesType_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.NotesType = comboBoxNotesType.Text;
            Properties.Settings.Default.Save();
        }

        // **************************************************************************************

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCellCollection cells = dataGridView1.CurrentRow.Cells;
            if (cells[0].Value != null)
            {
                ShowNote(cells[0].Value.ToString());

            }
        }

        private void ShowNote(string name)
        {
            ClearFilters();

            if (!File.Exists(@"Settings\Notes\" + name + ".bin"))
                return;

            var binformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (var fs = File.Open(@"Settings\Notes\" + name + ".bin", System.IO.FileMode.Open))
            {
                Hashtable notesHt = (Hashtable)binformatter.Deserialize(fs);
                foreach (DictionaryEntry item in notesHt)
                {
                    string key = (string)item.Key;
                    if (key.Contains("numericUpDown"))
                    {
                        NumericUpDown ctrl = Controls[key] as NumericUpDown;
                        if (ctrl == null)
                        {
                            if (key.Contains("Flop"))
                                ctrl = groupBoxFlop.Controls[key] as NumericUpDown;
                            else if (key.Contains("Turn"))
                                ctrl = groupBoxTurn.Controls[key] as NumericUpDown;
                            else if (key.Contains("River"))
                                ctrl = groupBoxRiver.Controls[key] as NumericUpDown;
                            else
                                ctrl = groupBoxPreflop.Controls[key] as NumericUpDown;
                        }
                        ctrl.Value = (decimal)item.Value;
                    }
                    else if (key.Contains("comboBox"))
                    {
                        ComboBox ctrl = Controls[key] as ComboBox;
                        ctrl.Text = item.Value.ToString();
                    }
                    else if (key.Contains("checkBox"))
                    {
                        CheckBox ctrl = Controls[key] as CheckBox;
                        if (ctrl == null)
                        {
                            if (key.Contains("Flop"))
                                ctrl = groupBoxFlop.Controls[key] as CheckBox;
                            else if (key.Contains("Turn"))
                                ctrl = groupBoxTurn.Controls[key] as CheckBox;
                            else if (key.Contains("River"))
                                ctrl = groupBoxRiver.Controls[key] as CheckBox;
                            else
                                ctrl = groupBoxPreflop.Controls[key] as CheckBox;
                        }
                        ctrl.Checked = (bool)item.Value;
                    }
                    else if (key.Contains("textBox"))
                    {
                        TextBox ctrl = Controls[key] as TextBox;
                        ctrl.Text = item.Value.ToString();
                    }
                    else if (key.Contains("radioButton"))
                    {
                        RadioButton ctrl = Controls[key] as RadioButton;
                        ctrl.Checked = (bool)item.Value;
                    }
                }
            }
        }

        private void dataGridViewOpponents_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCellCollection cells = dataGridViewOpponents.CurrentRow.Cells;
            string name = cells[0].Value.ToString();
            mainForm.ShowHandsNotes(handsQueryText, name);
        }

        // **************************************************************************************

        private void ClearFilters()
        {
            textBoxDescription.Text = "";
            textBoxNote.Text = "";

            //comboBoxFlopAct.Text = "";
            //comboBoxTurnAct.Text = "";
            //comboBoxRiverAct.Text = "";

            //numericUpDownMinPreflopBet.Value = 0;
            //numericUpDownMaxPreflopBet.Value = 0;
            //numericUpDownMinFlopBet.Value = 0;
            //numericUpDownMaxFlopBet.Value = 0;
            //numericUpDownMinTurnBet.Value = 0;
            //numericUpDownMaxTurnBet.Value = 0;
            //numericUpDownMinRiverBet.Value = 0;
            //numericUpDownMaxRiverBet.Value = 0;


            if (comboBoxClr.Text != "" && comboBoxClr.Text != "No Label")
            {
                comboBoxClr.Text = "No Label";
                comboBoxClr.Update();
            }

            if (comboBoxPreflopAct.Text != "")
                comboBoxPreflopAct.Text = "";
            if (comboBoxFlopAct.Text != "")
                comboBoxFlopAct.Text = "";
            if (comboBoxTurnAct.Text != "")
                comboBoxTurnAct.Text = "";
            if (comboBoxRiverAct.Text != "")
                comboBoxRiverAct.Text = "";

            if (numericUpDownMinPreflopBet.Value > 0)
                numericUpDownMinPreflopBet.Value = 0;
            if (numericUpDownMaxPreflopBet.Value > 0)
                numericUpDownMaxPreflopBet.Value = 0;
            if (numericUpDownMinFlopBet.Value > 0)
                numericUpDownMinFlopBet.Value = 0;
            if (numericUpDownMaxFlopBet.Value > 0)
                numericUpDownMaxFlopBet.Value = 0;
            if (numericUpDownMinTurnBet.Value > 0)
                numericUpDownMinTurnBet.Value = 0;
            if (numericUpDownMaxTurnBet.Value > 0)
                numericUpDownMaxTurnBet.Value = 0;
            if (numericUpDownMinRiverBet.Value > 0)
                numericUpDownMinRiverBet.Value = 0;
            if (numericUpDownMaxRiverBet.Value > 0)
                numericUpDownMaxRiverBet.Value = 0;

            if (comboBoxFlopComb.Text != "")
                comboBoxFlopComb.Text = "";
            if (comboBoxTurnComb.Text != "")
                comboBoxTurnComb.Text = "";
            if (comboBoxRiverComb.Text != "")
                comboBoxRiverComb.Text = "";
            if (comboBoxFlopCombCompare.Text != "")
                comboBoxFlopCombCompare.Text = "";
            if (comboBoxTurnCombCompare.Text != "")
                comboBoxTurnCombCompare.Text = "";
            if (comboBoxRiverCombCompare.Text != "")
                comboBoxRiverCombCompare.Text = "";
            if (numericUpDownFlopCombPercent.Value > 0)
                numericUpDownFlopCombPercent.Value = 0;
            if (numericUpDownTurnCombPercent.Value > 0)
                numericUpDownTurnCombPercent.Value = 0;
            if (numericUpDownRiverCombPercent.Value > 0)
                numericUpDownRiverCombPercent.Value = 0;

            if (numericUpDownFlopPlayers.Value > 0)
                numericUpDownFlopPlayers.Value = 0;
            if (numericUpDownTurnPlayers.Value > 0)
                numericUpDownTurnPlayers.Value = 0;
            if (numericUpDownRiverPlayers.Value > 0)
                numericUpDownRiverPlayers.Value = 0;

            if (radioButtonIP.Checked)
                radioButtonIP.Checked = false;
            else if (radioButtonOOP.Checked)
                radioButtonOOP.Checked = false;

            if (checkBoxVpip.Checked)
                checkBoxVpip.Checked = false;
            if (checkBoxPfr.Checked)
                checkBoxPfr.Checked = false;
            if (checkBoxCall.Checked)
                checkBoxCall.Checked = false;
            if (checkBox3bet.Checked)
                checkBox3bet.Checked = false;
            if (checkBoxFoldTo3bet.Checked)
                checkBoxFoldTo3bet.Checked = false;
            if (checkBox4bet.Checked)
                checkBox4bet.Checked = false;
            if (checkBoxFoldTo4bet.Checked)
                checkBoxFoldTo4bet.Checked = false;

            if (checkBoxFlopCBet.Checked)
                checkBoxFlopCBet.Checked = false;
            if (checkBoxFlopFoldToCB.Checked)
                checkBoxFlopFoldToCB.Checked = false;
            if (checkBoxRaiseCBFlop.Checked)
                checkBoxRaiseCBFlop.Checked = false;

            if (checkBoxTurnCBet.Checked)
                checkBoxTurnCBet.Checked = false;
            if (checkBoxTurnFoldToCB.Checked)
                checkBoxTurnFoldToCB.Checked = false;
            if (checkBoxRaiseCBTurn.Checked)
                checkBoxRaiseCBTurn.Checked = false;

            if (checkBoxRiverCBet.Checked)
                checkBoxRiverCBet.Checked = false;
            if (checkBoxRiverFoldToCB.Checked)
                checkBoxRiverFoldToCB.Checked = false;
            if (checkBoxRaiseCBRiver.Checked)
                checkBoxRaiseCBRiver.Checked = false;

            if (checkBoxHands.Checked)
                checkBoxHands.Checked = false;
            if (checkBoxWentToSD.Checked)
                checkBoxWentToSD.Checked = false;
            if (checkBoxWonAtSD.Checked)
                checkBoxWonAtSD.Checked = false;
            if (checkBoxAgg.Checked)
                checkBoxAgg.Checked = false;

            if (labelOppCount.Text != "")
            {
                dataGridViewOpponents.DataSource = new DataTable();
                labelOppCount.Text = "";
            }
            
        }

        private void buttonShowHands_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            string selection = " ";

            if (numericUpDownMinPreflopBet.Value > 0)
                selection += " PreflopBet >= '" + numericUpDownMinPreflopBet.Value + "' AND ";
            if (numericUpDownMaxPreflopBet.Value > 0)
                selection += " PreflopBet <= '" + numericUpDownMaxPreflopBet.Value + "' AND ";
            if (numericUpDownMinFlopBet.Value > 0)
                selection += " FlopBet >= '" + numericUpDownMinFlopBet.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "' AND ";
            if (numericUpDownMaxFlopBet.Value > 0)
                selection += " FlopBet <= '" + numericUpDownMaxFlopBet.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "' AND ";
            if (numericUpDownMinTurnBet.Value > 0)
                selection += " TurnBet >= '" + numericUpDownMinTurnBet.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "' AND ";
            if (numericUpDownMaxTurnBet.Value > 0)
                selection += " TurnBet <= '" + numericUpDownMaxTurnBet.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "' AND ";
            if (numericUpDownMinRiverBet.Value > 0)
                selection += " RiverBet >= '" + numericUpDownMinRiverBet.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "' AND ";
            if (numericUpDownMaxRiverBet.Value > 0)
                selection += " RiverBet <= '" + numericUpDownMaxRiverBet.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) + "' AND ";

            if (comboBoxPreflopAct.Text != "")
            {
                if (comboBoxPreflopAct.Text == "Aggressor")
                    selection += " PreflopAct IN ('openraise', 'raise', '3bet') AND ";
                else if (comboBoxPreflopAct.Text == "Caller")
                    selection += " PreflopAct LIKE '%call%' AND ";
                else if (comboBoxPreflopAct.Text == "Openraise")
                    selection += " PreflopAct = 'openraise' AND ";
                else if (comboBoxPreflopAct.Text == "Raise")
                    selection += " PreflopAct = 'raise' AND ";
                else if (comboBoxPreflopAct.Text == "Cold Call")
                    selection += " PreflopAct = 'cold call' AND ";
                else if (comboBoxPreflopAct.Text == "Limp")
                    selection += " PreflopAct IN ('limp', 'limp, call') AND ";
                else if (comboBoxPreflopAct.Text == "3bet")
                    selection += " PreflopAct = '3bet' AND ";
                else if (comboBoxPreflopAct.Text == "Call 3bet")
                    selection += " PreflopAct = 'call 3bet' AND ";
            }
            if (comboBoxRiverAct.Text != "")
            {
                if (comboBoxRiverAct.Text == "Bet")
                {
                    if (selection.Contains("RiverBet"))
                        selection += " RiverAct = 'bet' AND ";
                    else
                        selection += " RiverAct LIKE 'bet%' AND ";
                }
                else if (comboBoxRiverAct.Text == "Call")
                    selection += " RiverAct IN ('call', 'check, call') AND ";
                else if (comboBoxRiverAct.Text == "Check")
                    selection += " RiverAct IN ('check', 'check, call') AND ";
                else if (comboBoxRiverAct.Text == "Raise")
                    selection += " RiverAct IN ('raise', 'check, raise') AND ";
            }
            if (comboBoxTurnAct.Text != "")
            {
                if (comboBoxTurnAct.Text == "Bet")
                {
                    if (selection.Contains("River") || selection.Contains("TurnBet"))
                        selection += " TurnAct = 'bet' AND ";
                    else
                        selection += " TurnAct LIKE 'bet%' AND ";
                }
                else if (comboBoxTurnAct.Text == "Call")
                    selection += " TurnAct IN ('call', 'check, call') AND ";
                else if (comboBoxTurnAct.Text == "Check")
                {
                    if (selection.Contains("River"))
                        selection += " TurnAct = 'check' AND ";
                    else
                        selection += " TurnAct IN ('check', 'check, call') AND ";
                }
                else if (comboBoxTurnAct.Text == "Raise")
                    selection += " TurnAct IN ('raise', 'check, raise') AND ";
            }
            if (comboBoxFlopAct.Text != "")
            {
                if (comboBoxFlopAct.Text == "Bet")
                {
                    if (selection.Contains("Turn") || selection.Contains("River") || selection.Contains("FlopBet"))
                        selection += " FlopAct = 'bet' AND ";
                    else
                        selection += " FlopAct LIKE 'bet%' AND ";
                }
                else if (comboBoxFlopAct.Text == "Call")
                    selection += " FlopAct IN ('call', 'check, call') AND ";
                else if (comboBoxFlopAct.Text == "Check")
                {
                    if (selection.Contains("Turn") || selection.Contains("River"))
                        selection += " FlopAct = 'check' AND ";
                    else
                        selection += " FlopAct IN ('check', 'check, call') AND ";
                }
                else if (comboBoxFlopAct.Text == "Raise")
                    selection += " FlopAct IN ('raise', 'check, raise') AND ";
            }

            if (numericUpDownFlopPlayers.Value > 0)
                selection += " FlopPlayersCount = '" + numericUpDownFlopPlayers.Value + "' AND ";
            if (numericUpDownTurnPlayers.Value > 0)
                selection += " TurnPlayersCount = '" + numericUpDownTurnPlayers.Value + "' AND ";
            if (numericUpDownRiverPlayers.Value > 0)
                selection += " RiverPlayersCount = '" + numericUpDownRiverPlayers.Value + "' AND ";

            if (radioButtonIP.Checked)
                selection += " Pos = 'IP' AND ";
            else if (radioButtonOOP.Checked)
                selection += " Pos = 'OOP' AND ";

            string startQueryText = "SELECT name FROM hands WHERE ";
            string endQueryText = " GROUP BY name ORDER BY name ";
            string comb = "";
            string combCompare = "";
            decimal combPercent = 0;
            if (comboBoxFlopComb.Text != "")
            {
                if (comboBoxFlopCombCompare.Text != "" && numericUpDownFlopCombPercent.Value > 0)
                {
                    comb = comboBoxFlopComb.Text;
                    combCompare = comboBoxFlopCombCompare.Text;
                    combPercent = numericUpDownFlopCombPercent.Value;
                    if (comb.Contains("A high"))
                        startQueryText = "SELECT name, hand4, flopcomb AS comb, iflopcomb AS iComb FROM hands WHERE ";
                    else
                        startQueryText = "SELECT name, flopcomb AS comb, iflopcomb AS iComb FROM hands WHERE ";
                    endQueryText = " ORDER BY name ";
                }
                else
                {
                    selection += GetCombSelection(comboBoxFlopComb.Text);
                }
            }
            else if (comboBoxTurnComb.Text != "")
            {
                if (comboBoxTurnCombCompare.Text != "" && numericUpDownTurnCombPercent.Value > 0)
                {
                    comb = comboBoxTurnComb.Text;
                    combCompare = comboBoxTurnCombCompare.Text;
                    combPercent = numericUpDownTurnCombPercent.Value;
                    if (comb.Contains("A high"))
                        startQueryText = "SELECT name, hand4, turncomb AS comb, iturncomb AS iComb FROM hands WHERE ";
                    else
                        startQueryText = "SELECT name, turncomb AS comb, iturncomb AS iComb FROM hands WHERE ";
                    endQueryText = " ORDER BY name ";
                }
                else
                {
                    selection += GetCombSelection(comboBoxTurnComb.Text);
                    selection = selection.Replace("FlopComb", "TurnComb");
                }
            }
            else if (comboBoxRiverComb.Text != "")
            {
                if (comboBoxRiverCombCompare.Text != "" && numericUpDownRiverCombPercent.Value > 0)
                {
                    comb = comboBoxRiverComb.Text;
                    combCompare = comboBoxRiverCombCompare.Text;
                    combPercent = numericUpDownRiverCombPercent.Value;
                    if (comb.Contains("A high"))
                        startQueryText = "SELECT name, hand4, rivercomb AS comb, irivercomb AS iComb FROM hands WHERE ";
                    else
                        startQueryText = "SELECT name, rivercomb AS comb, irivercomb AS iComb FROM hands WHERE ";
                    endQueryText = " ORDER BY name ";
                }
                else
                {
                    selection += GetCombSelection(comboBoxRiverComb.Text);
                    selection = selection.Replace("FlopComb", "RiverComb");
                }
            }

            if (!selection.Contains(" AND "))
            {
                if (!checkBoxVpip.Checked && !checkBoxPfr.Checked && !checkBoxCall.Checked && !checkBox3bet.Checked && !checkBoxFoldTo3bet.Checked && !checkBox4bet.Checked && !checkBoxFoldTo4bet.Checked
                && !checkBoxFlopCBet.Checked && !checkBoxFlopFoldToCB.Checked && !checkBoxRaiseCBFlop.Checked && !checkBoxTurnCBet.Checked && !checkBoxTurnFoldToCB.Checked && !checkBoxRaiseCBTurn.Checked
                && !checkBoxRiverCBet.Checked && !checkBoxRiverFoldToCB.Checked && !checkBoxRaiseCBRiver.Checked && !checkBoxHands.Checked && !checkBoxWentToSD.Checked && !checkBoxWonAtSD.Checked && !checkBoxAgg.Checked)
                    return;
                handsQueryText = @"SELECT ID, Pos, Hand4 AS Hand, Board, RiverComb, FlopAct, TurnAct, RiverBet, RiverPlayersCount AS Players FROM hands WHERE Name = @name ";
                startQueryText = startQueryText.Replace("WHERE", "");
                string queryText1 = startQueryText + selection + endQueryText;
                namesTable = mainForm.GetHandsNotes(queryText1);
                namesTable = SetStatsSelection(namesTable);
                dataGridViewOpponents.DataSource = namesTable;
                labelOppCount.Text = dataGridViewOpponents.RowCount.ToString();
                Cursor = Cursors.Default;
                return;
            }
            selection = selection.Substring(0, selection.Length - 4);


            string queryText = startQueryText + selection + endQueryText;

            if (selection.Contains("River"))
                handsQueryText = @"SELECT ID, Pos, Hand4 AS Hand, Board, RiverComb, FlopAct, TurnAct, RiverBet, RiverPlayersCount AS Players FROM hands WHERE " + selection + " AND Name = @name ";
            else if (selection.Contains("Turn"))
                handsQueryText = @"SELECT ID, Pos, Hand4 AS Hand, Board, TurnComb, FlopAct, TurnAct, TurnBet, TurnPlayersCount AS Players FROM hands WHERE " + selection + " AND Name = @name ";
            else
                handsQueryText = @"SELECT ID, Pos, Hand4 AS Hand, Board, FlopComb, FlopAct, FlopBet, FlopPlayersCount AS Players FROM hands WHERE " + selection + " AND Name = @name ";

            namesTable = mainForm.GetHandsNotes(queryText);

            if (!endQueryText.Contains("GROUP BY name"))
            {
                namesTable = SetCombSelection(comb, namesTable, combCompare, combPercent);
                for (int i = namesTable.Columns.Count - 1; i > 0; i--)
                {
                    namesTable.Columns.RemoveAt(i);
                }
            }

            namesTable = SetStatsSelection(namesTable);

            dataGridViewOpponents.DataSource = namesTable;
            labelOppCount.Text = dataGridViewOpponents.RowCount.ToString();
            //dataGridViewOpponents.Columns["name"].DefaultCellStyle.Font = boldFont;

            Cursor = Cursors.Default;
        }

        private string GetCombSelection(string comb)
        {
            string selection = "";
            

//High card
//Draw
//Straight draw
//OESD
//Gutshot
//Flush Draw
//Overcards
//A high
//A high, Pair
//Pair
//Top pair
//Overpair
//Two pairs
//Three
//Straight
//Flush
//Full House
//Four +
//Pair +
//Top pair +
//Two pairs + 
//Three +
//Straight + 
//Flush +
//Full House +
//Pair -
//Top pair -
//Two pairs - 
//Three -
//Straight - 
//Flush -
//Full House -
            if (comb == "High card")
                selection = " iFlopComb < 30000 AND ";
            else if (comb == "Draw")
                selection = " iFlopComb > 10000 AND iFlopComb < 30000 AND ";
            else if (comb == "Straight draw")
                selection = " FlopComb IN ('StraightDraw', 'Gutshot', 'Overcards+StraightDraw', 'Overcards+Gutshot') AND ";
            else if (comb == "OESD")
                selection = " FlopComb IN ('StraightDraw', 'Overcards+StraightDraw') AND ";
            else if (comb == "Gutshot")
                selection = " FlopComb IN ('Gutshot', 'Overcards+Gutshot') AND ";
            else if (comb == "Flush Draw")
                selection = " FlopComb IN ('FlushDraw', 'Overcards+FlushDraw') AND ";
            else if (comb == "Overcards")
                selection = " FlopComb = 'Overcards' AND ";
            else if (comb == "A high")
                selection = " iFlopComb < 30000 AND Hand4 LIKE '%A%' AND ";
            else if (comb == "A high, Pair")
                selection = " ((iFlopComb < 30000 AND Hand4 LIKE '%A%') OR FlopComb LIKE 'Pair%') AND ";
            else if (comb == "Pair")
                selection = " FlopComb LIKE 'Pair%' AND ";
            else if (comb == "Top pair")
                selection = " FlopComb LIKE 'TopPair%' AND ";
            else if (comb == "Overpair")
                selection = " FlopComb LIKE 'OverPair%' AND ";
            else if (comb == "Two pairs")
                selection = " FlopComb = 'TwoPair' AND ";
            else if (comb == "Three")
                selection = " FlopComb = 'Three' AND ";
            else if (comb == "Straight")
                selection = " FlopComb = 'Straight' AND ";
            else if (comb == "Flush")
                selection = " FlopComb = 'Flush' AND ";
            else if (comb == "Full House")
                selection = " FlopComb = 'FullHouse' AND ";
            else if (comb == "Four +")
                selection = " iFlopComb > 90000 AND ";
            else if (comb == "Pair +")
                selection = " iFlopComb > 30000 AND ";
            else if (comb == "Top pair +")
                selection = " iFlopComb > 30000 AND FlopComb NOT LIKE 'Pair%' AND ";//    selection = " (iFlopComb > 40000 OR FlopComb LIKE 'TopPair%' OR FlopComb LIKE 'OverPair') AND ";
            else if (comb == "Two pairs +")
                selection = " iFlopComb > 40000 AND ";
            else if (comb == "Three +")
                selection = " iFlopComb > 50000 AND ";
            else if (comb == "Straight +")
                selection = " iFlopComb > 60000 AND ";
            else if (comb == "Flush +")
                selection = " iFlopComb > 70000 AND ";
            else if (comb == "Full House +")
                selection = " iFlopComb > 80000 AND ";
            else if (comb == "Pair -")
                selection = " (iFlopComb < 30000 OR FlopComb LIKE 'Pair%') AND ";
            else if (comb == "Top pair -")
                selection = " iFlopComb < 40000 AND ";
            else if (comb == "Two pairs -")
                selection = " iFlopComb < 50000 AND ";
            else if (comb == "Three -")
                selection = " iFlopComb < 60000 AND ";
            else if (comb == "Straight -")
                selection = " iFlopComb < 70000 AND ";
            else if (comb == "Flush -")
                selection = " iFlopComb < 80000 AND ";
            else if (comb == "Full House -")
                selection = " iFlopComb < 90000 AND ";


            return selection;
        }

        private DataTable SetCombSelection(string comb, DataTable namesTable, string combCompare, decimal combPercent)
        {
            int combs = 0;
            int allCombs = 0;
            string prevName = "";
            int percent = 0;
            
            if (comb == "High card")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb < 30000)
                        combs += 1;
 
                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//selection = " FlopComb IN ('', 'Overcards') AND ";
            else if (comb == "Draw")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb < 30000 && iComb > 10000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//    selection = " FlopComb IN ('StraightDraw', 'FlushDraw', 'Gutshot') AND ";
            else if (comb == "Straight draw")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if ((sComb.Contains("StraightDraw") || sComb.Contains("Gutshot")) && iComb < 30000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//    selection = " FlopComb IN ('StraightDraw', 'Gutshot') AND ";  ('StraightDraw', 'Gutshot', 'Overcards+StraightDraw', 'Overcards+Gutshot') 
            else if (comb == "OESD")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (sComb.Contains("StraightDraw") && iComb < 30000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//    selection = " FlopComb = 'StraightDraw' AND ";
            else if (comb == "Gutshot")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (sComb.Contains("Gutshot") && iComb < 30000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//   selection = " FlopComb = 'Gutshot' AND ";
            else if (comb == "Flush Draw")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (sComb.Contains("FlushDraw") && iComb < 30000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//   selection = " FlopComb = 'FlushDraw' AND ";
            else if (comb == "Overcards")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (sComb == "Overcards")
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//   selection = " FlopComb = 'Overcards' AND ";
            else if (comb == "A high")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    string hand4 = namesTable.Rows[i]["hand4"].ToString();
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb < 30000 && hand4.Contains("A"))
                    {
                        combs += 1;
                    }

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//   selection = " FlopComb IN ('', 'Overcards', 'StraightDraw', 'FlushDraw', 'Gutshot') AND Hand4 LIKE '%A%' AND ";
            else if (comb == "A high, Pair")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    string hand4 = namesTable.Rows[i]["hand4"].ToString();
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            { 
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i+1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            { 
                                if (percent < combPercent)
                                    namesTable.Rows[i+1].Delete(); 
                            }
                            else 
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i+1].Delete(); 
                            }
                        }
                    }
                    else 
                    {
                        namesTable.Rows[i+1].Delete();
                    }

                    allCombs += 1;
                    if (iComb < 30000)
                    {
                        if (hand4.Contains("A"))
                            combs += 1;
                    }
                    else if (sComb.Substring(0, 4) == "Pair")
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//   selection = " ((FlopComb IN ('', 'Overcards', 'StraightDraw', 'FlushDraw', 'Gutshot') AND Hand4 LIKE '%A%') OR FlopComb = 'Pair') AND ";
            else if (comb == "Pair")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (sComb.Length > 3 && sComb.Substring(0, 4) == "Pair")
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//   selection = " FlopComb = 'Pair' AND ";
            else if (comb == "Top pair")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (sComb.Length > 6 && sComb.Substring(0, 7) == "TopPair")
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//   selection = " FlopComb = 'TopPair' AND ";
            else if (comb == "Overpair")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (sComb.Length > 7 && sComb.Substring(0, 8) == "OverPair")
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//   selection = " FlopComb = 'OverPair' AND ";
            else if (comb == "Two pairs")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb > 40000 && iComb < 50000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//  selection = " FlopComb = 'TwoPair' AND ";
            else if (comb == "Three")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb > 50000 && iComb < 60000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//  selection = " FlopComb = 'Three' AND ";
            else if (comb == "Straight")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb > 60000 && iComb < 70000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//  selection = " FlopComb = 'Straight' AND ";
            else if (comb == "Flush")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb > 70000 && iComb < 80000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//  selection = " FlopComb = 'Flush' AND ";
            else if (comb == "Full House")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb > 80000 && iComb < 90000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//  selection = " FlopComb = 'FullHouse' AND ";
            else if (comb == "Four +")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb > 90000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//  selection = " iFlopComb > 90000 AND ";
            else if (comb == "Pair +")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb > 30000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }// selection = " iFlopComb > 30000 AND ";
            else if (comb == "Top pair +")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb > 30000 && sComb.Substring(0, 4) != "Pair")
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//   selection = " iFlopComb > 30000 AND FlopComb NOT LIKE 'Pair%' AND ";//    selection = " (iFlopComb > 40000 OR FlopComb LIKE 'TopPair%' OR FlopComb LIKE 'OverPair') AND ";
            else if (comb == "Two pairs +")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb > 40000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//  selection = " iFlopComb > 40000 AND ";
            else if (comb == "Three +")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb > 50000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//   selection = " iFlopComb > 50000 AND ";
            else if (comb == "Straight +")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb > 60000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//  selection = " iFlopComb > 60000 AND ";
            else if (comb == "Flush +")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb > 70000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//   selection = " iFlopComb > 70000 AND ";
            else if (comb == "Full House +")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb > 80000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//  selection = " iFlopComb > 80000 AND ";
            else if (comb == "Pair -")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb < 30000 || (sComb.Length > 3 && sComb.Substring(0, 4) == "Pair"))
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//  selection = " (iFlopComb < 30000 OR FlopComb LIKE 'Pair%') AND ";
            else if (comb == "Top pair -")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb < 40000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//  selection = " iFlopComb < 40000 AND ";
            else if (comb == "Two pairs -")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb < 50000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//   selection = " iFlopComb < 50000 AND ";
            else if (comb == "Three -")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb < 60000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }// selection = " iFlopComb < 60000 AND ";
            else if (comb == "Straight -")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb < 70000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//   selection = " iFlopComb < 70000 AND ";
            else if (comb == "Flush -")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb < 80000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }// selection = " iFlopComb < 80000 AND ";
            else if (comb == "Full House -")
            {
                for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
                {
                    string name = (string)namesTable.Rows[i]["name"];
                    string sComb = (string)namesTable.Rows[i]["comb"];
                    int iComb = (int)namesTable.Rows[i]["iComb"];
                    if (prevName != name)
                    {
                        if (prevName != "")
                        {
                            percent = Convert.ToInt32(combs * 100 / allCombs);
                            combs = 0;
                            allCombs = 0;
                            if (percent == combPercent)
                            {
                                if (!combCompare.Contains("="))
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else if (combCompare.Contains(">"))
                            {
                                if (percent < combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                            else
                            {
                                if (percent > combPercent)
                                    namesTable.Rows[i + 1].Delete();
                            }
                        }
                    }
                    else
                    {
                        namesTable.Rows[i + 1].Delete();
                    }

                    allCombs += 1;
                    if (iComb < 90000)
                        combs += 1;

                    prevName = name;
                }
                percent = Convert.ToInt32(combs * 100 / allCombs);
                if (percent == combPercent)
                {
                    if (!combCompare.Contains("="))
                        namesTable.Rows[0].Delete();
                }
                else if (combCompare.Contains(">"))
                {
                    if (percent < combPercent)
                        namesTable.Rows[0].Delete();
                }
                else
                {
                    if (percent > combPercent)
                        namesTable.Rows[0].Delete();
                }
            }//  selection = " iFlopComb < 90000 AND ";
            
            //for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
            //{
            //    string name = namesTable.Rows[i]["name"].ToString();
            //    string sComb = namesTable.Rows[i]["comb"].ToString();
            //    string iComb = namesTable.Rows[i]["iComb"].ToString();
            //}
            
            return namesTable;
        }

        private DataTable SetStatsSelection(DataTable namesTable)
        {
            if (!checkBoxVpip.Checked && !checkBoxPfr.Checked && !checkBoxCall.Checked && !checkBox3bet.Checked && !checkBoxFoldTo3bet.Checked && !checkBox4bet.Checked && !checkBoxFoldTo4bet.Checked
                && !checkBoxFlopCBet.Checked && !checkBoxFlopFoldToCB.Checked && !checkBoxRaiseCBFlop.Checked && !checkBoxTurnCBet.Checked && !checkBoxTurnFoldToCB.Checked && !checkBoxRaiseCBTurn.Checked
                && !checkBoxRiverCBet.Checked && !checkBoxRiverFoldToCB.Checked && !checkBoxRaiseCBRiver.Checked && !checkBoxHands.Checked && !checkBoxWentToSD.Checked && !checkBoxWonAtSD.Checked && !checkBoxAgg.Checked)
                return namesTable;
            
            if (statsTable.Rows.Count == 0)
                statsTable = classConn.GetPlayerStats();
            DataTable newStatsTable = statsTable.Copy();

            if (checkBoxVpip.Checked)
            {
                string stat = "vpip";
                decimal minStat = numericUpDownMinVpip.Value;
                decimal maxStat = numericUpDownMaxVpip.Value;
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
            if (checkBoxPfr.Checked)
            {
                string stat = "pfr";
                decimal minStat = numericUpDownMinPfr.Value;
                decimal maxStat = numericUpDownMaxPfr.Value;
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
            if (checkBoxCall.Checked)
            {

                string stat = "coldcall";
                decimal minStat = numericUpDownMinCall.Value;
                decimal maxStat = numericUpDownMaxCall.Value;
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
            if (checkBox3bet.Checked)
            {
  
                string stat = "threebet";
                decimal minStat = numericUpDownMin3bet.Value;
                decimal maxStat = numericUpDownMax3bet.Value;
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
            if (checkBoxFoldTo3bet.Checked)
            {

                string stat = "foldto3bet";
                decimal minStat = numericUpDownMinFoldTo3bet.Value;
                decimal maxStat = numericUpDownMaxFoldTo3bet.Value;
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
            if (checkBox4bet.Checked)
            {

                string stat = "fourbet";
                decimal minStat = numericUpDownMin4bet.Value;
                decimal maxStat = numericUpDownMax4bet.Value;
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
            if (checkBoxFoldTo4bet.Checked)
            {
      
                string stat = "foldto4bet";
                decimal minStat = numericUpDownMinFoldTo4bet.Value;
                decimal maxStat = numericUpDownMaxFoldTo4bet.Value;
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

            if (checkBoxFlopCBet.Checked)
            {
 
                string stat = "cBFlop";
                decimal minStat = numericUpDownMinFlopCBet.Value;
                decimal maxStat = numericUpDownMaxFlopCBet.Value;
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
            if (checkBoxFlopFoldToCB.Checked)
            {

                string stat = "foldToCBFlop";
                decimal minStat = numericUpDownMinFlopFoldToCB.Value;
                decimal maxStat = numericUpDownMaxFlopFoldToCB.Value;
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
            if (checkBoxRaiseCBFlop.Checked)
            {
     
                string stat = "raiseCBFlop";
                decimal minStat = numericUpDownMinRaiseCBFlop.Value;
                decimal maxStat = numericUpDownMaxRaiseCBFlop.Value;
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

            if (checkBoxTurnCBet.Checked)
            {

                string stat = "cBTurn";
                decimal minStat = numericUpDownMinTurnCBet.Value;
                decimal maxStat = numericUpDownMaxTurnCBet.Value;
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
            if (checkBoxTurnFoldToCB.Checked)
            {

                string stat = "foldToCBTurn";
                decimal minStat = numericUpDownMinTurnFoldToCB.Value;
                decimal maxStat = numericUpDownMaxTurnFoldToCB.Value;
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
            if (checkBoxRaiseCBTurn.Checked)
            {
  
                string stat = "raiseCBTurn";
                decimal minStat = numericUpDownMinRaiseCBTurn.Value;
                decimal maxStat = numericUpDownMaxRaiseCBTurn.Value;
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

            if (checkBoxRiverCBet.Checked)
            {
  
                string stat = "cBRiver";
                decimal minStat = numericUpDownMinRiverCBet.Value;
                decimal maxStat = numericUpDownMaxRiverCBet.Value;
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
            if (checkBoxRiverFoldToCB.Checked)
            {

                string stat = "foldToCBRiver";
                decimal minStat = numericUpDownMinRiverFoldToCB.Value;
                decimal maxStat = numericUpDownMaxRiverFoldToCB.Value;
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
            if (checkBoxRaiseCBRiver.Checked)
            {

                string stat = "raiseCBRiver";
                decimal minStat = numericUpDownMinRaiseCBRiver.Value;
                decimal maxStat = numericUpDownMaxRaiseCBRiver.Value;
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

            if (checkBoxHands.Checked)
            {

                string stat = "totalhands";
                decimal minStat = numericUpDownMinHands.Value;
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
            if (checkBoxWentToSD.Checked)
            {
   
                string stat = "wentToSD";
                decimal minStat = numericUpDownMinWentToSD.Value;
                decimal maxStat = numericUpDownMaxWentToSD.Value;
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
            if (checkBoxWonAtSD.Checked)
            {

                string stat = "wonAtSD";
                decimal minStat = numericUpDownMinWonAtSD.Value;
                decimal maxStat = numericUpDownMaxWonAtSD.Value;
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
            if (checkBoxAgg.Checked)
            {
  
                string stat = "agg";
                decimal minStat = numericUpDownMinAgg.Value;
                decimal maxStat = numericUpDownMaxAgg.Value;
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

           
            string names = " ";
            for (int i = 0; i < newStatsTable.Rows.Count; i++)
            {
                names += (string)newStatsTable.Rows[i]["name"] + " ";
            }

            for (int i = namesTable.Rows.Count - 1; i >= 0; i--)
            {
                if (namesTable.Rows[i].RowState != DataRowState.Deleted && !names.Contains(" " + namesTable.Rows[i]["name"].ToString() + " "))
                    namesTable.Rows.RemoveAt(i);
            }
          

            return namesTable;
        }

        private void buttonCreateNotes_Click(object sender, EventArgs e)
        {
            if (comboBoxNotesType.Text == "PartyPoker txt files")
                CreateNotesPartyPoker();
            else
                CreateNotesPokerStars();
        }

        private void CreateNotesPokerStars()
        {
            Cursor = Cursors.WaitCursor;

            if (!File.Exists(textBoxNotesFile.Text))
            {
                MessageBox.Show("File not found");
                Cursor = Cursors.Default;
                return;
            }

            try
            {
                File.Copy(textBoxNotesFile.Text, @"Settings\Notes\Archive\" + Path.GetFileNameWithoutExtension(textBoxNotesFile.Text) + " " + DateTime.Now.ToString().Replace(":", ".") + ".xml");
            }
            catch { }


            this.Text = "Creating notes ...";

            Hashtable notesHt = new Hashtable();
            Hashtable labelsHt = new Hashtable();
            //0-желтый, зеленый, голубой, синий, темносиний, фиолетовый, красный, оранжевый 

            foreach (DataGridViewRow row1 in dataGridView1.Rows)
            {
                if (row1.Cells[0].Style.Font != null && row1.Cells[0].Style.Font == regFont)
                    continue;

                dataGridView1.CurrentCell = dataGridView1[0, row1.Index];
                dataGridView1.Update();
                buttonShowHands_Click(null, null);
                this.Update();
                string note = textBoxNote.Text;
                int label = -1;
                if (comboBoxClr.Text != "" && comboBoxClr.Text != "No Label")
                {
                    if (comboBoxClr.Text == "Green")
                        label = 1;
                    else if (comboBoxClr.Text == "DeepSkyBlue")
                        label = 2;
                    else if (comboBoxClr.Text == "Blue")
                        label = 3;
                    else if (comboBoxClr.Text == "DarkBlue")
                        label = 4;
                    else if (comboBoxClr.Text == "BlueViolet")
                        label = 5;
                    else if (comboBoxClr.Text == "Red")
                        label = 6;
                    else if (comboBoxClr.Text == "Orange")
                        label = 7;
                }
                foreach (DataGridViewRow row in dataGridViewOpponents.Rows)
                {
                    dataGridViewOpponents.CurrentCell = dataGridViewOpponents[0, row.Index];
                    dataGridViewOpponents.Update();
                    string name = (string)row.Cells["name"].Value;
                    if (note != "")
                    {
                        if (notesHt[name] != null)
                            notesHt[name] += ". " + note;
                        else
                            notesHt.Add(name, note);
                    }
                    if (label != -1)
                    {
                        if (labelsHt[name] != null)
                            labelsHt[name] = label;
                        else
                            labelsHt.Add(name, label);
                    }
                }
            }

            string fileName = Path.GetFileName(textBoxNotesFile.Text);
            int notesCount = notesHt.Count + labelsHt.Count;
            int i = 0;
            string filePath = @"" + textBoxNotesFile.Text;
            root = XElement.Load(filePath);
            foreach (DictionaryEntry item in notesHt)
            {
                i += 1;
                this.Text = "Writing notes to a file " + fileName + " ... " + i.ToString() + " / " + notesCount.ToString();
                //this.Update();
                AddNoteXML((string)item.Value, (string)item.Key);

            }
            foreach (DictionaryEntry item in labelsHt)
            {
                i += 1;
                this.Text = "Writing notes to a file " + fileName + " ... " + i.ToString() + " / " + notesCount.ToString();
                int label = (int)labelsHt[(string)item.Key];
                AddLabelXML(label, (string)item.Key);

            }
            root.Save(filePath);

            this.Text = "Notes";
            Cursor = Cursors.Default;
        }

        private void CreateNotesPartyPoker()
        {
            Cursor = Cursors.WaitCursor;

            string folder = textBoxNotesFile.Text;
            if (folder == "")
            {
                MessageBox.Show("File not found");
                Cursor = Cursors.Default;
                return;
            }
            if (folder.Substring(folder.Length - 1, 1) != "/" && folder.Substring(folder.Length - 1, 1) != @"\")
            {
                if (folder.Contains("/"))
                    folder += "/";
                else
                    folder += @"\";
            }
            
            string fileNotes = folder + "Notes.txt";
            if (!File.Exists(fileNotes))
            {
                MessageBox.Show("File not found");
                Cursor = Cursors.Default;
                return;
            }
            string fileLabels = folder + "WatchList.txt";
            if (!File.Exists(fileLabels))
            {
                MessageBox.Show("File not found");
                Cursor = Cursors.Default;
                return;
            }

            try
            {
                File.Copy(fileNotes, @"Settings\Notes\Archive\" + Path.GetFileNameWithoutExtension(fileNotes) + " " + DateTime.Now.ToString().Replace(":", ".") + ".txt");
                File.Copy(fileLabels, @"Settings\Notes\Archive\" + Path.GetFileNameWithoutExtension(fileLabels) + " " + DateTime.Now.ToString().Replace(":", ".") + ".txt");
            }
            catch { }

            this.Text = "Creating notes ...";

            Hashtable notesHt = new Hashtable();
            Hashtable labelsHt = new Hashtable();

            foreach (DataGridViewRow row1 in dataGridView1.Rows)
            {
                if (row1.Cells[0].Style.Font != null && row1.Cells[0].Style.Font == regFont)
                    continue;

                dataGridView1.CurrentCell = dataGridView1[0, row1.Index];
                dataGridView1.Update();
                buttonShowHands_Click(null, null);
                this.Update();
                string note = textBoxNote.Text;
                string label = comboBoxClr.Text.ToLower();
                foreach (DataGridViewRow row in dataGridViewOpponents.Rows)
                {
                    dataGridViewOpponents.CurrentCell = dataGridViewOpponents[0, row.Index];
                    dataGridViewOpponents.Update();
                    string name = (string)row.Cells["name"].Value;
                    if (note != "")
                    {
                        if (notesHt[name] != null)
                            notesHt[name] += ". " + note;
                        else
                            notesHt.Add(name, note);
                    }
                    if (comboBoxClr.Text != "" && comboBoxClr.Text != "No Label")
                    {
                        if (labelsHt[name] != null)
                            labelsHt[name] = label;
                        else
                            labelsHt.Add(name, label);
                    }
                }
            }

            FileInfo f = new FileInfo(fileNotes);
            using (FileStream fileStream = f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader strIn = new StreamReader(fileStream))
                {
                    notesFileText = strIn.ReadToEnd();
                }
            }
            int notesCount = notesHt.Count + labelsHt.Count;
            int i = 0;
            foreach (DictionaryEntry item in notesHt)
            {
                i += 1;
                this.Text = "Writing notes to a file " + fileNotes + " ... " + i.ToString() + " / " + notesCount.ToString();
                AddNoteParty((string)item.Value, (string)item.Key);

            }
            if (notesHt.Count > 0)
            {
                using (var writer = new StreamWriter(fileNotes))
                {
                    writer.WriteLine(notesFileText);
                }
            }

            f = new FileInfo(fileLabels);
            using (FileStream fileStream = f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader strIn = new StreamReader(fileStream))
                {
                    labelsFileText = strIn.ReadToEnd();
                }
            }
            foreach (DictionaryEntry item in labelsHt)
            {
                i += 1;
                this.Text = "Writing notes to a file " + fileLabels + " ... " + i.ToString() + " / " + notesCount.ToString();
                AddLabelParty((string)item.Value, (string)item.Key);
            }
            if (labelsHt.Count > 0)
            {
                using (var writer = new StreamWriter(fileLabels))
                {
                    writer.WriteLine(labelsFileText);
                }
            }
          
            this.Text = "Notes";
            Cursor = Cursors.Default;
        }

        private void AddNoteXML(string notes, string name)  // добавление (обновление) нотсов в XML - PokerStars **********************************************************************
        {
         
            IEnumerable<System.Xml.Linq.XElement> note =
                from el in root.Elements("note")
                where (string)el.Attribute("player") == name
                select el;
            foreach (XElement el in note)
            {
                if (!el.Value.Contains("@"))
                {
                    el.Value = el.Value + "\r\n@ " + notes;
                }
                else
                {
                    int index = el.Value.IndexOf("@");
                    el.Value = el.Value.Substring(0, index + 1) + " " + notes;
                }
                //if (label != 0)
                //    el.SetAttributeValue("label", label); // 0-желтый, зеленый, голубой, синий, темносиний, фиолетовый, красный, оранжевый 
                name = "";
            }
            if (name != "")
            {
                var unixTime = Convert.ToInt32((DateTime.Now - new System.DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                XElement newElement = new XElement("note", new XAttribute("player", name), new XAttribute("label", 0), new XAttribute("update", unixTime));
                newElement.Value = "@ " + notes;
                root.Add(newElement);

            }
       














          
        }

        private void AddLabelXML(int label, string name)  // добавление (обновление) лейблов в XML - PokerStars **********************************************************************
        {

            IEnumerable<System.Xml.Linq.XElement> note =
                from el in root.Elements("note")
                where (string)el.Attribute("player") == name
                select el;
            foreach (XElement el in note)
            {

                el.SetAttributeValue("label", label); // 0-желтый, зеленый, голубой, синий, темносиний, фиолетовый, красный, оранжевый 
                name = "";
            }
            if (name != "")
            {
                var unixTime = Convert.ToInt32((DateTime.Now - new System.DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                XElement newElement = new XElement("note", new XAttribute("player", name), new XAttribute("label", label), new XAttribute("update", unixTime));
                root.Add(newElement);

            }
















        }

        private void AddNoteParty(string notes, string name) 
        {
            int index = notesFileText.IndexOf(name);
            if (index == -1)
            {
                notesFileText += name + "~@ " + notes + ">\r\n";
            }
            else
            {
                string text = notesFileText.Substring(index);
                text = text.Substring(0, text.IndexOf(">")+1);
                if (text.Contains("~@") || !text.Contains("@")) //ручного нотса нет
                {
                    notesFileText = notesFileText.Replace(text, name + "~@ " + notes + ">");
                }
                else
                {
                    string progNote = text.Substring(text.IndexOf("@")+2);
                    progNote = progNote.Substring(0, progNote.Length - 1);
                    string newText = text.Replace(progNote, notes);
                    notesFileText = notesFileText.Replace(text, newText);
                }

            }
            
        }

        private void AddLabelParty(string label, string name) 
        {
            int index = labelsFileText.IndexOf(name);
            if (index == -1)
            {
                labelsFileText += name + "~" + label + ">\r\n";
            }
            else
            {
                string text = labelsFileText.Substring(index);
                text = text.Substring(0, text.IndexOf(">") + 1);
                labelsFileText = labelsFileText.Replace(text, name + "~" + label + ">");
            }

        }


        //C:\Programs\PartyGaming\PartyPoker\Notes.txt
        //C:\Programs\PartyGaming\PartyPoker\WatchList.txt

        
  

       


        

           
    }
}
