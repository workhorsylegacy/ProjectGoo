using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace GameEngine
{
	public class CollisionRectAccess
	{
		ArrayList RectangleMatrix = null;

		public enum HitSide : uint
		{
			None,
			TopRight,
			TopLeft,
			BottomRight,
			BottomLeft,
		}

		//Get rectangular collision data for eash frame of animation in the texture array
		public CollisionRectAccess(Direct3D.Device ParentDevice, Direct3D.Texture[] ParentTextures, Direct3D.CustomVertex.PositionTextured[] ParentVerticies)
		{
			this.RectangleMatrix = new ArrayList();

			//The alpha color. this is the part that will be transparent.
			// the mask color is not used because the alpha blending replaces
			// the mask color with the alpha color (usually transparent black).
			const uint TRANSPARENT_BLACK = 0x00000000;

			//Holds texture size and pixels
			Direct3D.SurfaceDescription TextureDetails;
			uint[,] TexturePixels;
			
			//Holds texture width and height in pixels and floats
			int WidthInPixels, HeightInPixels;
			float WidthInFloats, HeightInFloats;
			float PixelSizeInFloats;

			//Holds a sprite's rect sides in pixels
			int SmallestXPixel, SmallestYPixel, LargestXPixel, LargestYPixel;

			//Holds a rect in floats
			float SmallestXFloat, SmallestYFloat, LargestXFloat, LargestYFloat;

			//for each frame of animation
			for(int f=0; f< ParentTextures.Length; f++)
			{
				//Get texture dimentions and create an array of its pixels
				TextureDetails = ParentTextures[f].GetLevelDescription(0);
				TexturePixels = (uint[,])ParentTextures[f].LockRectangle(typeof(uint), 0, Direct3D.LockFlags.ReadOnly, TextureDetails.Height,  TextureDetails.Width);

				//Get size of texture in pixels and floats
				WidthInPixels = TextureDetails.Width;
				HeightInPixels = TextureDetails.Height;
				WidthInFloats = ParentVerticies[2].X - ParentVerticies[3].X;
				HeightInFloats = ParentVerticies[1].Y - ParentVerticies[3].Y;
				PixelSizeInFloats = ((WidthInFloats / WidthInPixels) + (HeightInFloats / HeightInPixels)) * 0.5f;

				//create default values for the sides
				SmallestXPixel = WidthInPixels;
				SmallestYPixel = HeightInPixels;
				LargestXPixel = 0;
				LargestYPixel = 0;

				//Get the largest and smallest points in the texture that contain something
				for(int y=0; y<HeightInPixels; y++)
				{
					for(int x=0; x<WidthInPixels; x++)
					{
						if (TexturePixels[y, x] > TRANSPARENT_BLACK)
						{
							if (x > LargestXPixel)
								LargestXPixel = x;
							else if (x < SmallestXPixel)
								SmallestXPixel = x;

							if (y > LargestYPixel)
								LargestYPixel = y;
							else if (y < SmallestYPixel)
								SmallestYPixel = y;
						}
					}
				}

				//Get proper data if values weren't found
				if (SmallestXPixel > LargestXPixel || SmallestYPixel > LargestYPixel)
				{
					SmallestXPixel = 0;
					SmallestYPixel = 0;
					LargestXPixel = 0;
					LargestYPixel = 0;
				}

				//convert the sides in pixels to the sides in floats
				SmallestXFloat = SmallestXPixel * PixelSizeInFloats;
				SmallestYFloat = SmallestYPixel * PixelSizeInFloats;
				LargestXFloat = LargestXPixel * PixelSizeInFloats;
				LargestYFloat = LargestYPixel * PixelSizeInFloats;

				//Convert the sides to start from the center of the sprite
				// instead of the top left corner
				SmallestXFloat = (WidthInFloats * 0.5f) - SmallestXFloat;
				SmallestYFloat = (HeightInFloats * 0.5f) - SmallestYFloat;
				LargestXFloat = (WidthInFloats * 0.5f) - (WidthInFloats - LargestXFloat);
				LargestYFloat = (HeightInFloats * 0.5f) - (HeightInFloats - LargestYFloat);

				//record the sides in  floats
				float[] RectangleCollisions = {SmallestXFloat, SmallestYFloat, LargestXFloat, LargestYFloat};
				this.RectangleMatrix.Add(RectangleCollisions);

				ParentTextures[f].UnlockRectangle(0);
			}
		}

		/// <summary>
		/// Used to detect if there has been a collision between two objects such as sprites.
		/// </summary>
		public CollisionRectAccess.HitSide CheckObjectRectAgainst(SpriteAccess OtherObject, int CurrFrame, float CurrX, float CurrY)
		{
			CollisionRectAccess.HitSide RetVal = HitSide.None;

			//Get the locations of our rectange's sides in floats
			float[] RectangleCollisions = (float[]) this.RectangleMatrix[CurrFrame];
			float RectLeft = CurrX + RectangleCollisions[0];
			float RectTop = CurrY + RectangleCollisions[1];
			float RectRight = CurrX - RectangleCollisions[2];
			float RectBottom = CurrY - RectangleCollisions[3];

			//Get the locations the other rectange's sides in floats
			float[] OtherRectangleCollisions = (float[]) OtherObject.CollisionRects.GetIndex(OtherObject.Frame);
			float OtherRectLeft = OtherObject.X + OtherRectangleCollisions[0];
			float OtherRectTop = OtherObject.Y + OtherRectangleCollisions[1];
			float OtherRectRight = OtherObject.X - OtherRectangleCollisions[2];
			float OtherRectBottom = OtherObject.Y - OtherRectangleCollisions[3];

			//Check each side of the rect to see if it is within the other rect
			bool LeftInBounds = RectLeft >= OtherRectRight && RectRight <= OtherRectLeft;
			bool TopInBounds = RectTop >= OtherRectTop && RectBottom <= OtherRectTop;
			bool RightInBounds = RectRight >= OtherRectLeft && RectRight <= OtherRectRight;
			bool BottomInBounds = RectTop >= OtherRectBottom && RectBottom <= OtherRectBottom;

			//Check to see if both sides are in the rect
			//If they are pick one of the sides only depending on which is deeper
			if(RectTop <= OtherRectTop && RectBottom >= OtherRectBottom)
			{
				if(OtherRectTop - RectTop > RectBottom - OtherRectBottom)
					BottomInBounds = true;
				else
					TopInBounds = true;
			}
			if(RectLeft <= OtherRectLeft && RectRight >= OtherRectRight)
			{
				if(OtherRectLeft - RectLeft > RectRight - OtherRectRight)
					LeftInBounds = true;
				else
					RightInBounds = true;
			}


			//if this frame's rect is inside the other rect, there is a collision
			if(LeftInBounds && (BottomInBounds || TopInBounds))
			{
				if(BottomInBounds)
					RetVal = HitSide.BottomLeft;
				else if(TopInBounds)
					RetVal = HitSide.TopLeft;
			}
			else if(RightInBounds && (BottomInBounds || TopInBounds))
			{
				if(TopInBounds)
					RetVal = HitSide.TopRight;
				else
					RetVal = HitSide.BottomRight;
			}

			return(RetVal);
		}

		/// <summary>
		/// Used to detect collisions between the background and a sprite. Checks for collisions
		/// between the bottom of the sprite and the top of the background. Only returns a bool
		/// indicating if there was a collision.
		/// </summary>
		public bool CheckEnvironmentRectAgainst(SpriteAccess OtherSprite, int CurrFrame, float CurrX, float CurrY, ref float NewOtherSpriteY, float OtherSpriteMoreX, float OtherSpriteMoreY)
		{
			bool RetVal = false;

			//Get the locations of our rectange's sides in floats
			float[] RectangleCollisions = (float[]) this.RectangleMatrix[CurrFrame];
			float RectLeft = CurrX + RectangleCollisions[0];
			float RectTop = CurrY + RectangleCollisions[1];
			float RectRight = CurrX - RectangleCollisions[2];
			float RectBottom = CurrY - RectangleCollisions[3];

			//Get the locations the other rectange's sides in floats
			float[] OtherRectangleCollisions = (float[]) OtherSprite.CollisionRects.GetIndex(OtherSprite.Frame);
			float OtherRectLeft = OtherSprite.X + OtherRectangleCollisions[0];
			float OtherRectTop = OtherSprite.Y + OtherRectangleCollisions[1];
			float OtherRectRight = OtherSprite.X - OtherRectangleCollisions[2];
			float OtherRectBottom = OtherSprite.Y - OtherRectangleCollisions[3];

			//if this frame's rect is inside the other rect, there is a collision
			if(
					RectTop <= OtherRectBottom && 
					RectTop >= OtherRectBottom + OtherSpriteMoreY && 
					(
						(OtherRectRight <= RectLeft && OtherRectRight >= RectRight) || 
						(OtherRectLeft >= RectRight && OtherRectLeft <= RectLeft)
					)
				)
			{
				RetVal = true;

				//The OtherSprite's new Y will be
				// the environment's top + the distance from the
				// sprite's bottom to it's center.
				NewOtherSpriteY = RectTop + OtherRectangleCollisions[3];
			}

			return(RetVal);
		}

		public float[] GetIndex(int IndexToGet)
		{
			return((float[])this.RectangleMatrix[IndexToGet]);
		}
	}
}








