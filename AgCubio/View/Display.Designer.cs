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
            this.PlaytimeLabel = new System.Windows.Forms.Label();
            this.PlaytimeVal = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBoxServer
            // 
            this.textBoxServer.Location = new System.Drawing.Point(378, 91);
            this.textBoxServer.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBoxServer.Name = "textBoxServer";
            this.textBoxServer.Size = new System.Drawing.Size(150, 20);
            this.textBoxServer.TabIndex = 1;
            // 
            // connectButton
            // 
            this.connectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.connectButton.Location = new System.Drawing.Point(544, 69);
            this.connectButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(168, 61);
            this.connectButton.TabIndex = 3;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.Connect_Click);
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nameLabel.Location = new System.Drawing.Point(9, 88);
            this.nameLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(66, 24);
            this.nameLabel.TabIndex = 2;
            this.nameLabel.Text = "Name:";
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(76, 91);
            this.textBoxName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(153, 20);
            this.textBoxName.TabIndex = 0;
            // 
            // addressLabel
            // 
            this.addressLabel.AutoSize = true;
            this.addressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.addressLabel.Location = new System.Drawing.Point(233, 88);
            this.addressLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.addressLabel.Name = "addressLabel";
            this.addressLabel.Size = new System.Drawing.Size(145, 24);
            this.addressLabel.TabIndex = 4;
            this.addressLabel.Text = "Server Address:";
            // 
            // ExitToMainScreen
            // 
            this.ExitToMainScreen.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExitToMainScreen.Location = new System.Drawing.Point(183, 359);
            this.ExitToMainScreen.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ExitToMainScreen.Name = "ExitToMainScreen";
            this.ExitToMainScreen.Size = new System.Drawing.Size(291, 100);
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
            this.Statistics.Location = new System.Drawing.Point(261, 192);
            this.Statistics.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Statistics.Name = "Statistics";
            this.Statistics.Size = new System.Drawing.Size(162, 39);
            this.Statistics.TabIndex = 7;
            this.Statistics.Text = "Statistics:";
            this.Statistics.Visible = false;
            // 
            // MaxMassLabel
            // 
            this.MaxMassLabel.AutoSize = true;
            this.MaxMassLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaxMassLabel.Location = new System.Drawing.Point(178, 269);
            this.MaxMassLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.MaxMassLabel.Name = "MaxMassLabel";
            this.MaxMassLabel.Size = new System.Drawing.Size(159, 26);
            this.MaxMassLabel.TabIndex = 8;
            this.MaxMassLabel.Text = "Greatest Mass:";
            this.MaxMassLabel.Visible = false;
            // 
            // MaxPlayerMass
            // 
            this.MaxPlayerMass.AutoSize = true;
            this.MaxPlayerMass.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaxPlayerMass.Location = new System.Drawing.Point(418, 270);
            this.MaxPlayerMass.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.MaxPlayerMass.Name = "MaxPlayerMass";
            this.MaxPlayerMass.Size = new System.Drawing.Size(60, 26);
            this.MaxPlayerMass.TabIndex = 9;
            this.MaxPlayerMass.Text = "1000";
            this.MaxPlayerMass.Visible = false;
            // 
            // FPSlabel
            // 
            this.FPSlabel.AutoSize = true;
            this.FPSlabel.Location = new System.Drawing.Point(4, 4);
            this.FPSlabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.FPSlabel.Name = "FPSlabel";
            this.FPSlabel.Size = new System.Drawing.Size(30, 13);
            this.FPSlabel.TabIndex = 10;
            this.FPSlabel.Text = "FPS:";
            // 
            // FPSvalue
            // 
            this.FPSvalue.AutoSize = true;
            this.FPSvalue.Location = new System.Drawing.Point(40, 4);
            this.FPSvalue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.FPSvalue.Name = "FPSvalue";
            this.FPSvalue.Size = new System.Drawing.Size(13, 13);
            this.FPSvalue.TabIndex = 11;
            this.FPSvalue.Text = "0";
            // 
            // Masslabe
            // 
            this.Masslabe.AutoSize = true;
            this.Masslabe.Location = new System.Drawing.Point(4, 18);
            this.Masslabe.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Masslabe.Name = "Masslabe";
            this.Masslabe.Size = new System.Drawing.Size(35, 13);
            this.Masslabe.TabIndex = 12;
            this.Masslabe.Text = "Mass:";
            // 
            // MassValue
            // 
            this.MassValue.AutoSize = true;
            this.MassValue.Location = new System.Drawing.Point(40, 18);
            this.MassValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.MassValue.Name = "MassValue";
            this.MassValue.Size = new System.Drawing.Size(13, 13);
            this.MassValue.TabIndex = 13;
            this.MassValue.Text = "0";
            // 
            // PlaytimeLabel
            // 
            this.PlaytimeLabel.AutoSize = true;
            this.PlaytimeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PlaytimeLabel.Location = new System.Drawing.Point(178, 307);
            this.PlaytimeLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.PlaytimeLabel.Name = "PlaytimeLabel";
            this.PlaytimeLabel.Size = new System.Drawing.Size(103, 26);
            this.PlaytimeLabel.TabIndex = 14;
            this.PlaytimeLabel.Text = "Playtime:";
            this.PlaytimeLabel.Visible = false;
            // 
            // PlaytimeVal
            // 
            this.PlaytimeVal.AutoSize = true;
            this.PlaytimeVal.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PlaytimeVal.Location = new System.Drawing.Point(418, 307);
            this.PlaytimeVal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.PlaytimeVal.Name = "PlaytimeVal";
            this.PlaytimeVal.Size = new System.Drawing.Size(41, 26);
            this.PlaytimeVal.TabIndex = 15;
            this.PlaytimeVal.Text = "0 s";
            this.PlaytimeVal.Visible = false;
            // 
            // Display
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(736, 531);
            this.Controls.Add(this.PlaytimeVal);
            this.Controls.Add(this.PlaytimeLabel);
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
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
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
        private System.Windows.Forms.Label PlaytimeLabel;
        private System.Windows.Forms.Label PlaytimeVal;
    }
}

