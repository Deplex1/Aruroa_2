namespace AruroaMusicPlayer
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dataGridViewSongs;
        private System.Windows.Forms.Button btnLoadSongs;
        private System.Windows.Forms.Button btnPopular;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblNowPlaying;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TrackBar trackBarPosition;
        private System.Windows.Forms.Label lblCurrentTime;
        private System.Windows.Forms.Label lblTotalTime;
        private System.Windows.Forms.Timer timerUpdate;
        private System.Windows.Forms.Label lblGenres;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.dataGridViewSongs = new System.Windows.Forms.DataGridView();
            this.btnLoadSongs = new System.Windows.Forms.Button();
            this.btnPopular = new System.Windows.Forms.Button();
            this.btnNew = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblNowPlaying = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblGenres = new System.Windows.Forms.Label();
            this.trackBarPosition = new System.Windows.Forms.TrackBar();
            this.lblCurrentTime = new System.Windows.Forms.Label();
            this.lblTotalTime = new System.Windows.Forms.Label();
            this.timerUpdate = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSongs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPosition)).BeginInit();
            this.SuspendLayout();
            
            // dataGridViewSongs
            this.dataGridViewSongs.AllowUserToAddRows = false;
            this.dataGridViewSongs.AllowUserToDeleteRows = false;
            this.dataGridViewSongs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewSongs.Location = new System.Drawing.Point(12, 90);
            this.dataGridViewSongs.MultiSelect = false;
            this.dataGridViewSongs.Name = "dataGridViewSongs";
            this.dataGridViewSongs.ReadOnly = true;
            this.dataGridViewSongs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewSongs.Size = new System.Drawing.Size(760, 250);
            this.dataGridViewSongs.TabIndex = 0;
            this.dataGridViewSongs.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewSongs_CellDoubleClick);
            this.dataGridViewSongs.SelectionChanged += new System.EventHandler(this.dataGridViewSongs_SelectionChanged);
            
            // btnLoadSongs
            this.btnLoadSongs.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnLoadSongs.Location = new System.Drawing.Point(12, 12);
            this.btnLoadSongs.Name = "btnLoadSongs";
            this.btnLoadSongs.Size = new System.Drawing.Size(100, 30);
            this.btnLoadSongs.TabIndex = 1;
            this.btnLoadSongs.Text = "🔄 All Songs";
            this.btnLoadSongs.UseVisualStyleBackColor = true;
            this.btnLoadSongs.Click += new System.EventHandler(this.btnLoadSongs_Click);
            
            // btnPopular
            this.btnPopular.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnPopular.Location = new System.Drawing.Point(118, 12);
            this.btnPopular.Name = "btnPopular";
            this.btnPopular.Size = new System.Drawing.Size(100, 30);
            this.btnPopular.TabIndex = 2;
            this.btnPopular.Text = "🔥 Popular";
            this.btnPopular.UseVisualStyleBackColor = true;
            this.btnPopular.Click += new System.EventHandler(this.btnPopular_Click);
            
            // btnNew
            this.btnNew.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnNew.Location = new System.Drawing.Point(224, 12);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(100, 30);
            this.btnNew.TabIndex = 3;
            this.btnNew.Text = "✨ New";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            
            // txtSearch
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtSearch.Location = new System.Drawing.Point(12, 52);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.PlaceholderText = "Search songs...";
            this.txtSearch.Size = new System.Drawing.Size(660, 25);
            this.txtSearch.TabIndex = 4;
            this.txtSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSearch_KeyPress);
            
            // btnSearch
            this.btnSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSearch.Location = new System.Drawing.Point(678, 52);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(94, 25);
            this.btnSearch.TabIndex = 5;
            this.btnSearch.Text = "🔍 Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            
            // lblStatus
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(340, 20);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(200, 15);
            this.lblStatus.TabIndex = 6;
            this.lblStatus.Text = "Click a button to load songs";
            
            // lblGenres
            this.lblGenres.AutoSize = true;
            this.lblGenres.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblGenres.ForeColor = System.Drawing.Color.Gray;
            this.lblGenres.Location = new System.Drawing.Point(12, 345);
            this.lblGenres.Name = "lblGenres";
            this.lblGenres.Size = new System.Drawing.Size(100, 15);
            this.lblGenres.TabIndex = 7;
            this.lblGenres.Text = "Genres: -";
            
            // lblNowPlaying
            this.lblNowPlaying.AutoSize = true;
            this.lblNowPlaying.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblNowPlaying.Location = new System.Drawing.Point(12, 370);
            this.lblNowPlaying.Name = "lblNowPlaying";
            this.lblNowPlaying.Size = new System.Drawing.Size(150, 21);
            this.lblNowPlaying.TabIndex = 8;
            this.lblNowPlaying.Text = "🎵 Now Playing: -";
            
            // trackBarPosition
            this.trackBarPosition.Location = new System.Drawing.Point(12, 400);
            this.trackBarPosition.Maximum = 100;
            this.trackBarPosition.Name = "trackBarPosition";
            this.trackBarPosition.Size = new System.Drawing.Size(760, 45);
            this.trackBarPosition.TabIndex = 9;
            this.trackBarPosition.TickFrequency = 10;
            this.trackBarPosition.Scroll += new System.EventHandler(this.trackBarPosition_Scroll);
            
            // lblCurrentTime
            this.lblCurrentTime.AutoSize = true;
            this.lblCurrentTime.Location = new System.Drawing.Point(12, 445);
            this.lblCurrentTime.Name = "lblCurrentTime";
            this.lblCurrentTime.Size = new System.Drawing.Size(34, 15);
            this.lblCurrentTime.TabIndex = 10;
            this.lblCurrentTime.Text = "00:00";
            
            // lblTotalTime
            this.lblTotalTime.AutoSize = true;
            this.lblTotalTime.Location = new System.Drawing.Point(738, 445);
            this.lblTotalTime.Name = "lblTotalTime";
            this.lblTotalTime.Size = new System.Drawing.Size(34, 15);
            this.lblTotalTime.TabIndex = 11;
            this.lblTotalTime.Text = "00:00";
            
            // btnPlay
            this.btnPlay.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnPlay.Location = new System.Drawing.Point(250, 470);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(80, 40);
            this.btnPlay.TabIndex = 12;
            this.btnPlay.Text = "▶️ Play";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            
            // btnPause
            this.btnPause.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnPause.Location = new System.Drawing.Point(350, 470);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(80, 40);
            this.btnPause.TabIndex = 13;
            this.btnPause.Text = "⏸️ Pause";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            
            // btnStop
            this.btnStop.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStop.Location = new System.Drawing.Point(450, 470);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(80, 40);
            this.btnStop.TabIndex = 14;
            this.btnStop.Text = "⏹️ Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            
            // timerUpdate
            this.timerUpdate.Interval = 500;
            this.timerUpdate.Tick += new System.EventHandler(this.timerUpdate_Tick);
            
            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 521);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.lblTotalTime);
            this.Controls.Add(this.lblCurrentTime);
            this.Controls.Add(this.trackBarPosition);
            this.Controls.Add(this.lblNowPlaying);
            this.Controls.Add(this.lblGenres);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.btnNew);
            this.Controls.Add(this.btnPopular);
            this.Controls.Add(this.btnLoadSongs);
            this.Controls.Add(this.dataGridViewSongs);
            this.Name = "Form1";
            this.Text = "🎵 Aruroa Music Player";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSongs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPosition)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
