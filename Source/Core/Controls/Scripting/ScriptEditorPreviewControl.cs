#region ================== Namespaces

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Rendering;
#if Windows
using ScintillaNET;
#endif

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	internal partial class ScriptEditorPreviewControl : UserControl
	{
#region ================== Constants

		private const int HIGHLIGHT_INDICATOR = 8;

        #endregion

        #region ================== Variables
#if Windows
        private Dictionary<int, ScriptStyleType> styletranslation;
		private string highlightedword;
		private Color indicatorcolor;
		private int lastcaretpos;
		private readonly HashSet<char> bracechars;
#else
        private string fontname;
        private bool fontbold;
        private int fontsize;
#endif

#endregion

        #region ================== Properties

        public string FontName {
            set { ApplyFont(value); }
#if !Windows
            get { return fontname; }
#endif
        }
        public int FontSize
        {
            set { ApplyFontSize(value); }
#if !Windows
            get { return fontsize; }
#endif
        }
        public bool FontBold {
            set { ApplyFontBold(value); }
#if !Windows
            get { return fontbold; }
#endif
        }

#if Windows
        public int TabWidth { set { scriptedit.TabWidth = value; } }
        public bool ShowLineNumbers { set { UpdateLineNumbers(value); } }
		public bool ShowFolding { set { UpdateFolding(value); } }
#endif

        // Colors
        public Color ScriptBackground 
		{ 
			set
			{
#if Windows
                scriptedit.Styles[Style.Default].BackColor = value;
				scriptedit.CaretForeColor = PixelColor.FromColor(value).Inverse().ToColor();
				scriptedit.Styles[Style.LineNumber].BackColor = value;
				scriptedit.SetWhitespaceBackColor(true, value);

				foreach(KeyValuePair<int, ScriptStyleType> group in styletranslation)
					scriptedit.Styles[group.Key].BackColor = value;
#else
                scriptedit.BackColor = value;
#endif
            } 
		}

#if Windows
		public Color FoldForeColor { set { for(int i = 25; i < 32; i++) scriptedit.Markers[i].SetBackColor(value); } }
		public Color FoldBackColor
		{
			set
			{
                scriptedit.SetFoldMarginColor(true, value);
				scriptedit.SetFoldMarginHighlightColor(true, value);
				for(int i = 25; i < 32; i++) scriptedit.Markers[i].SetForeColor(value);
    }
}

        public Color LineNumbers { set { ApplyStyleColor(ScriptStyleType.LineNumber, value); } }
		public Color Comments { set { ApplyStyleColor(ScriptStyleType.Comment, value); } }
		public Color Keywords { set { ApplyStyleColor(ScriptStyleType.Keyword, value); } }
		public Color Properties { set { ApplyStyleColor(ScriptStyleType.Property, value); } }
		public Color Literals { set { ApplyStyleColor(ScriptStyleType.Literal, value); } }
		public Color Constants { set { ApplyStyleColor(ScriptStyleType.Constant, value); } }
		public Color Strings { set { ApplyStyleColor(ScriptStyleType.String, value); } }
		public Color Includes { set { ApplyStyleColor(ScriptStyleType.Include, value); } }
        public Color PlainText { set { ApplyStyleColor(ScriptStyleType.PlainText, value); } }
#else
        public Color PlainText { set { scriptedit.ForeColor = value; } }
#endif

#if Windows
        public Color SelectionForeColor { set { scriptedit.SetSelectionForeColor(true, value); } }
		public Color SelectionBackColor { set { scriptedit.SetSelectionBackColor(true, value); } }
		public Color WhitespaceColor
		{
			set
			{
				scriptedit.SetWhitespaceForeColor(true, value);
				scriptedit.SetWhitespaceBackColor(false, value); // Otherwise selection back color won't be applied correctly...
			}
		}
		public Color BraceHighlight { set { scriptedit.Styles[Style.BraceLight].BackColor = value; } }
		public Color BadBraceHighlight { set { scriptedit.Styles[Style.BraceBad].BackColor = value; } }
		public Color ScriptIndicator { set { indicatorcolor = value; UpdateWordHighlight(); } }
#endif

#endregion

#region ================== Constructor / Setup

        public ScriptEditorPreviewControl()
		{
			InitializeComponent();

			if(LicenseManager.UsageMode != LicenseUsageMode.Designtime)
			{
#if Windows
                bracechars = new HashSet<char> { '{', '}', '(', ')', '[', ']' };
#endif
                SetupStyles();

			}
		}

		// This sets up the script editor with default settings
		private void SetupStyles()
		{
#if Windows
            // Symbol margin
            scriptedit.Margins[0].Type = MarginType.Symbol;
			scriptedit.Margins[0].Width = 20;
			scriptedit.Margins[0].Mask = 0; // No markers here
			scriptedit.Margins[0].Cursor = MarginCursor.Arrow;
			scriptedit.Margins[0].Sensitive = true;

			// Line numbers margin
			if(General.Settings.ScriptShowLineNumbers)
			{
				scriptedit.Margins[1].Type = MarginType.Number;
				scriptedit.Margins[1].Width = 16;
			}
			scriptedit.Margins[1].Mask = 0; // No markers here

			// Spacing margin
			scriptedit.Margins[2].Type = MarginType.Symbol;
			scriptedit.Margins[2].Width = 5;
			scriptedit.Margins[2].Cursor = MarginCursor.Arrow;
			scriptedit.Margins[2].Mask = 0; // No markers here

			// Set lexer
			scriptedit.Lexer = Lexer.Cpp;

			// Set script editor preview text
			scriptedit.Text = "#include \"zcommon.acs\"" + "\n" +
							  "script 667 ENTER //A simple script example" + "\n" +
							  "{" + "\n" +
								"    int red = CR_RED;" + "\n" +
								"\tPrint(s:\"Welcome!\");" + "\n" +
								"\tThing_ChangeTID(0, 667);" + "\n" +
							  "} } // <- A spare brace!";

			// No text editing beyond this point!
			scriptedit.ReadOnly = true;

			// Set keywords (0)
			const string keywords = "Print HudMessageBold Thing_ChangeTID";
			scriptedit.SetKeywords(0, keywords.ToLowerInvariant());

			// Set constants (1)
			const string constants = "CR_RED";
			scriptedit.SetKeywords(1, constants.ToLowerInvariant());

			// Set properties (3)
			const string properties = "script enter int";
			scriptedit.SetKeywords(3, properties.ToLowerInvariant());

			// Reset document slyle
			scriptedit.ClearDocumentStyle();
			scriptedit.StyleResetDefault();

			// Create style translation dictionary
			styletranslation = new Dictionary<int, ScriptStyleType>
			{
				{ 0, ScriptStyleType.PlainText }, // End of line
				{ 10, ScriptStyleType.PlainText }, // Operator
				{ 11, ScriptStyleType.PlainText }, // Identifier
				{ 33, ScriptStyleType.LineNumber },
				{ 1, ScriptStyleType.Comment },
				{ 2, ScriptStyleType.Comment },
				{ 5, ScriptStyleType.Keyword },
				{ 4, ScriptStyleType.Literal },
				{ 7, ScriptStyleType.Literal },
				{ 16, ScriptStyleType.Constant },
				{ 37, ScriptStyleType.LineNumber },
				{ 6, ScriptStyleType.String },
				{ 9, ScriptStyleType.Include },
				{ 19, ScriptStyleType.Property },
			};

            // Set the default style and settings
            scriptedit.Styles[Style.Default].Font = General.Settings.ScriptFontName;
			scriptedit.Styles[Style.Default].Size = General.Settings.ScriptFontSize;
			scriptedit.Styles[Style.Default].Bold = General.Settings.ScriptFontBold;
			scriptedit.Styles[Style.Default].Italic = false;
			scriptedit.Styles[Style.Default].Underline = false;
			scriptedit.Styles[Style.Default].Case = StyleCase.Mixed;
			scriptedit.Styles[Style.Default].ForeColor = General.Colors.PlainText.ToColor();
			scriptedit.Styles[Style.Default].BackColor = General.Colors.ScriptBackground.ToColor();
			scriptedit.CaretPeriod = SystemInformation.CaretBlinkTime;
			scriptedit.CaretForeColor = General.Colors.ScriptBackground.Inverse().ToColor();

			// Set tabulation settings
			scriptedit.UseTabs = General.Settings.ScriptUseTabs;
			scriptedit.TabWidth = General.Settings.ScriptTabWidth;

			// This applies the default style to all styles
			scriptedit.StyleClearAll();

			// Set style for linenumbers and margins
			scriptedit.Styles[Style.LineNumber].BackColor = General.Colors.ScriptBackground.ToColor();
			scriptedit.SetFoldMarginColor(true, General.Colors.ScriptFoldBackColor.ToColor());
			scriptedit.SetFoldMarginHighlightColor(true, General.Colors.ScriptFoldBackColor.ToColor());
			for(int i = 25; i < 32; i++)
			{
				scriptedit.Markers[i].SetForeColor(General.Colors.ScriptFoldBackColor.ToColor());
				scriptedit.Markers[i].SetBackColor(General.Colors.ScriptFoldForeColor.ToColor());
			}

			// Set style for (mis)matching braces
			scriptedit.Styles[Style.BraceLight].BackColor = General.Colors.ScriptBraceHighlight.ToColor();
			scriptedit.Styles[Style.BraceBad].BackColor = General.Colors.ScriptBadBraceHighlight.ToColor();

			// Set whitespace color
			scriptedit.SetWhitespaceForeColor(true, General.Colors.ScriptWhitespace.ToColor());

			// Set selection colors
			scriptedit.SetSelectionForeColor(true, General.Colors.ScriptSelectionForeColor.ToColor());
			scriptedit.SetSelectionBackColor(true, General.Colors.ScriptSelectionBackColor.ToColor());
			indicatorcolor = General.Colors.ScriptIndicator.ToColor();

			// Set words colors
			foreach(KeyValuePair<int, ScriptStyleType> group in styletranslation)
			{
				// Apply color to style
				int colorindex;
				switch(group.Value)
				{
					case ScriptStyleType.PlainText: colorindex = ColorCollection.PLAINTEXT; break;
					case ScriptStyleType.Comment: colorindex = ColorCollection.COMMENTS; break;
					case ScriptStyleType.Constant: colorindex = ColorCollection.CONSTANTS; break;
					case ScriptStyleType.Keyword: colorindex = ColorCollection.KEYWORDS; break;
					case ScriptStyleType.LineNumber: colorindex = ColorCollection.LINENUMBERS; break;
					case ScriptStyleType.Literal: colorindex = ColorCollection.LITERALS; break;
					case ScriptStyleType.String: colorindex = ColorCollection.STRINGS; break;
					case ScriptStyleType.Include: colorindex = ColorCollection.INCLUDES; break;
					case ScriptStyleType.Property: colorindex = ColorCollection.PROPERTIES; break;
					default: colorindex = ColorCollection.PLAINTEXT; break;
				}

				scriptedit.Styles[group.Key].ForeColor = General.Colors.Colors[colorindex].ToColor();
				scriptedit.Styles[group.Key].BackColor = General.Colors.ScriptBackground.ToColor();
			}
            // Setup folding
            UpdateFolding(General.Settings.ScriptShowFolding);

			// Rearrange the layout
			this.PerformLayout();
#else
            scriptedit.ForeColor = General.Colors.Colors[ColorCollection.PLAINTEXT].ToColor();
            scriptedit.BackColor = General.Colors.ScriptBackground.ToColor();

            FontStyle fontStyle = General.Settings.ScriptFontBold ? FontStyle.Bold : FontStyle.Regular;
            scriptedit.Font = new Font(General.Settings.ScriptFontName, (float) General.Settings.ScriptFontSize, fontStyle);
#endif
		}

#endregion

#region ================== Methods

#if !Windows
        private Font GetFont()
        {
            FontStyle fs = fontbold ? FontStyle.Bold : FontStyle.Regular;
            Font font = new Font(fontname, (float) fontsize, fs);
            return font;
        }
#endif

#if Windows
        private void ApplyStyleColor(ScriptStyleType type, Color color)
		{
            foreach(KeyValuePair<int, ScriptStyleType> group in styletranslation)
				if(group.Value == type) scriptedit.Styles[group.Key].ForeColor = color;
        }
#endif

        private void ApplyFont(string font)
		{
#if Windows
            foreach(KeyValuePair<int, ScriptStyleType> group in styletranslation)
				scriptedit.Styles[group.Key].Font = font;
#else
            fontname = font;
            scriptedit.Font = GetFont();
#endif
        }

		private void ApplyFontBold(bool bold)
		{
#if Windows
            foreach(KeyValuePair<int, ScriptStyleType> group in styletranslation)
				scriptedit.Styles[group.Key].Bold = bold;
#else
            fontbold = bold;
            scriptedit.Font = GetFont();
#endif
        }

		private void ApplyFontSize(int size)
		{
#if Windows
            foreach(KeyValuePair<int, ScriptStyleType> group in styletranslation)
				scriptedit.Styles[group.Key].Size = size;
#else
            fontsize = size;
            scriptedit.Font = GetFont();
#endif
        }
#if Windows
        private void UpdateLineNumbers(bool show)
		{
			if(show)
			{
				scriptedit.Margins[1].Type = MarginType.Number;
				scriptedit.Margins[1].Width = 16;
			}
			else
			{
				scriptedit.Margins[1].Type = MarginType.Symbol;
				scriptedit.Margins[1].Width = 0;
			}
		}

		private void UpdateFolding(bool show)
		{
			if(show)
			{
				// Instruct the lexer to calculate folding
				scriptedit.SetProperty("fold", "1");
				scriptedit.SetProperty("fold.compact", "0"); // 1 = folds blank lines
				scriptedit.SetProperty("fold.comment", "1"); // Enable block comment folding
				scriptedit.SetProperty("fold.preprocessor", "1"); // Enable #region folding
				scriptedit.SetFoldFlags(FoldFlags.LineAfterContracted); // Draw line below if not expanded

				// Configure a margin to display folding symbols
				scriptedit.Margins[2].Type = MarginType.Symbol;
				scriptedit.Margins[2].Mask = Marker.MaskFolders;
				scriptedit.Margins[2].Sensitive = true;
				scriptedit.Margins[2].Width = 12;

				// Configure folding markers with respective symbols
				scriptedit.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
				scriptedit.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
				scriptedit.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
				scriptedit.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
				scriptedit.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
				scriptedit.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
				scriptedit.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

				// Enable automatic folding
				scriptedit.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);
			}
			else
			{
				// Disable folding
				scriptedit.SetProperty("fold", "0");
				scriptedit.SetProperty("fold.compact", "0");

				scriptedit.Margins[2].Type = MarginType.Symbol;
				scriptedit.Margins[2].Mask = 0; // No markers here
				scriptedit.Margins[2].Sensitive = false;
				scriptedit.Margins[2].Width = 5;

				scriptedit.AutomaticFold = AutomaticFold.None;
			}
		}

		private void UpdateWordHighlight()
		{
			// If a word is selected, highlight the same words
			if(scriptedit.SelectedText != highlightedword)
			{
				// Highlight only when whole word is selected
				if(!string.IsNullOrEmpty(scriptedit.SelectedText) && scriptedit.GetWordFromPosition(scriptedit.SelectionStart) == scriptedit.SelectedText) {
					HighlightWord(scriptedit.SelectedText);
				}
				else
				{
					// Clear highlight
					scriptedit.IndicatorCurrent = HIGHLIGHT_INDICATOR;
					scriptedit.IndicatorClearRange(0, scriptedit.TextLength);
				}

				highlightedword = scriptedit.SelectedText;
			}
		}

		private void HighlightWord(string text)
		{
			// Remove all uses of our indicator
			scriptedit.IndicatorCurrent = HIGHLIGHT_INDICATOR;
			scriptedit.IndicatorClearRange(0, scriptedit.TextLength);

			// Update indicator appearance
			scriptedit.Indicators[HIGHLIGHT_INDICATOR].Style = IndicatorStyle.RoundBox;
			scriptedit.Indicators[HIGHLIGHT_INDICATOR].Under = true;
			scriptedit.Indicators[HIGHLIGHT_INDICATOR].ForeColor = indicatorcolor;
			scriptedit.Indicators[HIGHLIGHT_INDICATOR].OutlineAlpha = 50;
			scriptedit.Indicators[HIGHLIGHT_INDICATOR].Alpha = 30;

			// Search the document
			scriptedit.TargetStart = 0;
			scriptedit.TargetEnd = scriptedit.TextLength;
			scriptedit.SearchFlags = SearchFlags.WholeWord;

			while(scriptedit.SearchInTarget(text) != -1)
			{
				//mxd. Don't mark currently selected word
				if(scriptedit.SelectionStart != scriptedit.TargetStart && scriptedit.SelectionEnd != scriptedit.TargetEnd)
				{
					// Mark the search results with the current indicator
					scriptedit.IndicatorFillRange(scriptedit.TargetStart, scriptedit.TargetEnd - scriptedit.TargetStart);
				}

				// Search the remainder of the document
				scriptedit.TargetStart = scriptedit.TargetEnd;
				scriptedit.TargetEnd = scriptedit.TextLength;
			}
		}
        
#endif
#endregion

#region ================== Events
#if Windows
        private void scriptedit_UpdateUI(object sender, UpdateUIEventArgs e)
		{
			UpdateWordHighlight();

			// Has the caret changed position?
			int caretpos = scriptedit.CurrentPosition;
			if(lastcaretpos != caretpos)
			{
				lastcaretpos = caretpos;
				int bracepos1 = -1;

				// Is there a brace to the left or right?
				if(caretpos > 0 && bracechars.Contains((char)scriptedit.GetCharAt(caretpos - 1)))
					bracepos1 = (caretpos - 1);
				else if(bracechars.Contains((char)(scriptedit.GetCharAt(caretpos))))
					bracepos1 = caretpos;

				if(bracepos1 > -1)
				{
					// Find the matching brace
					int bracepos2 = scriptedit.BraceMatch(bracepos1);
					if(bracepos2 == Scintilla.InvalidPosition)
						scriptedit.BraceBadLight(bracepos1);
					else
						scriptedit.BraceHighlight(bracepos1, bracepos2);
				}
				else
				{
					// Turn off brace matching
					scriptedit.BraceHighlight(Scintilla.InvalidPosition, Scintilla.InvalidPosition);
				}
			}
		}
#endif
#endregion
	}
}


