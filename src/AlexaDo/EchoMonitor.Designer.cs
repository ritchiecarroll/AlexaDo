//******************************************************************************************************
//  EchoMonitor.Designer.cs - Gbtc
//
//  Copyright © 2016, James Ritchie Carroll.  All Rights Reserved.
//  MIT License (MIT)
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/12/2015 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

namespace AlexaDo
{
    partial class EchoMonitor
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
            if (disposing)
            {
                components?.Dispose();
                m_authenticationManager?.Dispose();
                m_activityProcessor?.Dispose();
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EchoMonitor));
            this.AuthenticationBrowser = new System.Windows.Forms.WebBrowser();
            this.ActivityBrowser = new System.Windows.Forms.WebBrowser();
            this.URLFeebackStrip = new System.Windows.Forms.StatusStrip();
            this.URLFeedbackLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.TTSVoiceLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.VoiceDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            this.URLLoadingProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.QueryTimer = new System.Windows.Forms.Timer(this.components);
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.TestCommandButton = new System.Windows.Forms.ToolStripSplitButton();
            this.NotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.NotifyIconContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ShowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HideMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ReauthenticateMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.BrowserContainer = new System.Windows.Forms.SplitContainer();
            this.URLFeebackStrip.SuspendLayout();
            this.StatusStrip.SuspendLayout();
            this.NotifyIconContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BrowserContainer)).BeginInit();
            this.BrowserContainer.Panel1.SuspendLayout();
            this.BrowserContainer.Panel2.SuspendLayout();
            this.BrowserContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // AuthenticationBrowser
            // 
            this.AuthenticationBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AuthenticationBrowser.Location = new System.Drawing.Point(0, 0);
            this.AuthenticationBrowser.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.AuthenticationBrowser.MinimumSize = new System.Drawing.Size(30, 31);
            this.AuthenticationBrowser.Name = "AuthenticationBrowser";
            this.AuthenticationBrowser.ScriptErrorsSuppressed = true;
            this.AuthenticationBrowser.Size = new System.Drawing.Size(1228, 839);
            this.AuthenticationBrowser.TabIndex = 2;
            this.AuthenticationBrowser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.BrowserControl_DocumentCompleted);
            this.AuthenticationBrowser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.BrowserControl_Navigating);
            this.AuthenticationBrowser.ProgressChanged += new System.Windows.Forms.WebBrowserProgressChangedEventHandler(this.BrowserControl_ProgressChanged);
            // 
            // ActivityBrowser
            // 
            this.ActivityBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ActivityBrowser.Location = new System.Drawing.Point(0, 0);
            this.ActivityBrowser.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ActivityBrowser.MinimumSize = new System.Drawing.Size(30, 31);
            this.ActivityBrowser.Name = "ActivityBrowser";
            this.ActivityBrowser.ScriptErrorsSuppressed = true;
            this.ActivityBrowser.Size = new System.Drawing.Size(1228, 95);
            this.ActivityBrowser.TabIndex = 2;
            this.ActivityBrowser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.BrowserControl_DocumentCompleted);
            this.ActivityBrowser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.BrowserControl_Navigating);
            this.ActivityBrowser.ProgressChanged += new System.Windows.Forms.WebBrowserProgressChangedEventHandler(this.BrowserControl_ProgressChanged);
            // 
            // URLFeebackStrip
            // 
            this.URLFeebackStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.URLFeebackStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.URLFeebackStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.URLFeedbackLabel,
            this.TTSVoiceLabel,
            this.VoiceDropDown,
            this.URLLoadingProgress});
            this.URLFeebackStrip.Location = new System.Drawing.Point(0, 960);
            this.URLFeebackStrip.Name = "URLFeebackStrip";
            this.URLFeebackStrip.Padding = new System.Windows.Forms.Padding(2, 0, 21, 0);
            this.URLFeebackStrip.Size = new System.Drawing.Size(1228, 34);
            this.URLFeebackStrip.TabIndex = 1;
            // 
            // URLFeedbackLabel
            // 
            this.URLFeedbackLabel.AutoToolTip = true;
            this.URLFeedbackLabel.Name = "URLFeedbackLabel";
            this.URLFeedbackLabel.Size = new System.Drawing.Size(940, 29);
            this.URLFeedbackLabel.Spring = true;
            this.URLFeedbackLabel.Text = "URL Feeback";
            this.URLFeedbackLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TTSVoiceLabel
            // 
            this.TTSVoiceLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.TTSVoiceLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.TTSVoiceLabel.Name = "TTSVoiceLabel";
            this.TTSVoiceLabel.Size = new System.Drawing.Size(95, 29);
            this.TTSVoiceLabel.Text = "TTS &Voice:";
            // 
            // VoiceDropDown
            // 
            this.VoiceDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.VoiceDropDown.Image = ((System.Drawing.Image)(resources.GetObject("VoiceDropDown.Image")));
            this.VoiceDropDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.VoiceDropDown.Name = "VoiceDropDown";
            this.VoiceDropDown.Size = new System.Drawing.Size(18, 32);
            this.VoiceDropDown.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.VoiceDropDown_DropDownItemClicked);
            // 
            // URLLoadingProgress
            // 
            this.URLLoadingProgress.Name = "URLLoadingProgress";
            this.URLLoadingProgress.Size = new System.Drawing.Size(150, 28);
            // 
            // QueryTimer
            // 
            this.QueryTimer.Interval = 15000;
            this.QueryTimer.Tick += new System.EventHandler(this.QueryTimer_Tick);
            // 
            // StatusStrip
            // 
            this.StatusStrip.Dock = System.Windows.Forms.DockStyle.Top;
            this.StatusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel,
            this.TestCommandButton});
            this.StatusStrip.Location = new System.Drawing.Point(0, 0);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Padding = new System.Windows.Forms.Padding(2, 0, 21, 0);
            this.StatusStrip.Size = new System.Drawing.Size(1228, 22);
            this.StatusStrip.SizingGrip = false;
            this.StatusStrip.TabIndex = 0;
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(1205, 17);
            this.StatusLabel.Spring = true;
            this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TestCommandButton
            // 
            this.TestCommandButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.TestCommandButton.Enabled = false;
            this.TestCommandButton.Image = ((System.Drawing.Image)(resources.GetObject("TestCommandButton.Image")));
            this.TestCommandButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TestCommandButton.Name = "TestCommandButton";
            this.TestCommandButton.Size = new System.Drawing.Size(164, 29);
            this.TestCommandButton.Text = "&Test Command...";
            this.TestCommandButton.ToolTipText = "Test Command...";
            this.TestCommandButton.Visible = false;
            this.TestCommandButton.ButtonClick += new System.EventHandler(this.TestCommandButton_ButtonClick);
            // 
            // NotifyIcon
            // 
            this.NotifyIcon.BalloonTipTitle = "AlexaDo";
            this.NotifyIcon.ContextMenuStrip = this.NotifyIconContextMenu;
            this.NotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("NotifyIcon.Icon")));
            this.NotifyIcon.Tag = "AlexaDo";
            this.NotifyIcon.Text = "AlexaDo";
            this.NotifyIcon.Visible = true;
            this.NotifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon_MouseClick);
            this.NotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon_MouseDoubleClick);
            // 
            // NotifyIconContextMenu
            // 
            this.NotifyIconContextMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.NotifyIconContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ShowMenuItem,
            this.HideMenuItem,
            this.Separator1,
            this.ReauthenticateMenuItem,
            this.Separator2,
            this.ExitMenuItem});
            this.NotifyIconContextMenu.Name = "NotifyIconContextMenu";
            this.NotifyIconContextMenu.Size = new System.Drawing.Size(213, 136);
            // 
            // ShowMenuItem
            // 
            this.ShowMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.ShowMenuItem.Name = "ShowMenuItem";
            this.ShowMenuItem.Size = new System.Drawing.Size(212, 30);
            this.ShowMenuItem.Text = "Show";
            this.ShowMenuItem.Click += new System.EventHandler(this.ShowMenuItem_Click);
            // 
            // HideMenuItem
            // 
            this.HideMenuItem.Name = "HideMenuItem";
            this.HideMenuItem.Size = new System.Drawing.Size(212, 30);
            this.HideMenuItem.Text = "Hide";
            this.HideMenuItem.Click += new System.EventHandler(this.HideMenuItem_Click);
            // 
            // Separator1
            // 
            this.Separator1.Name = "Separator1";
            this.Separator1.Size = new System.Drawing.Size(209, 6);
            // 
            // ReauthenticateMenuItem
            // 
            this.ReauthenticateMenuItem.Name = "ReauthenticateMenuItem";
            this.ReauthenticateMenuItem.Size = new System.Drawing.Size(212, 30);
            this.ReauthenticateMenuItem.Text = "Reauthenticate";
            this.ReauthenticateMenuItem.Click += new System.EventHandler(this.ReauthenticateMenuItem_Click);
            // 
            // Separator2
            // 
            this.Separator2.Name = "Separator2";
            this.Separator2.Size = new System.Drawing.Size(209, 6);
            // 
            // ExitMenuItem
            // 
            this.ExitMenuItem.Name = "ExitMenuItem";
            this.ExitMenuItem.Size = new System.Drawing.Size(212, 30);
            this.ExitMenuItem.Text = "Exit";
            this.ExitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
            // 
            // BrowserContainer
            // 
            this.BrowserContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BrowserContainer.Location = new System.Drawing.Point(0, 22);
            this.BrowserContainer.Name = "BrowserContainer";
            this.BrowserContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // BrowserContainer.Panel1
            // 
            this.BrowserContainer.Panel1.Controls.Add(this.AuthenticationBrowser);
            // 
            // BrowserContainer.Panel2
            // 
            this.BrowserContainer.Panel2.Controls.Add(this.ActivityBrowser);
            this.BrowserContainer.Size = new System.Drawing.Size(1228, 938);
            this.BrowserContainer.SplitterDistance = 839;
            this.BrowserContainer.TabIndex = 3;
            // 
            // EchoMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1228, 994);
            this.Controls.Add(this.BrowserContainer);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.URLFeebackStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "EchoMonitor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AlexaDo - Echo Activity Monitor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Browser_FormClosing);
            this.Load += new System.EventHandler(this.Browser_Load);
            this.Shown += new System.EventHandler(this.Browser_Shown);
            this.URLFeebackStrip.ResumeLayout(false);
            this.URLFeebackStrip.PerformLayout();
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.NotifyIconContextMenu.ResumeLayout(false);
            this.BrowserContainer.Panel1.ResumeLayout(false);
            this.BrowserContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.BrowserContainer)).EndInit();
            this.BrowserContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip URLFeebackStrip;
        private System.Windows.Forms.ToolStripProgressBar URLLoadingProgress;
        private System.Windows.Forms.ToolStripDropDownButton VoiceDropDown;
        private System.Windows.Forms.ToolStripStatusLabel URLFeedbackLabel;
        private System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.NotifyIcon NotifyIcon;
        private System.Windows.Forms.ContextMenuStrip NotifyIconContextMenu;
        private System.Windows.Forms.ToolStripMenuItem ShowMenuItem;
        private System.Windows.Forms.ToolStripMenuItem HideMenuItem;
        private System.Windows.Forms.ToolStripSeparator Separator1;
        private System.Windows.Forms.ToolStripSeparator Separator2;
        private System.Windows.Forms.ToolStripMenuItem ReauthenticateMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ExitMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel TTSVoiceLabel;
        internal System.Windows.Forms.Timer QueryTimer;
        internal System.Windows.Forms.WebBrowser AuthenticationBrowser;
        internal System.Windows.Forms.WebBrowser ActivityBrowser;
        private System.Windows.Forms.ToolStripSplitButton TestCommandButton;
        private System.Windows.Forms.SplitContainer BrowserContainer;
    }
}

