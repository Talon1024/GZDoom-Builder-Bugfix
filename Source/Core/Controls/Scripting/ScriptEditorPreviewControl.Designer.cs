#region Namespaces
using System.Windows.Forms;
#endregion

namespace CodeImp.DoomBuilder.Controls
{
	partial class ScriptEditorPreviewControl
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
			if(disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
#if Windows
            this.scriptedit = new ScintillaNET.Scintilla();
			this.SuspendLayout();
			// 
			// scriptedit
			// 
			this.scriptedit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.scriptedit.CaretWidth = 2;
			this.scriptedit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.scriptedit.ExtraAscent = 1;
			this.scriptedit.ExtraDescent = 1;
			this.scriptedit.FontQuality = ScintillaNET.FontQuality.LcdOptimized;
			this.scriptedit.Location = new System.Drawing.Point(0, 0);
			this.scriptedit.Name = "scriptedit";
			this.scriptedit.ScrollWidth = 200;
			this.scriptedit.Size = new System.Drawing.Size(300, 150);
			this.scriptedit.TabIndex = 1;
			this.scriptedit.TabStop = false;
			this.scriptedit.UseTabs = true;
			this.scriptedit.ViewWhitespace = ScintillaNET.WhitespaceMode.VisibleAlways;
			this.scriptedit.WhitespaceSize = 2;
			this.scriptedit.UpdateUI += new System.EventHandler<ScintillaNET.UpdateUIEventArgs>(this.scriptedit_UpdateUI);
#else
            this.scriptedit = new TextBox();
            this.SuspendLayout();
            this.scriptedit.AcceptsReturn = true;
            this.scriptedit.AcceptsTab = true;
            this.scriptedit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.scriptedit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scriptedit.Location = new System.Drawing.Point(0, 0);
            this.scriptedit.Name = "scriptedit";
            this.scriptedit.Size = new System.Drawing.Size(300, 150);
            this.scriptedit.TabIndex = 1;
            this.scriptedit.TabStop = false;
            this.scriptedit.Multiline = true;
#endif
            // 
            // ScriptEditorPreviewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.scriptedit);
			this.Name = "ScriptEditorPreviewControl";
			this.Size = new System.Drawing.Size(300, 150);
			this.ResumeLayout(false);

		}

#endregion

#if Windows
        private ScintillaNET.Scintilla scriptedit;
#else
        private TextBox scriptedit;
#endif
    }
}
