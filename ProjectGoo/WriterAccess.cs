using System;
using System.Drawing;
using System.Collections;

using DirectX = Microsoft.DirectX;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace GameEngine
{
	public class WriterAccess
	{
		ScreenAccess ParentScreen = null;
		Direct3D.Font ScoreTextBuffer = null;
		System.Drawing.Font ScoreFont = null;

		public WriterAccess(ScreenAccess NewParentScreen)
		{
			this.ParentScreen = NewParentScreen;

			//Create fonts type and font buffer
			this.ScoreFont = new System.Drawing.Font("Verdana", 14.0f, FontStyle.Italic | FontStyle.Bold);
			this.ScoreTextBuffer = new Direct3D.Font(this.ParentScreen.device, this.ScoreFont);
		}

		private void WriteText(string TextToWrite, int NewX, int NewY, Color c)
		{
			this.ScoreTextBuffer.DrawText(
											null, TextToWrite, new Rectangle(NewX, NewY, 300, 30),
											Direct3D.DrawTextFormat.NoClip | Direct3D.DrawTextFormat.ExpandTabs |
											Direct3D.DrawTextFormat.WordBreak , c
										);
		}

		public void WriteScoreLeft(string NewScore)
		{
			this.WriteText(NewScore, 10, 10, System.Drawing.Color.Pink);
		}

		public void WriteScoreRight(string NewScore)
		{
			this.WriteText(NewScore, this.ParentScreen.ClientRectangle.Width - 220, 10, System.Drawing.Color.Pink);
		}
	}
}
