using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Npgsql;
using System.Data;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.IO;
using System.Collections;

namespace HmLineAnalysis
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
                
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            File.WriteAllText(@"log.txt", string.Empty);

            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs args)
            {
                System.IO.File.AppendAllText("log.txt", "====================" + DateTime.Now + "========================\r\n" + args.ExceptionObject.ToString() + "\r\n===============================================================\r\n");
            };
            Application.ThreadException += delegate(Object sender, System.Threading.ThreadExceptionEventArgs args)
            {
                System.IO.File.AppendAllText("log.txt", "====================" + DateTime.Now + "========================\r\n" + args.Exception.ToString() + "\r\n===============================================================\r\n");
            };

            System.Globalization.CultureInfo myCulture = new System.Globalization.CultureInfo("en-US") { /* Тут можно доопределить специфику культуры */ };

            MainForm mainForm = new MainForm();
            Application.Run(mainForm);

        }

        public static void Log(Exception e, string text = "")
        {
            string message = "=========Log=========" + DateTime.Now + "======================\r\n";
            try
            {
                message += Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software").OpenSubKey("PostgreSQL").OpenSubKey("Installations").GetSubKeyNames()[0] + "\r\n";
            }
            catch { }
            if (text != "")
                message += text + "\r\n";
            message += "Ошибка: " + e.Message + "\r\n" +
            "Объект: " + e.Source + "\r\n" +
            "Метод, вызвавший исключение: " + e.TargetSite + "\r\n" +
            "Стэк: " + e.StackTrace + "\r\n" +
            "===============================================================\r\n";
            System.IO.File.AppendAllText("log.txt", message);
        }

        public static void LogHand(string text)
        {
            string message = "========LogHand==+=====" + DateTime.Now + "=====================\r\n";
            message += text + "\r\n";
            message +=       "===============================================================\r\n";
            System.IO.File.AppendAllText("log.txt", message);
        }

        public static DateTime EndOfDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
        }

        public static DateTime StartOfDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
        }
                
        private static string GetIdentifier(string wmiClass, string wmiProperty)
        {
            string result = "";
            System.Management.ManagementClass mc = new System.Management.ManagementClass(wmiClass);
            System.Management.ManagementObjectCollection moc = mc.GetInstances();
            foreach (System.Management.ManagementObject mo in moc)
            {
                //Only get the first one
                if (result == "")
                {
                    try
                    {
                        result = mo[wmiProperty].ToString();
                        break;
                    }
                    catch
                    {
                    }
                }
            }
            return result;
        }

     
    }

    public class NpgsqlConn
    {
        string connString = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=postgrespass;Database=lineanalysis;Encoding=UNICODE;CommandTimeout=200";
        public string connHM2String = ""; //@"Server=127.0.0.1;Port=5432;User Id=postgres;Password=postgrespass;Database=nl_10;Encoding=UNICODE;CommandTimeout=200";
        public NpgsqlConnection conn;
        public NpgsqlConnection connHM2;
        DataTable table = new DataTable();
        DataTable tablePush = new DataTable();
        public MainForm mainForm;
        public string formText = "Line Analysis";
        public Thread StaticCaller;
        //public DateTime startDateTime;
        public DateTime endDateTime;

        System.Globalization.CultureInfo myCulture = new System.Globalization.CultureInfo("en-US") { /* Тут можно доопределить специфику культуры */ };
        
        System.Timers.Timer timer = new System.Timers.Timer();

        public void OpenDataBase(string parametr = "start")
        {
 
            if (Properties.Settings.Default.PostgrePassword != "" && Properties.Settings.Default.PostgrePassword != "postgrespass")
                connString = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=" + Properties.Settings.Default.PostgrePassword + ";Database=lineanalysis;Encoding=UNICODE;CommandTimeout=200";

            conn = new NpgsqlConnection(connString);

            try
            {
                conn.Open();
            }
            catch
            {
                CreateDatabase();
            }

            //Properties.Settings.Default.databaseIsAltered = "";
            //Properties.Settings.Default.LastDateLoad = DateTime.Now.AddDays(-1);
            //Properties.Settings.Default.Save();
            
            //DropAndCreateAllTables();

//            if (Properties.Settings.Default.databaseIsAltered == "")
//            {
//                string query = @"SELECT ordinal_position, column_name, data_type, character_maximum_length
//                            FROM information_schema.columns
//                            WHERE table_name='hands' AND column_name IN ('flopcomb', 'turncomb')";
//                NpgsqlCommand cmd = new NpgsqlCommand(query, conn);
//                DataTable tableColumns = new DataTable();
//                using (NpgsqlDataReader reader = cmd.ExecuteReader())
//                {
//                    tableColumns.Load(reader);
//                }
//                if (tableColumns.Rows.Count > 0 && (int)tableColumns.Rows[1]["character_maximum_length"] < 22)
//                {
//                    //mainForm.Text = "Creating database Postgresql ...";
//                    //query = @"DROP TABLE hands; DROP TABLE results;";
//                    //cmd = new NpgsqlCommand(query, conn);
//                    //cmd.ExecuteNonQuery();
//                    conn.Close();
//                    DropAndCreateAllTables();

//                    //for (int j = 0; j < tableColumns.Rows.Count; j++)
//                    //{
//                    //    if ((int)tableColumns.Rows[j]["character_maximum_length"] < 21)
//                    //    {
//                    //        mainForm.Text = "Changing LineAnalysis database structure ...";
//                    //        query = @"ALTER TABLE hands ALTER COLUMN " + (string)tableColumns.Rows[j]["column_name"] + " TYPE varchar(21)";
//                    //        cmd = new NpgsqlCommand(query, conn);
//                    //        cmd.ExecuteNonQuery();
//                    //    }
//                    //}
//                }
//                else 
//                {
//                    query = @"select * from pg_tables WHERE tablename = 'prefloppush' ";
//                    cmd = new NpgsqlCommand(query, conn);
//                    //DataTable tableTables = new DataTable();
//                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read())
//                        { }
//                        else
//                        {
//                            conn.Close();
//                            DropAndCreateAllTables();
//                        }
//                    }
//                }
//                Properties.Settings.Default.databaseIsAltered = "22";
//                Properties.Settings.Default.Save();
//            }
            
            if (Properties.Settings.Default.Server != "" && Properties.Settings.Default.Port != "" && Properties.Settings.Default.UserId != "" && Properties.Settings.Default.Password != "" && Properties.Settings.Default.Database != "")
            {
                connHM2String = @"Server=" + Properties.Settings.Default.Server + ";";
                connHM2String += "Port=" + Properties.Settings.Default.Port + ";";
                connHM2String += "User Id=" + Properties.Settings.Default.UserId + ";";
                connHM2String += "Password=" + Properties.Settings.Default.Password + ";";
                connHM2String += "Database=" + Properties.Settings.Default.Database + ";";
                connHM2String += "Encoding=UNICODE;CommandTimeout=200";
                connHM2 = new NpgsqlConnection(connHM2String);

                try
                {
                    connHM2.Open();
                }
                catch
                {
                    File.AppendAllText("log.txt", "====================" + DateTime.Now + "========================\r\nError connecting \r\n" + connHM2.ConnectionString + "\r\n" + "===============================================================\r\n");
                    if (mainForm.tracker == "pt4")
                        MessageBox.Show("Error connecting to the PT4 database");
                    else
                        MessageBox.Show("Error connecting to the HM2 database");
                    mainForm.BeginInvoke(new Action(delegate()
                    {
                        if (mainForm.tracker == "pt4")
                            mainForm.Text = " Error connecting to the PT4 database ";
                        else
                            mainForm.Text = " Error connecting to the HM2 database ";
                    }));
                    return;
                };
            }
            else
                connHM2String = "";

            table.Columns.Add("ID");
            table.Columns.Add("Name");
            table.Columns.Add("Hand3");
            table.Columns.Add("Hand4");
            table.Columns.Add("PreflopPos");
            table.Columns.Add("PreflopAct");
            table.Columns.Add("PreflopBet", typeof(decimal));
            table.Columns.Add("FlopComb");
            table.Columns.Add("iFlopComb");
            table.Columns.Add("FlopAct");
            table.Columns.Add("FlopBet", typeof(decimal));
            table.Columns.Add("FlopPlayersCount", typeof(int));
            table.Columns.Add("TurnComb");
            table.Columns.Add("iTurnComb");
            table.Columns.Add("TurnAct");
            table.Columns.Add("TurnBet", typeof(decimal));
            table.Columns.Add("TurnPlayersCount", typeof(int));
            table.Columns.Add("RiverComb");
            table.Columns.Add("iRiverComb");
            table.Columns.Add("RiverAct");
            table.Columns.Add("RiverBet", typeof(decimal));
            table.Columns.Add("RiverPlayersCount", typeof(int));
            table.Columns.Add("Board");
            table.Columns.Add("Pos");
            //table.Columns.Add("FlopBoardType");
            //table.Columns.Add("TurnBoardType");
            //table.Columns.Add("RiverBoardType");

            tablePush.Columns.Add("ID");
            tablePush.Columns.Add("Name");
            tablePush.Columns.Add("Hand3");
            tablePush.Columns.Add("PreflopPos");
            tablePush.Columns.Add("PreflopAct");
            tablePush.Columns.Add("Stack", typeof(int));

            StaticCaller = new Thread(new ThreadStart(StartLoadHands));
            StaticCaller.IsBackground = true;
            StaticCaller.Start();

            timer.Elapsed += new ElapsedEventHandler(TimerEvent);
            timer.Interval = 60000;
            timer.Start();
        }

        public void CreateDatabase(bool onlyTables = false)
        {
            if (Properties.Settings.Default.PostgrePassword != "" && Properties.Settings.Default.PostgrePassword != "postgrespass")
                connString = "Server=127.0.0.1;Port=5432;User Id=postgres;Password="+Properties.Settings.Default.PostgrePassword+";Database=lineanalysis;Encoding=UNICODE;CommandTimeout=200";
            
            mainForm.WindowState = FormWindowState.Normal;
            
            string sqlText = "";
            NpgsqlCommand cmd = new NpgsqlCommand();
            if (!onlyTables)
            {
                mainForm.Text = "Creating database Postgresql ...";

                NpgsqlConnection connNew = new NpgsqlConnection(connString.Replace("lineanalysis", ""));
                sqlText = @"CREATE DATABASE LineAnalysis; ";
                cmd = new NpgsqlCommand(sqlText, connNew);
                try
                {
                    connNew.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    mainForm.Text = "Creating database Postgresql ... Error";
                    Program.Log(e, connString);
                    FormCreateDataBase formCreateDatabase = new FormCreateDataBase();
                    formCreateDatabase.mainForm = mainForm;
                    formCreateDatabase.Show();
                    return;
                }
            }

            conn = new NpgsqlConnection(connString);
            conn.Open();
     
            sqlText = @"CREATE TABLE hands
            (
              id VARCHAR(33) NOT NULL PRIMARY KEY,
              name VARCHAR(20) NOT NULL,
              hand3 VARCHAR(3) NOT NULL,
              hand4 VARCHAR(5) NOT NULL,
              preflopPos VARCHAR(3) NOT NULL,
              preflopAct VARCHAR(20) NOT NULL,
              preflopBet FLOAT(8),
              flopComb VARCHAR(22),
              iFlopComb INTEGER NOT NULL,
              flopAct VARCHAR(20),
              flopBet DECIMAL,
              flopPlayersCount INTEGER NOT NULL,
              turnComb VARCHAR(22),
              iTurnComb INTEGER NOT NULL,
              turnAct VARCHAR(20),
              turnBet DECIMAL,
              turnPlayersCount INTEGER NOT NULL,
              riverComb VARCHAR(20),
              iRiverComb INTEGER NOT NULL,
              riverAct VARCHAR(20),
              riverBet DECIMAL,
              riverPlayersCount INTEGER NOT NULL,
              board VARCHAR(20) NOT NULL,
              pos VARCHAR(3) NOT NULL
            )";

            cmd = new NpgsqlCommand(sqlText, conn);
            cmd.ExecuteNonQuery();

//            sqlText = @"CREATE INDEX name ON hands (name);
//            CREATE INDEX hand3 ON hands (hand3);
//            CREATE INDEX preflopPos ON hands (preflopPos);
//            CREATE INDEX preflopAct ON hands (preflopAct);
//            CREATE INDEX preflopBet ON hands (preflopBet);
//            CREATE INDEX flopComb ON hands (flopComb);
//            CREATE INDEX iFlopComb ON hands (iFlopComb);
//            CREATE INDEX flopAct ON hands (flopAct);
//            CREATE INDEX flopBet ON hands (flopBet);
//            CREATE INDEX flopPlayersCount ON hands (flopPlayersCount);
//            CREATE INDEX turnComb ON hands (turnComb);
//            CREATE INDEX iTurnComb ON hands (iTurnComb);
//            CREATE INDEX turnAct ON hands (turnAct);
//            CREATE INDEX turnBet ON hands (turnBet);
//            CREATE INDEX turnPlayersCount ON hands (turnPlayersCount);
//            CREATE INDEX riverComb ON hands (riverComb);
//            CREATE INDEX iRiverComb ON hands (iRiverComb);
//            CREATE INDEX riverAct ON hands (riverAct);
//            CREATE INDEX riverBet ON hands (riverBet);
//            CREATE INDEX riverPlayersCount ON hands (riverPlayersCount);
//            CREATE INDEX pos ON hands (pos);";
         
            sqlText = @"CREATE INDEX preflopAct ON hands (preflopAct);
            CREATE INDEX name ON hands (name);
            CREATE INDEX flopAct ON hands (flopAct);
            CREATE INDEX turnAct ON hands (turnAct);
            CREATE INDEX riverAct ON hands (riverAct); ";

            cmd = new NpgsqlCommand(sqlText, conn);
            cmd.ExecuteNonQuery();

            sqlText = @"CREATE TABLE results
                        (
                          name VARCHAR(20) NOT NULL PRIMARY KEY,
                          acts VARCHAR(250)
                        )";

            cmd = new NpgsqlCommand(sqlText, conn);
            cmd.ExecuteNonQuery();

            sqlText = @"CREATE INDEX resultsname ON results (name)";

            cmd = new NpgsqlCommand(sqlText, conn);
            cmd.ExecuteNonQuery();

            sqlText = @"CREATE TABLE prefloppush
            (
              id VARCHAR(33) NOT NULL PRIMARY KEY,
              name VARCHAR(20) NOT NULL,
              hand3 VARCHAR(3) NOT NULL,
              preflopPos VARCHAR(3) NOT NULL,
              preflopAct VARCHAR(20) NOT NULL,
              stack INTEGER NOT NULL
 
            )";

            cmd = new NpgsqlCommand(sqlText, conn);
            cmd.ExecuteNonQuery();

            sqlText = @"CREATE INDEX prefloppushname ON prefloppush (name);
            CREATE INDEX prefloppushpreflopAct ON prefloppush (preflopAct);
            CREATE INDEX prefloppushpreflopPos ON prefloppush (preflopPos);
            CREATE INDEX prefloppushstack ON prefloppush (stack); ";

            cmd = new NpgsqlCommand(sqlText, conn);
            cmd.ExecuteNonQuery();

            Properties.Settings.Default.databaseIsAltered = "22";
            Properties.Settings.Default.Save();

            mainForm.Text = "Creating database Postgresql ... OK";
        }

        public void DropAndCreateAllTables()
        {
            mainForm.Text = "Creating database Postgresql ...";
            conn = new NpgsqlConnection(connString);
            conn.Open();
            string query = @"select * from pg_tables WHERE tablename = 'prefloppush' ";
            string queryDrop = @"DROP TABLE hands; DROP TABLE results;";
            NpgsqlCommand cmd = new NpgsqlCommand(query, conn);
            //DataTable tableColumns = new DataTable();
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                    queryDrop += " DROP TABLE prefloppush; ";
            }
            cmd = new NpgsqlCommand(queryDrop, conn);
            cmd.ExecuteNonQuery();
            CreateDatabase(true);
        }

        public void LoadHands()
        {
            if (connHM2String == "")
                return;

            timer.Stop();

            mainForm.BeginInvoke(new Action(delegate()
            {
                mainForm.Text = " Searching hands ... ";
            }));

            NpgsqlConnection conn = new NpgsqlConnection(connString);
            try
            {
                conn.Open();
            }
            catch (Exception e)
            {
                Program.Log(e, connString);
                mainForm.BeginInvoke(new Action(delegate()
                {
                    //mainForm.Text = " Error connecting to the LineAnalysis database ";
                }));
                return;
            };
    
            NpgsqlConnection connHM2 = new NpgsqlConnection(connHM2String);

            try
            {
                connHM2.Open();
            }
            catch (Exception e)
            {
                Program.Log(e, connString);
                mainForm.BeginInvoke(new Action(delegate()
                {
                    if (mainForm.tracker == "pt4")
                        mainForm.Text = " Error connecting to the PT4 database ";
                    else
                        mainForm.Text = " Error connecting to the HM2 database ";
                }));
                return;
            };

            Thread.CurrentThread.CurrentCulture = myCulture;
            Thread.CurrentThread.CurrentUICulture = myCulture;

            File.AppendAllText("log.txt", "======startload===========" + DateTime.Now + "==================\r\n" + connHM2.ConnectionString + "\r\n" + Properties.Settings.Default.LastDateLoad + "   " + endDateTime + "\r\n===============================================================\r\n");

            //mainForm.tracker = "pt4"; //отладка
            
            string query = "";
            if (mainForm.tracker == "hm2")
            {
                query = @"            SELECT 
              handhistories.gamenumber, 
              handhistories.handhistory, 
              handhistories.handtimestamp,
              handhistories.pokersite_id 
            FROM 
              handhistories 
            WHERE 
               handhistories.handtimestamp > @dateTime
               AND handhistories.handtimestamp <= @dateTimeEnd ";
            }
            else
            {
                query = @"            (SELECT 
                  cash_hand_summary.id_hand AS gamenumber,
                  cash_hand_histories.history AS handhistory,
                  cash_hand_summary.date_played,
                  cash_hand_summary.id_site AS pokersite_id
                FROM 
                  cash_hand_summary 
                JOIN   
                  cash_hand_histories
                ON
                  cash_hand_summary.id_hand = cash_hand_histories.id_hand 
                WHERE 
                   cash_hand_summary.date_played >= @dateTime
                   AND cash_hand_summary.date_played <= @dateTimeEnd ) 

                UNION

                (SELECT 
                  tourney_hand_summary.id_hand AS gamenumber, 
                  tourney_hand_histories.history ,
                  tourney_hand_summary.date_played,
                  tourney_hand_summary.id_site 
                FROM 
                  tourney_hand_summary 
                JOIN   
                  tourney_hand_histories
                ON
                  tourney_hand_summary.id_hand = tourney_hand_histories.id_hand 
                WHERE 
                   tourney_hand_summary.date_played >= @dateTime
                   AND tourney_hand_summary.date_played <= @dateTimeEnd ) ";
            }

            NpgsqlCommand cmd = new NpgsqlCommand(query, connHM2);
            cmd.Parameters.AddWithValue("@dateTime", Properties.Settings.Default.LastDateLoad); // startDateTime //startDateTime.AddDays(-600) DateTime.Now.AddDays(-2)
            cmd.Parameters.AddWithValue("@dateTimeEnd", endDateTime);

            NpgsqlCommand cmd2 = new NpgsqlCommand();
            cmd2.CommandType = System.Data.CommandType.Text;
            cmd2.Connection = conn;
            cmd2.Parameters.AddWithValue("@Name", "");
            cmd2.Parameters.AddWithValue("@Acts", "");

            int count = 0;
            int countAll = 0;
            int errors = 0;
            int duplicates = 0;            
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    countAll += 1;
                    if (mainForm.InvokeRequired)
                    {
                        mainForm.BeginInvoke(new Action(delegate()
                        {
                            mainForm.Text = " Loading hands ... " + countAll.ToString() + ". Loaded " + count.ToString() + ". Errors " + errors.ToString() + ". Duplicates " + duplicates.ToString();
                        }));
                    }
                    else
                        mainForm.Text = " Loading hands ... " + countAll.ToString() + ". Loaded " + count.ToString() + ". Errors " + errors.ToString() + ". Duplicates " + duplicates.ToString();

                    string handText = reader.GetString(1);
                    short roomId = (short)reader[3];
                    if (reader.GetDateTime(2) > Properties.Settings.Default.LastDateLoad)
                        Properties.Settings.Default.LastDateLoad = reader.GetDateTime(2);
                  
                    table.Rows.Clear();
                    tablePush.Rows.Clear();

                    try
                    {
                         
                        if ((roomId == 2 || roomId == 100) && handText.Contains(" Limit ") && handText.Contains(" shows ") && !handText.Contains("*** FIRST ") && handText.Contains("*** FLOP ***")) // PokerStars
                            ReadHandStars(handText);
                        else if ((roomId == 12 || roomId == 900) && handText.Contains(" shows ")) // 888, Lotos
                            ReadHand888(handText);
                        else if ((roomId == 0 || roomId == 200) && handText.Contains(" show") && handText.Contains("** Dealing Flop **")) // Party
                            ReadHandParty(handText);
                        else if ((roomId == 4) && handText.Contains(@"<round no=""2"">")) // IPoker HM2
                            ReadHandIPokerHM2(handText);
                        else if ((roomId == 1100 || roomId == 400)) //  IPoker PT4
                            ReadHandIPokerPT4(handText);
                        else if (roomId == 700 && handText.Contains("*** FLOP ***")) //  GTech PT4
                            ReadHandGTechPT4(handText);
                        else if (!(roomId == 2 || roomId == 100) && !(roomId == 12 || roomId == 900) && !(roomId == 0 || roomId == 200) && !(roomId == 4) && !(roomId == 1100 || roomId == 400) && roomId != 700)
                        {
                            Program.LogHand(mainForm.tracker + "\r\n" + "roomId = " + roomId + "\r\n" + handText + "\r\n");
                            continue;
                        }
                        else
                            continue;
                    }
                    catch (Exception e)
                    {
                        errors += 1;
                        Program.Log(e, "ReadHand() Error\r\n" + connString + "\r\n" + handText + "\r\n");
                        continue;
                    };

                    if (tablePush.Rows.Count > 0)
                        countAll = countAll;

                    if (table.Rows.Count == 0) 
                        continue;

                    for (int r = 0; r < table.Rows.Count; r++)
                    {
                        cmd2.CommandText = "INSERT INTO Hands VALUES (";
                        for (int c = 0; c < table.Columns.Count; c++)
                        {
                            string type = table.Columns[c].DataType.Name;
                            if (type == "Decimal")
                            {
                                string s = table.Rows[r][c].ToString();
                                s = s.Replace(",", ".");
                                if (s == "")
                                    s = "0";
                                cmd2.CommandText += s + ",";
                            }
                            else if (type == "Int32")
                            {
                                string s = table.Rows[r][c].ToString();
                                if (s == "")
                                    s = "0";
                                cmd2.CommandText += s + ",";
                            }
                            else
                                cmd2.CommandText += "'" + table.Rows[r][c] + "',";
                        }
                        cmd2.CommandText = cmd2.CommandText.Substring(0, cmd2.CommandText.Length - 1) + ")";
                        try
                        {
                            cmd2.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            if (e.Message.Contains("23505"))
                                duplicates += 1;
                            else
                            {
                                errors += 1;
                                Program.Log(e, "cmd2.ExecuteNonQuery() Error\r\n connString =\r\n" + connString + "\r\n" + "cmd2.CommandText =\r\n" + cmd2.CommandText + "handText =\r\n" + handText + "\r\n");
                            }
                            continue;
                        } 
                        count += 1;
                          
                        cmd2.Parameters["@Name"].Value = table.Rows[r]["Name"].ToString();
                        cmd2.CommandText = "SELECT acts FROM results WHERE name = @Name";
                        NpgsqlDataReader reader2 = cmd2.ExecuteReader();
                        string act = "";
                        if (reader2.Read()) 
                        {
                            act = reader2["acts"].ToString();
                            cmd2.CommandText = "UPDATE Results SET acts = @Acts WHERE name = @Name;";
                            reader2.Close();
                        }
                        else
                        {
                            cmd2.CommandText = "INSERT INTO Results VALUES (@Name, @Acts);";
                        }

                        string pAct = table.Rows[r]["PreflopAct"].ToString();
                        string fAct = table.Rows[r]["FlopAct"].ToString();
                        string tAct = table.Rows[r]["TurnAct"].ToString();
                        string rAct = table.Rows[r]["RiverAct"].ToString();

                        if (pAct.Contains("openraise") && !act.Contains("por_"))
                            act += "por_";
                        else if (pAct == "cold call" && !act.Contains("pc_"))
                            act += "pc_";
                        else if (pAct == "raise" && !act.Contains("pr_"))
                            act += "pr_";
                        else if (pAct.Contains("limp") && !act.Contains("pl_"))
                            act += "pl_";
                        else if (pAct == "call 3bet" && !act.Contains("pc3_"))
                            act += "pc3_";
                        else if (pAct.Contains("3bet") && !act.Contains("p3_"))
                            act += "p3_";
                        else if (pAct == "call 4bet" && !act.Contains("pc4_"))
                            act += "pc4_";
                        else if (pAct.Contains("4bet") && !act.Contains("p4_"))
                            act += "p4_";
              

                        if (!pAct.Contains("call") && !pAct.Contains("limp"))  //pAct == "raise" || pAct == "3bet" || pAct == "4bet")
                        {
                            if (fAct.Contains("bet"))
                            {
                                if (!act.Contains("fco_"))
                                    act += "fco_";
                                if (fAct == "bet")
                                {
                                    if (tAct.Contains("bet"))
                                    {
                                        if (!act.Contains("tco_"))
                                            act += "tco_";
                                        if (tAct == "bet")
                                        {
                                            if (rAct.Contains("bet") && !act.Contains("rco_"))
                                                act += "rco_";
                                            else if (rAct.Contains("check") && !act.Contains("rm_"))
                                                act += "rm_";
                                        }
                                    }
                                    else if (tAct.Contains("check") && !act.Contains("tm_"))
                                        act += "tm_";
                                }
                            }
                            else if (fAct.Contains("check") && !act.Contains("fm_"))
                                act += "fm_";
                            else if (fAct == "call" && !act.Contains("fcd_"))
                                act += "fcd_";
                            else if (fAct == "raise" && !act.Contains("frd_"))
                                act += "frd_";
                        }
                        else if (pAct.Contains("call")) //pAct == "cold call" || pAct == "call 3bet" || pAct == "call 4bet")
                        {
                            if (!act.Contains("fd_") && table.Rows[r]["Pos"].ToString() == "OOP" && fAct.Contains("bet"))
                                act += "fd_";
                            else if ((fAct == "call" || fAct == "check, call"))
                            {
                                if  (!act.Contains("fcco_"))
                                    act += "fcco_";
                                if ((tAct == "call" || tAct == "check, call"))
                                {
                                    if (!act.Contains("tcco_"))
                                        act += "tcco_";
                                    if ((rAct == "call" || rAct == "check, call"))
                                    {
                                        if (!act.Contains("rcco_"))
                                            act += "rcco_";
                                    }
                                    else if (rAct == "bet" && table.Rows[r]["Pos"].ToString() == "IP" && !act.Contains("rbm_"))
                                        act += "rbm_";
                                }
                                else if (tAct == "bet" && table.Rows[r]["Pos"].ToString() == "IP" && !act.Contains("tbm_"))
                                    act += "tbm_";
                            }
                            else if (fAct == "bet" && table.Rows[r]["Pos"].ToString() == "IP" && !act.Contains("fbm_"))
                                act += "fbm_";
                            else if ((fAct == "raise" || fAct == "check, raise") && !act.Contains("frco_"))
                                act += "frco_";
                        }

                        if ((fAct == "bet" || fAct.Contains("bet,")) && !act.Contains("fb_"))
                            act += "fb_";
                        else if ((fAct == "call" || fAct == "check, call") && !act.Contains("fc_"))
                            act += "fc_";
                        else if (fAct == "check" && !act.Contains("fx_"))
                            act += "fx_";
                        else if ((fAct == "raise" || fAct == "check, raise") && !act.Contains("fr_"))
                            act += "fr_";
                        else if (fAct == "call raise" && !act.Contains("fcr_"))
                            act += "fcr_";
                        else if (fAct == "3bet" && !act.Contains("f3_"))
                            act += "f3_";

                        if (tAct.Contains("bet"))
                        {
                            if (fAct == "bet" && !act.Contains("tbb_"))
                                act += "tbb_";
                            if ((fAct == "call" || fAct == "check, call") && !act.Contains("tcb_"))
                                act += "tcb_";
                            else if (fAct == "check" && !act.Contains("txb_"))
                                act += "txb_";
                        }
                        else if (tAct == "call" || tAct == "check, call")
                        {
                            if ((fAct == "call" || fAct == "check, call") && !act.Contains("tcc_"))
                                act += "tcc_";
                            if (fAct == "check" && !act.Contains("txc_"))
                                act += "txc_";
                        }
                        if (tAct == "check" || tAct == "check, call")
                        {
                            if (!act.Contains("tx_"))
                                act += "tx_";
                            if ((fAct == "call" || fAct == "check, call") && !act.Contains("tcx_"))
                                act += "tcx_";
                            else if (fAct == "check" && !act.Contains("txx_"))
                                act += "txx_";
                        }
                        else if ((tAct == "raise" || tAct == "check, raise") && !act.Contains("tr_"))
                            act += "tr_";
                        else if (tAct == "call raise" && !act.Contains("tcr_"))
                            act += "tcr_";

                        if (rAct.Contains("bet"))
                        {
                            if (!act.Contains("rb_"))
                                act += "rb_";
                            if (tAct == "bet" && !act.Contains("rbb_"))
                                act += "rbb_";
                            else if ((tAct == "call" || tAct == "check, call") && !act.Contains("rcb_"))
                                act += "rcb_";
                            else if (tAct == "check" && !act.Contains("rxb_"))
                                act += "rxb_";
                        }
                        else if (rAct == "call" || rAct == "check, call")
                        {
                            if (!act.Contains("rc_"))
                                act += "rc_";
                            if ((tAct == "call" || tAct == "check, call") && !act.Contains("rcc_"))
                                act += "rcc_";
                            else if (tAct == "check" && !act.Contains("rxc_"))
                                act += "rxc_";
                        }
                        if (rAct == "check" || rAct == "check, call")
                        {
                            if (!act.Contains("rx_"))
                                act += "rx_";
                            else if ((tAct == "call" || tAct == "check, call") && !act.Contains("rcx_"))
                                act += "rcx_";
                            else if (tAct == "check" && !act.Contains("rxx_"))
                                act += "rxx_";
                        }
                        else if ((rAct == "raise" || rAct == "check, raise") && !act.Contains("rr_"))
                            act += "rr_";
                        else if (rAct == "call raise" && !act.Contains("rcr_"))
                            act += "rcr_";
 
 
                        cmd2.Parameters["Acts"].Value = act;
                        cmd2.ExecuteNonQuery();

                    }//for (int r = 0; r < table.Rows.Count; r++)

                    for (int r = 0; r < tablePush.Rows.Count; r++)
                    {
                        cmd2.CommandText = "INSERT INTO preflopPush VALUES (";
                        for (int c = 0; c < tablePush.Columns.Count; c++)
                        {
                            string type = tablePush.Columns[c].DataType.Name;
                            if (type == "Int32")
                            {
                                string s = tablePush.Rows[r][c].ToString();
                                if (s == "")
                                    s = "0";
                                cmd2.CommandText += s + ",";
                            }
                            else
                                cmd2.CommandText += "'" + tablePush.Rows[r][c] + "',";
                        }
                        cmd2.CommandText = cmd2.CommandText.Substring(0, cmd2.CommandText.Length - 1) + ")";
                        try
                        {
                            cmd2.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            if (e.Message.Contains("23505"))
                                duplicates += 1;
                            else
                            {
                                errors += 1;
                                Program.Log(e, connString + "\r\n" + handText + "\r\n");
                            }
                            continue;
                        }
                        count += 1;

                        cmd2.Parameters["@Name"].Value = tablePush.Rows[r]["Name"].ToString();
                        cmd2.CommandText = "SELECT acts FROM results WHERE name = @Name";
                        NpgsqlDataReader reader2 = cmd2.ExecuteReader();
                        string act = "";
                        if (reader2.Read())
                        {
                            act = reader2["acts"].ToString();
                            cmd2.CommandText = "UPDATE Results SET acts = @Acts WHERE name = @Name;";
                            reader2.Close();
                        }
                        else
                        {
                            cmd2.CommandText = "INSERT INTO Results VALUES (@Name, @Acts);";
                        }

                        string pAct = tablePush.Rows[r]["PreflopAct"].ToString();
                        if (pAct == "call" && !act.Contains("ac_"))
                            act += "ac_";
                        if (pAct == "openraise" && !act.Contains("ao_"))
                            act += "ao_";
                        else if (pAct == "raise" && !act.Contains("ar_"))
                            act += "ar_";
                        else if (pAct == "reraise" && !act.Contains("arr_"))
                            act += "arr_";
                        else
                            act += "";

                        cmd2.Parameters["Acts"].Value = act;
                        cmd2.ExecuteNonQuery();

                    }//for (int r = 0; r < tablePush.Rows.Count; r++)

                }//while (reader.Read())

            }//using (NpgsqlDataReader reader = cmd.ExecuteReader())

            System.IO.File.AppendAllText("log.txt", "====================" + DateTime.Now + "========================\r\n" + "Completed. All hands " + countAll.ToString() + ". Loaded " + count.ToString() + ". Errors " + errors.ToString() + ". Duplicates " + duplicates.ToString() + "\r\n===============================================================\r\n");

            if (countAll > 0)
            {
                mainForm.BeginInvoke(new Action(delegate()
                {
                    mainForm.Text = "Completed. All hands " + countAll.ToString() + ". Loaded " + count.ToString() + ". Errors " + errors.ToString() + ". Duplicates " + duplicates.ToString();  
                }));
            }
            else 
            {
                mainForm.BeginInvoke(new Action(delegate()
                {
                    mainForm.Text = formText;
                }));
            }
  
            Properties.Settings.Default.Save();

                        timer.Start();

        }

        private void ReadHandStars(String handText)
        {
            if (handText.Contains(" Tournament "))
            {
                ReadHandStarsTournament(handText);
                return;
            }
            
            System.Collections.Hashtable names = new System.Collections.Hashtable();
            string[] lines = System.Text.RegularExpressions.Regex.Split(handText, "\r\n");
            if (lines.Count() < 3)
                lines = System.Text.RegularExpressions.Regex.Split(handText, "\n");
            int r = table.Rows.Count - 1;
            int preflopPosition = 1;
            int playersCount = 0;
            string name = "";

            foreach (string line in lines)
            {
                if (line.Contains(": shows ["))
                {
                    name = line.Substring(0, line.IndexOf(":"));
                    if (names[name] == null)
                    {
                        r += 1;
                        table.Rows.Add();
                        table.Rows[r]["id"] = handText.Substring(handText.IndexOf("Hand #") + 6, 12) + " " + name;
                        table.Rows[r]["hand4"] = line.Substring(line.IndexOf(": shows [") + 9, 5);
                        table.Rows[r]["name"] = name;
                        names.Add(name, r);
                    }
                }
                else if (line.Contains(" mucked ["))
                {
                    name = line.Substring(line.IndexOf(":") + 2);
                    name = name.Substring(0, name.IndexOf(" "));
                    if (names[name] == null)
                    {
                        r += 1;
                        table.Rows.Add();
                        table.Rows[r]["id"] = handText.Substring(handText.IndexOf("Hand #") + 6, 12) + " " + name;
                        table.Rows[r]["hand4"] = line.Substring(line.IndexOf(" mucked [") + 9, 5);
                        table.Rows[r]["name"] = name;
                        names.Add(name, r);
                    }
                }
                else if (line.Contains(" in chips)"))
                {
                    preflopPosition += 1;
                    playersCount += 1;
                }
            }
            if (names.Count == 0)
                return;

            string previosAction = "";
            string flopActions = "";
            string turnActions = "";
            string riverActions = "";
            string[] pocketHand = { "", "" };
            string street = "";
            string board = "";
            decimal bb = 0;
            decimal bank = 0;
            decimal streetStartBank = 0;
            decimal bet = 0;
            bool push = true;
            foreach (string line in lines)
            {
                if (line == "*** HOLE CARDS ***")
                {
                    street = "preflop";
                    continue;
                }
                else if (line.Contains("*** FLOP ***"))
                {
                    street = "flop";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board = line.Substring(13);
                }
                else if (line.Contains("*** TURN ***"))
                {
                    street = "turn";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board = line.Substring(13);
                    for (int r1 = 0; r1 < table.Rows.Count; r1++)
                        if (table.Rows[r1]["flopAct"].ToString() != "")
                            push = false;
                }
                else if (line.Contains("*** RIVER ***"))
                {
                    street = "river";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board = line.Substring(14);
                }
                else if (line == "*** SUMMARY ***")
                    street = "SUMMARY";

                if (street == "" && line.Contains(" posts "))
                {
                    if (line.Contains(" posts big blind "))
                    {
                        if (line.Contains(" and "))
                            bb = Convert.ToDecimal(line.Substring(line.IndexOf(" posts big blind ") + 18, line.IndexOf(" and ") - (line.IndexOf(" posts big blind ") + 18)));
                        else
                            bb = Convert.ToDecimal(line.Substring(line.IndexOf(" posts big blind ") + 18));
                        bank += bb;
                    }
                    else if (line.Contains(" posts small blind "))
                    {
                        bank += Convert.ToDecimal(line.Substring(line.IndexOf(" posts small blind ") + 20));
                    }
                }

                #region PREFLOP
                if (street == "preflop")
                {
                    if (line.Contains(" calls ") || line.Contains(" raises ") || line.Contains(" checks") || line.Contains(" folds"))
                        preflopPosition -= 1;
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    if (line.Contains(" calls "))
                    {
                        if (previosAction == "")
                            previosAction = "limp";
                        else if (previosAction == "openraise" || previosAction == "raise")
                            previosAction = "cold call";
                        else if (previosAction == "3bet")
                            previosAction = "call 3bet";
                        else if (previosAction == "4bet")
                            previosAction = "call 4bet";
                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8, line.IndexOf(" and ") - line.IndexOf(" calls ") - 8));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8));
                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        if (previosAction == "")
                            previosAction = "openraise";
                        else if (previosAction == "limp")
                            previosAction = "raise";
                        else if (previosAction == "openraise" || previosAction == "raise" || previosAction == "cold call")
                            previosAction = "3bet";
                        else if (previosAction == "3bet" || previosAction == "call 3bet")
                            previosAction = "4bet";
                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.IndexOf(" and ") - line.IndexOf(" to ") - 5));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.Length - line.IndexOf(" to ") - 5));
                        bank += bet;
                    }
                    else if (line.Contains(" checks"))
                    {
                        previosAction = "check";
                    }
                    if (line.Contains(" calls ") || line.Contains(" raises ") || line.Contains(" checks"))
                    {
                        name = line.Substring(0, line.IndexOf(":"));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["preflopAct"].ToString() == "limp")
                            {
                                if (line.Contains(" raises "))
                                    table.Rows[r]["preflopAct"] = "limp, raise";
                                else
                                    table.Rows[r]["preflopAct"] = "limp, call";
                            }
                            else
                                table.Rows[r]["preflopAct"] = previosAction;
                            if (table.Rows[r]["preflopBet"].ToString() != "")
                                table.Rows[r]["preflopBet"] = (decimal)table.Rows[r]["preflopBet"] + bet / bb;
                            else
                                table.Rows[r]["preflopBet"] = bet / bb;

                            if (table.Rows[r]["preflopPos"].ToString() == "")
                            {
                                if (preflopPosition == 1)
                                    table.Rows[r]["preflopPos"] = "BB";
                                else if (preflopPosition == 2)
                                    table.Rows[r]["preflopPos"] = "SB";
                                else if (preflopPosition == 3)
                                    table.Rows[r]["preflopPos"] = "BU";
                                else if (preflopPosition == 4)
                                    table.Rows[r]["preflopPos"] = "CO";
                                else if (preflopPosition == 5)
                                    table.Rows[r]["preflopPos"] = "MP3";
                                else if (preflopPosition == 6)
                                    table.Rows[r]["preflopPos"] = "MP2";
                                else if (preflopPosition == 7)
                                    table.Rows[r]["preflopPos"] = "MP1";
                                else if (preflopPosition == 8)
                                    table.Rows[r]["preflopPos"] = "UTG";
                                else if (preflopPosition == 9)
                                    table.Rows[r]["preflopPos"] = "UTG";
                            }
                        }
                    }
                }//if (street == "preflop")
                #endregion

                #region FLOP
                else if (street == "flop")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 7, line.IndexOf(" and ") - line.IndexOf(" bets ") - 7));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 7));
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise" || previosAction == "3bet")
                            previosAction = "call raise";
                        else
                            previosAction = "call";

                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8, line.IndexOf(" and ") - line.IndexOf(" calls ") - 8));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8, line.Length - line.IndexOf(" calls ") - 8));
                         bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        if (previosAction == "raise")
                            previosAction = "3bet";
                        else
                            previosAction = "raise";

                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.IndexOf(" and ") - line.IndexOf(" to ") - 5));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.Length - line.IndexOf(" to ") - 5));

                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls "))
                    {
                        name = line.Substring(0, line.IndexOf(":"));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["flopAct"].ToString() == "check")
                            {
                                table.Rows[r]["Pos"] = "OOP";
                                table.Rows[r]["flopAct"] += ", " + previosAction;
                            }
                            else if (table.Rows[r]["FlopAct"].ToString() == "bet")
                                table.Rows[r]["FlopAct"] += ", " + previosAction;
                            else
                            {
                                if (flopActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!flopActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                                table.Rows[r]["flopAct"] = previosAction;
                            }
                            table.Rows[r]["flopBet"] = Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["flopPlayersCount"] = playersCount;
                        }
                    }
                    flopActions += previosAction;
                }//if (street == "flop")
                #endregion

                #region TURN
                else if (street == "turn")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 7, line.IndexOf(" and ") - line.IndexOf(" bets ") - 7));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 7));
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";

                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8, line.IndexOf(" and ") - line.IndexOf(" calls ") - 8));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8, line.Length - line.IndexOf(" calls ") - 8));

                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        if (previosAction == "raise")
                            previosAction = "3bet";
                        else
                            previosAction = "raise";

                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.IndexOf(" and ") - line.IndexOf(" to ") - 5));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.Length - line.IndexOf(" to ") - 5));
                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls "))
                    {
                        name = line.Substring(0, line.IndexOf(":"));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["turnAct"].ToString() == "check")
                            {
                                table.Rows[r]["Pos"] = "OOP";
                                table.Rows[r]["turnAct"] += ", " + previosAction;
                            }
                            else if (table.Rows[r]["turnAct"].ToString() == "bet")
                                table.Rows[r]["turnAct"] += ", " + previosAction;
                            else
                            {
                                if (turnActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!turnActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                                table.Rows[r]["turnAct"] = previosAction;
                            }
                            table.Rows[r]["turnBet"] = Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["turnPlayersCount"] = playersCount;
                        }
                    }
                    turnActions += previosAction;
                }//if (street == "turn")
                #endregion

                #region RIVER
                else if (street == "river")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 7, line.IndexOf(" and ") - line.IndexOf(" bets ") - 7));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 7));
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";

                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8, line.IndexOf(" and ") - line.IndexOf(" calls ") - 8));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8, line.Length - line.IndexOf(" calls ") - 8));

                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        if (previosAction == "raise")
                            previosAction = "3bet";
                        else
                            previosAction = "raise";

                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.IndexOf(" and ") - line.IndexOf(" to ") - 5));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.Length - line.IndexOf(" to ") - 5));

                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls "))
                    {
                        name = line.Substring(0, line.IndexOf(":"));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["riverAct"].ToString() == "check")
                            {
                                table.Rows[r]["Pos"] = "OOP";
                                table.Rows[r]["riverAct"] += ", " + previosAction;
                            }
                            else if (table.Rows[r]["riverAct"].ToString() == "bet")
                                table.Rows[r]["riverAct"] += ", " + previosAction;
                            else
                            {
                                if (riverActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!riverActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                                table.Rows[r]["riverAct"] = previosAction;
                            }
                            table.Rows[r]["riverBet"] = Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["riverPlayersCount"] = playersCount;
                        }
                    }
                    riverActions += previosAction;
                }//if (street == "river")
                #endregion

            }//foreach (string line in lines)

            board = board.Replace("[", "");
            board = board.Replace("]", "");
           
            int iComb = 0;

            foreach (System.Collections.DictionaryEntry element in names)
            {
                name = (string)element.Key;
                r = (int)names[name];
                string hand = (string)table.Rows[r]["hand4"];
                pocketHand = System.Text.RegularExpressions.Regex.Split(hand, " ");
                if (hand.Substring(1, 1) != hand.Substring(4, 1) && hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "o";
                else if (hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "s";
                else
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1);
                if (hand[0].ToString() != hand[1].ToString())
                {
                    string card1 = hand[0].ToString();
                    card1 = card1.Replace("A", "14"); card1 = card1.Replace("K", "13"); card1 = card1.Replace("Q", "12"); card1 = card1.Replace("J", "11"); card1 = card1.Replace("T", "10");
                    string card2 = hand[1].ToString();
                    card2 = card2.Replace("A", "14"); card2 = card2.Replace("K", "13"); card2 = card2.Replace("Q", "12"); card2 = card2.Replace("J", "11"); card2 = card2.Replace("T", "10");
                    if (Convert.ToInt32(card1) < Convert.ToInt32(card2))
                        hand = hand.Substring(1, 1) + hand.Substring(0, 1) + hand.Substring(2, 1);
                }
                table.Rows[r]["hand3"] = hand;
                table.Rows[r]["board"] = board;

                string combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 8), " "));
                string[] array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                if (array.Count() == 1)
                    iComb = Convert.ToInt32(array[0]);
                else
                {
                    iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                    table.Rows[r]["flopComb"] = array[0];
                }
                table.Rows[r]["iFlopComb"] = iComb;

                if (board.Length >= 11)
                {
                    combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 11), " "));
                    array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                    if (array.Count() == 1)
                        iComb = Convert.ToInt32(array[0]);
                    else
                    {
                        iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                        table.Rows[r]["turnComb"] = array[0];
                    }
                    table.Rows[r]["iTurnComb"] = iComb;
                }

                if (board.Length >= 14)
                {
                    combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 14), " "));
                    array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                    if (array.Count() == 1)
                        iComb = Convert.ToInt32(array[0]);
                    else
                    {
                        iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                        table.Rows[r]["riverComb"] = array[0];
                    }
                    table.Rows[r]["iRiverComb"] = iComb;
                }
            }

            //if (push)
            //    ReadHandPushStars(lines, bb);
        }  

        private void ReadHandStarsTournament(String handText)
        {

//            handText = @"PokerStars Hand #180863938593: Tournament #2182809293, $14.69+$0.31 USD Hold'em No Limit - Match Round I, Level I (10/20) - 2018/01/15 3:42:12 MSK [2018/01/15 19:42:12 ET]
//Table '21828092931' 2-max Seat #1 is the button
//Seat 1: mc'goyo (330 in chips)
//Seat 2: Taburet (670 in chips)
//mc'goyo: posts small blind 10
//Taburet: posts big blind 20
//*** HOLE CARDS ***
//Dealt to Taburet [Kc 9d]
//mc'goyo: raises 310 to 330 and is all-in
//Taburet: calls 310
//*** FLOP *** [6h Ad Jc]
//*** TURN *** [6h Ad Jc] [Js]
//*** RIVER *** [6h Ad Jc Js] [2c]
//*** SHOW DOWN ***
//Taburet: shows [Kc 9d] (a pair of Jacks)
//mc'goyo: shows [Ks 4s] (a pair of Jacks - lower kicker)
//Taburet collected 660 from pot
//mc'goyo finished the tournament in 2nd place
//Taburet wins the tournament and receives $29.38 - congratulations!
//*** SUMMARY ***
//Total pot 660 | Rake 0
//Board [6h Ad Jc Js 2c]
//Seat 1: mc'goyo (button) (small blind) showed [Ks 4s] and lost with a pair of Jacks
//Seat 2: Taburet (big blind) showed [Kc 9d] and won (660) with a pair of Jacks
//";
            
            System.Collections.Hashtable names = new System.Collections.Hashtable();
            string[] lines = System.Text.RegularExpressions.Regex.Split(handText, "\r\n");
            if (lines.Count() < 3)
                lines = System.Text.RegularExpressions.Regex.Split(handText, "\n");
            int r = table.Rows.Count - 1;
            int preflopPosition = 1;
            int playersCount = 0;
            string name = "";

            foreach (string line in lines)
            {
                if (line.Contains(": shows ["))
                {
                    name = line.Substring(0, line.IndexOf(":"));
                    if (names[name] == null)
                    {
                        r += 1;
                        table.Rows.Add();
                        table.Rows[r]["id"] = handText.Substring(handText.IndexOf("Hand #") + 6, 12) + " " + name;
                        table.Rows[r]["hand4"] = line.Substring(line.IndexOf(": shows [") + 9, 5);
                        table.Rows[r]["name"] = name;
                        names.Add(name, r);
                    }
                }
                if (line.Contains(" in chips"))
                {
                    preflopPosition += 1;
                    playersCount += 1;
                }
            }
            if (names.Count == 0)
                return;

            string previosAction = "";
            string flopActions = "";
            string turnActions = "";
            string riverActions = "";
            string[] pocketHand = { "", "" };
            string street = "";
            string board = "";
            decimal bb = 0;
            decimal bank = 0;
            decimal streetStartBank = 0;
            decimal bet = 0;
            bool push = true;
            foreach (string line in lines)
            {
                if (line == "*** HOLE CARDS ***")
                {
                    street = "preflop";
                    continue;
                }
                else if (line.Contains("*** FLOP ***"))
                {
                    street = "flop";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board = line.Substring(13);
                }
                else if (line.Contains("*** TURN ***"))
                {
                    street = "turn";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board = line.Substring(13);
                    for (int r1 = 0; r1 < table.Rows.Count; r1++)
                        if (table.Rows[r1]["flopAct"].ToString() != "")
                            push = false;
                }
                else if (line.Contains("*** RIVER ***"))
                {
                    street = "river";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board = line.Substring(14);
                }
                else if (line == "*** SUMMARY ***")
                    street = "SUMMARY";

                if (street == "" && line.Contains(" posts "))
                {
                    if (line.Contains(" posts big blind "))
                    {
                        bb = Convert.ToDecimal(line.Substring(line.IndexOf(" posts big blind ") + 17).Replace("and is all-in", ""));
                        bank += bb;
                    }
                    else if (line.Contains(" posts small blind "))
                    {
                        bank += Convert.ToDecimal(line.Substring(line.IndexOf(" posts small blind ") + 19).Replace("and is all-in", ""));
                    }
                    else if (line.Contains(" posts the ante "))
                    {
                        bank += Convert.ToDecimal(line.Substring(line.IndexOf(" posts the ante ") + 16).Replace("and is all-in", ""));
                    }
                }

                #region PREFLOP
                if (street == "preflop")
                {
                    if (line.Contains(" calls ") || line.Contains(" raises ") || line.Contains(" checks") || line.Contains(" folds"))
                        preflopPosition -= 1;
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    if (line.Contains(" calls "))
                    {
                        if (previosAction == "")
                            previosAction = "limp";
                        else if (previosAction == "openraise" || previosAction == "raise")
                            previosAction = "cold call";
                        else if (previosAction == "3bet")
                            previosAction = "call 3bet";
                        else if (previosAction == "4bet")
                            previosAction = "call 4bet";
                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 7, line.IndexOf(" and ") - line.IndexOf(" calls ") - 6));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 7));
                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        if (previosAction == "")
                            previosAction = "openraise";
                        else if (previosAction == "limp")
                            previosAction = "raise";
                        else if (previosAction == "openraise" || previosAction == "raise" || previosAction == "cold call")
                            previosAction = "3bet";
                        else if (previosAction == "3bet" || previosAction == "call 3bet")
                            previosAction = "4bet";
                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 4, line.IndexOf(" and ") - line.IndexOf(" to ") - 4));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 4));
                        bank += bet;
                    }
                    else if (line.Contains(" checks"))
                    {
                        previosAction = "check";
                    }
                    if (line.Contains(" calls ") || line.Contains(" raises ") || line.Contains(" checks"))
                    {
                        name = line.Substring(0, line.IndexOf(":"));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["preflopAct"].ToString() == "limp")
                            {
                                if (line.Contains(" raises "))
                                    table.Rows[r]["preflopAct"] = "limp, raise";
                                else
                                    table.Rows[r]["preflopAct"] = "limp, call";
                            }
                            else
                                table.Rows[r]["preflopAct"] = previosAction;

                            if (table.Rows[r]["preflopBet"].ToString() != "")
                                table.Rows[r]["preflopBet"] = (decimal)table.Rows[r]["preflopBet"] + Math.Round(bet / bb, 1);
                            else
                                table.Rows[r]["preflopBet"] = Math.Round(bet / bb, 1);
      
                            if (table.Rows[r]["preflopPos"].ToString() == "")
                            {
                                if (preflopPosition == 1)
                                {
                                    table.Rows[r]["preflopPos"] = "BB";
                                    if (table.Rows[r]["preflopAct"].ToString() == "openraise") //отладка
                                        table.Rows[r]["preflopPos"] = "BB";
                                }
                                else if (preflopPosition == 2)
                                    table.Rows[r]["preflopPos"] = "SB";
                                else if (preflopPosition == 3)
                                    table.Rows[r]["preflopPos"] = "BU";
                                else if (preflopPosition == 4)
                                    table.Rows[r]["preflopPos"] = "CO";
                                else if (preflopPosition == 5)
                                    table.Rows[r]["preflopPos"] = "MP3";
                                else if (preflopPosition == 6)
                                    table.Rows[r]["preflopPos"] = "MP2";
                                else if (preflopPosition == 7)
                                    table.Rows[r]["preflopPos"] = "MP1";
                                else if (preflopPosition == 8)
                                    table.Rows[r]["preflopPos"] = "UTG";
                                else if (preflopPosition == 9)
                                    table.Rows[r]["preflopPos"] = "UTG";
                            }
                        }
                    }
                }//if (street == "preflop")
                #endregion

                #region FLOP
                else if (street == "flop")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 6, line.IndexOf(" and ") - line.IndexOf(" bets ") - 6));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 6));
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";

                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 7, line.IndexOf(" and ") - line.IndexOf(" calls ") - 7));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 7));
                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        if (previosAction == "raise")
                            previosAction = "3bet";
                        else
                            previosAction = "raise";

                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 4, line.IndexOf(" and ") - line.IndexOf(" to ") - 4));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 4));

                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls "))
                    {
                        name = line.Substring(0, line.IndexOf(":"));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["flopAct"].ToString() == "check")
                            {
                                table.Rows[r]["Pos"] = "OOP";
                                table.Rows[r]["flopAct"] += ", " + previosAction;
                            }
                            else if (table.Rows[r]["flopAct"].ToString() == "bet")
                                table.Rows[r]["flopAct"] += ", " + previosAction;
                            else
                            {
                                if (flopActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!flopActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                                table.Rows[r]["flopAct"] = previosAction;
                            }
                            table.Rows[r]["flopBet"] = Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["flopPlayersCount"] = playersCount;
                        }
                    }
                    flopActions += previosAction;
                }//if (street == "flop")
                #endregion

                #region TURN
                else if (street == "turn")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 6, line.IndexOf(" and ") - line.IndexOf(" bets ") - 6));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 6));
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";

                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 7, line.IndexOf(" and ") - line.IndexOf(" calls ") - 7));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 7));

                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        if (previosAction == "raise")
                            previosAction = "3bet";
                        else
                            previosAction = "raise";

                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 4, line.IndexOf(" and ") - line.IndexOf(" to ") - 4));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 4));
                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls "))
                    {
                        name = line.Substring(0, line.IndexOf(":"));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["turnAct"].ToString() == "check")
                            {
                                table.Rows[r]["Pos"] = "OOP";
                                table.Rows[r]["turnAct"] += ", " + previosAction;
                            }
                            else if (table.Rows[r]["turnAct"].ToString() == "bet")
                                table.Rows[r]["turnAct"] += ", " + previosAction;
                            else
                            {
                                if (turnActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!turnActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                                table.Rows[r]["turnAct"] = previosAction;
                            }
                            table.Rows[r]["turnBet"] = Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["turnPlayersCount"] = playersCount;
                        }
                    }
                    turnActions += previosAction;
                }//if (street == "turn")
                #endregion

                #region RIVER
                else if (street == "river")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 6, line.IndexOf(" and ") - line.IndexOf(" bets ") - 6));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 6));
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";

                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 7, line.IndexOf(" and ") - line.IndexOf(" calls ") - 7));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 7));

                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        if (previosAction == "raise")
                            previosAction = "3bet";
                        else
                            previosAction = "raise";

                        if (line.Contains(" and "))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 4, line.IndexOf(" and ") - line.IndexOf(" to ") - 4));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 4));

                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls "))
                    {
                        name = line.Substring(0, line.IndexOf(":"));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["riverAct"].ToString() == "check")
                            {
                                table.Rows[r]["Pos"] = "OOP";
                                table.Rows[r]["riverAct"] += ", " + previosAction;
                            }
                            else if (table.Rows[r]["riverAct"].ToString() == "bet")
                                table.Rows[r]["riverAct"] += ", " + previosAction;
                            else
                            {
                                if (riverActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!riverActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                                table.Rows[r]["riverAct"] = previosAction;
                            }
                            table.Rows[r]["riverBet"] = Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["riverPlayersCount"] = playersCount;
                        }
                    }
                    riverActions += previosAction;
                }//if (street == "river")
                #endregion

            }//foreach (string line in lines)

            board = board.Replace("[", "");
            board = board.Replace("]", "");
           
            int iComb = 0;

            foreach (System.Collections.DictionaryEntry element in names)
            {
                name = (string)element.Key;
                r = (int)names[name];
                string hand = (string)table.Rows[r]["hand4"];
                pocketHand = System.Text.RegularExpressions.Regex.Split(hand, " ");
                if (hand.Substring(1, 1) != hand.Substring(4, 1) && hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "o";
                else if (hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "s";
                else
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1);
                if (hand[0].ToString() != hand[1].ToString())
                {
                    string card1 = hand[0].ToString();
                    card1 = card1.Replace("A", "14"); card1 = card1.Replace("K", "13"); card1 = card1.Replace("Q", "12"); card1 = card1.Replace("J", "11"); card1 = card1.Replace("T", "10");
                    string card2 = hand[1].ToString();
                    card2 = card2.Replace("A", "14"); card2 = card2.Replace("K", "13"); card2 = card2.Replace("Q", "12"); card2 = card2.Replace("J", "11"); card2 = card2.Replace("T", "10");
                    if (Convert.ToInt32(card1) < Convert.ToInt32(card2))
                        hand = hand.Substring(1, 1) + hand.Substring(0, 1) + hand.Substring(2, 1);
                }
                table.Rows[r]["hand3"] = hand;
                table.Rows[r]["board"] = board;

                string combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 8), " "));
                string[] array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                if (array.Count() == 1)
                    iComb = Convert.ToInt32(array[0]);
                else
                {
                    iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                    table.Rows[r]["flopComb"] = array[0];
                }
                table.Rows[r]["iFlopComb"] = iComb;
 
                if (board.Length >= 11)
                {
                    combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 11), " "));
                    array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                    if (array.Count() == 1)
                        iComb = Convert.ToInt32(array[0]);
                    else
                    {
                        iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                        table.Rows[r]["turnComb"] = array[0];
                    }
                    table.Rows[r]["iTurnComb"] = iComb;
                }

                if (board.Length >= 14)
                {
                    combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 14), " "));
                    array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                    if (array.Count() == 1)
                        iComb = Convert.ToInt32(array[0]);
                    else
                    {
                        iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                        table.Rows[r]["riverComb"] = array[0];
                    }
                    table.Rows[r]["iRiverComb"] = iComb;
                }
            }

            if (push)
                ReadHandPushStars(lines, bb);
        }

        private void ReadHand888(String handText)
        {
            if (handText.Contains("Tournament "))
            {
                ReadHand888Tournament(handText);
                return;
            }
                        
            System.Collections.Hashtable names = new System.Collections.Hashtable();
            string[] lines = System.Text.RegularExpressions.Regex.Split(handText, "\r\n");
            if (lines.Count() < 3)
                lines = System.Text.RegularExpressions.Regex.Split(handText, "\n");
            int r = table.Rows.Count - 1;
            int preflopPosition = 0;
            int playersCount = 6;
            string name = "";

            foreach (string line in lines)
            {
                if (line.Contains(" shows [") || line.Contains(" mucks ["))
                {
                    if (line.Contains(" shows ["))
                        name = line.Substring(0, line.IndexOf(" shows ["));
                    else
                        name = line.Substring(0, line.IndexOf(" mucks ["));
                    name = name.ToLower();
                    if (names[name] == null)
                    {
                        r += 1;
                        table.Rows.Add();
                        table.Rows[r]["ID"] = handText.Substring(handText.IndexOf(" for Game") + 10, 9) + " " + name;
                        if (line.Contains(" shows ["))
                            table.Rows[r]["Hand4"] = line.Substring(line.IndexOf(" shows [") + 9, 6);
                        else
                            table.Rows[r]["Hand4"] = line.Substring(line.IndexOf(" mucks [") + 9, 6);
                        table.Rows[r]["Hand4"] = table.Rows[r]["Hand4"].ToString().Replace(",", "");
                        table.Rows[r]["Name"] = name;
                        table.Rows[r]["FlopBet"] = 0;
                        table.Rows[r]["TurnBet"] = 0;
                        table.Rows[r]["RiverBet"] = 0;
                        names.Add(name, r);
                    }
                }
                else if (line.Contains("Total number of players :"))
                {
                    playersCount = Convert.ToInt32(line.Substring(26));
                    preflopPosition = playersCount + 1;
                }
            }
            if (names.Count == 0)
                return;

            string previosAction = "";
            string flopActions = "";
            string turnActions = "";
            string riverActions = "";
            string[] pocketHand = { "", "" };
            string street = "";
            string board = "";
            decimal bb = 0;
            decimal bank = 0;
            decimal streetStartBank = 0;
            decimal bet = 0;
            bool push = true;
            foreach (string line in lines)
            {
                if (line.Contains("** Dealing down cards **"))
                {
                    street = "preflop";
                    continue;
                }
                else if (line.Contains("** Dealing flop "))
                {
                    street = "flop";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board = line.Substring(line.IndexOf("[")+2, 10).Replace(",", "");
                }
                else if (line.Contains("** Dealing turn "))
                {
                    street = "turn";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board += " " + line.Substring(line.IndexOf("[") + 2, 2);
                    for (int r1 = 0; r1 < table.Rows.Count; r1++)
                        if (table.Rows[r1]["flopAct"].ToString() != "")
                            push = false;
                }
                else if (line.Contains("** Dealing river "))
                {
                    street = "river";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board += " " + line.Substring(line.IndexOf("[") + 2, 2);
                }
                else if (line == "** Summary **")
                    street = "SUMMARY";

                if (street == "" && line.Contains(" posts "))
                {
                    if (line.Contains(" posts big blind "))
                    {
                        if (line.Contains(" + dead "))
                            bb = Convert.ToDecimal(line.Substring(line.IndexOf(" posts big blind ") + 25).Replace('.', ',').Replace("]", "").Replace("$", "").Trim());
                        else
                            bb = Convert.ToDecimal(line.Substring(line.IndexOf(" posts big blind ") + 19).Replace('.', ',').Replace("]", "").Replace("$", "").Trim());
                        bank += bb;
                    }
                    else if (line.Contains(" posts small blind "))
                    {
                        bank += Convert.ToDecimal(line.Substring(line.IndexOf(" posts small blind ") + 20).Replace('.', ',').Replace("]", "").Replace("$", "").Trim());
                    }
                    else if (line.Contains(" posts ante "))
                    {
                        bank += Convert.ToDecimal(line.Substring(line.IndexOf(" posts ante ") + 14).Replace('.', ',').Replace("]", "").Replace("$", "").Trim());
                    }
                }

                #region PREFLOP
                if (street == "preflop")
                {
                    if (line.Contains(" calls ") || line.Contains(" raises ") || line.Contains(" checks") || line.Contains(" folds"))
                        preflopPosition -= 1;
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    if (line.Contains(" calls "))
                    {
                        if (previosAction == "")
                            previosAction = "limp";
                        else if (previosAction == "raise" || previosAction == "openraise")
                            previosAction = "cold call";
                        else if (previosAction == "3bet")
                            previosAction = "call 3bet";
                        else if (previosAction == "4bet")
                            previosAction = "call 4bet";
                        if (line.Contains(" all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8, line.IndexOf(" and ") - line.IndexOf(" calls ") - 8).Replace('.', ',').Replace("$", "").Trim());
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8).Replace("$", "").Replace("]", "").Replace('.', ',').Trim());
                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        if (previosAction == "")
                            previosAction = "openraise";
                        if (previosAction == "limp")
                            previosAction = "raise";
                        else if (previosAction == "raise" || previosAction == "cold call")
                            previosAction = "3bet";
                        else if (previosAction == "3bet" || previosAction == "call 3bet")
                            previosAction = "4bet";
                        if (line.Contains(" all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.IndexOf(" and ") - line.IndexOf(" to ") - 5).Replace('.', ',').Replace("$", "").Trim());
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 10).Replace("]", "").Replace('.', ',').Replace("$", "").Trim());
                        bank += bet;
                    }
                    else if (line.Contains(" checks"))
                    {
                        previosAction = "check";
                    }
                    if (line.Contains(" calls ") || line.Contains(" raises ") || line.Contains(" checks"))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        name = name.ToLower();
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["PreflopAct"].ToString() == "limp" && line.Contains(" raises "))
                                table.Rows[r]["PreflopAct"] = "limp, raise";
                            else
                                table.Rows[r]["PreflopAct"] = previosAction;
                            table.Rows[r]["PreflopBet"] = bet / bb;

                            if (table.Rows[r]["PreflopPos"].ToString() == "")
                            {
                                if (preflopPosition == 1)
                                    table.Rows[r]["PreflopPos"] = "BB";
                                else if (preflopPosition == 2)
                                    table.Rows[r]["PreflopPos"] = "SB";
                                else if (preflopPosition == 3)
                                    table.Rows[r]["PreflopPos"] = "BU";
                                else if (preflopPosition == 4)
                                    table.Rows[r]["PreflopPos"] = "CO";
                                else if (preflopPosition == 5)
                                    table.Rows[r]["PreflopPos"] = "MP3";
                                else if (preflopPosition == 6)
                                    table.Rows[r]["PreflopPos"] = "MP2";
                                else if (preflopPosition == 7)
                                    table.Rows[r]["PreflopPos"] = "MP1";
                                else if (preflopPosition == 8)
                                    table.Rows[r]["PreflopPos"] = "UTG";
                                else if (preflopPosition == 9)
                                    table.Rows[r]["PreflopPos"] = "UTG";
                            }
                        }
                    }
                }//if (street == "preflop")
                #endregion

                #region FLOP
                else if (street == "flop")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 7).Replace('$', ' ').Replace(']', ' ').Replace('.', ',').Replace("$", "").Trim());
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 9).Replace(']', ' ').Replace('.', ',').Replace("$", "").Trim());
                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        previosAction = "raise";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 10).Replace(']', ' ').Replace('.', ',').Replace("$", "").Trim());
                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        name = name.ToLower();
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["FlopAct"].ToString() == "check")
                            {
                                table.Rows[r]["FlopAct"] += ", " + previosAction;
                                table.Rows[r]["Pos"] = "OOP";
                            }
                            else if (table.Rows[r]["FlopAct"].ToString() == "bet")
                                table.Rows[r]["FlopAct"] += ", " + previosAction;
                            else
                            {
                                table.Rows[r]["FlopAct"] = previosAction;
                                if (flopActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!flopActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                            }
                            table.Rows[r]["FlopBet"] = (decimal)table.Rows[r]["FlopBet"] + Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["FlopPlayersCount"] = playersCount;
                        }
                    }
                    flopActions += previosAction;
                }//if (street == "flop")
                #endregion

                #region TURN
                else if (street == "turn")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        if (line.Contains("all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 7, line.IndexOf(" and ") - line.IndexOf(" bets ") - 7).Replace('.', ',').Replace("$", "").Trim());
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 8).Replace(']', ' ').Replace('.', ',').Replace("$", "").Trim());
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";

                        if (line.Contains("all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8, line.IndexOf(" and ") - line.IndexOf(" calls ") - 8).Replace('.', ',').Replace("$", "").Trim());
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 9).Replace(']', ' ').Replace('.', ',').Replace("$", "").Trim());

                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        previosAction = "raise";
                        if (line.Contains(" all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.IndexOf(" and ") - line.IndexOf(" to ") - 5).Replace('.', ',').Replace("$", "").Trim());
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 10).Replace(']', ' ').Replace('.', ',').Replace("$", "").Trim());
                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        name = name.ToLower();
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["TurnAct"].ToString() == "check")
                            {
                                table.Rows[r]["TurnAct"] += ", " + previosAction;
                                table.Rows[r]["Pos"] = "OOP";
                            }
                            else if (table.Rows[r]["TurnAct"].ToString() == "bet")
                                table.Rows[r]["TurnAct"] += ", " + previosAction;
                            else
                            {
                                table.Rows[r]["TurnAct"] = previosAction;
                                if (turnActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!turnActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                            }
                            table.Rows[r]["TurnBet"] = (decimal)table.Rows[r]["TurnBet"] + Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["TurnPlayersCount"] = playersCount;
                        }
                        turnActions += previosAction;
                    }
                }//if (street == "turn")
                #endregion

                #region RIVER
                else if (street == "river")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 8).Replace(']', ' ').Replace('.', ',').Replace("$", "").Trim());
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 9).Replace(']', ' ').Replace('.', ',').Replace("$", "").Trim());
                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        previosAction = "raise";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 10).Replace(']', ' ').Replace('.', ',').Replace("$", "").Trim());
                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        name = name.ToLower();
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["RiverAct"].ToString() == "check")
                            {
                                table.Rows[r]["RiverAct"] += ", " + previosAction;
                                table.Rows[r]["Pos"] = "OOP";
                            }
                            else if (table.Rows[r]["RiverAct"].ToString() == "bet")
                                table.Rows[r]["RiverAct"] += ", " + previosAction;
                            else
                            {
                                table.Rows[r]["RiverAct"] = previosAction;
                                if (riverActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!riverActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                            }
                            table.Rows[r]["RiverBet"] = (decimal)table.Rows[r]["RiverBet"] + Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["RiverPlayersCount"] = playersCount;
                        }
                    }
                    riverActions += previosAction;
                }//if (street == "river")
                #endregion

            }//foreach (string line in lines)
       
            int iComb = 0;

            foreach (System.Collections.DictionaryEntry element in names)
            {
                name = (string)element.Key;
                r = (int)names[name];
                string hand = (string)table.Rows[r]["Hand4"];
                pocketHand = System.Text.RegularExpressions.Regex.Split(hand, " ");
                if (hand.Substring(1, 1) != hand.Substring(4, 1) && hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "o";
                else if (hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "s";
                else
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1);
                if (hand[0].ToString() != hand[1].ToString())
                {
                    string card1 = hand[0].ToString();
                    card1 = card1.Replace("A", "14"); card1 = card1.Replace("K", "13"); card1 = card1.Replace("Q", "12"); card1 = card1.Replace("J", "11"); card1 = card1.Replace("T", "10");
                    string card2 = hand[1].ToString();
                    card2 = card2.Replace("A", "14"); card2 = card2.Replace("K", "13"); card2 = card2.Replace("Q", "12"); card2 = card2.Replace("J", "11"); card2 = card2.Replace("T", "10");
                    if (Convert.ToInt32(card1) < Convert.ToInt32(card2))
                        hand = hand.Substring(1, 1) + hand.Substring(0, 1) + hand.Substring(2, 1);
                }

                table.Rows[r]["hand3"] = hand;
                table.Rows[r]["board"] = board;

                string combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 8), " "));
                string[] array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                if (array.Count() == 1)
                    iComb = Convert.ToInt32(array[0]);
                else
                {
                    iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                    table.Rows[r]["FlopComb"] = array[0];
                }
                table.Rows[r]["iFlopComb"] = iComb;

                combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 11), " "));
                array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                if (array.Count() == 1)
                    iComb = Convert.ToInt32(array[0]);
                else
                {
                    iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                    table.Rows[r]["TurnComb"] = array[0];
                }
                table.Rows[r]["iTurnComb"] = iComb;

                combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 14), " "));
                array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                if (array.Count() == 1)
                    iComb = Convert.ToInt32(array[0]);
                else
                {
                    iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                    table.Rows[r]["RiverComb"] = array[0];
                }
                table.Rows[r]["iRiverComb"] = iComb;

            }

            if (push)
                ReadHandPush888(lines, bb);
        }

        private void ReadHand888Tournament(String handText)
        {
            System.Collections.Hashtable names = new System.Collections.Hashtable();
            string[] lines = System.Text.RegularExpressions.Regex.Split(handText, "\r\n");
            if (lines.Count() < 3)
                lines = System.Text.RegularExpressions.Regex.Split(handText, "\n");
            int r = table.Rows.Count - 1;
            int preflopPosition = 0;
            int playersCount = 6;
            string name = "";

            foreach (string line in lines)
            {
                if (line.Contains(" shows [") || line.Contains(" mucks ["))
                {
                    if (line.Contains(" shows ["))
                        name = line.Substring(0, line.IndexOf(" shows ["));
                    else
                        name = line.Substring(0, line.IndexOf(" mucks ["));
                    name = name.ToLower();
                    if (names[name] == null)
                    {
                        r += 1;
                        table.Rows.Add();
                        table.Rows[r]["ID"] = handText.Substring(handText.IndexOf(" for Game") + 10, 9) + " " + name;
                        if (line.Contains(" shows ["))
                            table.Rows[r]["Hand4"] = line.Substring(line.IndexOf(" shows [") + 9, 6);
                        else
                            table.Rows[r]["Hand4"] = line.Substring(line.IndexOf(" mucks [") + 9, 6);
                        table.Rows[r]["Hand4"] = table.Rows[r]["Hand4"].ToString().Replace(",", "");
                        table.Rows[r]["Name"] = name;
                        table.Rows[r]["FlopBet"] = 0;
                        table.Rows[r]["TurnBet"] = 0;
                        table.Rows[r]["RiverBet"] = 0;
                        names.Add(name, r);
                    }
                }
                else if (line.Contains("Total number of players :"))
                {
                    playersCount = Convert.ToInt32(line.Substring(26));
                    preflopPosition = playersCount + 1;
                }
            }
            if (names.Count == 0)
                return;

            string previosAction = "";
            string flopActions = "";
            string turnActions = "";
            string riverActions = "";
            string[] pocketHand = { "", "" };
            string street = "";
            string board = "";
            decimal bb = 0;
            decimal bank = 0;
            decimal streetStartBank = 0;
            decimal bet = 0;
            bool push = true;
            foreach (string line in lines)
            {
                if (line.Contains("** Dealing down cards **"))
                {
                    street = "preflop";
                    continue;
                }
                else if (line.Contains("** Dealing flop "))
                {
                    street = "flop";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board = line.Substring(line.IndexOf("[") + 2, 10).Replace(",", "");
                }
                else if (line.Contains("** Dealing turn "))
                {
                    street = "turn";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board += " " + line.Substring(line.IndexOf("[") + 2, 2);
                    for (int r1 = 0; r1 < table.Rows.Count; r1++)
                        if (table.Rows[r1]["flopAct"].ToString() != "")
                            push = false;
                }
                else if (line.Contains("** Dealing river "))
                {
                    street = "river";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board += " " + line.Substring(line.IndexOf("[") + 2, 2);
                }
                else if (line == "** Summary **")
                    street = "SUMMARY";

                if (street == "" && line.Contains(" posts "))
                {
                    if (line.Contains(" posts big blind "))
                    {
                        if (line.Contains(" + dead "))
                            bb = Convert.ToDecimal(line.Substring(line.IndexOf(" posts big blind ") + 25).Replace(",", "").Replace("]", ""));
                        else
                            bb = Convert.ToDecimal(line.Substring(line.IndexOf(" posts big blind ") + 19).Replace(",", "").Replace("]", ""));
                        bank += bb;
                    }
                    else if (line.Contains(" posts small blind "))
                    {
                        bank += Convert.ToDecimal(line.Substring(line.IndexOf(" posts small blind ") + 21).Replace(",", "").Replace("]", ""));
                    }
                    else if (line.Contains(" posts ante "))
                    {
                        bank += Convert.ToDecimal(line.Substring(line.IndexOf(" posts ante ") + 14).Replace(",", "").Replace("]", ""));
                    }
                }

                #region PREFLOP
                if (street == "preflop")
                {
                    if (line.Contains(" calls ") || line.Contains(" raises ") || line.Contains(" checks") || line.Contains(" folds"))
                        preflopPosition -= 1;
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    if (line.Contains(" calls "))
                    {
                        if (previosAction == "")
                            previosAction = "limp";
                        else if (previosAction == "raise" || previosAction == "openraise")
                            previosAction = "cold call";
                        else if (previosAction == "3bet")
                            previosAction = "call 3bet";
                        else if (previosAction == "4bet")
                            previosAction = "call 4bet";
                        if (line.Contains(" all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8, line.IndexOf(" and ") - line.IndexOf(" calls ") - 8).Replace(",", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 9).Replace("]", "").Replace(",", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        if (previosAction == "")
                            previosAction = "openraise";
                        else if (previosAction == "limp")
                            previosAction = "raise";
                        else if (previosAction == "raise" || previosAction == "cold call")
                            previosAction = "3bet";
                        else if (previosAction == "3bet" || previosAction == "call 3bet")
                            previosAction = "4bet";
                        if (line.Contains(" all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.IndexOf(" and ") - line.IndexOf(" to ") - 5).Replace(",", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 10).Replace("]", "").Replace(",", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" checks"))
                    {
                        previosAction = "check";
                    }
                    if (line.Contains(" calls ") || line.Contains(" raises ") || line.Contains(" checks"))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        name = name.ToLower();
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["PreflopAct"].ToString() == "limp" && line.Contains(" raises "))
                                table.Rows[r]["PreflopAct"] = "limp, raise";
                            else
                                table.Rows[r]["PreflopAct"] = previosAction;
                            table.Rows[r]["PreflopBet"] = Math.Round(bet / bb, 1);
                            //table.Rows[r]["TurnBet"] = (decimal)table.Rows[r]["TurnBet"] + Math.Round(bet / streetStartBank, 1);

                            if (table.Rows[r]["PreflopPos"].ToString() == "")
                            {
                                if (preflopPosition == 1)
                                    table.Rows[r]["PreflopPos"] = "BB";
                                else if (preflopPosition == 2)
                                    table.Rows[r]["PreflopPos"] = "SB";
                                else if (preflopPosition == 3)
                                    table.Rows[r]["PreflopPos"] = "BU";
                                else if (preflopPosition == 4)
                                    table.Rows[r]["PreflopPos"] = "CO";
                                else if (preflopPosition == 5)
                                    table.Rows[r]["PreflopPos"] = "MP3";
                                else if (preflopPosition == 6)
                                    table.Rows[r]["PreflopPos"] = "MP2";
                                else if (preflopPosition == 7)
                                    table.Rows[r]["PreflopPos"] = "MP1";
                                else if (preflopPosition == 8)
                                    table.Rows[r]["PreflopPos"] = "UTG";
                                else if (preflopPosition == 9)
                                    table.Rows[r]["PreflopPos"] = "UTG";
                            }
                        }
                    }
                }//if (street == "preflop")
                #endregion

                #region FLOP
                else if (street == "flop")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 8).Replace(']', ' ').Replace(",", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 9).Replace(']', ' ').Replace(",", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        previosAction = "raise";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 10).Replace(']', ' ').Replace(",", ""));
                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        name = name.ToLower();
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["FlopAct"].ToString() == "check")
                            {
                                table.Rows[r]["FlopAct"] += ", " + previosAction;
                                table.Rows[r]["Pos"] = "OOP";
                            }
                            else if (table.Rows[r]["FlopAct"].ToString() == "bet")
                                table.Rows[r]["FlopAct"] += ", " + previosAction;
                            else
                            {
                                table.Rows[r]["FlopAct"] = previosAction;
                                if (flopActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!flopActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                            }
                            table.Rows[r]["FlopBet"] = (decimal)table.Rows[r]["FlopBet"] + Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["FlopPlayersCount"] = playersCount;
                        }
                    }
                    flopActions += previosAction;
                }//if (street == "flop")
                #endregion

                #region TURN
                else if (street == "turn")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        if (line.Contains("all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 7, line.IndexOf(" and ") - line.IndexOf(" bets ") - 7).Replace(",", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 8).Replace(']', ' ').Replace(",", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";

                        if (line.Contains("all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8, line.IndexOf(" and ") - line.IndexOf(" calls ") - 8).Replace(",", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 9).Replace(']', ' ').Replace(",", ""));

                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        previosAction = "raise";
                        if (line.Contains(" all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.IndexOf(" and ") - line.IndexOf(" to ") - 5).Replace(",", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 10).Replace(']', ' ').Replace(",", ""));
                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        name = name.ToLower();
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["TurnAct"].ToString() == "check")
                            {
                                table.Rows[r]["TurnAct"] += ", " + previosAction;
                                table.Rows[r]["Pos"] = "OOP";
                            }
                            else if (table.Rows[r]["TurnAct"].ToString() == "bet")
                                table.Rows[r]["TurnAct"] += ", " + previosAction;
                            else
                            {
                                table.Rows[r]["TurnAct"] = previosAction;
                                if (turnActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!turnActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                            }
                            table.Rows[r]["TurnBet"] = (decimal)table.Rows[r]["TurnBet"] + Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["TurnPlayersCount"] = playersCount;
                        }
                        turnActions += previosAction;
                    }
                }//if (street == "turn")
                #endregion

                #region RIVER
                else if (street == "river")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 8).Replace(']', ' ').Replace(",", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 9).Replace(']', ' ').Replace(",", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        previosAction = "raise";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 10).Replace(']', ' ').Replace(",", ""));
                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        name = name.ToLower();
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["RiverAct"].ToString() == "check")
                            {
                                table.Rows[r]["RiverAct"] += ", " + previosAction;
                                table.Rows[r]["Pos"] = "OOP";
                            }
                            else if (table.Rows[r]["RiverAct"].ToString() == "bet")
                                table.Rows[r]["RiverAct"] += ", " + previosAction;
                            else
                            {
                                table.Rows[r]["RiverAct"] = previosAction;
                                if (riverActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!riverActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                            }
                            table.Rows[r]["RiverBet"] = (decimal)table.Rows[r]["RiverBet"] + Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["RiverPlayersCount"] = playersCount;
                        }
                    }
                    riverActions += previosAction;
                }//if (street == "river")
                #endregion

            }//foreach (string line in lines)

            int iComb = 0;

            foreach (System.Collections.DictionaryEntry element in names)
            {
                name = (string)element.Key;
                r = (int)names[name];
                string hand = (string)table.Rows[r]["Hand4"];
                pocketHand = System.Text.RegularExpressions.Regex.Split(hand, " ");
                if (hand.Substring(1, 1) != hand.Substring(4, 1) && hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "o";
                else if (hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "s";
                else
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1);
                if (hand[0].ToString() != hand[1].ToString())
                {
                    string card1 = hand[0].ToString();
                    card1 = card1.Replace("A", "14"); card1 = card1.Replace("K", "13"); card1 = card1.Replace("Q", "12"); card1 = card1.Replace("J", "11"); card1 = card1.Replace("T", "10");
                    string card2 = hand[1].ToString();
                    card2 = card2.Replace("A", "14"); card2 = card2.Replace("K", "13"); card2 = card2.Replace("Q", "12"); card2 = card2.Replace("J", "11"); card2 = card2.Replace("T", "10");
                    if (Convert.ToInt32(card1) < Convert.ToInt32(card2))
                        hand = hand.Substring(1, 1) + hand.Substring(0, 1) + hand.Substring(2, 1);
                }

                table.Rows[r]["hand3"] = hand;
                table.Rows[r]["board"] = board;

                string combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 8), " "));
                string[] array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                if (array.Count() == 1)
                    iComb = Convert.ToInt32(array[0]);
                else
                {
                    iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                    table.Rows[r]["FlopComb"] = array[0];
                }
                table.Rows[r]["iFlopComb"] = iComb;

                combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 11), " "));
                array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                if (array.Count() == 1)
                    iComb = Convert.ToInt32(array[0]);
                else
                {
                    iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                    table.Rows[r]["TurnComb"] = array[0];
                }
                table.Rows[r]["iTurnComb"] = iComb;

                combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 14), " "));
                array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                if (array.Count() == 1)
                    iComb = Convert.ToInt32(array[0]);
                else
                {
                    iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                    table.Rows[r]["RiverComb"] = array[0];
                }
                table.Rows[r]["iRiverComb"] = iComb;
            }

            if (push)
                ReadHandPush888Tournament(lines, bb);
        }

        private void ReadHandParty(String handText)
        {

            if (handText.Contains("17068760344")) // отладка
                handText = handText;
            
            if (handText.Contains(" Buy-in "))
            {
                ReadHandPartyTournament(handText);
                return;
            }
            
            System.Collections.Hashtable names = new System.Collections.Hashtable();
            string[] lines = System.Text.RegularExpressions.Regex.Split(handText, "\r\n");
            if (lines.Count() < 3)
                lines = System.Text.RegularExpressions.Regex.Split(handText, "\n");
            int r = table.Rows.Count - 1;
            int preflopPosition = 0;
            int playersCount = 6;
            string name = "";

            foreach (string line in lines)
            {
                if (line.Contains(" shows [") || line.Contains(" show [") || line.Contains(" mucks ["))
                {
                    if (line.Contains(" shows ["))
                        name = line.Substring(0, line.IndexOf(" shows ["));
                    else if (line.Contains(" show ["))
                        name = line.Substring(0, line.IndexOf(" doesn't show "));
                    else
                        name = line.Substring(0, line.IndexOf(" mucks ["));
                    if (names[name] == null)
                    {
                        r += 1;
                        table.Rows.Add();
                        table.Rows[r]["ID"] = handText.Substring(handText.IndexOf(" for Game") + 10, 9) + " " + name;
                        if (line.Contains(" shows ["))
                            table.Rows[r]["Hand4"] = line.Substring(line.IndexOf(" shows [") + 9, 6);
                        else if (line.Contains(" show ["))
                            table.Rows[r]["Hand4"] = line.Substring(line.IndexOf(" show [") + 8, 6);
                        else
                            table.Rows[r]["Hand4"] = line.Substring(line.IndexOf(" mucks [") + 9, 6);
                        table.Rows[r]["Hand4"] = table.Rows[r]["Hand4"].ToString().Replace(",", "");
                        table.Rows[r]["Name"] = name;
                        table.Rows[r]["FlopBet"] = 0;
                        table.Rows[r]["TurnBet"] = 0;
                        table.Rows[r]["RiverBet"] = 0;
                        names.Add(name, r);
                    }
                }
                else if (line.Contains("Total number of players :"))
                {
                    playersCount = Convert.ToInt32(line.Substring(26, 1));
                    preflopPosition = playersCount + 1;
                }
            }
            if (names.Count == 0)
                return;

            string previosAction = "";
            string flopActions = "";
            string turnActions = "";
            string riverActions = "";
            string[] pocketHand = { "", "" };
            string street = "";
            string board = "";
            decimal bb = 0;
            decimal bank = 0;
            decimal streetStartBank = 0;
            decimal bet = 0;
            bool push = true;
            foreach (string line in lines)
            {
                if (line == "** Dealing down cards **")
                {
                    street = "preflop";
                    continue;
                }
                else if (line.Contains("** Dealing Flop **"))
                {
                    street = "flop";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board = line.Substring(21, 10).Replace(",", "");
                }
                else if (line.Contains("** Dealing Turn **"))
                {
                    street = "turn";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board += " " + line.Substring(21, 2);
                    for (int r1 = 0; r1 < table.Rows.Count; r1++)
                        if (table.Rows[r1]["flopAct"].ToString() != "")
                            push = false;
                }
                else if (line.Contains("** Dealing River **"))
                {
                    street = "river";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board += " " + line.Substring(22, 2);
                }
                else if (line == "** Summary **")
                    street = "SUMMARY";

                if (street == "" && line.Contains(" posts "))
                {
                    if (line.Contains(" posts big blind "))
                    {
                        if (line.Contains(" + dead "))
                            bb = Convert.ToDecimal(line.Substring(line.IndexOf(" posts big blind ") + 25, 4).Replace('.', ','));
                        else
                            bb = Convert.ToDecimal(line.Substring(line.IndexOf(" posts big blind ") + 19, 4).Replace('.', ','));
                        bank += bb;
                    }
                    else if (line.Contains(" posts small blind "))
                    {
                        bank += Convert.ToDecimal(line.Substring(line.IndexOf(" posts small blind ") + 21, 4).Replace('.', ','));
                    }
                }

                #region PREFLOP
                if (street == "preflop")
                {
                    if (line.Contains(" calls ") || line.Contains(" raises ") || line.Contains(" checks") || line.Contains(" folds"))
                        preflopPosition -= 1;
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    if (line.Contains(" calls "))
                    {
                        if (previosAction == "")
                            previosAction = "limp";
                        else if (previosAction.Contains("raise"))
                            previosAction = "cold call";
                        else if (previosAction == "3bet")
                            previosAction = "call 3bet";
                        else if (previosAction == "4bet")
                            previosAction = "call 4bet";
                        if (line.Contains(" all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8, line.IndexOf(" and ") - line.IndexOf(" calls ") - 8).Replace('.', ',').Replace("USD", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 9).Replace("]", "").Replace('.', ',').Replace("USD", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        if (previosAction == "")
                            previosAction = "openraise";
                        else if (previosAction == "limp")
                            previosAction = "raise";
                        else if (previosAction == "raise" || previosAction == "cold call")
                            previosAction = "3bet";
                        else if (previosAction == "3bet" || previosAction == "call 3bet")
                            previosAction = "4bet";
                        if (line.Contains(" all-In"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.IndexOf(" and ") - line.IndexOf(" to ") - 5).Replace('.', ',').Replace("USD", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 10).Replace("]", "").Replace('.', ',').Replace("USD", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" checks"))
                    {
                        previosAction = "check";
                    }
                    else if (line.Contains(" all-In "))
                    {
                        previosAction = "raise";
                    }
                    if (line.Contains(" calls ") || line.Contains(" raises ") || line.Contains(" checks"))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["PreflopAct"].ToString() == "limp" && line.Contains(" raises "))
                                table.Rows[r]["PreflopAct"] = "limp, raise";
                            else
                                table.Rows[r]["PreflopAct"] = previosAction;
                            table.Rows[r]["PreflopBet"] = bet / bb;

                            if (table.Rows[r]["PreflopPos"].ToString() == "")
                            {
                                if (preflopPosition == 1)
                                    table.Rows[r]["PreflopPos"] = "BB";
                                else if (preflopPosition == 2)
                                    table.Rows[r]["PreflopPos"] = "SB";
                                else if (preflopPosition == 3)
                                    table.Rows[r]["PreflopPos"] = "BU";
                                else if (preflopPosition == 4)
                                    table.Rows[r]["PreflopPos"] = "CO";
                                else if (preflopPosition == 5)
                                    table.Rows[r]["PreflopPos"] = "MP3";
                                else if (preflopPosition == 6)
                                    table.Rows[r]["PreflopPos"] = "MP2";
                                else if (preflopPosition == 7)
                                    table.Rows[r]["PreflopPos"] = "MP1";
                                else if (preflopPosition == 8)
                                    table.Rows[r]["PreflopPos"] = "UTG";
                                else if (preflopPosition == 9)
                                    table.Rows[r]["PreflopPos"] = "UTG";
                            }
                        }
                    }
                }//if (street == "preflop")
                #endregion

                #region FLOP
                else if (street == "flop")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        if (line.Contains("all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 7, line.IndexOf(" and ") - line.IndexOf(" bets ") - 7).Replace('.', ',').Replace("USD", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 8).Replace(']', ' ').Replace('.', ',').Replace("USD", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";
                        if (line.Contains("all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8, line.IndexOf(" and ") - line.IndexOf(" calls ") - 8).Replace('.', ',').Replace("USD", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 9).Replace(']', ' ').Replace('.', ',').Replace("USD", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        previosAction = "raise";
                        if (line.Contains(" all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.IndexOf(" and ") - line.IndexOf(" to ") - 5).Replace('.', ',').Replace("USD", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 10).Replace(']', ' ').Replace('.', ',').Replace("USD", ""));
                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["FlopAct"].ToString() == "check")
                            {
                                table.Rows[r]["FlopAct"] += ", " + previosAction;
                                table.Rows[r]["Pos"] = "OOP";
                            }
                            else if (table.Rows[r]["FlopAct"].ToString() == "bet")
                                table.Rows[r]["FlopAct"] += ", " + previosAction;
                            else
                            {
                                table.Rows[r]["FlopAct"] = previosAction;
                                if (flopActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!flopActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                            }
                            table.Rows[r]["FlopBet"] = (decimal)table.Rows[r]["FlopBet"] + Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["FlopPlayersCount"] = playersCount;
                        }
                    }
                    flopActions += previosAction;
                }//if (street == "flop")
                #endregion

                #region TURN
                else if (street == "turn")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        if (line.Contains("all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 7, line.IndexOf(" and ") - line.IndexOf(" bets ") - 7).Replace('.', ',').Replace("USD", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 8).Replace(']', ' ').Replace('.', ',').Replace("USD", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";

                        if (line.Contains("all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8, line.IndexOf(" and ") - line.IndexOf(" calls ") - 8).Replace('.', ',').Replace("USD", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 9).Replace(']', ' ').Replace('.', ',').Replace("USD", ""));

                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        previosAction = "raise";
                        if (line.Contains(" all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.IndexOf(" and ") - line.IndexOf(" to ") - 5).Replace('.', ',').Replace("USD", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 10).Replace(']', ' ').Replace('.', ',').Replace("USD", ""));
                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["TurnAct"].ToString() == "check")
                            {
                                table.Rows[r]["TurnAct"] += ", " + previosAction;
                                table.Rows[r]["Pos"] = "OOP";
                            }
                            else if (table.Rows[r]["TurnAct"].ToString() == "bet")
                                table.Rows[r]["TurnAct"] += ", " + previosAction;
                            else
                            {
                                table.Rows[r]["TurnAct"] = previosAction;
                                if (turnActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!turnActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                            }
                            table.Rows[r]["TurnBet"] = (decimal)table.Rows[r]["TurnBet"] + Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["TurnPlayersCount"] = playersCount;
                        }
                        turnActions += previosAction;
                    }
                }//if (street == "turn")
                #endregion

                #region RIVER
                else if (street == "river")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        if (line.Contains("all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 7, line.IndexOf(" and ") - line.IndexOf(" bets ") - 7).Replace('.', ',').Replace("USD", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 8).Replace(']', ' ').Replace('.', ',').Replace("USD", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";
                        if (line.Contains("all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8, line.IndexOf(" and ") - line.IndexOf(" calls ") - 8).Replace('.', ',').Replace("USD", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 9).Replace(']', ' ').Replace('.', ',').Replace("USD", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        previosAction = "raise";
                        if (line.Contains(" all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.IndexOf(" and ") - line.IndexOf(" to ") - 5).Replace('.', ',').Replace("USD", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 10).Replace(']', ' ').Replace('.', ',').Replace("USD", ""));
                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["RiverAct"].ToString() == "check")
                            {
                                table.Rows[r]["RiverAct"] += ", " + previosAction;
                                table.Rows[r]["Pos"] = "OOP";
                            }
                            else if (table.Rows[r]["RiverAct"].ToString() == "bet")
                                table.Rows[r]["RiverAct"] += ", " + previosAction;
                            else
                            {
                                table.Rows[r]["RiverAct"] = previosAction;
                                if (riverActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!riverActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                            }
                            table.Rows[r]["RiverBet"] = (decimal)table.Rows[r]["RiverBet"] + Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["RiverPlayersCount"] = playersCount;
                        }
                    }
                    riverActions += previosAction;
                }//if (street == "river")
                #endregion

            }//foreach (string line in lines)

          
            int iComb = 0;
            foreach (System.Collections.DictionaryEntry element in names)
            {
                name = (string)element.Key;
                r = (int)names[name];
                string hand = (string)table.Rows[r]["Hand4"];
                pocketHand = System.Text.RegularExpressions.Regex.Split(hand, " ");
                if (hand.Substring(1, 1) != hand.Substring(4, 1) && hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "o";
                else if (hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "s";
                else
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1);
                if (hand[0].ToString() != hand[1].ToString())
                {
                    string card1 = hand[0].ToString();
                    card1 = card1.Replace("A", "14"); card1 = card1.Replace("K", "13"); card1 = card1.Replace("Q", "12"); card1 = card1.Replace("J", "11"); card1 = card1.Replace("T", "10");
                    string card2 = hand[1].ToString();
                    card2 = card2.Replace("A", "14"); card2 = card2.Replace("K", "13"); card2 = card2.Replace("Q", "12"); card2 = card2.Replace("J", "11"); card2 = card2.Replace("T", "10");
                    if (Convert.ToInt32(card1) < Convert.ToInt32(card2))
                        hand = hand.Substring(1, 1) + hand.Substring(0, 1) + hand.Substring(2, 1);
                }

                table.Rows[r]["hand3"] = hand;
                table.Rows[r]["board"] = board;

                string combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 8), " "));
                string[] array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                if (array.Count() == 1)
                    iComb = Convert.ToInt32(array[0]);
                else
                {
                    iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                    table.Rows[r]["FlopComb"] = array[0];
                }
                table.Rows[r]["iFlopComb"] = iComb;

                if (board.Length >= 11)
                {
                    combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 11), " "));
                    array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                    if (array.Count() == 1)
                        iComb = Convert.ToInt32(array[0]);
                    else
                    {
                        iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                        table.Rows[r]["TurnComb"] = array[0];
                    }
                    table.Rows[r]["iTurnComb"] = iComb;
                }

                if (board.Length >= 14)
                {
                    combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 14), " "));
                    array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                    if (array.Count() == 1)
                        iComb = Convert.ToInt32(array[0]);
                    else
                    {
                        iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                        table.Rows[r]["RiverComb"] = array[0];
                    }
                    table.Rows[r]["iRiverComb"] = iComb;
                }
            }

            //if (push)
            //    ReadHandPushParty(lines, bb);
        }

        private void ReadHandPartyTournament(String handText)
        {

            System.Collections.Hashtable names = new System.Collections.Hashtable();
            string[] lines = System.Text.RegularExpressions.Regex.Split(handText, "\r\n");
            if (lines.Count() < 3)
                lines = System.Text.RegularExpressions.Regex.Split(handText, "\n");
            int r = table.Rows.Count - 1;
            int preflopPosition = 0;
            int playersCount = 6;
            string name = "";

            foreach (string line in lines)
            {
                if (line.Contains(" shows [") || line.Contains(" mucks ["))
                {
                    if (line.Contains(" shows ["))
                        name = line.Substring(0, line.IndexOf(" shows ["));
                    else
                        name = line.Substring(0, line.IndexOf(" mucks ["));
                    if (names[name] == null)
                    {
                        r += 1;
                        table.Rows.Add();
                        table.Rows[r]["ID"] = handText.Substring(handText.IndexOf(" for Game") + 10, 9) + " " + name;
                        if (line.Contains(" shows ["))
                            table.Rows[r]["Hand4"] = line.Substring(line.IndexOf(" shows [") + 9, 6);
                        else
                            table.Rows[r]["Hand4"] = line.Substring(line.IndexOf(" mucks [") + 9, 6);
                        table.Rows[r]["Hand4"] = table.Rows[r]["Hand4"].ToString().Replace(",", "");
                        table.Rows[r]["Name"] = name;
                        table.Rows[r]["FlopBet"] = 0;
                        table.Rows[r]["TurnBet"] = 0;
                        table.Rows[r]["RiverBet"] = 0;
                        names.Add(name, r);
                    }
                }
                else if (line.Contains("Total number of players :"))
                {
                    playersCount = Convert.ToInt32(line.Substring(26, 1));
                    preflopPosition = playersCount + 1;
                }
            }
            if (names.Count == 0)
                return;

            string previosAction = "";
            string flopActions = "";
            string turnActions = "";
            string riverActions = "";
            string[] pocketHand = { "", "" };
            string street = "";
            string board = "";
            decimal bb = 0;
            decimal bank = 0;
            decimal streetStartBank = 0;
            decimal bet = 0;
            bool push = true;
            foreach (string line in lines)
            {
                if (line == "** Dealing down cards **")
                {
                    street = "preflop";
                    continue;
                }
                else if (line.Contains("** Dealing Flop **"))
                {
                    street = "flop";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board = line.Substring(21, 10).Replace(",", "");
                }
                else if (line.Contains("** Dealing Turn **"))
                {
                    street = "turn";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board += " " + line.Substring(21, 2);
                    for (int r1 = 0; r1 < table.Rows.Count; r1++)
                        if (table.Rows[r1]["flopAct"].ToString() != "")
                            push = false;
                }
                else if (line.Contains("** Dealing River **"))
                {
                    street = "river";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board += " " + line.Substring(22, 2);
                }
                else if (line == "** Summary **")
                    street = "SUMMARY";

                if (street == "" && line.Contains(" posts "))
                {
                    if (line.Contains(" posts big blind "))
                    {
                        if (line.Contains(" + dead "))
                            bb = Convert.ToDecimal(line.Substring(line.IndexOf(" posts big blind ") + 23).Replace("]", "").Replace(",", ""));
                        else
                            bb = Convert.ToDecimal(line.Substring(line.IndexOf(" posts big blind ") + 18).Replace("]", "").Replace(",", ""));
                        bank += bb;
                    }
                    else if (line.Contains(" posts small blind "))
                    {
                        bank += Convert.ToDecimal(line.Substring(line.IndexOf(" posts small blind ") + 20).Replace("]", "").Replace(",", ""));
                    }
                }

                #region PREFLOP
                if (street == "preflop")
                {
                    if (line.Contains(" calls ") || line.Contains(" raises ") || line.Contains(" checks") || line.Contains(" folds") || line.Contains(" all-In"))
                        preflopPosition -= 1;
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    if (line.Contains(" calls "))
                    {
                        if (previosAction == "")
                            previosAction = "limp";
                        else if (previosAction == "raise")
                            previosAction = "cold call";
                        else if (previosAction == "3bet")
                            previosAction = "call 3bet";
                        else if (previosAction == "4bet")
                            previosAction = "call 4bet";
                        if (line.Contains(" all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8, line.IndexOf(" and ") - line.IndexOf(" calls ") - 8).Replace("]", "").Replace(",", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8).Replace("]", "").Replace(",", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        if (previosAction == "" || previosAction == "limp")
                            previosAction = "raise";
                        else if (previosAction == "raise" || previosAction == "cold call")
                            previosAction = "3bet";
                        else if (previosAction == "3bet" || previosAction == "call 3bet")
                            previosAction = "4bet";
                        if (line.Contains(" all-In"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.IndexOf(" and ") - line.IndexOf(" to ") - 5).Replace("]", "").Replace(",", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 9).Replace("]", "").Replace(",", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" all-In"))
                    {
                        if (previosAction == "" || previosAction == "limp")
                            previosAction = "raise";
                        else if (previosAction == "raise" || previosAction == "cold call")
                            previosAction = "3bet";
                        else if (previosAction == "3bet" || previosAction == "call 3bet")
                            previosAction = "4bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" all-In") + 10).Replace("]", "").Replace(",", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" checks"))
                    {
                        previosAction = "check";
                    }
                    if (line.Contains(" calls ") || line.Contains(" raises ") || line.Contains(" checks") || line.Contains(" all-In"))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["PreflopAct"].ToString() == "limp" && (line.Contains(" raises ") || line.Contains(" all-In")))
                                table.Rows[r]["PreflopAct"] = "limp, raise";
                            else
                                table.Rows[r]["PreflopAct"] = previosAction;
                            table.Rows[r]["PreflopBet"] = bet / bb;

                            if (table.Rows[r]["PreflopPos"].ToString() == "")
                            {
                                if (preflopPosition == 1)
                                    table.Rows[r]["PreflopPos"] = "BB";
                                else if (preflopPosition == 2)
                                    table.Rows[r]["PreflopPos"] = "SB";
                                else if (preflopPosition == 3)
                                    table.Rows[r]["PreflopPos"] = "BU";
                                else if (preflopPosition == 4)
                                    table.Rows[r]["PreflopPos"] = "CO";
                                else if (preflopPosition == 5)
                                    table.Rows[r]["PreflopPos"] = "MP3";
                                else if (preflopPosition == 6)
                                    table.Rows[r]["PreflopPos"] = "MP2";
                                else if (preflopPosition == 7)
                                    table.Rows[r]["PreflopPos"] = "MP1";
                                else if (preflopPosition == 8)
                                    table.Rows[r]["PreflopPos"] = "UTG";
                                else if (preflopPosition == 9)
                                    table.Rows[r]["PreflopPos"] = "UTG";
                            }
                        }
                    }
                }//if (street == "preflop")
                #endregion

                #region FLOP
                else if (street == "flop")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        if (line.Contains("all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 7, line.IndexOf(" and ") - line.IndexOf(" bets ") - 7).Replace("]", "").Replace(",", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 7).Replace("]", "").Replace(",", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";
                        if (line.Contains("all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8, line.IndexOf(" and ") - line.IndexOf(" calls ") - 8).Replace("]", "").Replace(",", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 9).Replace(']', ' ').Replace("]", "").Replace(",", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        previosAction = "raise";
                        if (line.Contains(" all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.IndexOf(" and ") - line.IndexOf(" to ") - 5).Replace("]", "").Replace(",", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 9).Replace("]", "").Replace(",", ""));
                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["FlopAct"].ToString() == "check")
                            {
                                table.Rows[r]["FlopAct"] += ", " + previosAction;
                                table.Rows[r]["Pos"] = "OOP";
                            }
                            else if (table.Rows[r]["FlopAct"].ToString() == "bet")
                                table.Rows[r]["FlopAct"] += ", " + previosAction;
                            else
                            {
                                table.Rows[r]["FlopAct"] = previosAction;
                                if (flopActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!flopActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                            }
                            table.Rows[r]["FlopBet"] = (decimal)table.Rows[r]["FlopBet"] + Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["FlopPlayersCount"] = playersCount;
                        }
                    }
                    flopActions += previosAction;
                }//if (street == "flop")
                #endregion

                #region TURN
                else if (street == "turn")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        if (line.Contains("all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 7, line.IndexOf(" and ") - line.IndexOf(" bets ") - 7).Replace("]", "").Replace(",", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 7).Replace("]", "").Replace(",", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";

                        if (line.Contains("all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8, line.IndexOf(" and ") - line.IndexOf(" calls ") - 8).Replace("]", "").Replace(",", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 9).Replace(']', ' ').Replace("]", "").Replace(",", ""));

                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        previosAction = "raise";
                        if (line.Contains(" all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.IndexOf(" and ") - line.IndexOf(" to ") - 5).Replace("]", "").Replace(",", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 9).Replace("]", "").Replace(",", ""));
                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["TurnAct"].ToString() == "check")
                            {
                                table.Rows[r]["TurnAct"] += ", " + previosAction;
                                table.Rows[r]["Pos"] = "OOP";
                            }
                            else if (table.Rows[r]["TurnAct"].ToString() == "bet")
                                table.Rows[r]["TurnAct"] += ", " + previosAction;
                            else
                            {
                                table.Rows[r]["TurnAct"] = previosAction;
                                if (turnActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!turnActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                            }
                            table.Rows[r]["TurnBet"] = (decimal)table.Rows[r]["TurnBet"] + Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["TurnPlayersCount"] = playersCount;
                        }
                        turnActions += previosAction;
                    }
                }//if (street == "turn")
                #endregion

                #region RIVER
                else if (street == "river")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        if (line.Contains("all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 7, line.IndexOf(" and ") - line.IndexOf(" bets ") - 7).Replace("]", "").Replace(",", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 7).Replace(']', ' ').Replace("]", "").Replace(",", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";
                        if (line.Contains("all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 8, line.IndexOf(" and ") - line.IndexOf(" calls ") - 8).Replace("]", "").Replace(",", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 9).Replace(']', ' ').Replace("]", "").Replace(",", ""));
                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        previosAction = "raise";
                        if (line.Contains(" all-in"))
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" to ") + 5, line.IndexOf(" and ") - line.IndexOf(" to ") - 5).Replace("]", "").Replace(",", ""));
                        else
                            bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 9).Replace("]", "").Replace(",", ""));
                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["RiverAct"].ToString() == "check")
                            {
                                table.Rows[r]["RiverAct"] += ", " + previosAction;
                                table.Rows[r]["Pos"] = "OOP";
                            }
                            else if (table.Rows[r]["RiverAct"].ToString() == "bet")
                                table.Rows[r]["RiverAct"] += ", " + previosAction;
                            else
                            {
                                table.Rows[r]["RiverAct"] = previosAction;
                                if (riverActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!riverActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                            }
                            table.Rows[r]["RiverBet"] = (decimal)table.Rows[r]["RiverBet"] + Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["RiverPlayersCount"] = playersCount;
                        }
                    }
                    riverActions += previosAction;
                }//if (street == "river")
                #endregion

            }//foreach (string line in lines)


            int iComb = 0;

            foreach (System.Collections.DictionaryEntry element in names)
            {
                name = (string)element.Key;
                r = (int)names[name];
                string hand = (string)table.Rows[r]["Hand4"];
                pocketHand = System.Text.RegularExpressions.Regex.Split(hand, " ");
                if (hand.Substring(1, 1) != hand.Substring(4, 1) && hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "o";
                else if (hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "s";
                else
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1);
                if (hand[0].ToString() != hand[1].ToString())
                {
                    string card1 = hand[0].ToString();
                    card1 = card1.Replace("A", "14"); card1 = card1.Replace("K", "13"); card1 = card1.Replace("Q", "12"); card1 = card1.Replace("J", "11"); card1 = card1.Replace("T", "10");
                    string card2 = hand[1].ToString();
                    card2 = card2.Replace("A", "14"); card2 = card2.Replace("K", "13"); card2 = card2.Replace("Q", "12"); card2 = card2.Replace("J", "11"); card2 = card2.Replace("T", "10");
                    if (Convert.ToInt32(card1) < Convert.ToInt32(card2))
                        hand = hand.Substring(1, 1) + hand.Substring(0, 1) + hand.Substring(2, 1);
                }

                table.Rows[r]["hand3"] = hand;
                table.Rows[r]["board"] = board;

                string combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 8), " "));
                string[] array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                if (array.Count() == 1)
                    iComb = Convert.ToInt32(array[0]);
                else
                {
                    iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                    table.Rows[r]["FlopComb"] = array[0];
                }
                table.Rows[r]["iFlopComb"] = iComb;

                combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 11), " "));
                array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                if (array.Count() == 1)
                    iComb = Convert.ToInt32(array[0]);
                else
                {
                    iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                    table.Rows[r]["TurnComb"] = array[0];
                }
                table.Rows[r]["iTurnComb"] = iComb;

                combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 14), " "));
                array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                if (array.Count() == 1)
                    iComb = Convert.ToInt32(array[0]);
                else
                {
                    iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                    table.Rows[r]["RiverComb"] = array[0];
                }
                table.Rows[r]["iRiverComb"] = iComb;
            }

            //if (push)
            //    ReadHandPushStars(lines, bb);
        }

        private void ReadHandPushStars(String[] lines, decimal bb)
        { 
            decimal stack = 0;
            decimal prevEffStack = 0;
            System.Collections.Hashtable stacks = new System.Collections.Hashtable();
            foreach (string line in lines)
            {
                //if (line.Contains("*** HOLE CARDS ***") || line == "")
                //{
                //    //    break;
                //}
                if (line.Contains("*** FLOP ***") || line == "")
                    break;
                else if (line.Substring(0, 5) == "Seat ")
                {
                    string name = line.Substring(line.IndexOf(":")+2);
                    name = name.Substring(0, name.IndexOf(" ("));
                    string s = line.Substring(line.IndexOf("(") + 1);
                    s = s.Substring(0, s.IndexOf(" "));
                    s = s.Replace("$", "");
                    s = s.Replace(".", ",");
                    stack = Convert.ToDecimal(s);
                    stack = stack / bb;
                    stacks.Add(name, stack);
                }
                else if (line.Contains(" folds"))
                {
                    string name = line.Substring(0, line.IndexOf(":"));
                    stacks.Remove(name);
                }
                else if (line.Contains(" calls ") || line.Contains(" raises "))
                {
                    string name = line.Substring(0, line.IndexOf(":"));
                    DataRow[] foundRows = table.Select("[name] = '" + name + "'");
                    if (foundRows.Count() > 0)
                    {
                        decimal stack1 = 0;
                        decimal stack2 = 0;
                        foreach (System.Collections.DictionaryEntry element in stacks)
                        {
                            if ((decimal)element.Value > stack1)
                            {
                                if (stack1 > 0)
                                    stack2 = stack1;
                                stack1 = (decimal)element.Value;
                            }
                            else if ((decimal)element.Value > stack2)
                                stack2 = (decimal)element.Value;
                        }
                        
                        string preflopAct = (string)foundRows[0]["preflopAct"];
                        if (preflopAct.Contains("call"))
                            preflopAct = "call";
                        else if (preflopAct == "openraise")
                            preflopAct = "openraise";
                        else if (preflopAct == "raise")
                            preflopAct = "raise";
                        else if (preflopAct.Contains("bet"))
                            preflopAct = "reraise";
                        else
                            continue;
                        decimal effStack = (decimal)stacks[name];
                        if (effStack > stack2)
                            effStack = stack2;
                        if ((decimal)foundRows[0]["PreflopBet"] < effStack)
                            effStack = (decimal)foundRows[0]["PreflopBet"];
                        DataRow[] foundRowsPush = tablePush.Select("[name] = '" + name + "'");
                        if (foundRowsPush.Count() > 0)
                        {
                            foundRowsPush[0]["Stack"] = Convert.ToInt32(effStack);
                            
                            //if (preflopAct == "call")
                            //{
                            //    if ((decimal)stacks[name] < prevEffStack)
                            //        foundRowsPush[0]["Stack"] = (decimal)stacks[name];
                            //    else
                            //        foundRowsPush[0]["Stack"] = Convert.ToInt32(prevEffStack);
                            //}
                            //else
                            //    foundRowsPush[0]["Stack"] = Convert.ToInt32(effStack);
                        }
                        else
                        {
                            tablePush.Rows.Add();
                            int r = tablePush.Rows.Count - 1;
                            tablePush.Rows[r]["ID"] = (string)foundRows[0]["ID"]; ;
                            tablePush.Rows[r]["Name"] = name;
                            tablePush.Rows[r]["Hand3"] = (string)foundRows[0]["Hand3"];
                            tablePush.Rows[r]["PreflopPos"] = (string)foundRows[0]["PreflopPos"];
                            tablePush.Rows[r]["PreflopAct"] = preflopAct;
                            tablePush.Rows[r]["Stack"] = Convert.ToInt32(effStack);
                        }
                        prevEffStack = effStack;
                    }
                }
            }

            //for (int r = 0; r < table.Rows.Count; r++)
            //{ 
            //}
            //foreach (System.Collections.DictionaryEntry element in stacks)
            //{
            //    if ((decimal)element.Value > stack1)
            //        stack1 = (decimal)element.Value;
            //    else if ((decimal)element.Value > stack2)
            //        stack2 = (decimal)element.Value;
            //}
            //foreach (System.Collections.DictionaryEntry element in stacks)
            //{
            //    string name = (string)element.Key;
            //    DataRow[] foundRows = table.Select("[name] = '" + name + "'");
            //    if (foundRows.Count() > 0)
            //    {
            //        string preflopAct = (string)foundRows[0]["preflopAct"];
            //        if (preflopAct.Contains("call"))
            //            preflopAct = "call";
            //        else if (preflopAct == "openraise")
            //            preflopAct = "openraise";
            //        else if (preflopAct == "raise")
            //            preflopAct = "raise";
            //        else if (preflopAct.Contains("bet"))
            //            preflopAct = "reraise";
            //        else
            //            continue;
            //        tablePush.Rows.Add();
            //        int r = tablePush.Rows.Count - 1;
            //        tablePush.Rows[r]["ID"] = (string)foundRows[0]["ID"]; ;
            //        tablePush.Rows[r]["Name"] = name;
            //        tablePush.Rows[r]["Hand3"] = (string)foundRows[0]["Hand3"];
            //        tablePush.Rows[r]["PreflopPos"] = (string)foundRows[0]["PreflopPos"];
            //        tablePush.Rows[r]["PreflopAct"] = preflopAct;
            //        decimal effStack = (decimal)element.Value;
            //        if (effStack > stack2)
            //            effStack = stack2;
            //        tablePush.Rows[r]["Stack"] = Convert.ToInt32(effStack);
            //    }
            //}
        }

        private void ReadHandPushParty(String[] lines, decimal bb)
        {
            decimal stack = 0;
            decimal prevEffStack = 0;
            System.Collections.Hashtable stacks = new System.Collections.Hashtable();
            foreach (string line in lines)
            {
                //if (line.Contains("*** HOLE CARDS ***") || line == "")
                //{
                //    //    break;
                //}
                if (line.Contains("*** FLOP ***") || line == "")
                    break;
                else if (line.Substring(0, 5) == "Seat ")
                {
                    string name = line.Substring(line.IndexOf(":") + 2);
                    name = name.Substring(0, name.IndexOf(" ("));
                    string s = line.Substring(line.IndexOf("(") + 1);
                    s = s.Substring(0, s.IndexOf(" "));
                    s = s.Replace("$", "");
                    s = s.Replace(".", ",");
                    stack = Convert.ToDecimal(s);
                    stack = stack / bb;
                    stacks.Add(name, stack);
                }
                else if (line.Contains(" folds"))
                {
                    string name = line.Substring(0, line.IndexOf(":"));
                    stacks.Remove(name);
                }
                else if (line.Contains(" calls ") || line.Contains(" raises "))
                {
                    string name = line.Substring(0, line.IndexOf(":"));
                    DataRow[] foundRows = table.Select("[name] = '" + name + "'");
                    if (foundRows.Count() > 0)
                    {
                        decimal stack1 = 0;
                        decimal stack2 = 0;
                        foreach (System.Collections.DictionaryEntry element in stacks)
                        {
                            if ((decimal)element.Value > stack1)
                            {
                                if (stack1 > 0)
                                    stack2 = stack1;
                                stack1 = (decimal)element.Value;
                            }
                            else if ((decimal)element.Value > stack2)
                                stack2 = (decimal)element.Value;
                        }

                        string preflopAct = (string)foundRows[0]["preflopAct"];
                        if (preflopAct.Contains("call"))
                            preflopAct = "call";
                        else if (preflopAct == "openraise")
                            preflopAct = "openraise";
                        else if (preflopAct == "raise")
                            preflopAct = "raise";
                        else if (preflopAct.Contains("bet"))
                            preflopAct = "reraise";
                        else
                            continue;
                        decimal effStack = (decimal)stacks[name];
                        if (effStack > stack2)
                            effStack = stack2;
                        if ((decimal)foundRows[0]["PreflopBet"] < effStack)
                            effStack = (decimal)foundRows[0]["PreflopBet"];
                        DataRow[] foundRowsPush = tablePush.Select("[name] = '" + name + "'");
                        if (foundRowsPush.Count() > 0)
                        {
                            foundRowsPush[0]["Stack"] = Convert.ToInt32(effStack);

                            //if (preflopAct == "call")
                            //{
                            //    if ((decimal)stacks[name] < prevEffStack)
                            //        foundRowsPush[0]["Stack"] = (decimal)stacks[name];
                            //    else
                            //        foundRowsPush[0]["Stack"] = Convert.ToInt32(prevEffStack);
                            //}
                            //else
                            //    foundRowsPush[0]["Stack"] = Convert.ToInt32(effStack);
                        }
                        else
                        {
                            tablePush.Rows.Add();
                            int r = tablePush.Rows.Count - 1;
                            tablePush.Rows[r]["ID"] = (string)foundRows[0]["ID"]; ;
                            tablePush.Rows[r]["Name"] = name;
                            tablePush.Rows[r]["Hand3"] = (string)foundRows[0]["Hand3"];
                            tablePush.Rows[r]["PreflopPos"] = (string)foundRows[0]["PreflopPos"];
                            tablePush.Rows[r]["PreflopAct"] = preflopAct;
                            tablePush.Rows[r]["Stack"] = Convert.ToInt32(effStack);
                        }
                        prevEffStack = effStack;
                    }
                }
            }

            //for (int r = 0; r < table.Rows.Count; r++)
            //{ 
            //}
            //foreach (System.Collections.DictionaryEntry element in stacks)
            //{
            //    if ((decimal)element.Value > stack1)
            //        stack1 = (decimal)element.Value;
            //    else if ((decimal)element.Value > stack2)
            //        stack2 = (decimal)element.Value;
            //}
            //foreach (System.Collections.DictionaryEntry element in stacks)
            //{
            //    string name = (string)element.Key;
            //    DataRow[] foundRows = table.Select("[name] = '" + name + "'");
            //    if (foundRows.Count() > 0)
            //    {
            //        string preflopAct = (string)foundRows[0]["preflopAct"];
            //        if (preflopAct.Contains("call"))
            //            preflopAct = "call";
            //        else if (preflopAct == "openraise")
            //            preflopAct = "openraise";
            //        else if (preflopAct == "raise")
            //            preflopAct = "raise";
            //        else if (preflopAct.Contains("bet"))
            //            preflopAct = "reraise";
            //        else
            //            continue;
            //        tablePush.Rows.Add();
            //        int r = tablePush.Rows.Count - 1;
            //        tablePush.Rows[r]["ID"] = (string)foundRows[0]["ID"]; ;
            //        tablePush.Rows[r]["Name"] = name;
            //        tablePush.Rows[r]["Hand3"] = (string)foundRows[0]["Hand3"];
            //        tablePush.Rows[r]["PreflopPos"] = (string)foundRows[0]["PreflopPos"];
            //        tablePush.Rows[r]["PreflopAct"] = preflopAct;
            //        decimal effStack = (decimal)element.Value;
            //        if (effStack > stack2)
            //            effStack = stack2;
            //        tablePush.Rows[r]["Stack"] = Convert.ToInt32(effStack);
            //    }
            //}
        }

        private void ReadHandPush888(String[] lines, decimal bb)
        {
            decimal stack = 0;
            //decimal prevEffStack = 0;
            System.Collections.Hashtable stacks = new System.Collections.Hashtable();
            foreach (string line in lines)
            {
                if (line.Contains("** Dealing flop") || line == "")
                    break;
                else if (line.Substring(0, 5) == "Seat " && line.Substring(6, 1) == ":")
                {
                    string name = line.Substring(line.IndexOf(":") + 2);
                    name = name.Substring(0, name.IndexOf(" "));
                    name = name.ToLower();
                    string s = line.Substring(line.IndexOf("(") + 1);
                    s = s.Replace(")", "");
                    s = s.Replace("$", "");
                    s = s.Replace(".", ",");
                    s = s.Trim();
                    stack = Convert.ToDecimal(s);
                    stack = stack / bb;
                    stacks.Add(name, stack);
                }
                else if (line.Contains(" folds"))
                {
                    string name = line.Substring(0, line.IndexOf(" folds"));
                    stacks.Remove(name);
                }
                else if (line.Contains(" calls ") || line.Contains(" raises "))
                {
                    string name = "";
                    if (line.Contains(" calls "))
                        name = line.Substring(0, line.IndexOf(" calls "));
                    else
                        name = line.Substring(0, line.IndexOf(" raises "));
                    name = name.ToLower();
                    DataRow[] foundRows = table.Select("[name] = '" + name + "'");
                    if (foundRows.Count() > 0)
                    {
                        decimal stack1 = 0;
                        decimal stack2 = 0;
                        foreach (System.Collections.DictionaryEntry element in stacks)
                        {
                            if ((decimal)element.Value > stack1)
                            {
                                if (stack1 > 0)
                                    stack2 = stack1;
                                stack1 = (decimal)element.Value;
                            }
                            else if ((decimal)element.Value > stack2)
                                stack2 = (decimal)element.Value;
                        }

                        string preflopAct = (string)foundRows[0]["preflopAct"];
                        if (preflopAct.Contains("call"))
                            preflopAct = "call";
                        else if (preflopAct == "openraise")
                            preflopAct = "openraise";
                        else if (preflopAct == "raise")
                            preflopAct = "raise";
                        else if (preflopAct.Contains("bet"))
                            preflopAct = "reraise";
                        else
                            continue;
                        decimal effStack = (decimal)stacks[name];
                        if (effStack > stack2)
                            effStack = stack2;
                        //if ((decimal)foundRows[0]["PreflopBet"] < effStack)
                        //    effStack = (decimal)foundRows[0]["PreflopBet"];
                        DataRow[] foundRowsPush = tablePush.Select("[name] = '" + name + "'");
                        if (foundRowsPush.Count() > 0)
                        {
                            foundRowsPush[0]["Stack"] = Convert.ToInt32(effStack);
                        }
                        else
                        {
                            tablePush.Rows.Add();
                            int r = tablePush.Rows.Count - 1;
                            tablePush.Rows[r]["ID"] = (string)foundRows[0]["ID"]; ;
                            tablePush.Rows[r]["Name"] = name;
                            tablePush.Rows[r]["Hand3"] = (string)foundRows[0]["Hand3"];
                            tablePush.Rows[r]["PreflopPos"] = (string)foundRows[0]["PreflopPos"];
                            tablePush.Rows[r]["PreflopAct"] = preflopAct;
                            tablePush.Rows[r]["Stack"] = Convert.ToInt32(effStack);
                        }
                       // prevEffStack = effStack;
                    }
                }
            }
        }

        private void ReadHandPush888Tournament(String[] lines, decimal bb)
        {
            decimal stack = 0;
            //decimal prevEffStack = 0;
            System.Collections.Hashtable stacks = new System.Collections.Hashtable();
            foreach (string line in lines)
            {
                if (line.Contains("** Dealing flop") || line == "")
                    break;
                else if (line.Substring(0, 5) == "Seat " && line.Substring(6, 1) == ":")
                {
                    string name = line.Substring(line.IndexOf(":") + 2);
                    name = name.Substring(0, name.IndexOf(" "));
                    name = name.ToLower();
                    string s = line.Substring(line.IndexOf("(") + 1);
                    s = s.Replace(")", "");
                    s = s.Replace("$", "");
                    s = s.Replace(",", "");
                    s = s.Trim();
                    stack = Convert.ToDecimal(s);
                    stack = stack / bb;
                    stacks.Add(name, stack);
                }
                else if (line.Contains(" folds"))
                {
                    string name = line.Substring(0, line.IndexOf(" folds"));
                    stacks.Remove(name);
                }
                else if (line.Contains(" calls ") || line.Contains(" raises "))
                {
                    string name = "";
                    if (line.Contains(" calls "))
                        name = line.Substring(0, line.IndexOf(" calls "));
                    else
                        name = line.Substring(0, line.IndexOf(" raises "));
                    name = name.ToLower();
                    DataRow[] foundRows = table.Select("[name] = '" + name + "'");
                    if (foundRows.Count() > 0)
                    {
                        decimal stack1 = 0;
                        decimal stack2 = 0;
                        foreach (System.Collections.DictionaryEntry element in stacks)
                        {
                            if ((decimal)element.Value > stack1)
                            {
                                if (stack1 > 0)
                                    stack2 = stack1;
                                stack1 = (decimal)element.Value;
                            }
                            else if ((decimal)element.Value > stack2)
                                stack2 = (decimal)element.Value;
                        }

                        string preflopAct = (string)foundRows[0]["preflopAct"];
                        if (preflopAct.Contains("call"))
                            preflopAct = "call";
                        else if (preflopAct == "openraise")
                            preflopAct = "openraise";
                        else if (preflopAct == "raise")
                            preflopAct = "raise";
                        else if (preflopAct.Contains("bet"))
                            preflopAct = "reraise";
                        else
                            continue;
                        decimal effStack = (decimal)stacks[name];
                        if (effStack > stack2)
                            effStack = stack2;
                        //if ((decimal)foundRows[0]["PreflopBet"] < effStack)
                        //    effStack = (decimal)foundRows[0]["PreflopBet"];
                        DataRow[] foundRowsPush = tablePush.Select("[name] = '" + name + "'");
                        if (foundRowsPush.Count() > 0)
                        {
                            foundRowsPush[0]["Stack"] = Convert.ToInt32(effStack);
                        }
                        else
                        {
                            tablePush.Rows.Add();
                            int r = tablePush.Rows.Count - 1;
                            tablePush.Rows[r]["ID"] = (string)foundRows[0]["ID"]; ;
                            tablePush.Rows[r]["Name"] = name;
                            tablePush.Rows[r]["Hand3"] = (string)foundRows[0]["Hand3"];
                            tablePush.Rows[r]["PreflopPos"] = (string)foundRows[0]["PreflopPos"];
                            tablePush.Rows[r]["PreflopAct"] = preflopAct;
                            tablePush.Rows[r]["Stack"] = Convert.ToInt32(effStack);
                        }
                        // prevEffStack = effStack;
                    }
                }
            }
        }

        private void ReadHandIPokerHM2(String handText)
        {
            
            
            handText = handText.Replace(@"=""€", @"=""");
            handText = handText.Replace(@"=""$", @"=""");

            System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
            xDoc.LoadXml(handText);
            System.Xml.XmlElement xRoot = xDoc.DocumentElement;

            Hashtable names = new Hashtable();
            int r = table.Rows.Count - 1;
            //string street = "";
            int preflopPosition = 0;
            int playersCount = 0;
            string name;

            foreach (System.Xml.XmlNode xnode in xRoot)
            {
                if (xnode.Name == "game")
                {
                    foreach (System.Xml.XmlNode childnode in xnode.ChildNodes)
                    {
                        if (childnode.Name == "general")
                        {
                            foreach (System.Xml.XmlNode childnode2 in childnode.ChildNodes)
                            {
                                if (childnode2.Name == "players")
                                {
                                    foreach (System.Xml.XmlNode childnode3 in childnode2.ChildNodes)
                                    {
                                        preflopPosition += 1;
                                        playersCount += 1;
                                    }
                                }
                            }
                        }
                        else if (childnode.Name == "round" && childnode.Attributes["no"].Value == "1")
                        {
                            foreach (System.Xml.XmlNode childnode2 in childnode.ChildNodes)
                            {
                                if (childnode2.Name == "cards" && childnode2.Attributes["type"].Value == "Pocket")
                                {
                                    string handTextHoleCards = childnode2.InnerText;
                                    if (handTextHoleCards != "" && handTextHoleCards != "X X")
                                    {
                                        handTextHoleCards = handTextHoleCards.Replace("10", "T");
                                        string[] cards = System.Text.RegularExpressions.Regex.Split(handTextHoleCards, " ");
                                        cards[0] = cards[0].Substring(1) + cards[0].Substring(0, 1).ToLower();
                                        cards[1] = cards[1].Substring(0, 2);
                                        cards[1] = cards[1].Substring(1) + cards[1].Substring(0, 1).ToLower();
                                        name = childnode2.Attributes["player"].Value;
                                        if (names[name] == null)
                                        {
                                            r += 1;
                                            table.Rows.Add();
                                            table.Rows[r]["ID"] = xnode.Attributes["gamecode"].Value;
                                            table.Rows[r]["Name"] = name;
                                            names.Add(name, r);
                                            table.Rows[r]["Hand4"] = cards[0] + " " + cards[1]; 
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (names.Count == 0)
                return;

            string previosAction = "";
            string thisStreetActions = "";
            string street = "";
            string board = "";
            decimal bb = 0;
            decimal bank = 0;
            decimal streetStartBank = 0;
            decimal bet = 0;

            Hashtable acts = new Hashtable();  //0-fold 3-call 4-check 5-bet
            acts.Add("0", "fold");
            acts.Add("3", "call");
            acts.Add("4", "check");
            acts.Add("5", "bet");
            acts.Add("7", "raise");
            acts.Add("23", "raise");

            foreach (System.Xml.XmlNode xnode in xRoot)
            {
                if (xnode.Name == "game")
                {
                    foreach (System.Xml.XmlNode childnode in xnode.ChildNodes)
                    {
                        if (childnode.Name == "round" && childnode.Attributes["no"].Value == "0")
                        {
                            foreach (System.Xml.XmlNode childnode2 in childnode.ChildNodes)
                            {
                                bet = Convert.ToDecimal(childnode2.Attributes["sum"].Value);
                                bank += bet;
                                if (childnode2.Attributes["type"].Value == "2")
                                    bb = bet;
                            }
                        } // 0 (blinds)
                        else if (childnode.Name == "round" && childnode.Attributes["no"].Value == "1")
                        {
                            foreach (System.Xml.XmlNode childnode2 in childnode.ChildNodes)
                            {
                                if (childnode2.Name == "action")
                                {
                                    string act = childnode2.Attributes["type"].Value;
                                    act = (string)acts[act];
                                    preflopPosition -= 1;
                                    name = childnode2.Attributes["player"].Value;
                                    if (act == "fold")
                                    {
                                        playersCount -= 1;
                                        if (names[name] != null)
                                        {
                                            //r = (int)names[name];
                                            //table.Rows.RemoveAt(r);
                                            names.Remove(name);
                                        }
                                        //if (names.Count == 0)
                                        //    return;
                                        continue;
                                    }

                                    if (act == "call")
                                    {
                                        if (previosAction == "")
                                            previosAction = "limp";
                                        else if (previosAction == "raise")
                                            previosAction = "cold call";
                                        else if (previosAction == "3bet")
                                            previosAction = "call 3bet";
                                        else if (previosAction == "4bet")
                                            previosAction = "call 4bet";
                                    }
                                    else if (act == "raise")
                                    {
                                        if (previosAction == "" || previosAction == "limp")
                                            previosAction = "raise";
                                        else if (previosAction == "raise" || previosAction == "cold call")
                                            previosAction = "3bet";
                                        else if (previosAction == "3bet" || previosAction == "call 3bet")
                                            previosAction = "4bet";
                                    }
                                    else if (act == "check")
                                        previosAction = "check";
                                    else //отладка
                                        previosAction = previosAction;

                                    decimal value = Convert.ToDecimal(childnode2.Attributes["sum"].Value);
                                    bank += value;
                                    
                                    if (names[name] != null)
                                    {
                                        r = (int)names[name];
                                        if (table.Rows[r]["PreflopPos"].ToString() == "")
                                        {
                                            if (preflopPosition == 0)
                                                table.Rows[r]["PreflopPos"] = "BB";
                                            else if (preflopPosition == 1)
                                                table.Rows[r]["PreflopPos"] = "SB";
                                            else if (preflopPosition == 2)
                                                table.Rows[r]["PreflopPos"] = "BU";
                                            else if (preflopPosition == 3)
                                                table.Rows[r]["PreflopPos"] = "CO";
                                            else if (preflopPosition == 4)
                                                table.Rows[r]["PreflopPos"] = "MP3";
                                            else if (preflopPosition == 5)
                                                table.Rows[r]["PreflopPos"] = "MP2";
                                            else if (preflopPosition == 6)
                                                table.Rows[r]["PreflopPos"] = "MP1";
                                            else if (preflopPosition == 7)
                                                table.Rows[r]["PreflopPos"] = "UTG2";
                                            else if (preflopPosition == 8)
                                                table.Rows[r]["PreflopPos"] = "UTG1";
                                        }
                                        if (previosAction == "cold call" && (string)table.Rows[r]["PreflopAct"].ToString() == "limp")
                                            table.Rows[r]["PreflopAct"] = "limp, call";
                                        else
                                            table.Rows[r]["PreflopAct"] = previosAction;
                                        table.Rows[r]["PreflopBet"] = value / bb;
   
                                    }
                                }
                                
                            }
                        } // 1 (preflop)
                        else if (childnode.Name == "round" && childnode.Attributes["no"].Value == "2")
                        {
                            streetStartBank = bank;
                            thisStreetActions = "";
                            foreach (System.Collections.DictionaryEntry element in names)
                            {
                                name = (string)element.Key;
                                r = (int)names[name];
                                table.Rows[r]["FlopPlayersCount"] = playersCount;
                            }

                            foreach (System.Xml.XmlNode childnode2 in childnode.ChildNodes)
                            {
                                if (childnode2.Name == "action")
                                {
                                    string act = childnode2.Attributes["type"].Value;
                                    act = (string)acts[act];

                                    if (act == "check")
                                        previosAction = "check";
                                    else if (act == "bet")
                                        previosAction = "bet";
                                    else if (act == "fold")
                                    {
                                        playersCount -= 1;
                                        previosAction = "fold";
                                    }
                                    else if (act == "call")
                                    {
                                        if (previosAction == "raise")
                                            previosAction = "call raise";
                                        else
                                            previosAction = "call";
                                    }
                                    else if (act == "raise")
                                    {
                                        if (previosAction == "raise")
                                            previosAction = "3bet";
                                        else
                                            previosAction = "raise";
                                    }
                                    else //отладка
                                        previosAction = previosAction;

                                    bet = Convert.ToDecimal(childnode2.Attributes["sum"].Value);
                                    bank += bet;

                                    name = childnode2.Attributes["player"].Value;
                                    if (names[name] != null)
                                    {
                                        r = (int)names[name];
                                        if (table.Rows[r]["FlopAct"].ToString() != "")
                                        {
                                            table.Rows[r]["Pos"] = "OOP";
                                            table.Rows[r]["FlopAct"] += ", " + previosAction;
                                        }
                                        else
                                        {
                                            if (thisStreetActions == "")
                                                table.Rows[r]["Pos"] = "OOP";
                                            else if (!thisStreetActions.Contains("raise"))
                                                table.Rows[r]["Pos"] = "IP";
                                            table.Rows[r]["FlopAct"] = previosAction;
                                        }
                  
                                        table.Rows[r]["FlopBet"] = Math.Round(bet / streetStartBank, 1);
                                        
                                    }
                                    thisStreetActions += previosAction;
                                }
                                else if (childnode2.Name == "cards")
                                {
                                    string handTextHoleCards = childnode2.InnerText;
                                    handTextHoleCards = handTextHoleCards.Replace("10", "T");
                                    string[] cards = System.Text.RegularExpressions.Regex.Split(handTextHoleCards, " ");
                                    cards[0] = cards[0].Substring(1) + cards[0].Substring(0, 1).ToLower();
                                    cards[1] = cards[1].Substring(1) + cards[1].Substring(0, 1).ToLower();
                                    cards[2] = cards[2].Substring(1) + cards[2].Substring(0, 1).ToLower();
                                    board = cards[0] + " " + cards[1] + " " + cards[2];
                                }
                            }
                        } // 2 (flop)
                        else if (childnode.Name == "round" && childnode.Attributes["no"].Value == "3")
                        {
                            streetStartBank = bank;
                            thisStreetActions = "";
                            foreach (System.Collections.DictionaryEntry element in names)
                            {
                                name = (string)element.Key;
                                r = (int)names[name];
                                table.Rows[r]["TurnPlayersCount"] = playersCount;
                            }

                            foreach (System.Xml.XmlNode childnode2 in childnode.ChildNodes)
                            {
                                if (childnode2.Name == "action")
                                {
                                    string act = childnode2.Attributes["type"].Value;
                                    act = (string)acts[act];

                                    if (act == "check")
                                        previosAction = "check";
                                    else if (act == "bet")
                                        previosAction = "bet";
                                    else if (act == "fold")
                                    {
                                        playersCount -= 1;
                                        previosAction = "fold";
                                    }
                                    else if (act == "call")
                                    {
                                        if (previosAction == "raise")
                                            previosAction = "call raise";
                                        else
                                            previosAction = "call";
                                    }
                                    else if (act == "raise")
                                    {
                                        if (previosAction == "raise")
                                            previosAction = "3bet";
                                        else
                                            previosAction = "raise";
                                    }
                                    else //отладка
                                        previosAction = previosAction;

                                    bet = Convert.ToDecimal(childnode2.Attributes["sum"].Value);
                                    bank += bet;

                                    name = childnode2.Attributes["player"].Value;
                                    if (names[name] != null)
                                    {
                                        r = (int)names[name];
                                        if (table.Rows[r]["TurnAct"].ToString() != "")
                                        {
                                            table.Rows[r]["Pos"] = "OOP";
                                            table.Rows[r]["TurnAct"] += ", " + previosAction;
                                        }
                                        else
                                        {
                                            if (thisStreetActions == "")
                                                table.Rows[r]["Pos"] = "OOP";
                                            else if (!thisStreetActions.Contains("raise"))
                                                table.Rows[r]["Pos"] = "IP";
                                            table.Rows[r]["TurnAct"] = previosAction;
                                        }
                 
                                        table.Rows[r]["TurnBet"] = Math.Round(bet / streetStartBank, 1);
                                      
                                    }
                                    thisStreetActions += previosAction;
                                }
                                else if (childnode2.Name == "cards")
                                {
                                    string handTextHoleCards = childnode2.InnerText;
                                    handTextHoleCards = handTextHoleCards.Replace("10", "T");
                                    board += " " + handTextHoleCards.Substring(1) + handTextHoleCards.Substring(0, 1).ToLower();
                                }
                            }
                        } // 3 (turn)
                        else if (childnode.Name == "round" && childnode.Attributes["no"].Value == "4")
                        {
                            streetStartBank = bank;
                            thisStreetActions = "";
                            foreach (System.Collections.DictionaryEntry element in names)
                            {
                                name = (string)element.Key;
                                r = (int)names[name];
                                table.Rows[r]["RiverPlayersCount"] = playersCount;
                            }

                            foreach (System.Xml.XmlNode childnode2 in childnode.ChildNodes)
                            {
                                if (childnode2.Name == "action")
                                {
                                    string act = childnode2.Attributes["type"].Value;
                                    act = (string)acts[act];

                                    if (act == "check")
                                        previosAction = "check";
                                    else if (act == "bet")
                                        previosAction = "bet";
                                    else if (act == "fold")
                                    {
                                        playersCount -= 1;
                                        previosAction = "fold";
                                    }
                                    else if (act == "call")
                                    {
                                        if (previosAction == "raise")
                                            previosAction = "call raise";
                                        else
                                            previosAction = "call";
                                    }
                                    else if (act == "raise")
                                    {
                                        if (previosAction == "raise")
                                            previosAction = "3bet";
                                        else
                                            previosAction = "raise";
                                    }
                                    else //отладка
                                        previosAction = previosAction;

                                    bet = Convert.ToDecimal(childnode2.Attributes["sum"].Value);
                                    bank += bet;

                                    name = childnode2.Attributes["player"].Value;
                                    if (names[name] != null)
                                    {
                                        r = (int)names[name];
                                        if (table.Rows[r]["RiverAct"].ToString() != "")
                                        {
                                            table.Rows[r]["Pos"] = "OOP";
                                            table.Rows[r]["RiverAct"] += ", " + previosAction;
                                        }
                                        else
                                        {
                                            if (thisStreetActions == "")
                                                table.Rows[r]["Pos"] = "OOP";
                                            else if (!thisStreetActions.Contains("raise"))
                                                table.Rows[r]["Pos"] = "IP";
                                            table.Rows[r]["RiverAct"] = previosAction;
                                        }
                                  
                                        table.Rows[r]["RiverBet"] = Math.Round(bet / streetStartBank, 1);
                                        
                                    }
                                    thisStreetActions += previosAction;
                                }
                                else if (childnode2.Name == "cards")
                                {
                                    string handTextHoleCards = childnode2.InnerText;
                                    handTextHoleCards = handTextHoleCards.Replace("10", "T");
                                    board += " " + handTextHoleCards.Substring(1) + handTextHoleCards.Substring(0, 1).ToLower();
                                }
                            }
                        } // 4 (river)
                    }// rounds
                }//xnode.Name == "game"
            } //foreach (System.Xml.XmlNode xnode in xRoot) 

            foreach (System.Collections.DictionaryEntry element in names)
            {
                int iComb = 0;

                name = (string)element.Key;
                r = (int)names[name];
                string hand = (string)table.Rows[r]["Hand4"];
                string[] pocketHand = System.Text.RegularExpressions.Regex.Split(hand, " ");
                if (hand.Substring(1, 1) != hand.Substring(4, 1) && hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "o";
                else if (hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "s";
                else
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1);
                if (hand[0].ToString() != hand[1].ToString())
                {
                    string card1 = hand[0].ToString();
                    card1 = card1.Replace("A", "14"); card1 = card1.Replace("K", "13"); card1 = card1.Replace("Q", "12"); card1 = card1.Replace("J", "11"); card1 = card1.Replace("T", "10");
                    string card2 = hand[1].ToString();
                    card2 = card2.Replace("A", "14"); card2 = card2.Replace("K", "13"); card2 = card2.Replace("Q", "12"); card2 = card2.Replace("J", "11"); card2 = card2.Replace("T", "10");
                    if (Convert.ToInt32(card1) < Convert.ToInt32(card2))
                        hand = hand.Substring(1, 1) + hand.Substring(0, 1) + hand.Substring(2, 1);
                }

                table.Rows[r]["hand3"] = hand;
                table.Rows[r]["board"] = board;

                string combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 8), " "));
                string[] array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                if (array.Count() == 1)
                    iComb = Convert.ToInt32(array[0]);
                else
                {
                    iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                    table.Rows[r]["FlopComb"] = array[0];
                }
                table.Rows[r]["iFlopComb"] = iComb;

                if (board.Length >= 11)
                {
                    combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 11), " "));
                    array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                    if (array.Count() == 1)
                        iComb = Convert.ToInt32(array[0]);
                    else
                    {
                        iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                        table.Rows[r]["TurnComb"] = array[0];
                    }
                    table.Rows[r]["iTurnComb"] = iComb;
                }

                if (board.Length >= 14)
                {
                    combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 14), " "));
                    array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                    if (array.Count() == 1)
                        iComb = Convert.ToInt32(array[0]);
                    else
                    {
                        iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                        table.Rows[r]["RiverComb"] = array[0];
                    }
                    table.Rows[r]["iRiverComb"] = iComb;
                }
            }

            for (int i = table.Rows.Count-1; i >= 0; i--)
            {
                name = (string)table.Rows[i]["Name"];
                if (names[name] == null)
                    table.Rows.RemoveAt(i);
                    
            }

                

        }

        private void ReadHandIPokerPT4(String handText)
        {
                        
            System.Collections.Hashtable names = new System.Collections.Hashtable();
            string[] lines = System.Text.RegularExpressions.Regex.Split(handText, "\r\n");
            if (lines.Count() < 3)
                lines = System.Text.RegularExpressions.Regex.Split(handText, "\n");
            int r = table.Rows.Count - 1;
            string street = "";
            int preflopPosition = 1;
            int playersCount = 0;
            string name;

            foreach (string line in lines)
            {
                if (line.Contains(" Shows ") || line.Contains(" Mucks "))
                {

                    name = line.Substring(0, line.IndexOf(":"));
                    if (names[name] == null)
                    {
                        r += 1;
                        table.Rows.Add();
                        table.Rows[r]["ID"] = handText.Substring(handText.IndexOf("#") + 1, 10) + " " + name;
                        string hand = "";
                        if (line.Contains(" Shows "))
                            hand = line.Substring(line.IndexOf(" Shows ") + 8);
                        else
                            hand = line.Substring(line.IndexOf(" Mucks ") + 8);
                        hand = hand.Replace("]", "");
                        hand = hand.Replace("10", "T");
                        hand = hand.Substring(1, 1) + hand.Substring(0, 1).ToLower() + " " + hand.Substring(4, 1) + hand.Substring(3, 1).ToLower();
                        table.Rows[r]["Hand4"] = hand;
                        table.Rows[r]["Name"] = name;
                        table.Rows[r]["FlopBet"] = 0;
                        table.Rows[r]["TurnBet"] = 0;
                        table.Rows[r]["RiverBet"] = 0;
                        names.Add(name, r);
                    }
                }
                if (line.Contains(" in chips)")) // && !line.Contains("[Sitting out]")
                {
                    preflopPosition += 1;
                    playersCount += 1;
                }
            }
            if (names.Count == 0)
                return;

            string previosAction = "";
            string thisStreetActions = "";
            street = "";
            string board = "";
            decimal bb = 0;
            decimal bank = 0;
            decimal streetStartBank = 0;
            decimal bet = 0;
            foreach (string line in lines)
            {
                if (line.Contains("*** HOLE CARDS ***"))
                {
                    street = "preflop";
                    continue;
                }
                else if (line.Contains("*** FLOP ***"))
                {
                    street = "flop";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board = line.Substring(14);
                    board = board.Replace("]", "");
                    board = board.Replace("10", "T");
                    board = board.Substring(1, 1) + board.Substring(0, 1).ToLower() + " " + board.Substring(4, 1) + board.Substring(3, 1).ToLower() + " " + board.Substring(7, 1) + board.Substring(6, 1).ToLower();

                    foreach (System.Collections.DictionaryEntry element in names)
                    {
                        name = (string)element.Key;
                        r = (int)names[name];
                        table.Rows[r]["FlopPlayersCount"] = playersCount;
                    }
                }
                else if (line.Contains("*** TURN ***"))
                {
                    street = "turn";
                    previosAction = "";
                    thisStreetActions = "";
                    streetStartBank = bank;
                    bet = 0;
                    string card = line.Substring(14);
                    card = card.Replace("]", "");
                    card = card.Replace("10", "T");
                    card = card.Substring(1, 1) + card.Substring(0, 1).ToLower();
                    board += " " + card;
                    foreach (System.Collections.DictionaryEntry element in names)
                    {
                        name = (string)element.Key;
                        r = (int)names[name];
                        table.Rows[r]["TurnPlayersCount"] = playersCount;
                    }
                }
                else if (line.Contains("*** RIVER ***"))
                {
                    street = "river";
                    previosAction = "";
                    thisStreetActions = "";
                    streetStartBank = bank;
                    bet = 0;
                    string card = line.Substring(15);
                    card = card.Replace("]", "");
                    card = card.Replace("10", "T");
                    card = card.Substring(1, 1) + card.Substring(0, 1).ToLower();
                    board += " " + card;
                    foreach (System.Collections.DictionaryEntry element in names)
                    {
                        name = (string)element.Key;
                        r = (int)names[name];
                        table.Rows[r]["RiverPlayersCount"] = playersCount;
                    }
                }
                else if (street == "" && line.Contains(": Post "))
                {
                    if (line.Contains(" Post BB "))
                    {
                        bb = Convert.ToDecimal(line.Substring(line.IndexOf("€") + 1));
                        bank += bb;
                    }
                    else if (line.Contains(" Post SB "))
                    {
                        bank += Convert.ToDecimal(line.Substring(line.IndexOf("€") + 1));
                    }
                }

                #region PREFLOP
                else if (street == "preflop")
                {
                    if (line.Contains(" Call ") || line.Contains(" Raise ") || line.Contains(" Check") || line.Contains(" Fold"))
                        preflopPosition -= 1;
                    if (line.Contains(" Fold"))
                        playersCount -= 1;
                    if (line.Contains(" Call "))
                    {
                        if (previosAction == "")
                            previosAction = "limp";
                        else if (previosAction.Contains("raise"))
                            previosAction = "cold call";
                        else if (previosAction == "3bet")
                            previosAction = "call 3bet";
                        else if (previosAction == "4bet")
                            previosAction = "call 4bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" Call ") + 7));
                        bank += bet;
                    }
                    else if (line.Contains(" Raise "))
                    {
                        if (previosAction == "")
                            previosAction = "openraise";
                        else if (previosAction == "limp")
                            previosAction = "raise";
                        else if (previosAction == "raise" || previosAction == "cold call")
                            previosAction = "3bet";
                        else if (previosAction == "3bet" || previosAction == "call 3bet")
                            previosAction = "4bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf("€") + 1));
                        bank += bet;
                    }
                    else if (line.Contains(" Check"))
                    {
                        previosAction = "check";
                    }
                    else if (line.Contains(" went all-in "))
                    {
                        previosAction = "raise";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" went all-in ") + 15));
                        bank += bet;
                    }

                    if (line.Contains(" Call ") || line.Contains(" Raise ") || line.Contains(" Check") || line.Contains(" went all-in "))
                    {
                        name = line.Substring(0, line.IndexOf(":"));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["PreflopAct"].ToString() == "limp" && line.Contains(" raised "))
                                table.Rows[r]["PreflopAct"] = "limp, raise";
                            else
                                table.Rows[r]["PreflopAct"] = previosAction;
                            table.Rows[r]["PreflopBet"] = Convert.ToInt32(bet / bb);

                            if (table.Rows[r]["PreflopPos"].ToString() == "")
                            {
                                if (preflopPosition == 1)
                                    table.Rows[r]["PreflopPos"] = "BB";
                                else if (preflopPosition == 2)
                                    table.Rows[r]["PreflopPos"] = "SB";
                                else if (preflopPosition == 3)
                                    table.Rows[r]["PreflopPos"] = "BU";
                                else if (preflopPosition == 4)
                                    table.Rows[r]["PreflopPos"] = "CO";
                                else if (preflopPosition == 5)
                                    table.Rows[r]["PreflopPos"] = "MP3";
                                else if (preflopPosition == 6)
                                    table.Rows[r]["PreflopPos"] = "MP2";
                                else if (preflopPosition == 7)
                                    table.Rows[r]["PreflopPos"] = "MP1";
                                else if (preflopPosition == 8)
                                    table.Rows[r]["PreflopPos"] = "UTG2";
                                else if (preflopPosition == 9)
                                    table.Rows[r]["PreflopPos"] = "UTG1";
                            }
                        }
                    }
                }//if (street == "preflop")
                #endregion

                #region FLOP
                else if (street == "flop")
                {
                    if (line.Contains(" Check"))
                        previosAction = "check";
                    else if (line.Contains(" Bet "))
                    {
                        previosAction = "bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" Bet ") + 6));
                        bank += bet;
                    }
                    else if (line.Contains(" Call "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" Call ") + 7));
                        bank += bet;
                    }
                    else if (line.Contains(" Raise "))
                    {
                        if (previosAction == "raise")
                            previosAction = "3bet";
                        else
                            previosAction = "raise";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf("€") + 1));
                        bank += bet;
                    }
                    else if (line.Contains(" went all-in "))
                    {
                        previosAction = "bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" went all-in ") + 15));
                        bank += bet;
                    }
                    if (line.Contains(" Check") || line.Contains(" Bet ") || line.Contains(" Call ") || line.Contains(" Raise ") || line.Contains(" Fold") || line.Contains(" went all-in "))
                    {
                        name = line.Substring(0, line.IndexOf(":"));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["FlopAct"].ToString() == "check")
                            {
                                table.Rows[r]["Pos"] = "OOP";
                                if (line.Contains(" folded"))
                                    table.Rows[r]["FlopAct"] += ", fold";
                                else
                                    table.Rows[r]["FlopAct"] += ", " + previosAction;
                            }
                            else
                            {
                                if (thisStreetActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!thisStreetActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                                if (line.Contains(" Fold"))
                                    table.Rows[r]["FlopAct"] += "fold";
                                else
                                    table.Rows[r]["FlopAct"] = previosAction;
                            }
                            table.Rows[r]["FlopBet"] = Math.Round(bet / streetStartBank, 1);
                        }
                    }
                    if (line.Contains(" Fold"))
                        playersCount -= 1;
                    thisStreetActions += previosAction;
                }//if (street == "flop")
                #endregion

                #region TURN
                else if (street == "turn")
                {
                    if (line.Contains(" Check"))
                        previosAction = "check";
                    else if (line.Contains(" Bet "))
                    {
                        previosAction = "bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" Bet ") + 6));
                        bank += bet;
                    }
                    else if (line.Contains(" Call "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" Call ") + 7));
                        bank += bet;
                    }
                    else if (line.Contains(" Raise "))
                    {
                        if (previosAction == "raise")
                            previosAction = "3bet";
                        else
                            previosAction = "raise";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf("€") + 1));
                        bank += bet;
                    }
                    else if (line.Contains(" went all-in "))
                    {
                        previosAction = "bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" went all-in ") + 15));
                        bank += bet;
                    }
                    if (line.Contains(" Bet ") || line.Contains(" Check") || line.Contains(" Raise ") || line.Contains(" Call ") || line.Contains(" Fold") || line.Contains(" went all-in "))
                    {
                        name = line.Substring(0, line.IndexOf(":"));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["TurnAct"].ToString() == "check")
                            {
                                table.Rows[r]["Pos"] = "OOP";
                                if (line.Contains(" Fold"))
                                    table.Rows[r]["TurnAct"] += ", fold";
                                else
                                    table.Rows[r]["TurnAct"] += ", " + previosAction;
                            }
                            else
                            {
                                if (thisStreetActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!thisStreetActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                                if (line.Contains(" Fold"))
                                    table.Rows[r]["TurnAct"] += "fold";
                                else
                                    table.Rows[r]["TurnAct"] = previosAction;
                            }
                            table.Rows[r]["TurnBet"] = Math.Round(bet / streetStartBank, 1);
                        }
                    }
                    if (line.Contains(" Fold"))
                        playersCount -= 1;
                    thisStreetActions += previosAction;
                }//if (street == "turn")
                #endregion

                #region RIVER
                else if (street == "river")
                {
                    if (line.Contains(" Check"))
                        previosAction = "check";
                    else if (line.Contains(" Bet "))
                    {
                        previosAction = "bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" Bet ") + 6));
                        bank += bet;
                    }
                    else if (line.Contains(" Call "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" Call ") + 7));
                        bank += bet;
                    }
                    else if (line.Contains(" Raise "))
                    {
                        if (previosAction == "raise")
                            previosAction = "3bet";
                        else
                            previosAction = "raise";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf("€") + 1));
                        bank += bet;
                    }
                    else if (line.Contains(" went all-in "))
                    {
                        previosAction = "bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" went all-in ") + 15));
                        bank += bet;
                    }
                    if (line.Contains(" Bet ") || line.Contains(" Check") || line.Contains(" Raise ") || line.Contains(" Call ") || line.Contains(" Fold") || line.Contains(" went all-in "))
                    {
                        name = line.Substring(0, line.IndexOf(":"));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["RiverAct"].ToString() == "check")
                            {
                                table.Rows[r]["Pos"] = "OOP";
                                if (line.Contains(" Fold"))
                                    table.Rows[r]["RiverAct"] += ", fold";
                                else
                                    table.Rows[r]["RiverAct"] += ", " + previosAction;
                            }
                            else
                            {
                                if (thisStreetActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!thisStreetActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                                if (line.Contains(" Fold"))
                                    table.Rows[r]["RiverAct"] += "fold";
                                else
                                    table.Rows[r]["RiverAct"] = previosAction;
                            }
                            table.Rows[r]["RiverBet"] = Math.Round(bet / streetStartBank, 1);
                        }
                    }
                    if (line.Contains(" Fold"))
                        playersCount -= 1;
                    thisStreetActions += previosAction;
                }//if (street == "river")
                #endregion


            }// foreach (string line in lines)


            if (board == "")
                return;

            foreach (System.Collections.DictionaryEntry element in names)
            {
                int iComb = 0;

                name = (string)element.Key;
                r = (int)names[name];
                string hand = (string)table.Rows[r]["Hand4"];
                string[] pocketHand = System.Text.RegularExpressions.Regex.Split(hand, " ");
                if (hand.Substring(1, 1) != hand.Substring(4, 1) && hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "o";
                else if (hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "s";
                else
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1);
                if (hand[0].ToString() != hand[1].ToString())
                {
                    string card1 = hand[0].ToString();
                    card1 = card1.Replace("A", "14"); card1 = card1.Replace("K", "13"); card1 = card1.Replace("Q", "12"); card1 = card1.Replace("J", "11"); card1 = card1.Replace("T", "10");
                    string card2 = hand[1].ToString();
                    card2 = card2.Replace("A", "14"); card2 = card2.Replace("K", "13"); card2 = card2.Replace("Q", "12"); card2 = card2.Replace("J", "11"); card2 = card2.Replace("T", "10");
                    if (Convert.ToInt32(card1) < Convert.ToInt32(card2))
                        hand = hand.Substring(1, 1) + hand.Substring(0, 1) + hand.Substring(2, 1);
                }

                table.Rows[r]["hand3"] = hand;
                table.Rows[r]["board"] = board;

                string combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 8), " "));
                string[] array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                if (array.Count() == 1)
                    iComb = Convert.ToInt32(array[0]);
                else
                {
                    iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                    table.Rows[r]["FlopComb"] = array[0];
                }
                table.Rows[r]["iFlopComb"] = iComb;

                if (board.Length >= 11)
                {
                    combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 11), " "));
                    array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                    if (array.Count() == 1)
                        iComb = Convert.ToInt32(array[0]);
                    else
                    {
                        iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                        table.Rows[r]["TurnComb"] = array[0];
                    }
                    table.Rows[r]["iTurnComb"] = iComb;
                }

                if (board.Length >= 14)
                {
                    combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 14), " "));
                    array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                    if (array.Count() == 1)
                        iComb = Convert.ToInt32(array[0]);
                    else
                    {
                        iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                        table.Rows[r]["RiverComb"] = array[0];
                    }
                    table.Rows[r]["iRiverComb"] = iComb;
                }
            }

        }

        private void ReadHandIPoker2PT4(String handText)
        {

            System.Collections.Hashtable names = new System.Collections.Hashtable();
            string[] lines = System.Text.RegularExpressions.Regex.Split(handText, "\r\n");
            if (lines.Count() < 3)
                lines = System.Text.RegularExpressions.Regex.Split(handText, "\n");
            int r = table.Rows.Count - 1;
            string street = "";
            int preflopPosition = 1;
            int playersCount = 0;
            string name;

            foreach (string line in lines)
            {
                if (line.Contains(" Shows ") || line.Contains(" Mucks "))
                {

                    name = line.Substring(0, line.IndexOf(":"));
                    if (names[name] == null)
                    {
                        r += 1;
                        table.Rows.Add();
                        table.Rows[r]["ID"] = handText.Substring(handText.IndexOf("#") + 4, 10) + " " + name;
                        string hand = "";
                        if (line.Contains(" Shows "))
                            hand = line.Substring(line.IndexOf(" Shows ") + 8);
                        else
                            hand = line.Substring(line.IndexOf(" Mucks ") + 8);
                        hand = hand.Substring(0, hand.IndexOf("]"));
                        table.Rows[r]["Hand4"] = hand;
                        table.Rows[r]["Name"] = name;
                        table.Rows[r]["FlopBet"] = 0;
                        table.Rows[r]["TurnBet"] = 0;
                        table.Rows[r]["RiverBet"] = 0;
                        names.Add(name, r);
                    }
                }
                if (line.Contains(" sitting in seat ")) // && !line.Contains("[Sitting out]")
                {
                    preflopPosition += 1;
                    playersCount += 1;
                }
            }
            if (names.Count == 0)
                return;

            string previosAction = "";
            string thisStreetActions = "";
            street = "";
            string board = "";
            decimal bb = 0;
            decimal bank = 0;
            decimal streetStartBank = 0;
            decimal bet = 0;
            foreach (string line in lines)
            {
                if (line.Contains("** Dealing card "))
                {
                    street = "preflop";
                    continue;
                }
                else if (line.Contains("** Dealing the flop:"))
                {
                    street = "flop";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board = line.Substring(21);
                    board = board.Replace(" of ", "");
                    board = board.Replace(",", "");
                    board = board.Replace("10", "T");

                    foreach (System.Collections.DictionaryEntry element in names)
                    {
                        name = (string)element.Key;
                        r = (int)names[name];
                        table.Rows[r]["FlopPlayersCount"] = playersCount;
                    }
                }
                else if (line.Contains("** Dealing the turn:"))
                {
                    street = "turn";
                    previosAction = "";
                    thisStreetActions = "";
                    streetStartBank = bank;
                    bet = 0;
                    string card = line.Substring(21);
                    card = card.Replace(" of ", "");
                    card = card.Replace(",", "");
                    card = card.Replace("10", "T");
                    board += " " + card;
                    foreach (System.Collections.DictionaryEntry element in names)
                    {
                        name = (string)element.Key;
                        r = (int)names[name];
                        table.Rows[r]["TurnPlayersCount"] = playersCount;
                    }
                }
                else if (line.Contains("** Dealing the river:"))
                {
                    street = "river";
                    previosAction = "";
                    thisStreetActions = "";
                    streetStartBank = bank;
                    bet = 0;
                    string card = line.Substring(22);
                    card = card.Replace(" of ", "");
                    card = card.Replace(",", "");
                    card = card.Replace("10", "T");
                    board += " " + card;
                    foreach (System.Collections.DictionaryEntry element in names)
                    {
                        name = (string)element.Key;
                        r = (int)names[name];
                        table.Rows[r]["RiverPlayersCount"] = playersCount;
                    }
                }
                else if (street == "" && line.Contains(" posted the "))
                {
                    if (line.Contains(" posted the small blind "))
                    {
                        bb = Convert.ToDecimal(line.Substring(line.IndexOf(" posted the small blind ") + 26));
                        bank += bb;
                    }
                    else if (line.Contains(" posted the big blind "))
                    {
                        bank += Convert.ToDecimal(line.Substring(line.IndexOf(" posted the big blind ") + 24));
                    }
                }

                #region PREFLOP
                else if (street == "preflop")
                {
                    if (line.Contains(" called ") || line.Contains(" raised ") || line.Contains(" checked") || line.Contains(" folded"))
                        preflopPosition -= 1;
                    if (line.Contains(" folded"))
                        playersCount -= 1;
                    if (line.Contains(" called "))
                    {
                        if (previosAction == "")
                            previosAction = "limp";
                        else if (previosAction == "raise")
                            previosAction = "cold call";
                        else if (previosAction == "3bet")
                            previosAction = "call 3bet";
                        else if (previosAction == "4bet")
                            previosAction = "call 4bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" called ") + 10));
                        bank += bet;
                    }
                    else if (line.Contains(" raised "))
                    {
                        if (previosAction == "" || previosAction == "limp")
                            previosAction = "raise";
                        else if (previosAction == "raise" || previosAction == "cold call")
                            previosAction = "3bet";
                        else if (previosAction == "3bet" || previosAction == "call 3bet")
                            previosAction = "4bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raised ") + 10));
                        bank += bet;
                    }
                    else if (line.Contains(" checked"))
                    {
                        previosAction = "check";
                    }
                    else if (line.Contains(" went all-in "))
                    {
                        previosAction = "raise";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" went all-in ") + 15));
                        bank += bet;
                    }

                    if (line.Contains(" called ") || line.Contains(" raised ") || line.Contains(" checked") || line.Contains(" went all-in "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["PreflopAct"].ToString() == "limp" && line.Contains(" raised "))
                                table.Rows[r]["PreflopAct"] = "limp, raise";
                            else
                                table.Rows[r]["PreflopAct"] = previosAction;
                            table.Rows[r]["PreflopBet"] = bet / bb;

                            if (table.Rows[r]["PreflopPos"].ToString() == "")
                            {
                                if (preflopPosition == 1)
                                    table.Rows[r]["PreflopPos"] = "BB";
                                else if (preflopPosition == 2)
                                    table.Rows[r]["PreflopPos"] = "SB";
                                else if (preflopPosition == 3)
                                    table.Rows[r]["PreflopPos"] = "BU";
                                else if (preflopPosition == 4)
                                    table.Rows[r]["PreflopPos"] = "CO";
                                else if (preflopPosition == 5)
                                    table.Rows[r]["PreflopPos"] = "MP3";
                                else if (preflopPosition == 6)
                                    table.Rows[r]["PreflopPos"] = "MP2";
                                else if (preflopPosition == 7)
                                    table.Rows[r]["PreflopPos"] = "MP1";
                                else if (preflopPosition == 8)
                                    table.Rows[r]["PreflopPos"] = "UTG2";
                                else if (preflopPosition == 9)
                                    table.Rows[r]["PreflopPos"] = "UTG1";
                            }
                        }
                    }
                }//if (street == "preflop")
                #endregion

                #region FLOP
                else if (street == "flop")
                {
                    if (line.Contains(" checked"))
                        previosAction = "check";
                    else if (line.Contains(" bet "))
                    {
                        previosAction = "bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bet ") + 7));
                        bank += bet;
                    }
                    else if (line.Contains(" called "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" called ") + 10));
                        bank += bet;
                    }
                    else if (line.Contains(" raised "))
                    {
                        if (previosAction == "raise")
                            previosAction = "3bet";
                        else
                            previosAction = "raise";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raised ") + 10));
                        bank += bet;
                    }
                    else if (line.Contains(" went all-in "))
                    {
                        previosAction = "bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" went all-in ") + 15));
                        bank += bet;
                    }
                    if (line.Contains(" checked") || line.Contains(" bet ") || line.Contains(" called ") || line.Contains(" raised ") || line.Contains(" folded") || line.Contains(" went all-in "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["FlopAct"].ToString() == "check")
                            {
                                table.Rows[r]["Pos"] = "OOP";
                                if (line.Contains(" folded"))
                                    table.Rows[r]["FlopAct"] += ", fold";
                                else
                                    table.Rows[r]["FlopAct"] += ", " + previosAction;
                            }
                            else
                            {
                                if (thisStreetActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!thisStreetActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                                if (line.Contains(" folded"))
                                    table.Rows[r]["FlopAct"] += "fold";
                                else
                                    table.Rows[r]["FlopAct"] = previosAction;
                            }
                            table.Rows[r]["FlopBet"] = Math.Round(bet / streetStartBank, 1);
                        }
                    }
                    if (line.Contains(" folded"))
                        playersCount -= 1;
                    thisStreetActions += previosAction;
                }//if (street == "flop")
                #endregion

                #region TURN
                else if (street == "turn")
                {
                    if (line.Contains(" checked"))
                        previosAction = "check";
                    else if (line.Contains(" bet "))
                    {
                        previosAction = "bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bet ") + 7));
                        bank += bet;
                    }
                    else if (line.Contains(" called "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" called ") + 10));
                        bank += bet;
                    }
                    else if (line.Contains(" raised "))
                    {
                        if (previosAction == "raise")
                            previosAction = "3bet";
                        else
                            previosAction = "raise";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raised ") + 10));
                        bank += bet;
                    }
                    else if (line.Contains(" went all-in "))
                    {
                        previosAction = "bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" went all-in ") + 15));
                        bank += bet;
                    }
                    if (line.Contains(" bet ") || line.Contains(" checked") || line.Contains(" raised ") || line.Contains(" called ") || line.Contains(" folded") || line.Contains(" went all-in "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["TurnAct"].ToString() == "check")
                            {
                                table.Rows[r]["Pos"] = "OOP";
                                if (line.Contains(" folded"))
                                    table.Rows[r]["TurnAct"] += ", fold";
                                else
                                    table.Rows[r]["TurnAct"] += ", " + previosAction;
                            }
                            else
                            {
                                if (thisStreetActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!thisStreetActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                                if (line.Contains(" folded"))
                                    table.Rows[r]["TurnAct"] += "fold";
                                else
                                    table.Rows[r]["TurnAct"] = previosAction;
                            }
                            table.Rows[r]["TurnBet"] = Math.Round(bet / streetStartBank, 1);
                        }
                    }
                    if (line.Contains(" folded"))
                        playersCount -= 1;
                    thisStreetActions += previosAction;
                }//if (street == "turn")
                #endregion

                #region RIVER
                else if (street == "river")
                {
                    if (line.Contains(" checked"))
                        previosAction = "check";
                    else if (line.Contains(" bet "))
                    {
                        previosAction = "bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bet ") + 7));
                        bank += bet;
                    }
                    else if (line.Contains(" called "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" called ") + 10));
                        bank += bet;
                    }
                    else if (line.Contains(" raised "))
                    {
                        if (previosAction == "raise")
                            previosAction = "3bet";
                        else
                            previosAction = "raise";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raised ") + 10));
                        bank += bet;
                    }
                    else if (line.Contains(" went all-in "))
                    {
                        previosAction = "bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" went all-in ") + 15));
                        bank += bet;
                    }
                    if (line.Contains(" bet ") || line.Contains(" checked") || line.Contains(" raised ") || line.Contains(" called ") || line.Contains(" folded") || line.Contains(" went all-in "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["RiverAct"].ToString() == "check")
                            {
                                table.Rows[r]["Pos"] = "OOP";
                                if (line.Contains(" folded"))
                                    table.Rows[r]["RiverAct"] += ", fold";
                                else
                                    table.Rows[r]["RiverAct"] += ", " + previosAction;
                            }
                            else
                            {
                                if (thisStreetActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!thisStreetActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                                if (line.Contains(" folded"))
                                    table.Rows[r]["RiverAct"] += "fold";
                                else
                                    table.Rows[r]["RiverAct"] = previosAction;
                            }
                            table.Rows[r]["RiverBet"] = Math.Round(bet / streetStartBank, 1);
                        }
                    }
                    if (line.Contains(" folded"))
                        playersCount -= 1;
                    thisStreetActions += previosAction;
                }//if (street == "river")
                #endregion


            }// foreach (string line in lines)

            foreach (System.Collections.DictionaryEntry element in names)
            {
                int iComb = 0;

                name = (string)element.Key;
                r = (int)names[name];
                string hand = (string)table.Rows[r]["Hand4"];
                string[] pocketHand = System.Text.RegularExpressions.Regex.Split(hand, " ");
                if (hand.Substring(1, 1) != hand.Substring(4, 1) && hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "o";
                else if (hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "s";
                else
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1);
                if (hand[0].ToString() != hand[1].ToString())
                {
                    string card1 = hand[0].ToString();
                    card1 = card1.Replace("A", "14"); card1 = card1.Replace("K", "13"); card1 = card1.Replace("Q", "12"); card1 = card1.Replace("J", "11"); card1 = card1.Replace("T", "10");
                    string card2 = hand[1].ToString();
                    card2 = card2.Replace("A", "14"); card2 = card2.Replace("K", "13"); card2 = card2.Replace("Q", "12"); card2 = card2.Replace("J", "11"); card2 = card2.Replace("T", "10");
                    if (Convert.ToInt32(card1) < Convert.ToInt32(card2))
                        hand = hand.Substring(1, 1) + hand.Substring(0, 1) + hand.Substring(2, 1);
                }

                table.Rows[r]["hand3"] = hand;
                table.Rows[r]["board"] = board;

                string combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 8), " "));
                string[] array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                if (array.Count() == 1)
                    iComb = Convert.ToInt32(array[0]);
                else
                {
                    iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                    table.Rows[r]["FlopComb"] = array[0];
                }
                table.Rows[r]["iFlopComb"] = iComb;

                if (board.Length >= 11)
                {
                    combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 11), " "));
                    array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                    if (array.Count() == 1)
                        iComb = Convert.ToInt32(array[0]);
                    else
                    {
                        iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                        table.Rows[r]["TurnComb"] = array[0];
                    }
                    table.Rows[r]["iTurnComb"] = iComb;
                }

                if (board.Length >= 14)
                {
                    combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 14), " "));
                    array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                    if (array.Count() == 1)
                        iComb = Convert.ToInt32(array[0]);
                    else
                    {
                        iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                        table.Rows[r]["RiverComb"] = array[0];
                    }
                    table.Rows[r]["iRiverComb"] = iComb;
                }
            }

        }

        private void ReadHandGTechPT4(String handText)
        {
            System.Collections.Hashtable names = new System.Collections.Hashtable();
            string[] lines = System.Text.RegularExpressions.Regex.Split(handText, "\r\n");
            if (lines.Count() < 3)
                lines = System.Text.RegularExpressions.Regex.Split(handText, "\n");
            int r = table.Rows.Count - 1;
            int preflopPosition = 1;
            int playersCount = 0;
            string name = "";
            string street = "";
            string preflopActNames = " ";

            foreach (string line in lines)
            {
                if (line.Contains(" showed ["))
                {
                    name = line.Substring(0, line.IndexOf(" "));
                    if (names[name] == null)
                    {
                        r += 1;
                        table.Rows.Add();
                        table.Rows[r]["id"] = handText.Substring(handText.IndexOf("Game #") + 6, 9) + " " + name;
                        table.Rows[r]["hand4"] = line.Substring(line.IndexOf(" showed [") + 9, 5);
                        table.Rows[r]["name"] = name;
                        names.Add(name, r);
                    }
                }
                else if (line.Contains(" mucked ["))
                {
                    name = line.Substring(line.IndexOf(":") + 2);
                    name = name.Substring(0, name.IndexOf(" "));
                    if (names[name] == null)
                    {
                        r += 1;
                        table.Rows.Add();
                        table.Rows[r]["id"] = handText.Substring(handText.IndexOf("Hand #") + 6, 12) + " " + name;
                        table.Rows[r]["hand4"] = line.Substring(line.IndexOf(" mucked [") + 9, 5);
                        table.Rows[r]["name"] = name;
                        names.Add(name, r);
                    }
                }
                else if (line == "*** HOLE CARDS ***")
                {
                    street = "preflop";
                }
                else if (line.Contains("*** FLOP ***"))
                {
                    street = "flop";
                }
                else if (street == "preflop")
                {
                    if (line.Contains(" folds") || line.Contains(" calls ") || line.Contains(" raises ") || line.Contains(" checks") || line.Contains(" goes all-in"))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        if (!preflopActNames.Contains(" " + name + " "))
                        {
                            preflopActNames += name + " ";
                            preflopPosition += 1;
                            playersCount += 1;
                        }
                    }
                }
                //else if (line.Length >= 5 && line.Substring(0, 5) == "Seat ")
                //{
                //    preflopPosition += 1;
                //    playersCount += 1;
                //}
            }
            if (names.Count == 0)
                return;

            string previosAction = "";
            string flopActions = "";
            string turnActions = "";
            string riverActions = "";
            string[] pocketHand = { "", "" };
            street = "";
            string board = "";
            decimal bb = 0;
            decimal bank = 0;
            decimal streetStartBank = 0;
            decimal bet = 0;
            bool push = true;
            foreach (string line in lines)
            {
                if (line == "*** HOLE CARDS ***")
                {
                    street = "preflop";
                    continue;
                }
                else if (line.Contains("*** FLOP ***"))
                {
                    street = "flop";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board = line.Substring(13);
                }
                else if (line.Contains("*** TURN ***"))
                {
                    street = "turn";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board = line.Substring(13);
                    for (int r1 = 0; r1 < table.Rows.Count; r1++)
                        if (table.Rows[r1]["flopAct"].ToString() != "")
                            push = false;
                }
                else if (line.Contains("*** RIVER ***"))
                {
                    street = "river";
                    previosAction = "";
                    streetStartBank = bank;
                    bet = 0;
                    board = line.Substring(14);
                }
                else if (line == "*** SUMMARY ***")
                    street = "SUMMARY";

                if (street == "" && line.Contains(" posts "))
                {
                    if (line.Contains(" posts the big blind "))
                    {
                        bb = Convert.ToDecimal(line.Substring(line.IndexOf(" posts the big blind ") + 26));
                        bank += bb;
                    }
                    else if (line.Contains(" posts the small blind "))
                    {
                        bank += Convert.ToDecimal(line.Substring(line.IndexOf(" posts the small blind ") + 28));
                    }
                }

                #region PREFLOP
                if (street == "preflop")
                {
                    if (line.Contains(" calls ") || line.Contains(" raises ") || line.Contains(" checks") || line.Contains(" folds") || line.Contains(" goes all-in "))
                        preflopPosition -= 1;
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    if (line.Contains(" calls "))
                    {
                        if (previosAction == "")
                            previosAction = "limp";
                        else if (previosAction == "openraise" || previosAction == "raise")
                            previosAction = "cold call";
                        else if (previosAction == "3bet")
                            previosAction = "call 3bet";
                        else if (previosAction == "4bet")
                            previosAction = "call 4bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 9));
                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        if (previosAction == "")
                            previosAction = "openraise";
                        else if (previosAction == "limp")
                            previosAction = "raise";
                        else if (previosAction == "openraise" || previosAction == "raise" || previosAction == "cold call")
                            previosAction = "3bet";
                        else if (previosAction == "3bet" || previosAction == "call 3bet")
                            previosAction = "4bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 10));
                        bank += bet;
                    }
                    else if (line.Contains(" goes all-in "))
                    {
                        if (previosAction == "")
                            previosAction = "openraise";
                        else if (previosAction == "limp")
                            previosAction = "raise";
                        else if (previosAction == "openraise" || previosAction == "raise" || previosAction == "cold call")
                            previosAction = "3bet";
                        else if (previosAction == "3bet" || previosAction == "call 3bet")
                            previosAction = "4bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" goes all-in ") + 20));
                        bank += bet;
                    }
                    else if (line.Contains(" checks"))
                    {
                        previosAction = "check";
                    }
                    if (line.Contains(" calls ") || line.Contains(" raises ") || line.Contains(" checks") || line.Contains(" goes all-in "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["preflopAct"].ToString() == "limp")
                            {
                                if (line.Contains(" raises "))
                                    table.Rows[r]["preflopAct"] = "limp, raise";
                                else
                                    table.Rows[r]["preflopAct"] = "limp, call";
                            }
                            else
                                table.Rows[r]["preflopAct"] = previosAction;
                            if (table.Rows[r]["preflopBet"].ToString() != "")
                                table.Rows[r]["preflopBet"] = (decimal)table.Rows[r]["preflopBet"] + bet / bb;
                            else
                                table.Rows[r]["preflopBet"] = bet / bb;

                            if (table.Rows[r]["preflopPos"].ToString() == "")
                            {
                                if (preflopPosition == 1)
                                    table.Rows[r]["preflopPos"] = "BB";
                                else if (preflopPosition == 2)
                                    table.Rows[r]["preflopPos"] = "SB";
                                else if (preflopPosition == 3)
                                    table.Rows[r]["preflopPos"] = "BU";
                                else if (preflopPosition == 4)
                                    table.Rows[r]["preflopPos"] = "CO";
                                else if (preflopPosition == 5)
                                    table.Rows[r]["preflopPos"] = "MP3";
                                else if (preflopPosition == 6)
                                    table.Rows[r]["preflopPos"] = "MP2";
                                else if (preflopPosition == 7)
                                    table.Rows[r]["preflopPos"] = "MP1";
                                else if (preflopPosition == 8)
                                    table.Rows[r]["preflopPos"] = "UTG";
                                else if (preflopPosition >= 9)
                                    table.Rows[r]["preflopPos"] = "UTG";
                            }
                        }
                    }
                }//if (street == "preflop")
                #endregion

                #region FLOP
                else if (street == "flop")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 8));
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise" || previosAction == "3bet")
                            previosAction = "call raise";
                        else
                            previosAction = "call";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 9));
                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        if (previosAction == "raise")
                            previosAction = "3bet";
                        else
                            previosAction = "raise";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 10));
                        bank += bet;
                    }
                    else if (line.Contains(" goes all-in "))
                    {
                        if (previosAction == "")
                            previosAction = "bet";
                        else if (previosAction == "bet")
                            previosAction = "call";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" goes all-in ") + 20));
                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls ") || line.Contains(" goes all-in "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["flopAct"].ToString() == "check")
                            {
                                table.Rows[r]["Pos"] = "OOP";
                                table.Rows[r]["flopAct"] += ", " + previosAction;
                            }
                            else if (table.Rows[r]["FlopAct"].ToString() == "bet")
                                table.Rows[r]["FlopAct"] += ", " + previosAction;
                            else
                            {
                                if (flopActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!flopActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                                table.Rows[r]["flopAct"] = previosAction;
                            }
                            table.Rows[r]["flopBet"] = Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["flopPlayersCount"] = playersCount;
                        }
                    }
                    flopActions += previosAction;
                }//if (street == "flop")
                #endregion

                #region TURN
                else if (street == "turn")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 8));
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 9));
                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        if (previosAction == "raise")
                            previosAction = "3bet";
                        else
                            previosAction = "raise";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 10));
                        bank += bet;
                    }
                    else if (line.Contains(" goes all-in "))
                    {
                        if (previosAction == "")
                            previosAction = "bet";
                        else if (previosAction == "bet")
                            previosAction = "call";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" goes all-in ") + 20));
                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls ") || line.Contains(" goes all-in "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["turnAct"].ToString() == "check")
                            {
                                table.Rows[r]["Pos"] = "OOP";
                                table.Rows[r]["turnAct"] += ", " + previosAction;
                            }
                            else if (table.Rows[r]["turnAct"].ToString() == "bet")
                                table.Rows[r]["turnAct"] += ", " + previosAction;
                            else
                            {
                                if (turnActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!turnActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                                table.Rows[r]["turnAct"] = previosAction;
                            }
                            table.Rows[r]["turnBet"] = Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["turnPlayersCount"] = playersCount;
                        }
                    }
                    turnActions += previosAction;
                }//if (street == "turn")
                #endregion

                #region RIVER
                else if (street == "river")
                {
                    if (line.Contains(" checks"))
                        previosAction = "check";
                    if (line.Contains(" folds"))
                        playersCount -= 1;
                    else if (line.Contains(" bets "))
                    {
                        previosAction = "bet";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" bets ") + 8));
                        bank += bet;
                    }
                    else if (line.Contains(" calls "))
                    {
                        if (previosAction == "raise")
                            previosAction = "call raise";
                        else
                            previosAction = "call";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" calls ") + 9));
                        bank += bet;
                    }
                    else if (line.Contains(" raises "))
                    {
                        if (previosAction == "raise")
                            previosAction = "3bet";
                        else
                            previosAction = "raise";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" raises ") + 10));
                        bank += bet;
                    }
                    else if (line.Contains(" goes all-in "))
                    {
                        if (previosAction == "")
                            previosAction = "bet";
                        else if (previosAction == "bet")
                            previosAction = "call";
                        bet = Convert.ToDecimal(line.Substring(line.IndexOf(" goes all-in ") + 20));
                        bank += bet;
                    }
                    if (line.Contains(" bets ") || line.Contains(" checks") || line.Contains(" raises ") || line.Contains(" calls ") || line.Contains(" goes all-in "))
                    {
                        name = line.Substring(0, line.IndexOf(" "));
                        if (names[name] != null)
                        {
                            r = (int)names[name];
                            if (table.Rows[r]["riverAct"].ToString() == "check")
                            {
                                table.Rows[r]["Pos"] = "OOP";
                                table.Rows[r]["riverAct"] += ", " + previosAction;
                            }
                            else if (table.Rows[r]["riverAct"].ToString() == "bet")
                                table.Rows[r]["riverAct"] += ", " + previosAction;
                            else
                            {
                                if (riverActions == "")
                                    table.Rows[r]["Pos"] = "OOP";
                                else if (!riverActions.Contains("raise"))
                                    table.Rows[r]["Pos"] = "IP";
                                table.Rows[r]["riverAct"] = previosAction;
                            }
                            table.Rows[r]["riverBet"] = Math.Round(bet / streetStartBank, 1);
                            table.Rows[r]["riverPlayersCount"] = playersCount;
                        }
                    }
                    riverActions += previosAction;
                }//if (street == "river")
                #endregion

            }//foreach (string line in lines)

            board = board.Replace("[", "");
            board = board.Replace("]", "");

            int iComb = 0;

            foreach (System.Collections.DictionaryEntry element in names)
            {
                name = (string)element.Key;
                r = (int)names[name];
                string hand = (string)table.Rows[r]["hand4"];
                pocketHand = System.Text.RegularExpressions.Regex.Split(hand, " ");
                if (hand.Substring(1, 1) != hand.Substring(4, 1) && hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "o";
                else if (hand.Substring(0, 1) != hand.Substring(3, 1))
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1) + "s";
                else
                    hand = hand.Substring(0, 1) + hand.Substring(3, 1);
                if (hand[0].ToString() != hand[1].ToString())
                {
                    string card1 = hand[0].ToString();
                    card1 = card1.Replace("A", "14"); card1 = card1.Replace("K", "13"); card1 = card1.Replace("Q", "12"); card1 = card1.Replace("J", "11"); card1 = card1.Replace("T", "10");
                    string card2 = hand[1].ToString();
                    card2 = card2.Replace("A", "14"); card2 = card2.Replace("K", "13"); card2 = card2.Replace("Q", "12"); card2 = card2.Replace("J", "11"); card2 = card2.Replace("T", "10");
                    if (Convert.ToInt32(card1) < Convert.ToInt32(card2))
                        hand = hand.Substring(1, 1) + hand.Substring(0, 1) + hand.Substring(2, 1);
                }
                table.Rows[r]["hand3"] = hand;
                table.Rows[r]["board"] = board;

                string combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 8), " "));
                string[] array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                if (array.Count() == 1)
                    iComb = Convert.ToInt32(array[0]);
                else
                {
                    iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                    table.Rows[r]["flopComb"] = array[0];
                }
                table.Rows[r]["iFlopComb"] = iComb;

                if (board.Length >= 11)
                {
                    combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 11), " "));
                    array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                    if (array.Count() == 1)
                        iComb = Convert.ToInt32(array[0]);
                    else
                    {
                        iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                        table.Rows[r]["turnComb"] = array[0];
                    }
                    table.Rows[r]["iTurnComb"] = iComb;
                }

                if (board.Length >= 14)
                {
                    combAndCards = CombinationOpredeletionB(pocketHand, System.Text.RegularExpressions.Regex.Split(board.Substring(0, 14), " "));
                    array = System.Text.RegularExpressions.Regex.Split(combAndCards, " ");
                    if (array.Count() == 1)
                        iComb = Convert.ToInt32(array[0]);
                    else
                    {
                        iComb = CombStrengthOpredeletion(array[0], Convert.ToInt32(array[1]));
                        table.Rows[r]["riverComb"] = array[0];
                    }
                    table.Rows[r]["iRiverComb"] = iComb;
                }
            }

            //if (push)
            //    ReadHandPushStars(lines, bb);
        }  

        private string CombinationOpredeletionB(string[] pocketHand, string[] board)
        {
            DataTable combTable = new DataTable();
            combTable.Columns.Add("iКарта", typeof(Int32));
            combTable.Columns.Add("Масть");
            combTable.Columns.Add("sКарта");
            //combTable.Clear();
            string itemR = "";
            foreach (var item in pocketHand)
            {
                itemR = item.Replace("A", "14"); itemR = itemR.Replace("K", "13"); itemR = itemR.Replace("Q", "12"); itemR = itemR.Replace("J", "11"); itemR = itemR.Replace("T", "10");
                combTable.Rows.Add(new Object[] { Convert.ToInt32(itemR.Substring(0, itemR.Length - 1)), itemR.Substring(itemR.Length - 1), itemR });
            }
            foreach (var item in board)
            {
                itemR = item.Replace("A", "14"); itemR = itemR.Replace("K", "13"); itemR = itemR.Replace("Q", "12"); itemR = itemR.Replace("J", "11"); itemR = itemR.Replace("T", "10");
                combTable.Rows.Add(new Object[] { Convert.ToInt32(itemR.Substring(0, itemR.Length - 1)), itemR.Substring(itemR.Length - 1), itemR });
            }
            int iMyCard1 = (int)combTable.Rows[0]["iКарта"];
            int iMyCard2 = (int)combTable.Rows[1]["iКарта"];

            //***************************************************** проверка на пары, тройки, фулхаузы, карэ
            int myCardsComb = 0;
            int cardsComb = 0;
            DataView SortedView = new DataView(combTable);
            SortedView.Sort = "iКарта";
            string hand = "";
            for (int i = 1; i < SortedView.Count; i++)
            {
                if ((int)SortedView[i - 1]["iКарта"] == (int)SortedView[i]["iКарта"])
                {
                    if (hand != "TwoPair" && hand != "Three")
                    {
                        if (iMyCard1 == (int)SortedView[i]["iКарта"] && iMyCard1 * 100 > myCardsComb)
                            myCardsComb = iMyCard1 * 100 + iMyCard2;
                        else if (iMyCard2 == (int)SortedView[i]["iКарта"] && iMyCard2 * 100 > myCardsComb)
                            myCardsComb = iMyCard2 * 100 + iMyCard1;
                        else
                            cardsComb = (int)SortedView[i]["iКарта"];
                    }
                    if (hand == "Pair")
                    {
                        if ((int)SortedView[i - 2]["iКарта"] == (int)SortedView[i - 1]["iКарта"])
                        {
                            hand = "Three";
                            myCardsComb = (int)SortedView[i - 1]["iКарта"];
                        }
                        else
                            hand = "TwoPair";
                    }
                    else if (hand == "TwoPair")
                    {
                        if ((int)SortedView[i - 2]["iКарта"] == (int)SortedView[i - 1]["iКарта"])
                        {
                            hand = "FullHouse";
                            if (cardsComb == 0)
                            { }
                            else if (cardsComb > myCardsComb / 100)
                                myCardsComb = (int)SortedView[i]["iКарта"] * 100 + myCardsComb / 100;
                            else
                                myCardsComb = (int)SortedView[i]["iКарта"] * 100 + cardsComb;
                        }
                        else
                        {
                            hand = "ТриПары";
                            myCardsComb = 0;
                            if (iMyCard1 == (int)SortedView[i - 1]["iКарта"])
                            {
                                myCardsComb = iMyCard1;
                                if (iMyCard2 == iMyCard1)
                                    myCardsComb += iMyCard2 * 100;
                            }
                            else if (iMyCard2 == (int)SortedView[i - 1]["iКарта"])
                                myCardsComb += iMyCard2;
                            if (myCardsComb < 100)
                            {
                                if ((int)SortedView[i - 2]["iКарта"] == iMyCard1 || (int)SortedView[i - 2]["iКарта"] == iMyCard2)
                                    myCardsComb = myCardsComb * 100 + (int)SortedView[i - 2]["iКарта"];
                                else if (i < SortedView.Count - 1 && ((int)SortedView[i + 1]["iКарта"] == iMyCard1 || (int)SortedView[i + 1]["iКарта"] == iMyCard2))
                                    myCardsComb = myCardsComb * 100 + (int)SortedView[i + 1]["iКарта"];
                            }
                        }
                    }
                    else if (hand == "Three")
                    {
                        if ((int)SortedView[i - 2]["iКарта"] == (int)SortedView[i - 1]["iКарта"])
                            hand = "Four";
                        else
                        {
                            hand = "FullHouse";
                            myCardsComb = myCardsComb * 100 + (int)SortedView[i]["iКарта"];
                        }
                    }
                    else if (hand == "")
                    {
                        hand = "Pair";
                    }

                }
            }//for (int i = 1; i < SortedView.Count; i++)

            if (hand == "FullHouse")
            {
                Array.Sort(board);
                //int x = 0; 
                for (int i = 2; i < board.Count(); i++)
                {
                    if (board[i - 2].Substring(0, 1) == board[i - 1].Substring(0, 1) && board[i - 1].Substring(0, 1) == board[i].Substring(0, 1)) // если 3 одинаковые карты на борде то это пара
                    {
                        if (iMyCard1 == iMyCard2)
                            hand = "OverPair";
                        else
                            hand = "TopPair";
                        DataView tableView = new DataView(combTable);
                        myCardsComb = myCardsComb % 100;
                        for (i = 2; i < tableView.Count; i++)
                        {
                            if (myCardsComb < (int)tableView[i]["iКарта"])
                                hand = "Pair";
                        }
                    }
                }
                return hand + " " + myCardsComb;
            }

            //***************************************************** проверка на флеш
            int cardsSuit = 0;
            DataView SuitView = new DataView(combTable);
            SuitView.Sort = "Масть";
            int suitMax = 0;
            for (int i = 3; i < SuitView.Count; i++)
            {
                if (i - 4 > -1 && (string)SuitView[i - 4]["Масть"] == (string)SuitView[i - 3]["Масть"] && (string)SuitView[i - 3]["Масть"] == (string)SuitView[i - 2]["Масть"] && (string)SuitView[i - 2]["Масть"] == (string)SuitView[i - 1]["Масть"] && (string)SuitView[i - 1]["Масть"] == (string)SuitView[i]["Масть"])
                {
                    cardsSuit = 0;
                    if ((string)combTable.Rows[0]["Масть"] == (string)SuitView[i]["Масть"])
                        cardsSuit = (int)combTable.Rows[0]["iКарта"];
                    if ((string)combTable.Rows[1]["Масть"] == (string)SuitView[i]["Масть"] && (int)combTable.Rows[1]["iКарта"] > cardsSuit)
                        cardsSuit = (int)combTable.Rows[1]["iКарта"];
                    if (cardsSuit != 0)
                        suitMax = 5;
                    break;
                }
                if ((string)SuitView[i - 3]["Масть"] == (string)SuitView[i - 2]["Масть"] && (string)SuitView[i - 2]["Масть"] == (string)SuitView[i - 1]["Масть"] && (string)SuitView[i - 1]["Масть"] == (string)SuitView[i]["Масть"])
                {
                    if ((string)combTable.Rows[0]["Масть"] == (string)SuitView[i]["Масть"])
                        cardsSuit = (int)combTable.Rows[0]["iКарта"];
                    if ((string)combTable.Rows[1]["Масть"] == (string)SuitView[i]["Масть"] && (int)combTable.Rows[1]["iКарта"] > cardsSuit)
                        cardsSuit = (int)combTable.Rows[1]["iКарта"];
                    if (cardsSuit != 0)
                        suitMax = 4;
                }
            }
            if (suitMax == 5)
                return "Flush " + cardsSuit;

            //***************************************************** проверка на стрит
            DataTable sortedViewTable1 = SortedView.ToTable(); // нужна отсортированная последовательность без тузов для определения третьей пары
            int cardsConnector = 0;
            DataRow[] foundRows = combTable.Select("iКарта = '14'");
            if (foundRows.Count() > 0)
                combTable.Rows.Add(new Object[] { 1, foundRows[0]["Масть"], foundRows[0]["sКарта"] });
            DataView dataView = new DataView(combTable);
            int[] straightCheckArray = new int[combTable.Rows.Count];
            for (int i = 0; i < combTable.Rows.Count; i++)
                straightCheckArray[i] = (int)dataView[i]["iКарта"];
            var straightCheck = straightCheckArray.Distinct().ToList();
            straightCheck.Sort();
            String connectors = "";
            for (int i = straightCheck.Count - 4; i > -1; i--)  //for (int i = 0; i < straightCheck.Count - 3; i++)
            {
                if (i + 4 < straightCheck.Count && straightCheck[i + 4] - straightCheck[i] == 4)
                {
                    connectors = "Straight";
                    cardsConnector = straightCheck[i + 4];
                    break;
                }
                else if (connectors == "StraightDraw")
                {
                    continue;
                }
                else if (straightCheck[i + 3] - straightCheck[i] == 3)
                {
                    connectors = straightCheck[i] != 1 && straightCheck[i + 3] != 14 ? "StraightDraw" : "Gutshot";
                    cardsConnector = straightCheck[i + 3];
                }
                else if (i + 4 < straightCheck.Count && straightCheck[i + 4] - straightCheck[i] == 6 && !straightCheck.Contains(straightCheck[i] + 1) && !straightCheck.Contains(straightCheck[i] + 5))
                {
                    connectors = "StraightDraw";  //double gutshot типа 3 567 9
                    cardsConnector = straightCheck[i + 4];
                }
                else if (i + 5 < straightCheck.Count && straightCheck[i + 5] - straightCheck[i] == 7 && !straightCheck.Contains(straightCheck[i] + 2) && !straightCheck.Contains(straightCheck[i] + 5))
                {
                    connectors = "StraightDraw";//double gutshot типа 35 78 TJ
                    cardsConnector = straightCheck[i + 5];
                }
                else if (straightCheck[i + 3] - straightCheck[i] == 4)
                {
                    connectors = "Gutshot";
                    cardsConnector = straightCheck[i + 3];
                }
            }//for (int i = 0; i < SortedView.Count - 3; i++)
            if ((connectors == "StraightDraw" || connectors == "Gutshot") && board.Count() == 4 && BoardContainsDraw(board, "StraightDraw"))
                connectors = "";
            else if (connectors == "Straight" && board.Count() == 5 && BoardContainsDraw(board, "Straight"))
            {
                if (iMyCard1 == cardsConnector || iMyCard2 == cardsConnector)
                {
                    for (int i = 2; i < 7; i++)
                    {
                        if ((int)combTable.Rows[i]["iКарта"] >= cardsConnector)
                            connectors = "";
                    }
                }
                else
                    connectors = "";
            }
            //***************************************************** итог проверки на стрит
            if (connectors == "Straight")
                return "Straight " + cardsConnector;

            if (hand != "") //убираем пары борда и определяем топ пару или оверпару
            {
                Array.Sort(board);
                int x = 0; //одинаковые карты на борде
                for (int i = 1; i < board.Count(); i++)
                {
                    if (board[i - 1].Substring(0, board[i - 1].Length - 1) == board[i].Substring(0, board[i].Length - 1))
                        x += 1;
                }
                if (hand == "Pair")
                {
                    if (x == 1)
                        hand = "";
                    else
                    {
                        DataView tableView = new DataView(combTable);
                        if ((int)tableView[1]["iКарта"] == (int)tableView[0]["iКарта"])//карманная пара
                        {
                            if ((int)tableView[0]["iКарта"] == (int)SortedView[SortedView.Count - 1]["iКарта"])
                                hand = "OverPair";
                            else
                                hand = "Pair";
                        }
                        else
                        {
                            hand = "TopPair";
                            for (int i = 2; i < tableView.Count; i++)
                            {
                                if (myCardsComb / 100 < (int)tableView[i]["iКарта"])
                                    hand = "Pair";
                            }
                        }
                    }
                }
                else if (hand == "TwoPair")
                {
                    if (x == 1)
                    {
                        DataView tableView = new DataView(combTable);
                        if ((int)tableView[1]["iКарта"] == (int)tableView[0]["iКарта"])//карманная пара
                        {
                            if ((int)tableView[0]["iКарта"] == (int)SortedView[SortedView.Count - 1]["iКарта"])
                                hand = "OverPair";
                            else
                                hand = "Pair";
                        }
                        else
                        {
                            hand = "TopPair";
                            for (int i = 2; i < tableView.Count; i++)
                            {
                                if (myCardsComb / 100 < (int)tableView[i]["iКарта"])
                                    hand = "Pair";
                            }
                        }
                    }
                    else if (x == 2)
                    {
                        hand = "";
                    }
                }
                else if (hand == "ТриПары")
                {
                    DataView tableView = new DataView(combTable);
                    DataView sortedView1 = new DataView(sortedViewTable1); // сортированная таблица всех карт без туз=1
                    if ((iMyCard1 == (int)sortedView1[1]["iКарта"] || iMyCard2 == (int)sortedView1[1]["iКарта"]) && x == 2)
                    {
                        hand = ""; // третья пара на дважды спаренном борде = пусто
                    }
                    else
                    {
                        if (iMyCard1 == iMyCard2)//карманная пара
                        {
                            if (iMyCard1 == (int)SortedView[SortedView.Count - 1]["iКарта"])
                                hand = "OverPair";
                            else
                                hand = "Pair";
                            myCardsComb = 30000 + (int)tableView[0]["iКарта"] * 100 + (int)tableView[0]["iКарта"];
                        }
                        else
                        {
                            hand = "TopPair";
                            for (int i = 2; i < tableView.Count; i++)
                            {
                                if (myCardsComb / 100 < (int)tableView[i]["iКарта"])
                                    hand = "Pair";
                            }
                        }
                    }
                }
                else if (hand == "Three")
                {
                    if (x == 2)
                    {
                        hand = "";
                        myCardsComb = (iMyCard1 > iMyCard2) ? iMyCard1 * 100 + iMyCard2 : iMyCard2 * 100 + iMyCard1;
                    }
                    else if (x == 1)
                        myCardsComb = (iMyCard1 == myCardsComb) ? iMyCard1 * 100 + iMyCard2 : iMyCard2 * 100 + iMyCard1;
                    else
                        myCardsComb = iMyCard1 * 100 + iMyCard2;
                }
            }

            // проверка на оверкарты если пусто
            if (hand == "" && board.Count() < 5)
            {
                DataView NotSortedView = new DataView(combTable);
                SortedView.Sort = "iКарта DESC";
                if ((SortedView[0]["sКарта"] == NotSortedView[0]["sКарта"] && SortedView[1]["sКарта"] == NotSortedView[1]["sКарта"]) || (SortedView[0]["sКарта"] == NotSortedView[1]["sКарта"] && SortedView[1]["sКарта"] == NotSortedView[0]["sКарта"]))
                    hand = "Overcards";
            }

            if ((connectors == "StraightDraw" || connectors == "Gutshot" || suitMax == 4) && board.Count() < 5)
            {
                if (hand == "Three" || hand == "TwoPair")
                    return hand + " " + myCardsComb;
                if (suitMax == 4)
                {
                    if (hand == "Overcards")
                        return hand + "+" + "FlushDraw " + cardsSuit;
                    else if (hand != "")
                        return hand + "+FlushDraw " + myCardsComb;
                    else
                        return "FlushDraw " + cardsSuit;
                }
                else
                {
                    if (hand == "Overcards")
                        return hand + "+" + connectors + " " + cardsConnector;
                    else if (hand != "")
                        return hand + "+" + connectors + " " + myCardsComb;
                    else
                        return connectors + " " + cardsConnector;
                }
            }

            if (myCardsComb == 0)
            {
                if (iMyCard1 > iMyCard2)
                    myCardsComb = iMyCard1 * 100 + iMyCard2;
                else
                    myCardsComb = iMyCard2 * 100 + iMyCard1;
            }
            return hand + " " + myCardsComb;

        }

        private int CombStrengthOpredeletion(string comb, int iComb)
        {
            if (comb == "StraightDraw" || comb == "Gutshot" || comb == "Overcards+Gutshot" || comb == "Overcards+StraightDraw")
                iComb += 10000;
            else if (comb == "FlushDraw" || comb == "Overcards+FlushDraw" || comb == "Overcards+FlushDraw")
                iComb += 20000;
            else if (comb == "TwoPair")
                iComb += 40000;
            else if (comb.Contains("Pair"))
                iComb += 30000;
            else if (comb == "Three")
                iComb += 50000;
            else if (comb == "Straight")
                iComb += 60000;
            else if (comb == "Flush")
                iComb += 70000;
            else if (comb == "FullHouse")
                iComb += 80000;
            else if (comb == "Four")
                iComb += 90000;
            return iComb;
        }

        private bool BoardContainsDraw(string[] board, string comb)
        {
            if (comb.Contains("Straight"))
            {
                List<int> cardsList = new List<int>();
                foreach (var item in board)
                {
                    string itemR = item.Substring(0, 1);
                    itemR = itemR.Replace("A", "14"); itemR = itemR.Replace("K", "13"); itemR = itemR.Replace("Q", "12"); itemR = itemR.Replace("J", "11"); itemR = itemR.Replace("T", "10");
                    cardsList.Add(Convert.ToInt32(itemR));
                    if (itemR == "14")
                        cardsList.Add(1);
                }
                cardsList = cardsList.Distinct().ToList();
                cardsList.Sort();
                if (comb == "StraightDraw")
                {
                    for (int i = 0; i < cardsList.Count - 3; i++)
                    {
                        if (cardsList[i + 3] - cardsList[i] < 5)
                            return true;
                    }
                }
                else if (comb == "Straight")
                    for (int i = 0; i < cardsList.Count - 4; i++)
                    {
                        if (cardsList[i + 4] - cardsList[i] < 5 && comb == "Straight")
                            return true;
                    }
            }
            return false;
        }

        public void TimerEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                LoadHands();
            }
            catch { }
            
        }

        void StartLoadHands()
        {
            try
            {
                LoadHands();
            }
            catch { }
        }

        public DataTable GetPlayerStats() 
        {
            string sqlText = "";
            if (mainForm.tracker == "hm2")
            {
                sqlText = @"SELECT 
                            playername AS name,
                            SUM(totalhands) AS totalhands,
                            ROUND (100.0*SUM(vpiphands)/SUM(totalhands), 0) AS vpip,
                            ROUND (100.0*SUM(pfrhands)/SUM(totalhands), 0) AS pfr ,
                            CASE WHEN SUM(couldcoldcall)>0 THEN ROUND (100.0*SUM(didcoldcall)/SUM(couldcoldcall), 0) END AS coldCall,
                            CASE WHEN SUM(couldthreebet)>0 THEN ROUND (100.0*SUM(didthreebet)/SUM(couldthreebet), 0) END AS threebet,
                            CASE WHEN SUM(facedthreebetpreflop)>0 THEN ROUND (100.0*SUM(foldedtothreebetpreflop)/SUM(facedthreebetpreflop), 0) END AS foldTo3bet,
                            CASE WHEN SUM(facedthreebetpreflop)>0 THEN ROUND (100.0*SUM(raisedthreebetpreflop)/SUM(facedthreebetpreflop), 0) END AS fourbet,
                            CASE WHEN SUM(facedfourbetpreflop)>0 THEN ROUND (100.0*SUM(foldedtofourbetpreflop)/SUM(facedfourbetpreflop), 0) END AS foldTo4bet,
                            CASE WHEN SUM(flopcontinuationbetpossible)>0 THEN ROUND (100.0*SUM(flopcontinuationbetmade)/SUM(flopcontinuationbetpossible), 0) END AS cBFlop,
                            CASE WHEN SUM(facingflopcontinuationbet)>0 THEN ROUND (100.0*SUM(foldedtoflopcontinuationbet)/SUM(facingflopcontinuationbet), 0) END AS foldToCBFlop,
                            CASE WHEN SUM(facingflopcontinuationbet)>0 THEN ROUND (100.0*SUM(raisedflopcontinuationbet)/SUM(facingflopcontinuationbet), 0) END AS raiseCBFlop,
                            CASE WHEN SUM(turncontinuationbetpossible)>0 THEN ROUND (100.0*SUM(turncontinuationbetmade)/SUM(turncontinuationbetpossible), 0) END AS cBTurn,
                            CASE WHEN SUM(facingturncontinuationbet)>0 THEN ROUND (100.0*SUM(foldedtoturncontinuationbet)/SUM(facingturncontinuationbet), 0) END AS foldToCBTurn,
                            CASE WHEN SUM(facingturncontinuationbet)>0 THEN ROUND (100.0*SUM(raisedturncontinuationbet)/SUM(facingturncontinuationbet), 0) END AS raiseCBTurn,
                            CASE WHEN SUM(rivercontinuationbetpossible)>0 THEN ROUND (100.0*SUM(rivercontinuationbetmade)/SUM(rivercontinuationbetpossible), 0) END AS cBRiver,
                            CASE WHEN SUM(facingrivercontinuationbet)>0 THEN ROUND (100.0*SUM(foldedtorivercontinuationbet)/SUM(facingrivercontinuationbet), 0) END AS foldToCBRiver,
                            CASE WHEN SUM(facingrivercontinuationbet)>0 THEN ROUND (100.0*SUM(raisedrivercontinuationbet)/SUM(facingrivercontinuationbet), 0) END AS raiseCBRiver,
                            CASE WHEN SUM(sawshowdown)>0 THEN ROUND(100.0*SUM(wonshowdown)/SUM(sawshowdown), 0) END AS wonAtSD,
                            CASE WHEN SUM(sawflop)>0 THEN ROUND(100.0*SUM(sawshowdown)/SUM(sawflop), 0) END AS wentToSD,
                            CASE WHEN SUM(totalcalls)>0 THEN SUM(totalbets)/SUM(totalcalls) END AS aggF,
                            CASE WHEN SUM(totalpostflopstreetsseen)>0 THEN ROUND (100.0*SUM(totalaggressivepostflopstreetsseen)/SUM(totalpostflopstreetsseen), 0) END AS agg
                        FROM 
                            public.compiledplayerresults
                        JOIN   
                            public.players
                        ON
                            players.player_id = compiledplayerresults.player_id 
                        GROUP BY playername";
                // pokersite_id pokerstars=2(PT4-100), pp=0, prima(redstar)=3, lotos(pasific)=12  
            }
            else
            {
//                sqlText = @"SELECT
//                    player_name AS name,
//                    SUM(cnt_hands) AS totalhands,
//                    CASE WHEN SUM(cnt_pfr_opp)>0 THEN ROUND(100.0*SUM(cnt_pfr)/SUM(cnt_pfr_opp), 0) END AS pfr,
//                    CASE WHEN SUM(cnt_hands)-SUM(cnt_walks)>0 THEN ROUND(100.0*SUM(cnt_vpip)/(SUM(cnt_hands)-SUM(cnt_walks)), 0) END AS vpip,
//                    CASE WHEN SUM(cnt_p_ccall_opp)>0 THEN ROUND(100.0*SUM(cnt_p_ccall)/SUM(cnt_p_ccall_opp), 0) END AS coldCall,
//                    CASE WHEN SUM(cnt_p_3bet_opp)>0 THEN ROUND(100.0*SUM(cnt_p_3bet)/SUM(cnt_p_3bet_opp), 0) END AS threebet,
//                    CASE WHEN SUM(cnt_p_3bet_def_opp)>0 THEN ROUND(100.0*SUM(cnt_p_3bet_def_action_fold)/SUM(cnt_p_3bet_def_opp), 0) END AS foldTo3bet,
//                    CASE WHEN SUM(cnt_p_4bet_opp)-SUM(cnt_p_5bet_opp)>0 THEN ROUND(100.0*SUM(cnt_p_raise_3bet)/(SUM(cnt_p_4bet_opp)-SUM(cnt_p_5bet_opp)), 0) END AS fourbet,
//                    CASE WHEN SUM(cnt_p_4bet_def_opp)>0 THEN ROUND(100.0*SUM(cnt_p_4bet_def_action_fold)/SUM(cnt_p_4bet_def_opp), 0) END AS foldTo4bet,
//                    CASE WHEN SUM(cnt_f_cbet_opp)>0 THEN ROUND(100.0*SUM(cnt_f_cbet)/SUM(cnt_f_cbet_opp), 0) END AS cBFlop,
//                    CASE WHEN SUM(cnt_f_cbet_def_opp)>0 THEN ROUND(100.0*SUM(cnt_f_cbet_def_action_fold)/SUM(cnt_f_cbet_def_opp), 0) END AS foldToCBFlop,
//                    CASE WHEN SUM(cnt_f_cbet_def_opp)>0 THEN ROUND(100.0*SUM(cnt_f_cbet_def_action_raise)/SUM(cnt_f_cbet_def_opp), 0) END AS raiseCBFlop,
//                    CASE WHEN SUM(cnt_t_cbet_opp)>0 THEN ROUND(100.0*SUM(cnt_t_cbet)/SUM(cnt_t_cbet_opp), 0) END AS cBTurn,
//                    CASE WHEN SUM(cnt_t_cbet_def_opp)>0 THEN ROUND (100.0*SUM(cnt_t_cbet_def_action_fold)/SUM(cnt_t_cbet_def_opp), 0) END AS foldToCBTurn,
//                    CASE WHEN SUM(cnt_t_cbet_def_opp)>0 THEN ROUND (100.0*SUM(cnt_t_cbet_def_action_raise)/SUM(cnt_t_cbet_def_opp), 0) END AS raiseCBTurn,
//                    CASE WHEN SUM(cnt_r_cbet_opp)>0 THEN ROUND (100.0*SUM(cnt_r_cbet)/SUM(cnt_r_cbet_opp), 0) END AS cBRiver,
//                    CASE WHEN SUM(cnt_r_cbet_def_opp)>0 THEN ROUND (100.0*SUM(cnt_r_cbet_def_action_fold)/SUM(cnt_r_cbet_def_opp), 0) END AS foldToCBRiver,
//                    CASE WHEN SUM(cnt_r_cbet_def_opp)>0 THEN ROUND (100.0*SUM(cnt_r_cbet_def_action_raise)/SUM(cnt_r_cbet_def_opp), 0) END AS raiseCBRiver,
//                    CASE WHEN SUM(cnt_wtsd)>0 THEN ROUND(100.0*SUM(cnt_wtsd_won)/SUM(cnt_wtsd), 0) END AS wonAtSD,
//                    CASE WHEN SUM(cnt_f_saw)>0 THEN ROUND(100.0*SUM(cnt_wtsd)/SUM(cnt_f_saw), 0) END AS wentToSD,
//                    CASE WHEN SUM(cnt_f_call)+SUM(cnt_t_call)+SUM(cnt_r_call)>0 THEN (SUM(cnt_f_bet)+SUM(cnt_f_raise)+SUM(cnt_t_bet)+SUM(cnt_t_raise)+SUM(cnt_r_bet)+SUM(cnt_r_raise))/(SUM(cnt_f_call)+SUM(cnt_t_call)+SUM(cnt_r_call)) END AS aggF,
//                    CASE WHEN SUM(cnt_f_call)+SUM(cnt_f_fold)+SUM(cnt_t_call)+SUM(cnt_t_fold)+SUM(cnt_r_call)+SUM(cnt_r_fold)+SUM(cnt_f_bet)+SUM(cnt_f_raise)+SUM(cnt_t_bet)+SUM(cnt_t_raise)+SUM(cnt_r_bet)+SUM(cnt_r_raise)>0 THEN ROUND (100.0*(SUM(cnt_f_bet)+SUM(cnt_f_raise)+SUM(cnt_t_bet)+SUM(cnt_t_raise)+SUM(cnt_r_bet)+SUM(cnt_r_raise))/(SUM(cnt_f_call)+SUM(cnt_f_fold)+SUM(cnt_t_call)+SUM(cnt_t_fold)+SUM(cnt_r_call)+SUM(cnt_r_fold)+SUM(cnt_f_bet)+SUM(cnt_f_raise)+SUM(cnt_t_bet)+SUM(cnt_t_raise)+SUM(cnt_r_bet)+SUM(cnt_r_raise)), 0) END AS agg
//                FROM 
//                    cash_cache
//                JOIN 
//                    player 
//                ON 
//                    cash_cache.id_player = player.id_player 
//                GROUP BY player_name";

//                sqlText = @"SELECT
//                                    player_name AS name,
//                                    SUM(cash_cache.cnt_hands + tourney_cache.cnt_hands) AS totalhands,
//                                    CASE WHEN SUM(cash_cache.cnt_pfr_opp+tourney_cache.cnt_pfr_opp)>0 THEN ROUND(100.0*SUM(cash_cache.cnt_pfr+tourney_cache.cnt_pfr)/SUM(cash_cache.cnt_pfr_opp+tourney_cache.cnt_pfr_opp), 0) END AS pfr,
//                                    CASE WHEN SUM(cash_cache.cnt_hands+tourney_cache.cnt_hands)-SUM(cash_cache.cnt_walks+tourney_cache.cnt_walks)>0 THEN ROUND(100.0*SUM(cash_cache.cnt_vpip+tourney_cache.cnt_vpip)/(SUM(cash_cache.cnt_hands+tourney_cache.cnt_hands)-SUM(cash_cache.cnt_walks+tourney_cache.cnt_walks)), 0) END AS vpip,
//                                    CASE WHEN SUM(cnt_p_ccall_opp)>0 THEN ROUND(100.0*SUM(cnt_p_ccall)/SUM(cnt_p_ccall_opp), 0) END AS coldCall,
//                                    CASE WHEN SUM(cnt_p_3bet_opp)>0 THEN ROUND(100.0*SUM(cnt_p_3bet)/SUM(cnt_p_3bet_opp), 0) END AS threebet,
//                                    CASE WHEN SUM(cnt_p_3bet_def_opp)>0 THEN ROUND(100.0*SUM(cnt_p_3bet_def_action_fold)/SUM(cnt_p_3bet_def_opp), 0) END AS foldTo3bet,
//                                    CASE WHEN SUM(cnt_p_4bet_opp)-SUM(cnt_p_5bet_opp)>0 THEN ROUND(100.0*SUM(cnt_p_raise_3bet)/(SUM(cnt_p_4bet_opp)-SUM(cnt_p_5bet_opp)), 0) END AS fourbet,
//                                    CASE WHEN SUM(cnt_p_4bet_def_opp)>0 THEN ROUND(100.0*SUM(cnt_p_4bet_def_action_fold)/SUM(cnt_p_4bet_def_opp), 0) END AS foldTo4bet,
//                                    CASE WHEN SUM(cnt_f_cbet_opp)>0 THEN ROUND(100.0*SUM(cnt_f_cbet)/SUM(cnt_f_cbet_opp), 0) END AS cBFlop,
//                                    CASE WHEN SUM(cnt_f_cbet_def_opp)>0 THEN ROUND(100.0*SUM(cnt_f_cbet_def_action_fold)/SUM(cnt_f_cbet_def_opp), 0) END AS foldToCBFlop,
//                                    CASE WHEN SUM(cnt_f_cbet_def_opp)>0 THEN ROUND(100.0*SUM(cnt_f_cbet_def_action_raise)/SUM(cnt_f_cbet_def_opp), 0) END AS raiseCBFlop,
//                                    CASE WHEN SUM(cnt_t_cbet_opp)>0 THEN ROUND(100.0*SUM(cnt_t_cbet)/SUM(cnt_t_cbet_opp), 0) END AS cBTurn,
//                                    CASE WHEN SUM(cnt_t_cbet_def_opp)>0 THEN ROUND (100.0*SUM(cnt_t_cbet_def_action_fold)/SUM(cnt_t_cbet_def_opp), 0) END AS foldToCBTurn,
//                                    CASE WHEN SUM(cnt_t_cbet_def_opp)>0 THEN ROUND (100.0*SUM(cnt_t_cbet_def_action_raise)/SUM(cnt_t_cbet_def_opp), 0) END AS raiseCBTurn,
//                                    CASE WHEN SUM(cnt_r_cbet_opp)>0 THEN ROUND (100.0*SUM(cnt_r_cbet)/SUM(cnt_r_cbet_opp), 0) END AS cBRiver,
//                                    CASE WHEN SUM(cnt_r_cbet_def_opp)>0 THEN ROUND (100.0*SUM(cnt_r_cbet_def_action_fold)/SUM(cnt_r_cbet_def_opp), 0) END AS foldToCBRiver,
//                                    CASE WHEN SUM(cnt_r_cbet_def_opp)>0 THEN ROUND (100.0*SUM(cnt_r_cbet_def_action_raise)/SUM(cnt_r_cbet_def_opp), 0) END AS raiseCBRiver,
//                                    CASE WHEN SUM(cnt_wtsd)>0 THEN ROUND(100.0*SUM(cnt_wtsd_won)/SUM(cnt_wtsd), 0) END AS wonAtSD,
//                                    CASE WHEN SUM(cnt_f_saw)>0 THEN ROUND(100.0*SUM(cnt_wtsd)/SUM(cnt_f_saw), 0) END AS wentToSD,
//                                    CASE WHEN SUM(cnt_f_call)+SUM(cnt_t_call)+SUM(cnt_r_call)>0 THEN (SUM(cnt_f_bet)+SUM(cnt_f_raise)+SUM(cnt_t_bet)+SUM(cnt_t_raise)+SUM(cnt_r_bet)+SUM(cnt_r_raise))/(SUM(cnt_f_call)+SUM(cnt_t_call)+SUM(cnt_r_call)) END AS aggF,
//                                    CASE WHEN SUM(cnt_f_call)+SUM(cnt_f_fold)+SUM(cnt_t_call)+SUM(cnt_t_fold)+SUM(cnt_r_call)+SUM(cnt_r_fold)+SUM(cnt_f_bet)+SUM(cnt_f_raise)+SUM(cnt_t_bet)+SUM(cnt_t_raise)+SUM(cnt_r_bet)+SUM(cnt_r_raise)>0 THEN ROUND (100.0*(SUM(cnt_f_bet)+SUM(cnt_f_raise)+SUM(cnt_t_bet)+SUM(cnt_t_raise)+SUM(cnt_r_bet)+SUM(cnt_r_raise))/(SUM(cnt_f_call)+SUM(cnt_f_fold)+SUM(cnt_t_call)+SUM(cnt_t_fold)+SUM(cnt_r_call)+SUM(cnt_r_fold)+SUM(cnt_f_bet)+SUM(cnt_f_raise)+SUM(cnt_t_bet)+SUM(cnt_t_raise)+SUM(cnt_r_bet)+SUM(cnt_r_raise)), 0) END AS agg
//                                FROM 
//                                    cash_cache, tourney_cache
//                                JOIN 
//                                    player 
//                                ON 
//                                    cash_cache.id_player = player.id_player AND tourney_cache.id_player = player.id_player
//                                GROUP BY player_name";

                //string handText = dataGridViewHands.CurrentRow.Cells["handhistory"].Value.ToString();
                //if (handText.Contains(" Tournament "))
                //    sqlText = sqlText.Replace("cash_cache", "tourney_cache");

                sqlText = @"SELECT
                                    player_name AS name,
                                    SUM(cnt_hands) AS totalhands,
                                    CASE WHEN SUM(cnt_pfr_opp)>0 THEN ROUND(100.0*SUM(cnt_pfr)/SUM(cnt_pfr_opp), 0) END AS pfr,
                                    CASE WHEN SUM(cnt_hands)-SUM(cnt_walks)>0 THEN ROUND(100.0*SUM(cnt_vpip)/(SUM(cnt_hands)-SUM(cnt_walks)), 0) END AS vpip,
                                    CASE WHEN SUM(cnt_p_ccall_opp)>0 THEN ROUND(100.0*SUM(cnt_p_ccall)/SUM(cnt_p_ccall_opp), 0) END AS coldCall,
                                    CASE WHEN SUM(cnt_p_3bet_opp)>0 THEN ROUND(100.0*SUM(cnt_p_3bet)/SUM(cnt_p_3bet_opp), 0) END AS threebet,
                                    CASE WHEN SUM(cnt_p_3bet_def_opp)>0 THEN ROUND(100.0*SUM(cnt_p_3bet_def_action_fold)/SUM(cnt_p_3bet_def_opp), 0) END AS foldTo3bet,
                                    CASE WHEN SUM(cnt_p_4bet_opp)-SUM(cnt_p_5bet_opp)>0 THEN ROUND(100.0*SUM(cnt_p_raise_3bet)/(SUM(cnt_p_4bet_opp)-SUM(cnt_p_5bet_opp)), 0) END AS fourbet,
                                    CASE WHEN SUM(cnt_p_4bet_def_opp)>0 THEN ROUND(100.0*SUM(cnt_p_4bet_def_action_fold)/SUM(cnt_p_4bet_def_opp), 0) END AS foldTo4bet,
                                    CASE WHEN SUM(cnt_f_cbet_opp)>0 THEN ROUND(100.0*SUM(cnt_f_cbet)/SUM(cnt_f_cbet_opp), 0) END AS cBFlop,
                                    CASE WHEN SUM(cnt_f_cbet_def_opp)>0 THEN ROUND(100.0*SUM(cnt_f_cbet_def_action_fold)/SUM(cnt_f_cbet_def_opp), 0) END AS foldToCBFlop,
                                    CASE WHEN SUM(cnt_f_cbet_def_opp)>0 THEN ROUND(100.0*SUM(cnt_f_cbet_def_action_raise)/SUM(cnt_f_cbet_def_opp), 0) END AS raiseCBFlop,
                                    CASE WHEN SUM(cnt_t_cbet_opp)>0 THEN ROUND(100.0*SUM(cnt_t_cbet)/SUM(cnt_t_cbet_opp), 0) END AS cBTurn,
                                    CASE WHEN SUM(cnt_t_cbet_def_opp)>0 THEN ROUND (100.0*SUM(cnt_t_cbet_def_action_fold)/SUM(cnt_t_cbet_def_opp), 0) END AS foldToCBTurn,
                                    CASE WHEN SUM(cnt_t_cbet_def_opp)>0 THEN ROUND (100.0*SUM(cnt_t_cbet_def_action_raise)/SUM(cnt_t_cbet_def_opp), 0) END AS raiseCBTurn,
                                    CASE WHEN SUM(cnt_r_cbet_opp)>0 THEN ROUND (100.0*SUM(cnt_r_cbet)/SUM(cnt_r_cbet_opp), 0) END AS cBRiver,
                                    CASE WHEN SUM(cnt_r_cbet_def_opp)>0 THEN ROUND (100.0*SUM(cnt_r_cbet_def_action_fold)/SUM(cnt_r_cbet_def_opp), 0) END AS foldToCBRiver,
                                    CASE WHEN SUM(cnt_r_cbet_def_opp)>0 THEN ROUND (100.0*SUM(cnt_r_cbet_def_action_raise)/SUM(cnt_r_cbet_def_opp), 0) END AS raiseCBRiver,
                                    CASE WHEN SUM(cnt_wtsd)>0 THEN ROUND(100.0*SUM(cnt_wtsd_won)/SUM(cnt_wtsd), 0) END AS wonAtSD,
                                    CASE WHEN SUM(cnt_f_saw)>0 THEN ROUND(100.0*SUM(cnt_wtsd)/SUM(cnt_f_saw), 0) END AS wentToSD,
                                    CASE WHEN SUM(cnt_f_call)+SUM(cnt_t_call)+SUM(cnt_r_call)>0 THEN (SUM(cnt_f_bet)+SUM(cnt_f_raise)+SUM(cnt_t_bet)+SUM(cnt_t_raise)+SUM(cnt_r_bet)+SUM(cnt_r_raise))/(SUM(cnt_f_call)+SUM(cnt_t_call)+SUM(cnt_r_call)) END AS aggF,
                                    CASE WHEN SUM(cnt_f_call)+SUM(cnt_f_fold)+SUM(cnt_t_call)+SUM(cnt_t_fold)+SUM(cnt_r_call)+SUM(cnt_r_fold)+SUM(cnt_f_bet)+SUM(cnt_f_raise)+SUM(cnt_t_bet)+SUM(cnt_t_raise)+SUM(cnt_r_bet)+SUM(cnt_r_raise)>0 THEN ROUND (100.0*(SUM(cnt_f_bet)+SUM(cnt_f_raise)+SUM(cnt_t_bet)+SUM(cnt_t_raise)+SUM(cnt_r_bet)+SUM(cnt_r_raise))/(SUM(cnt_f_call)+SUM(cnt_f_fold)+SUM(cnt_t_call)+SUM(cnt_t_fold)+SUM(cnt_r_call)+SUM(cnt_r_fold)+SUM(cnt_f_bet)+SUM(cnt_f_raise)+SUM(cnt_t_bet)+SUM(cnt_t_raise)+SUM(cnt_r_bet)+SUM(cnt_r_raise)), 0) END AS agg
                                FROM 
                                    cash_cache
                                JOIN 
                                    player 
                                ON 
                                    cash_cache.id_player = player.id_player 
                                GROUP BY player_name

                    UNION

                            SELECT
                                    player_name AS name,
                                    SUM(cnt_hands) AS totalhands,
                                    CASE WHEN SUM(cnt_pfr_opp)>0 THEN ROUND(100.0*SUM(cnt_pfr)/SUM(cnt_pfr_opp), 0) END AS pfr,
                                    CASE WHEN SUM(cnt_hands)-SUM(cnt_walks)>0 THEN ROUND(100.0*SUM(cnt_vpip)/(SUM(cnt_hands)-SUM(cnt_walks)), 0) END AS vpip,
                                    CASE WHEN SUM(cnt_p_ccall_opp)>0 THEN ROUND(100.0*SUM(cnt_p_ccall)/SUM(cnt_p_ccall_opp), 0) END AS coldCall,
                                    CASE WHEN SUM(cnt_p_3bet_opp)>0 THEN ROUND(100.0*SUM(cnt_p_3bet)/SUM(cnt_p_3bet_opp), 0) END AS threebet,
                                    CASE WHEN SUM(cnt_p_3bet_def_opp)>0 THEN ROUND(100.0*SUM(cnt_p_3bet_def_action_fold)/SUM(cnt_p_3bet_def_opp), 0) END AS foldTo3bet,
                                    CASE WHEN SUM(cnt_p_4bet_opp)-SUM(cnt_p_5bet_opp)>0 THEN ROUND(100.0*SUM(cnt_p_raise_3bet)/(SUM(cnt_p_4bet_opp)-SUM(cnt_p_5bet_opp)), 0) END AS fourbet,
                                    CASE WHEN SUM(cnt_p_4bet_def_opp)>0 THEN ROUND(100.0*SUM(cnt_p_4bet_def_action_fold)/SUM(cnt_p_4bet_def_opp), 0) END AS foldTo4bet,
                                    CASE WHEN SUM(cnt_f_cbet_opp)>0 THEN ROUND(100.0*SUM(cnt_f_cbet)/SUM(cnt_f_cbet_opp), 0) END AS cBFlop,
                                    CASE WHEN SUM(cnt_f_cbet_def_opp)>0 THEN ROUND(100.0*SUM(cnt_f_cbet_def_action_fold)/SUM(cnt_f_cbet_def_opp), 0) END AS foldToCBFlop,
                                    CASE WHEN SUM(cnt_f_cbet_def_opp)>0 THEN ROUND(100.0*SUM(cnt_f_cbet_def_action_raise)/SUM(cnt_f_cbet_def_opp), 0) END AS raiseCBFlop,
                                    CASE WHEN SUM(cnt_t_cbet_opp)>0 THEN ROUND(100.0*SUM(cnt_t_cbet)/SUM(cnt_t_cbet_opp), 0) END AS cBTurn,
                                    CASE WHEN SUM(cnt_t_cbet_def_opp)>0 THEN ROUND (100.0*SUM(cnt_t_cbet_def_action_fold)/SUM(cnt_t_cbet_def_opp), 0) END AS foldToCBTurn,
                                    CASE WHEN SUM(cnt_t_cbet_def_opp)>0 THEN ROUND (100.0*SUM(cnt_t_cbet_def_action_raise)/SUM(cnt_t_cbet_def_opp), 0) END AS raiseCBTurn,
                                    CASE WHEN SUM(cnt_r_cbet_opp)>0 THEN ROUND (100.0*SUM(cnt_r_cbet)/SUM(cnt_r_cbet_opp), 0) END AS cBRiver,
                                    CASE WHEN SUM(cnt_r_cbet_def_opp)>0 THEN ROUND (100.0*SUM(cnt_r_cbet_def_action_fold)/SUM(cnt_r_cbet_def_opp), 0) END AS foldToCBRiver,
                                    CASE WHEN SUM(cnt_r_cbet_def_opp)>0 THEN ROUND (100.0*SUM(cnt_r_cbet_def_action_raise)/SUM(cnt_r_cbet_def_opp), 0) END AS raiseCBRiver,
                                    CASE WHEN SUM(cnt_wtsd)>0 THEN ROUND(100.0*SUM(cnt_wtsd_won)/SUM(cnt_wtsd), 0) END AS wonAtSD,
                                    CASE WHEN SUM(cnt_f_saw)>0 THEN ROUND(100.0*SUM(cnt_wtsd)/SUM(cnt_f_saw), 0) END AS wentToSD,
                                    CASE WHEN SUM(cnt_f_call)+SUM(cnt_t_call)+SUM(cnt_r_call)>0 THEN (SUM(cnt_f_bet)+SUM(cnt_f_raise)+SUM(cnt_t_bet)+SUM(cnt_t_raise)+SUM(cnt_r_bet)+SUM(cnt_r_raise))/(SUM(cnt_f_call)+SUM(cnt_t_call)+SUM(cnt_r_call)) END AS aggF,
                                    CASE WHEN SUM(cnt_f_call)+SUM(cnt_f_fold)+SUM(cnt_t_call)+SUM(cnt_t_fold)+SUM(cnt_r_call)+SUM(cnt_r_fold)+SUM(cnt_f_bet)+SUM(cnt_f_raise)+SUM(cnt_t_bet)+SUM(cnt_t_raise)+SUM(cnt_r_bet)+SUM(cnt_r_raise)>0 THEN ROUND (100.0*(SUM(cnt_f_bet)+SUM(cnt_f_raise)+SUM(cnt_t_bet)+SUM(cnt_t_raise)+SUM(cnt_r_bet)+SUM(cnt_r_raise))/(SUM(cnt_f_call)+SUM(cnt_f_fold)+SUM(cnt_t_call)+SUM(cnt_t_fold)+SUM(cnt_r_call)+SUM(cnt_r_fold)+SUM(cnt_f_bet)+SUM(cnt_f_raise)+SUM(cnt_t_bet)+SUM(cnt_t_raise)+SUM(cnt_r_bet)+SUM(cnt_r_raise)), 0) END AS agg
                                FROM 
                                    tourney_cache
                                JOIN 
                                    player 
                                ON 
                                    tourney_cache.id_player = player.id_player 
                        GROUP BY player_name";

            }

            NpgsqlConnection connHM2 = new NpgsqlConnection(connHM2String);
            DataTable statsTable = new DataTable();
            try
            {
                connHM2.Open();
            }
            catch (Exception e)
            {
                Program.Log(e, connString);
                mainForm.BeginInvoke(new Action(delegate()
                {
                    if (mainForm.tracker == "pt4")
                        mainForm.Text = " Error connecting to the PT4 database ";
                    else
                        mainForm.Text = " Error connecting to the HM2 database ";
                }));
                return statsTable;
            };

            NpgsqlCommand npgSqlCommand = new NpgsqlCommand(sqlText, connHM2);
            NpgsqlDataReader npgSqlDataReader = npgSqlCommand.ExecuteReader();
            statsTable.Load(npgSqlDataReader);

            //Hashtable statsHashtable = new Hashtable();
            //dataGridViewStats.Rows.Clear();
            //foreach (var column in statsTable.Columns)
            //{
            //    string columnName = column.ToString();
            //    if (statsTable.Rows.Count == 0)
            //        statsHashtable.Add(columnName, -1);
            //    else if (columnName == "name")
            //    { }
            //    else
            //    {
            //        statsHashtable.Add(columnName, (statsTable.Rows[0][columnName].ToString() == "" ? -1 : Convert.ToInt32(statsTable.Rows[0][columnName].ToString())));
            //        if (statsTable.Rows[0][columnName].ToString() != "")
            //            dataGridViewStats.Rows.Add(columnName, statsTable.Rows[0][columnName].ToString());
            //    }
            //}

            return statsTable;
        }


    }

    public class Tables
    {
        public MainForm mainForm;

        public bool showCoord = true;
        //string windowClass = "#32770";//"GGnet.exe"
        string windowText = "";

        public IntPtr windowPtr;

        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;        // x position of upper-left corner
            public int top;         // y position of upper-left corner
            public int right;       // x position of lower-right corner
            public int bottom;      // y position of lower-right corner
            public override string ToString()
            {
                return string.Format(
                            "left, top: {0}, {1}; right, bottom {2},{3}; width x height: {4}x{5}",
                            left, top, right, bottom, right - left, bottom - top
                            );
            }
        }

        public string GetWindowClass(IntPtr hWnd)
        {
            int len = 260;
            StringBuilder sb = new StringBuilder(len);
            len = GetClassName(hWnd, sb, len);

            return sb.ToString(0, len);
        }

        string GetWindowText(IntPtr hWnd)
        {
            int len = GetWindowTextLength(hWnd) + 1;
            StringBuilder sb = new StringBuilder(len);
            len = GetWindowText(hWnd, sb, len);

            return sb.ToString(0, len);
        }

        public string FindWindow()
        {
            string name = "";
            EnumWindows(new EnumWindowsProc((hWnd, lParam) =>
            {
                windowText = GetWindowText(hWnd);
                if (windowText.Contains("Notes for "))
                {
                    name = windowText.Substring(10);
                    windowPtr = hWnd;
                }
                return true;
            }), IntPtr.Zero);

            return name;
        }

       

    }
   
}
