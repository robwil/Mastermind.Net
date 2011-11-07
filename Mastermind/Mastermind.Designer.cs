namespace Mastermind
{
    partial class Mastermind
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
            this.btnSubmitRow = new System.Windows.Forms.Button();
            this.btnStartGame = new System.Windows.Forms.Button();
            this.btnAIStart = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblPossibilities = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnSubmitRow
            // 
            this.btnSubmitRow.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSubmitRow.Location = new System.Drawing.Point(330, 470);
            this.btnSubmitRow.Name = "btnSubmitRow";
            this.btnSubmitRow.Size = new System.Drawing.Size(85, 30);
            this.btnSubmitRow.TabIndex = 0;
            this.btnSubmitRow.Text = "Submit Row";
            this.btnSubmitRow.UseVisualStyleBackColor = true;
            this.btnSubmitRow.Click += new System.EventHandler(this.btnSubmitRow_Click);
            // 
            // btnStartGame
            // 
            this.btnStartGame.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStartGame.Location = new System.Drawing.Point(122, 583);
            this.btnStartGame.Name = "btnStartGame";
            this.btnStartGame.Size = new System.Drawing.Size(85, 30);
            this.btnStartGame.TabIndex = 1;
            this.btnStartGame.Text = "Start Game";
            this.btnStartGame.UseVisualStyleBackColor = true;
            this.btnStartGame.Click += new System.EventHandler(this.btnStartGame_Click);
            // 
            // btnAIStart
            // 
            this.btnAIStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAIStart.Location = new System.Drawing.Point(256, 583);
            this.btnAIStart.Name = "btnAIStart";
            this.btnAIStart.Size = new System.Drawing.Size(134, 30);
            this.btnAIStart.TabIndex = 2;
            this.btnAIStart.Text = "Let Computer Play";
            this.btnAIStart.UseVisualStyleBackColor = true;
            this.btnAIStart.Click += new System.EventHandler(this.btnAIStart_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(303, 470);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(140, 18);
            this.progressBar.TabIndex = 3;
            this.progressBar.UseWaitCursor = true;
            // 
            // lblPossibilities
            // 
            this.lblPossibilities.AutoSize = true;
            this.lblPossibilities.Location = new System.Drawing.Point(301, 492);
            this.lblPossibilities.Name = "lblPossibilities";
            this.lblPossibilities.Size = new System.Drawing.Size(96, 13);
            this.lblPossibilities.TabIndex = 4;
            this.lblPossibilities.Text = "Possibilities: 32768";
            // 
            // Mastermind
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(475, 625);
            this.Controls.Add(this.lblPossibilities);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnAIStart);
            this.Controls.Add(this.btnStartGame);
            this.Controls.Add(this.btnSubmitRow);
            this.Name = "Mastermind";
            this.Text = "Mastermind.NET";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSubmitRow;
        private System.Windows.Forms.Button btnStartGame;
        private System.Windows.Forms.Button btnAIStart;
        protected internal System.Windows.Forms.ProgressBar progressBar;
        protected internal System.Windows.Forms.Label lblPossibilities;
    }
}

