namespace AgCubio
{
    partial class Display
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxServer = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.nameLabel = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.addressLabel = new System.Windows.Forms.Label();
            this.ExitToMainScreen = new System.Windows.Forms.Button();
            this.Statistics = new System.Windows.Forms.Label();
            this.MaxMassLabel = new System.Windows.Forms.Label();
            this.MaxPlayerMass = new System.Windows.Forms.Label();
            this.FPSlabel = new System.Windows.Forms.Label();
            this.FPSvalue = new System.Windows.Forms.Label();
            this.Masslabe = new System.Windows.Forms.Label();
            this.MassValue = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBoxServer
            // 
            this.textBoxServer.Location = new System.Drawing.Point(504, 112);
            this.textBoxServer.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxServer.Name = "textBoxServer";
            this.textBoxServer.Size = new System.Drawing.Size(199, 22);
            this.textBoxServer.TabIndex = 1;
            // 
            // connectButton
            // 
            this.connectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.connectButton.Location = new System.Drawing.Point(726, 85);
            this.connectButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(224, 75);
            this.connectButton.TabIndex = 3;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.Connect_Click);
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nameLabel.Location = new System.Drawing.Point(12, 108);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(84, 29);
            this.nameLabel.TabIndex = 2;
            this.nameLabel.Text = "Name:";
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(102, 112);
            this.textBoxName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(203, 22);
            this.textBoxName.TabIndex = 0;
            // 
            // addressLabel
            // 
            this.addressLabel.AutoSize = true;
            this.addressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.addressLabel.Location = new System.Drawing.Point(311, 108);
            this.addressLabel.Name = "addressLabel";
            this.addressLabel.Size = new System.Drawing.Size(185, 29);
            this.addressLabel.TabIndex = 4;
            this.addressLabel.Text = "Server Address:";
            // 
            // ExitToMainScreen
            // 
            this.ExitToMainScreen.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExitToMainScreen.Location = new System.Drawing.Point(244, 442);
            this.ExitToMainScreen.Name = "ExitToMainScreen";
            this.ExitToMainScreen.Size = new System.Drawing.Size(388, 123);
            this.ExitToMainScreen.TabIndex = 6;
            this.ExitToMainScreen.Text = "Exit To Main Screen";
            this.ExitToMainScreen.UseVisualStyleBackColor = true;
            this.ExitToMainScreen.Visible = false;
            this.ExitToMainScreen.Click += new System.EventHandler(this.ExitToMainScreen_Click);
            // 
            // Statistics
            // 
            this.Statistics.AutoSize = true;
            this.Statistics.Font = new System.Drawing.Font("Microsoft Sans Serif", 25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Statistics.Location = new System.Drawing.Point(348, 236);
            this.Statistics.Name = "Statistics";
            this.Statistics.Size = new System.Drawing.Size(202, 48);
            this.Statistics.TabIndex = 7;
            this.Statistics.Text = "Statistics:";
            this.Statistics.Visible = false;
            // 
            // MaxMassLabel
            // 
            this.MaxMassLabel.AutoSize = true;
            this.MaxMassLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaxMassLabel.Location = new System.Drawing.Point(238, 331);
            this.MaxMassLabel.Name = "MaxMassLabel";
            this.MaxMassLabel.Size = new System.Drawing.Size(206, 32);
            this.MaxMassLabel.TabIndex = 8;
            this.MaxMassLabel.Text = "Greatest Mass:";
            this.MaxMassLabel.Visible = false;
            // 
            // MaxPlayerMass
            // 
            this.MaxPlayerMass.AutoSize = true;
            this.MaxPlayerMass.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaxPlayerMass.Location = new System.Drawing.Point(558, 332);
            this.MaxPlayerMass.Name = "MaxPlayerMass";
            this.MaxPlayerMass.Size = new System.Drawing.Size(74, 31);
            this.MaxPlayerMass.TabIndex = 9;
            this.MaxPlayerMass.Text = "1000";
            this.MaxPlayerMass.Visible = false;
            // 
            // FPSlabel
            // 
            this.FPSlabel.AutoSize = true;
            this.FPSlabel.Location = new System.Drawing.Point(5, 5);
            this.FPSlabel.Name = "FPSlabel";
            this.FPSlabel.Size = new System.Drawing.Size(38, 17);
            this.FPSlabel.TabIndex = 10;
            this.FPSlabel.Text = "FPS:";
            // 
            // FPSvalue
            // 
            this.FPSvalue.AutoSize = true;
            this.FPSvalue.Location = new System.Drawing.Point(54, 5);
            this.FPSvalue.Name = "FPSvalue";
            this.FPSvalue.Size = new System.Drawing.Size(63, 17);
            this.FPSvalue.TabIndex = 11;
            this.FPSvalue.Text = "waiting...";
            // 
            // Masslabe
            // 
            this.Masslabe.AutoSize = true;
            this.Masslabe.Location = new System.Drawing.Point(5, 22);
            this.Masslabe.Name = "Masslabe";
            this.Masslabe.Size = new System.Drawing.Size(45, 17);
            this.Masslabe.TabIndex = 12;
            this.Masslabe.Text = "Mass:";
            // 
            // MassValue
            // 
            this.MassValue.AutoSize = true;
            this.MassValue.Location = new System.Drawing.Point(54, 22);
            this.MassValue.Name = "MassValue";
            this.MassValue.Size = new System.Drawing.Size(16, 17);
            this.MassValue.TabIndex = 13;
            this.MassValue.Text = "0";
            // 
            // Display
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(982, 653);
            this.Controls.Add(this.MassValue);
            this.Controls.Add(this.Masslabe);
            this.Controls.Add(this.FPSvalue);
            this.Controls.Add(this.FPSlabel);
            this.Controls.Add(this.MaxPlayerMass);
            this.Controls.Add(this.MaxMassLabel);
            this.Controls.Add(this.Statistics);
            this.Controls.Add(this.ExitToMainScreen);
            this.Controls.Add(this.addressLabel);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.nameLabel);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.textBoxServer);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Display";
            this.Text = "AgCubio Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxServer;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label addressLabel;
        private System.Windows.Forms.Button ExitToMainScreen;
        private System.Windows.Forms.Label Statistics;
        private System.Windows.Forms.Label MaxMassLabel;
        private System.Windows.Forms.Label MaxPlayerMass;
        private System.Windows.Forms.Label FPSlabel;
        private System.Windows.Forms.Label FPSvalue;
        private System.Windows.Forms.Label Masslabe;
        private System.Windows.Forms.Label MassValue;
    }
}

