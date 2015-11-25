namespace kmine
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tm = new System.Windows.Forms.Timer(this.components);
            this.logBox = new System.Windows.Forms.ListBox();
            this.infoBox = new System.Windows.Forms.ListBox();
            this.CMode = new System.Windows.Forms.Label();
            this.pb1 = new System.Windows.Forms.PictureBox();
            this.ind = new System.Windows.Forms.PictureBox();
            this.grid = new kmine.GridField();
            this.shaft = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pb1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ind)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            // 
            // tm
            // 
            this.tm.Interval = 333;
            this.tm.Tick += new System.EventHandler(this.tm_Tick);
            // 
            // logBox
            // 
            this.logBox.FormattingEnabled = true;
            this.logBox.Location = new System.Drawing.Point(12, 216);
            this.logBox.Name = "logBox";
            this.logBox.Size = new System.Drawing.Size(170, 121);
            this.logBox.TabIndex = 2;
            // 
            // infoBox
            // 
            this.infoBox.FormattingEnabled = true;
            this.infoBox.Location = new System.Drawing.Point(188, 216);
            this.infoBox.Name = "infoBox";
            this.infoBox.Size = new System.Drawing.Size(165, 121);
            this.infoBox.TabIndex = 3;
            // 
            // CMode
            // 
            this.CMode.AutoSize = true;
            this.CMode.Location = new System.Drawing.Point(53, 9);
            this.CMode.Name = "CMode";
            this.CMode.Size = new System.Drawing.Size(0, 13);
            this.CMode.TabIndex = 5;
            // 
            // pb1
            // 
            this.pb1.Location = new System.Drawing.Point(56, 37);
            this.pb1.Name = "pb1";
            this.pb1.Size = new System.Drawing.Size(297, 173);
            this.pb1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb1.TabIndex = 1;
            this.pb1.TabStop = false;
            // 
            // ind
            // 
            this.ind.Image = global::kmine.Properties.Resources.clock;
            this.ind.Location = new System.Drawing.Point(12, 12);
            this.ind.Name = "ind";
            this.ind.Size = new System.Drawing.Size(16, 16);
            this.ind.TabIndex = 0;
            this.ind.TabStop = false;
            // 
            // grid
            // 
            this.grid.Image = ((System.Drawing.Image)(resources.GetObject("grid.Image")));
            this.grid.Location = new System.Drawing.Point(30, 343);
            this.grid.Name = "grid";
            this.grid.Size = new System.Drawing.Size(300, 300);
            this.grid.TabIndex = 4;
            this.grid.TabStop = false;
            // 
            // shaft
            // 
            this.shaft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.shaft.AutoSize = true;
            this.shaft.Location = new System.Drawing.Point(353, 9);
            this.shaft.Name = "shaft";
            this.shaft.Size = new System.Drawing.Size(0, 13);
            this.shaft.TabIndex = 6;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(383, 656);
            this.Controls.Add(this.shaft);
            this.Controls.Add(this.CMode);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.infoBox);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.pb1);
            this.Controls.Add(this.ind);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Location = new System.Drawing.Point(0, 128);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Mine";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pb1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ind)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer tm;
        private System.Windows.Forms.PictureBox ind;
        private System.Windows.Forms.PictureBox pb1;
        private System.Windows.Forms.ListBox logBox;
        private System.Windows.Forms.ListBox infoBox;
        private GridField grid;
        private System.Windows.Forms.Label CMode;
        private System.Windows.Forms.Label shaft;
    }
}

