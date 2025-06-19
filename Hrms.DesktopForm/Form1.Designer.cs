namespace Hrms.DesktopForm
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            connectionString = new TextBox();
            label1 = new Label();
            label2 = new Label();
            testDatabaseConnection = new Button();
            connectionStatus = new Label();
            button1 = new Button();
            deviceStatus = new Label();
            deviceCheckBoxList = new CheckedListBox();
            SuspendLayout();
            // 
            // connectionString
            // 
            connectionString.Location = new Point(12, 49);
            connectionString.Multiline = true;
            connectionString.Name = "connectionString";
            connectionString.Size = new Size(569, 43);
            connectionString.TabIndex = 0;
            connectionString.TextChanged += textBox1_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Tahoma", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(12, 19);
            label1.Name = "label1";
            label1.Size = new Size(182, 23);
            label1.TabIndex = 1;
            label1.Text = "Connection String";
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Tahoma", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(12, 118);
            label2.Name = "label2";
            label2.Size = new Size(83, 23);
            label2.TabIndex = 2;
            label2.Text = "Devices";
            label2.Click += label2_Click;
            // 
            // testDatabaseConnection
            // 
            testDatabaseConnection.BackColor = Color.LightGreen;
            testDatabaseConnection.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            testDatabaseConnection.Location = new Point(615, 49);
            testDatabaseConnection.Name = "testDatabaseConnection";
            testDatabaseConnection.Size = new Size(111, 42);
            testDatabaseConnection.TabIndex = 3;
            testDatabaseConnection.Text = "Connect";
            testDatabaseConnection.UseVisualStyleBackColor = false;
            testDatabaseConnection.Click += button1_Click;
            // 
            // connectionStatus
            // 
            connectionStatus.AutoSize = true;
            connectionStatus.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point);
            connectionStatus.Location = new Point(405, 19);
            connectionStatus.Name = "connectionStatus";
            connectionStatus.Size = new Size(0, 21);
            connectionStatus.TabIndex = 6;
            connectionStatus.Click += label3_Click;
            // 
            // button1
            // 
            button1.BackColor = Color.SpringGreen;
            button1.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            button1.Location = new Point(12, 367);
            button1.Name = "button1";
            button1.Size = new Size(280, 63);
            button1.TabIndex = 12;
            button1.Text = "Set Database and Devices";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click_1;
            // 
            // deviceStatus
            // 
            deviceStatus.AutoSize = true;
            deviceStatus.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point);
            deviceStatus.Location = new Point(333, 387);
            deviceStatus.Name = "deviceStatus";
            deviceStatus.Size = new Size(0, 21);
            deviceStatus.TabIndex = 13;
            // 
            // deviceCheckBoxList
            // 
            deviceCheckBoxList.BackColor = SystemColors.GradientInactiveCaption;
            deviceCheckBoxList.Font = new Font("Trebuchet MS", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            deviceCheckBoxList.FormattingEnabled = true;
            deviceCheckBoxList.Location = new Point(12, 156);
            deviceCheckBoxList.Name = "deviceCheckBoxList";
            deviceCheckBoxList.RightToLeft = RightToLeft.No;
            deviceCheckBoxList.Size = new Size(714, 179);
            deviceCheckBoxList.TabIndex = 20;
            deviceCheckBoxList.ThreeDCheckBoxes = true;
            deviceCheckBoxList.SelectedIndexChanged += checkedListBox1_SelectedIndexChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(738, 533);
            Controls.Add(deviceCheckBoxList);
            Controls.Add(deviceStatus);
            Controls.Add(button1);
            Controls.Add(connectionStatus);
            Controls.Add(testDatabaseConnection);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(connectionString);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox connectionString;
        private Label label1;
        private Label label2;
        private Button testDatabaseConnection;
        private Label connectionStatus;
        private Button button1;
        private Label deviceStatus;
        private CheckedListBox deviceCheckBoxList;
    }
}