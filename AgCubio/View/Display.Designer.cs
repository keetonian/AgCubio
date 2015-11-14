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
            this.MassLabel = new System.Windows.Forms.Label();
            this.PlayerMass = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBoxServer
            // 
            this.textBoxServer.Location = new System.Drawing.Point(469, 114);
            this.textBoxServer.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxServer.Name = "textBoxServer";
            this.textBoxServer.Size = new System.Drawing.Size(199, 22);
            this.textBoxServer.TabIndex = 1;
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(693, 89);
            this.connectButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(224, 75);
            this.connectButton.TabIndex = 3;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(51, 114);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(49, 17);
            this.nameLabel.TabIndex = 2;
            this.nameLabel.Text = "Name:";
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(107, 114);
            this.textBoxName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(203, 22);
            this.textBoxName.TabIndex = 0;
            // 
            // addressLabel
            // 
            this.addressLabel.AutoSize = true;
            this.addressLabel.Location = new System.Drawing.Point(353, 117);
            this.addressLabel.Name = "addressLabel";
            this.addressLabel.Size = new System.Drawing.Size(110, 17);
            this.addressLabel.TabIndex = 4;
            this.addressLabel.Text = "Server Address:";
            // 
            // ExitToMainScreen
            // 
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
            // MassLabel
            // 
            this.MassLabel.AutoSize = true;
            this.MassLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MassLabel.Location = new System.Drawing.Point(235, 331);
            this.MassLabel.Name = "MassLabel";
            this.MassLabel.Size = new System.Drawing.Size(206, 32);
            this.MassLabel.TabIndex = 8;
            this.MassLabel.Text = "Greatest Mass:";
            this.MassLabel.Visible = false;
            // 
            // PlayerMass
            // 
            this.PlayerMass.AutoSize = true;
            this.PlayerMass.Location = new System.Drawing.Point(489, 341);
            this.PlayerMass.Name = "PlayerMass";
            this.PlayerMass.Size = new System.Drawing.Size(0, 17);
            this.PlayerMass.TabIndex = 9;
            this.PlayerMass.Visible = false;
            // 
            // Display
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(982, 1000);
            this.Controls.Add(this.PlayerMass);
            this.Controls.Add(this.MassLabel);
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
        private System.Windows.Forms.Label MassLabel;
        private System.Windows.Forms.Label PlayerMass;
    }
}

