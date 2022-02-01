using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Npgsql;
using System.Runtime.InteropServices;

namespace HmLineAnalysis
{
    public partial class FormHand : Form
    {
        [DllImport("user32.dll")]
        static extern bool HideCaret(IntPtr hWnd);
        
        public FormHand()
        {
            InitializeComponent();
        }

        public void ShowHandText(NpgsqlDataReader reader)
        {
            richTextBox1.Clear();

            richTextBox1.SelectionColor = Color.Indigo;
            richTextBox1.AppendText((string)reader["PreflopPos"] + "  ");

            string hand4 = (string)reader["Hand4"];
            string suit = hand4.Substring(1, 1);
            richTextBox1.SelectionColor = Color.White;
            if (suit == "h")
                richTextBox1.SelectionBackColor = Color.Red;
            else if (suit == "d")
                richTextBox1.SelectionBackColor = Color.Blue;
            else if (suit == "c")
                richTextBox1.SelectionBackColor = Color.Green;
            else
                richTextBox1.SelectionBackColor = Color.Black;
            richTextBox1.AppendText(hand4.Substring(0, 1));
            richTextBox1.SelectionBackColor = Color.Gainsboro;
            richTextBox1.AppendText(" ");
            suit = hand4.Substring(4, 1);
            richTextBox1.SelectionColor = Color.White;
            if (suit == "h")
                richTextBox1.SelectionBackColor = Color.Red;
            else if (suit == "d")
                richTextBox1.SelectionBackColor = Color.Blue;
            else if (suit == "c")
                richTextBox1.SelectionBackColor = Color.Green;
            else
                richTextBox1.SelectionBackColor = Color.Black;
            richTextBox1.AppendText(hand4.Substring(3, 1));

            richTextBox1.SelectionBackColor = Color.Gainsboro;
            richTextBox1.AppendText("  ");
            string preflopAct = (string)reader["PreflopAct"];
            if (preflopAct.Contains("call") || preflopAct == "limp")
                richTextBox1.SelectionColor = Color.DarkCyan;
            else
                richTextBox1.SelectionColor = Color.Brown;
            richTextBox1.AppendText(preflopAct + "\n");

            richTextBox1.SelectionFont = new Font("Serif", 6);
            richTextBox1.AppendText("\n");
            richTextBox1.SelectionFont = new Font("Serif", 12, FontStyle.Bold);

            // ************ FLOP ***************
            string board = (string)reader["Board"];
            suit = board.Substring(1, 1);
            richTextBox1.SelectionColor = Color.White;
            if (suit == "h")
                richTextBox1.SelectionBackColor = Color.Red;
            else if (suit == "d")
                richTextBox1.SelectionBackColor = Color.Blue;
            else if (suit == "c")
                richTextBox1.SelectionBackColor = Color.Green;
            else
                richTextBox1.SelectionBackColor = Color.Black;
            richTextBox1.AppendText(board.Substring(0, 1));
            richTextBox1.SelectionBackColor = Color.Gainsboro;
            richTextBox1.AppendText(" ");
            suit = board.Substring(4, 1);
            richTextBox1.SelectionColor = Color.White;
            if (suit == "h")
                richTextBox1.SelectionBackColor = Color.Red;
            else if (suit == "d")
                richTextBox1.SelectionBackColor = Color.Blue;
            else if (suit == "c")
                richTextBox1.SelectionBackColor = Color.Green;
            else
                richTextBox1.SelectionBackColor = Color.Black;
            richTextBox1.AppendText(board.Substring(3, 1));
            richTextBox1.SelectionBackColor = Color.Gainsboro;
            richTextBox1.AppendText(" ");
            suit = board.Substring(7, 1);
            richTextBox1.SelectionColor = Color.White;
            if (suit == "h")
                richTextBox1.SelectionBackColor = Color.Red;
            else if (suit == "d")
                richTextBox1.SelectionBackColor = Color.Blue;
            else if (suit == "c")
                richTextBox1.SelectionBackColor = Color.Green;
            else
                richTextBox1.SelectionBackColor = Color.Black;
            richTextBox1.AppendText(board.Substring(6, 1));

            richTextBox1.SelectionBackColor = Color.Gainsboro;
            richTextBox1.AppendText("  ");

            string flopAct = (string)reader["FlopAct"];
            string flopBet = reader["FlopBet"].ToString();
            if (flopBet == "0")
                flopBet = "";
            if (flopAct.Contains("call")  || flopAct.Contains("check"))
                richTextBox1.SelectionColor = Color.DarkCyan;
            else
                richTextBox1.SelectionColor = Color.Brown;
            if (flopAct == "bet, call raise")
                richTextBox1.AppendText(flopAct + "\n");
            else
                richTextBox1.AppendText(flopAct + "  " + flopBet + "\n");

            richTextBox1.SelectionFont = new Font("Serif", 6);
            richTextBox1.AppendText("\n");
            richTextBox1.SelectionFont = new Font("Serif", 12, FontStyle.Bold);

            // ************ TURN ***************
            suit = board.Substring(10, 1);
            richTextBox1.SelectionColor = Color.White;
            if (suit == "h")
                richTextBox1.SelectionBackColor = Color.Red;
            else if (suit == "d")
                richTextBox1.SelectionBackColor = Color.Blue;
            else if (suit == "c")
                richTextBox1.SelectionBackColor = Color.Green;
            else
                richTextBox1.SelectionBackColor = Color.Black;
            richTextBox1.AppendText(board.Substring(9, 1));
            
            richTextBox1.SelectionBackColor = Color.Gainsboro;
            richTextBox1.AppendText("  ");

            string turnAct = (string)reader["TurnAct"];
            string turnBet = reader["TurnBet"].ToString();
            if (turnBet == "0")
                turnBet = "";
            if (turnAct.Contains("call") || turnAct.Contains("check"))
                richTextBox1.SelectionColor = Color.DarkCyan;
            else
                richTextBox1.SelectionColor = Color.Brown;
            if (turnAct == "bet, call raise")
                richTextBox1.AppendText(turnAct + "\n");
            else
                richTextBox1.AppendText(turnAct + "  " + turnBet + "\n");

            richTextBox1.SelectionFont = new Font("Serif", 6);
            richTextBox1.AppendText("\n");
            richTextBox1.SelectionFont = new Font("Serif", 12, FontStyle.Bold);

            // ************ RIVER ***************
            suit = board.Substring(13, 1);
            richTextBox1.SelectionColor = Color.White;
            if (suit == "h")
                richTextBox1.SelectionBackColor = Color.Red;
            else if (suit == "d")
                richTextBox1.SelectionBackColor = Color.Blue;
            else if (suit == "c")
                richTextBox1.SelectionBackColor = Color.Green;
            else
                richTextBox1.SelectionBackColor = Color.Black;
            richTextBox1.AppendText(board.Substring(12, 1));

            richTextBox1.SelectionBackColor = Color.Gainsboro;
            richTextBox1.AppendText("  ");

            string riverAct = (string)reader["RiverAct"];
            string riverBet = reader["RiverBet"].ToString();
            if (riverBet == "0")
                riverBet = "";
            if (riverAct.Contains("call") || riverAct.Contains("check"))
                richTextBox1.SelectionColor = Color.DarkCyan;
            else
                richTextBox1.SelectionColor = Color.Brown;
            if (riverAct == "bet, call raise")
                richTextBox1.AppendText(riverAct + "  ");
            else
                richTextBox1.AppendText(riverAct + "  " + riverBet + "  ");
            if (riverAct != "")
            {
                richTextBox1.SelectionColor = Color.Indigo;
                richTextBox1.AppendText("("+(string)reader["Pos"] + ") ");    
            }

            HideCaret(richTextBox1.Handle);
        }
    }
}
