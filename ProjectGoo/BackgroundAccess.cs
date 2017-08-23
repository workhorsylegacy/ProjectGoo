using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace GameEngine
{
	public class BackgroundAccess
	{
		private ArrayList Platforms; // holds SpriteAccess
		private ArrayList Polls; // holds PollAccess

		private class PollAccess : SpriteAccess
		{
			public PollType TypeOfPoll;

			public PollAccess(Direct3D.Device ParentDevice, string TextureLocation, float NewX, float NewY, float NewZ, int NewWidth, int NewHeight, int NewSheetWidth, int NewSheetHeight, Color NewMaskColor, uint CollisionRectWidth, uint CollisionRectHeight, PollType NewTypeOfPoll)
				: base(ParentDevice, TextureLocation, NewX, NewY, NewZ, NewWidth, NewHeight, NewSheetWidth, NewSheetHeight, NewMaskColor, CollisionRectWidth, CollisionRectHeight)
			{
				this.TypeOfPoll = NewTypeOfPoll;
			}

			public PollAccess(SpriteAccess ParentSprite, float NewX, float NewY, float NewZ, PollType NewTypeOfPoll)
				: base(ParentSprite, NewX, NewY, NewZ)
			{
				this.TypeOfPoll = NewTypeOfPoll;
			}
		}

		private enum PollType : int
		{
			Top,
			Center,
			Bottom
		}

		public BackgroundAccess(Direct3D.Device NewParentDevice)
		{
			float PlatformWidthInFloats = SpaceAndTime.LengthFrom2DTo3D(64.0f);
			float CurrColmn = 2.0f;

			this.Platforms = new ArrayList();
			SpriteAccess TemplatePlatform = new SpriteAccess(NewParentDevice, GameConfig.Files.Platform, 2.0f-(-1*SpaceAndTime.LengthFrom2DTo3D(64.0f)), -1.4f, SpaceAndTime.SpriteZLocation, 64, 64, 64, 64, Color.FromArgb(0x00, 0x00, 0xFF, 0x00), 0, 0);
			this.Platforms.Add(TemplatePlatform);
			this.Platforms.Add(new SpriteAccess(TemplatePlatform, 2.0f-(0*SpaceAndTime.LengthFrom2DTo3D(64.0f)), -1.4f, SpaceAndTime.SpriteZLocation));
			this.Platforms.Add(new SpriteAccess(TemplatePlatform, 2.0f-(1*SpaceAndTime.LengthFrom2DTo3D(64.0f)), -1.4f, SpaceAndTime.SpriteZLocation));
			this.Platforms.Add(new SpriteAccess(TemplatePlatform, 2.0f-(6*SpaceAndTime.LengthFrom2DTo3D(64.0f)), -1.4f, SpaceAndTime.SpriteZLocation));
			this.Platforms.Add(new SpriteAccess(TemplatePlatform, 2.0f-(7*SpaceAndTime.LengthFrom2DTo3D(64.0f)), -1.4f, SpaceAndTime.SpriteZLocation));
			this.Platforms.Add(new SpriteAccess(TemplatePlatform, 2.0f-(8*SpaceAndTime.LengthFrom2DTo3D(64.0f)), -1.4f, SpaceAndTime.SpriteZLocation));

			this.Platforms.Add(new SpriteAccess(TemplatePlatform, 2.0f-(-1*SpaceAndTime.LengthFrom2DTo3D(64.0f)), -.1f, SpaceAndTime.SpriteZLocation));
			this.Platforms.Add(new SpriteAccess(TemplatePlatform, 2.0f-(1*SpaceAndTime.LengthFrom2DTo3D(64.0f)), -.1f, SpaceAndTime.SpriteZLocation));
			this.Platforms.Add(new SpriteAccess(TemplatePlatform, 2.0f-(2.5f*SpaceAndTime.LengthFrom2DTo3D(64.0f)), -.1f, SpaceAndTime.SpriteZLocation));
			this.Platforms.Add(new SpriteAccess(TemplatePlatform, 2.0f-(3.5f*SpaceAndTime.LengthFrom2DTo3D(64.0f)), -.1f, SpaceAndTime.SpriteZLocation));
			this.Platforms.Add(new SpriteAccess(TemplatePlatform, 2.0f-(4.5f*SpaceAndTime.LengthFrom2DTo3D(64.0f)), -.1f, SpaceAndTime.SpriteZLocation));
			this.Platforms.Add(new SpriteAccess(TemplatePlatform, 2.0f-(6*SpaceAndTime.LengthFrom2DTo3D(64.0f)), -.1f, SpaceAndTime.SpriteZLocation));			
			this.Platforms.Add(new SpriteAccess(TemplatePlatform, 2.0f-(8*SpaceAndTime.LengthFrom2DTo3D(64.0f)), -.1f, SpaceAndTime.SpriteZLocation));

			this.Platforms.Add(new SpriteAccess(TemplatePlatform, 2.0f-(-.5f*SpaceAndTime.LengthFrom2DTo3D(64.0f)), 1.2f, SpaceAndTime.SpriteZLocation));
			this.Platforms.Add(new SpriteAccess(TemplatePlatform, 2.0f-(0.5f*SpaceAndTime.LengthFrom2DTo3D(64.0f)), 1.2f, SpaceAndTime.SpriteZLocation));
			this.Platforms.Add(new SpriteAccess(TemplatePlatform, 2.0f-(1.5f*SpaceAndTime.LengthFrom2DTo3D(64.0f)), 1.2f, SpaceAndTime.SpriteZLocation));
			this.Platforms.Add(new SpriteAccess(TemplatePlatform, 2.0f-(3.5f*SpaceAndTime.LengthFrom2DTo3D(64.0f)), 1.2f, SpaceAndTime.SpriteZLocation));
			this.Platforms.Add(new SpriteAccess(TemplatePlatform, 2.0f-(5.5f*SpaceAndTime.LengthFrom2DTo3D(64.0f)), 1.2f, SpaceAndTime.SpriteZLocation));
			this.Platforms.Add(new SpriteAccess(TemplatePlatform, 2.0f-(6.5f*SpaceAndTime.LengthFrom2DTo3D(64.0f)), 1.2f, SpaceAndTime.SpriteZLocation));			
			this.Platforms.Add(new SpriteAccess(TemplatePlatform, 2.0f-(7.5f*SpaceAndTime.LengthFrom2DTo3D(64.0f)), 1.2f, SpaceAndTime.SpriteZLocation));

			//Set location of platforms
			foreach(SpriteAccess CurrPlatform in this.Platforms)
				CurrPlatform.Location = GameConfig.Locations.Platforms;
	
			//polls
			float CurrPollY = 0.46f;
			SpriteAccess PollTop = new SpriteAccess(NewParentDevice, GameConfig.Files.PollTop, 2.0f-(0*SpaceAndTime.LengthFrom2DTo3D(64.0f)), CurrPollY, SpaceAndTime.SpriteZLocation, 64, 64, 64, 64, Color.FromArgb(0x00, 0x00, 0xFF, 0x00), 0, 0);
			SpriteAccess PollCenter = new SpriteAccess(NewParentDevice, GameConfig.Files.PollCenter, 2.0f-(0*SpaceAndTime.LengthFrom2DTo3D(64.0f)), CurrPollY, SpaceAndTime.SpriteZLocation, 64, 64, 64, 64, Color.FromArgb(0x00, 0x00, 0xFF, 0x00), 0, 0);
			SpriteAccess PollBottom = new SpriteAccess(NewParentDevice, GameConfig.Files.PollBottom, 2.0f-(0*SpaceAndTime.LengthFrom2DTo3D(64.0f)), CurrPollY, SpaceAndTime.SpriteZLocation, 64, 64, 64, 64, Color.FromArgb(0x00, 0x00, 0xFF, 0x00), 0, 0);

			this.Polls = new ArrayList();
			this.Polls.Add(new PollAccess(PollTop, 2.0f-(0*SpaceAndTime.LengthFrom2DTo3D(64.0f)), CurrPollY, SpaceAndTime.SpriteZLocation, PollType.Top));
			
			CurrPollY -= PlatformWidthInFloats;
			this.Polls.Add(new PollAccess(PollCenter, 2.0f-(0*SpaceAndTime.LengthFrom2DTo3D(64.0f)), CurrPollY, SpaceAndTime.SpriteZLocation, PollType.Center));

			CurrPollY -= PlatformWidthInFloats;
			this.Polls.Add(new PollAccess(PollCenter, CurrColmn, CurrPollY, SpaceAndTime.SpriteZLocation, PollType.Center));

			CurrPollY -= PlatformWidthInFloats;
			this.Polls.Add(new PollAccess(PollBottom, 2.0f-(0*SpaceAndTime.LengthFrom2DTo3D(64.0f)), CurrPollY, SpaceAndTime.SpriteZLocation, PollType.Bottom));

			CurrPollY = 0.46f;
			this.Polls.Add(new PollAccess(PollTop, 2.0f-(7*SpaceAndTime.LengthFrom2DTo3D(64.0f)), CurrPollY, SpaceAndTime.SpriteZLocation, PollType.Top));
			
			CurrPollY -= PlatformWidthInFloats;
			this.Polls.Add(new PollAccess(PollCenter, 2.0f-(7*SpaceAndTime.LengthFrom2DTo3D(64.0f)), CurrPollY, SpaceAndTime.SpriteZLocation, PollType.Center));

			CurrPollY -= PlatformWidthInFloats;
			this.Polls.Add(new PollAccess(PollCenter, 2.0f-(7*SpaceAndTime.LengthFrom2DTo3D(64.0f)), CurrPollY, SpaceAndTime.SpriteZLocation, PollType.Center));

			CurrPollY -= PlatformWidthInFloats;
			this.Polls.Add(new PollAccess(PollBottom, 2.0f-(7*SpaceAndTime.LengthFrom2DTo3D(64.0f)), CurrPollY, SpaceAndTime.SpriteZLocation, PollType.Bottom));

			CurrPollY = 1.76f;
			this.Polls.Add(new PollAccess(PollTop, 2.0f-(2.5f*SpaceAndTime.LengthFrom2DTo3D(64.0f)), CurrPollY, SpaceAndTime.SpriteZLocation, PollType.Top));
			
			CurrPollY -= PlatformWidthInFloats;
			this.Polls.Add(new PollAccess(PollCenter, 2.0f-(2.5f*SpaceAndTime.LengthFrom2DTo3D(64.0f)), CurrPollY, SpaceAndTime.SpriteZLocation, PollType.Center));

			CurrPollY -= PlatformWidthInFloats;
			this.Polls.Add(new PollAccess(PollCenter, 2.0f-(2.5f*SpaceAndTime.LengthFrom2DTo3D(64.0f)), CurrPollY, SpaceAndTime.SpriteZLocation, PollType.Center));

			CurrPollY -= PlatformWidthInFloats;
			this.Polls.Add(new PollAccess(PollBottom, 2.0f-(2.5f*SpaceAndTime.LengthFrom2DTo3D(64.0f)), CurrPollY, SpaceAndTime.SpriteZLocation, PollType.Bottom));

			CurrPollY = 1.76f;
			this.Polls.Add(new PollAccess(PollTop, 2.0f-(4.5f*SpaceAndTime.LengthFrom2DTo3D(64.0f)), CurrPollY, SpaceAndTime.SpriteZLocation, PollType.Top));
			
			CurrPollY -= PlatformWidthInFloats;
			this.Polls.Add(new PollAccess(PollCenter, 2.0f-(4.5f*SpaceAndTime.LengthFrom2DTo3D(64.0f)), CurrPollY, SpaceAndTime.SpriteZLocation, PollType.Center));

			CurrPollY -= PlatformWidthInFloats;
			this.Polls.Add(new PollAccess(PollCenter, 2.0f-(4.5f*SpaceAndTime.LengthFrom2DTo3D(64.0f)), CurrPollY, SpaceAndTime.SpriteZLocation, PollType.Center));

			CurrPollY -= PlatformWidthInFloats;
			this.Polls.Add(new PollAccess(PollBottom, 2.0f-(4.5f*SpaceAndTime.LengthFrom2DTo3D(64.0f)), CurrPollY, SpaceAndTime.SpriteZLocation, PollType.Bottom));
            
			//Set location of polls
			foreach(SpriteAccess CurrPoll in this.Polls)
				CurrPoll.Location = GameConfig.Locations.Polls;

			Direct3D.Device ParentDevice = NewParentDevice;
		}

		public void Draw()
		{
			foreach(SpriteAccess CurrPlatform in this.Platforms)
			{
				CurrPlatform.Draw();
			}

			foreach(SpriteAccess CurrPoll in this.Polls)
			{
				CurrPoll.Draw();
			}
		}

		public bool CheckPlatformCollision(SpriteAccess OtherSprite, float OtherMoreX, float OtherMoreY, ref float OtherCurrX, ref float OtherCurrY)
		{
			bool HitPlatform = false;
			float NewOtherSpriteY = 0.0f;

			//See if the sprite hit any of the platforms
			foreach(SpriteAccess CurrPlatform in this.Platforms)
			{
				if(CurrPlatform.CollisionRects.CheckEnvironmentRectAgainst(OtherSprite, CurrPlatform.Frame, CurrPlatform.X, CurrPlatform.Y, ref NewOtherSpriteY, OtherMoreX, OtherMoreY))
				{
					//The sprite hit, so move its bottom to the top of the platform +1
					OtherCurrX = OtherSprite.X + OtherMoreX;
					OtherCurrY = NewOtherSpriteY;

					HitPlatform = true;
					break;
				}
			}

			//The sprite hit nothing, so move it normally
			if(HitPlatform == false)
			{
				OtherCurrX = OtherSprite.X + OtherMoreX;
				OtherCurrY = OtherSprite.Y + OtherMoreY;
			}

			return(HitPlatform);
		}

		public bool CheckPlatformCollisionAndKillPlatforms(SpriteAccess OtherSprite, float OtherMoreX, float OtherMoreY, ref float OtherCurrX, ref float OtherCurrY)
		{
			bool HitPlatform = false;
			float NewOtherSpriteY = 0.0f;

			//See if the sprite hit any of the platforms
			for(int i=0; i<this.Platforms.Count; i++)
			{
				if(((SpriteAccess)this.Platforms[i]).CollisionRects.CheckEnvironmentRectAgainst(OtherSprite, ((SpriteAccess)this.Platforms[i]).Frame, ((SpriteAccess)this.Platforms[i]).X, ((SpriteAccess)this.Platforms[i]).Y, ref NewOtherSpriteY, OtherMoreX, OtherMoreY))
				{
					//The sprite hit, so move its bottom to the top of the platform +1
					OtherCurrX = OtherSprite.X + OtherMoreX;
					OtherCurrY = NewOtherSpriteY;

					//remove the platform that was hit
					this.Platforms.RemoveAt(i);

					HitPlatform = true;
					break;
				}
			}

			//The sprite hit nothing, so move it normally
			if(HitPlatform == false)
			{
				OtherCurrX = OtherSprite.X + OtherMoreX;
				OtherCurrY = OtherSprite.Y + OtherMoreY;
			}

			return(HitPlatform);
		}

		public bool CanGrabPoll(SpriteAccess OtherSprite)
		{
			bool HitPoll = false;

			//See if the sprite hit any of the Polls
			foreach(PollAccess CurrPoll in this.Polls)
			{
				//If player close to poll x
				if(OtherSprite.X < CurrPoll.X + 0.1f && OtherSprite.X > CurrPoll.X - 0.1f)
				{
					float[] OtherSpriteRectSides = OtherSprite.CollisionRects.GetIndex(OtherSprite.Frame);
					float[] CurrPollRectSides = CurrPoll.CollisionRects.GetIndex(CurrPoll.Frame);

					float OtherSpriteTop = OtherSprite.Y + OtherSpriteRectSides[1];
					float OtherSpriteBottom = OtherSprite.Y - OtherSpriteRectSides[3];
					float CurrPollTop = CurrPoll.Y + CurrPollRectSides[1];
					float CurrPollBottom = CurrPoll.Y - CurrPollRectSides[3];

					//If player close to poll y

					//poll of type center
					if(CurrPoll.TypeOfPoll == PollType.Center)
					{
						if((OtherSpriteTop >= CurrPollTop && OtherSpriteBottom <= CurrPollTop) ||
							(OtherSpriteTop <= CurrPollTop && OtherSpriteBottom >= CurrPollBottom) ||
							(OtherSpriteTop >= CurrPollBottom && OtherSpriteBottom <= CurrPollBottom)
							)
							{
								HitPoll = true;
								break;
							}
					}
					//poll of type top
					else if(CurrPoll.TypeOfPoll == PollType.Top)
					{
						if((OtherSpriteTop <= CurrPollTop && OtherSpriteBottom >= CurrPollBottom) ||
							(OtherSpriteTop >= CurrPollBottom && OtherSpriteBottom <= CurrPollBottom)
							)
						{
							HitPoll = true;
							break;
						}
					}
					//poll of type bottom
					else if(CurrPoll.TypeOfPoll == PollType.Bottom)
					{
						if((OtherSpriteTop >= CurrPollTop && OtherSpriteBottom <= CurrPollTop) ||
							(OtherSpriteTop <= CurrPollTop && OtherSpriteBottom >= CurrPollBottom)
							)
						{
							HitPoll = true;
							break;
						}
					}
				}
			}

			return(HitPoll);
		}

		public float GetPollCenter(SpriteAccess OtherSprite)
		{
			float RetVal = 0.0f;

			//See if the sprite hit any of the Polls
			foreach(SpriteAccess CurrPoll in this.Polls)
			{
				if(OtherSprite.X <= CurrPoll.X+ 0.1 && OtherSprite.X >= CurrPoll.X- 0.1)
				{
					//The sprite hit, so move it to the center of the poll
					RetVal = CurrPoll.X;
					break;
				}
			}

			return(RetVal);
		}
	}
}
