namespace HmLineAnalysis
{
    partial class FormSettings
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxDoNotCloseNotesWindow = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.checkBoxAlwaysOnTop = new System.Windows.Forms.CheckBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.checkBoxStopTimerWhenShowing = new System.Windows.Forms.CheckBox();
            this.checkBoxLog = new System.Windows.Forms.CheckBox();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxLog);
            this.groupBox2.Controls.Add(this.checkBoxDoNotCloseNotesWindow);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.dateTimePicker1);
            this.groupBox2.Controls.Add(this.checkBoxAlwaysOnTop);
            this.groupBox2.Controls.Add(this.buttonCancel);
            this.groupBox2.Controls.Add(this.buttonSave);
            this.groupBox2.Controls.Add(this.checkBoxStopTimerWhenShowing);
            this.groupBox2.Location = new System.Drawing.Point(7, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(175, 175);
            this.groupBox2.TabIndex = 591;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Settings";
            // 
            // checkBoxDoNotCloseNotesWindow
            // 
            this.checkBoxDoNotCloseNotesWindow.AutoSize = true;
            this.checkBoxDoNotCloseNotesWindow.Location = new System.Drawing.Point(5, 41);
            this.checkBoxDoNotCloseNotesWindow.Name = "checkBoxDoNotCloseNotesWindow";
            this.checkBoxDoNotCloseNotesWindow.Size = new System.Drawing.Size(152, 17);
            this.checkBoxDoNotCloseNotesWindow.TabIndex = 589;
            this.checkBoxDoNotCloseNotesWindow.Text = "do not close notes window";
            this.checkBoxDoNotCloseNotesWindow.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(-1, 123);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 13);
            this.label5.TabIndex = 588;
            this.label5.Text = " last loaded hand:";
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.CalendarTitleBackColor = System.Drawing.Color.Lavender;
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePicker1.Location = new System.Drawing.Point(91, 119);
            this.dateTimePicker1.MinDate = new System.DateTime(2000, 1, 1, 0, 0, 0, 0);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(78, 20);
            this.dateTimePicker1.TabIndex = 581;
            // 
            // checkBoxAlwaysOnTop
            // 
            this.checkBoxAlwaysOnTop.AutoSize = true;
            this.checkBoxAlwaysOnTop.Location = new System.Drawing.Point(5, 66);
            this.checkBoxAlwaysOnTop.Name = "checkBoxAlwaysOnTop";
            this.checkBoxAlwaysOnTop.Size = new System.Drawing.Size(91, 17);
            this.checkBoxAlwaysOnTop.TabIndex = 6;
            this.checkBoxAlwaysOnTop.Text = "always on top";
            this.checkBoxAlwaysOnTop.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(95, 145);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(4, 145);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 4;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // checkBoxStopTimerWhenShowing
            // 
            this.checkBoxStopTimerWhenShowing.AutoSize = true;
            this.checkBoxStopTimerWhenShowing.Location = new System.Drawing.Point(5, 18);
            this.checkBoxStopTimerWhenShowing.Name = "checkBoxStopTimerWhenShowing";
            this.checkBoxStopTimerWhenShowing.Size = new System.Drawing.Size(142, 17);
            this.checkBoxStopTimerWhenShowing.TabIndex = 3;
            this.checkBoxStopTimerWhenShowing.Text = "stop timer when showing";
            this.checkBoxStopTimerWhenShowing.UseVisualStyleBackColor = true;
            // 
            // checkBoxLog
            // 
            this.checkBoxLog.AutoSize = true;
            this.checkBoxLog.Location = new System.Drawing.Point(5, 91);
            this.checkBoxLog.Name = "checkBoxLog";
            this.checkBoxLog.Size = new System.Drawing.Size(44, 17);
            this.checkBoxLog.TabIndex = 590;
            this.checkBoxLog.Text = "Log";
            this.checkBoxLog.UseVisualStyleBackColor = true;
            // 
            // FormSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(188, 181);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FormSettings";
            this.Text = "Settings";
            this.TopMost = true;
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBoxAlwaysOnTop;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.CheckBox checkBoxStopTimerWhenShowing;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBoxDoNotCloseNotesWindow;
        private System.Windows.Forms.CheckBox checkBoxLog;
    }
}