using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;
using Direct3D = Microsoft.DirectX.Direct3D;


namespace GameEngine
{
	//TODO:Make all these attributes private and use get
	// properties to access them if needed.
	public class SpriteAccess
	{
		private Direct3D.Texture[] textures = null;
		protected Direct3D.Device ParentDevice = null;
		protected InputDeviceManagerAccess ParentInputDeviceManager = null;
		public InputDeviceManagerAccess.ControllerIdType ControllerId;
		public int Width; //Frame Width
		public int Height; //Frame Height
		private int SheetWidth; //Sheet Width
		private int SheetHeight; //Sheet Height
		protected float RotationX;
		protected float RotationY;
		protected float RotationZ;
		public float X;
		public float Y;
		public float Z;
		protected float VelocityX;
		protected float VelocityY;
		protected float VelocityZ;
		public int Frame; 
		private int AnimCount;

		//Used if this sprite uses another sprite's texture
		protected SpriteAccess ParentSprite = null;

		//These are for drawing the sprite as SpriteAccess.DrawingSurface
		private string Image = null;
		private Color MaskColor;
		private Direct3D.CustomVertex.PositionTextured[] Verticies;

		//Environmental and object collision detection
		public CollisionRectAccess CollisionRects = null;

		protected bool _FaceLeft;

		/// <summary> Links a sprite on a remote machine to one on the local machine </summary>
		private uint UniqueSpriteIdentifier;

		/// <summary> Holds sprite data that is shared between a sprite, that is on both a remote machine and the local one. </summary>
		private SpriteInformation SpriteInfo = null;

		/// <summary> Designates wheather a sprite is processed on a local or remote machine. Must be set. Can only be set once. </summary>
		private LocationMode _Location = LocationMode.NotSet;

		//Flips the sprite to face right or left
		protected bool FaceLeft
		{
			get
			{
				return(this._FaceLeft);
			}
			set
			{
				if(this._FaceLeft != value)
				{
					//Save right top and bottom
					float PrevTopRightX = this.Verticies[1].X;
					float PrevTopRightY = this.Verticies[1].Y;
					float PrevTopRightZ = this.Verticies[1].Z;
					float PrevBottomRightX = this.Verticies[3].X;
					float PrevBottomRightY = this.Verticies[3].Y;
					float PrevBottomRightZ = this.Verticies[3].Z;

					//Update right top and bottom
					this.Verticies[1].X = this.Verticies[0].X;
					this.Verticies[1].Y = this.Verticies[0].Y;
					this.Verticies[1].Z = this.Verticies[0].Z;
					this.Verticies[3].X = this.Verticies[2].X;
					this.Verticies[3].Y = this.Verticies[2].Y;
					this.Verticies[3].Z = this.Verticies[2].Z;

					//Update left top and bottom
					this.Verticies[0].X = PrevTopRightX;
					this.Verticies[0].Y = PrevTopRightY;
					this.Verticies[0].Z = PrevTopRightZ;
					this.Verticies[2].X = PrevBottomRightX;
					this.Verticies[2].Y = PrevBottomRightY;
					this.Verticies[2].Z = PrevBottomRightZ;
				}

				this._FaceLeft = value;
			}
		}

		//Sprite with it's own texture
		public SpriteAccess(Direct3D.Device NewParentDevice, string TextureLocation, float NewX, float NewY, float NewZ, int NewWidth, int NewHeight, int NewSheetWidth, int NewSheetHeight, Color NewMaskColor, uint NewCollisionRectWidth, uint NewCollisionRectHeight)
		{
			//Make sure the image exists
			if(System.IO.File.Exists(TextureLocation) == false)
			{
				throw new Exception("The SpriteAccess constructor cannot continue because the Texture Location " +
					"does not point to an existing image: '" + TextureLocation + "'");
			}

			this.Image = TextureLocation;
			this.MaskColor = NewMaskColor;
			this.ParentDevice = NewParentDevice;

			this.Initialize(NewX, NewY, NewZ, NewWidth, NewHeight, NewSheetWidth, NewSheetHeight);
			this.SetTextureToScreen();
		}

		//Sprite that refernces another sprite's texture
		public SpriteAccess(SpriteAccess NewParentSprite, float NewX, float NewY, float NewZ)
		{
			this.ParentSprite = (SpriteAccess)NewParentSprite;
			this.Image = null;
			this.ParentDevice = this.ParentSprite.ParentDevice;
			this.MaskColor = this.ParentSprite.MaskColor;

			this.textures = this.ParentSprite.textures;
			this.CollisionRects = this.ParentSprite.CollisionRects;

			this.Initialize(NewX, NewY, NewZ, ParentSprite.Width, ParentSprite.Height, ParentSprite.SheetWidth, ParentSprite.SheetHeight);
		}

		private void Initialize(float NewX, float NewY, float NewZ, int NewWidth, int NewHeight, int NewSheetWidth, int NewSheetHeight)
		{
			this.Width = NewWidth;
			this.Height = NewHeight;
			this.SheetWidth = NewSheetWidth;
			this.SheetHeight = NewSheetHeight;
			this.Frame = 0;

			this.X = NewX;
			this.Y = NewY;
			this.Z = NewZ;

			this.AnimCount = ((int)(this.SheetWidth / this.Width)) * ((int)(this.SheetHeight / this.Height));

			//Since these are drawn in 3d coordinates
			// the verticies are specified in counter-clockwise order
			// TODO: IS VERTICIES SPELLED WRONG?
			this.Verticies = new Direct3D.CustomVertex.PositionTextured[4];

			float HalfWidth = SpaceAndTime.LengthFrom2DTo3D(this.Width) * 0.5f;
			float HalfHeight = SpaceAndTime.LengthFrom2DTo3D(this.Height) * 0.5f;

			//Bottom right
			this.Verticies[0].X = HalfWidth;
			this.Verticies[0].Y = HalfHeight;
			this.Verticies[0].Z = 0f;
			this.Verticies[0].Tu = 0.0f;
			this.Verticies[0].Tv = 0.0f;

			//Bottom left
			this.Verticies[1].X = - HalfWidth;
			this.Verticies[1].Y = HalfHeight;
			this.Verticies[1].Z = 0f;
			this.Verticies[1].Tu = 1.0f;
			this.Verticies[1].Tv = 0.0f;

			//Top right
			this.Verticies[2].X = HalfWidth;
			this.Verticies[2].Y = - HalfHeight;
			this.Verticies[2].Z = 0f;
			this.Verticies[2].Tu = 0.0f;
			this.Verticies[2].Tv = 1.0f;

			//Top left
			this.Verticies[3].X = - HalfWidth;
			this.Verticies[3].Y = - HalfHeight;
			this.Verticies[3].Z = 0f;
			this.Verticies[3].Tu = 1.0f;
			this.Verticies[3].Tv = 1.0f;

			//Set sprite unique id
			this.UniqueSpriteIdentifier = ServerManager.UniqueIdentifier.GetNewUniqueId();
		}


		//Loads the texture into a sprite
		private void SetTextureToScreen()
		{
			// Load texture from other sprite's texture
			if(this.ParentSprite == null)
			{
				uint f = 0; //Frame of animation
				uint x = 0; //Frame x reltive to frame
				uint y = 0; //Frame y reletive to frame
				uint SourceXStart = 0; //Frame's starting x reletive to sprite sheet
				uint SourceYStart = 0; //Frame's starting y reletive to sprite sheet
				uint DestinationY = 0;
				uint DestinationX = 0;
				uint AnimsPerSheetWidth = (uint)(this.SheetWidth / this.Width);

				//Load the whole texture till we can chop it up
				Direct3D.Texture SpriteSheet = Direct3D.TextureLoader.FromFile
				(
					this.ParentDevice, this.Image, 0, 0, 0, Direct3D.Usage.None,
					Direct3D.Format.A8B8G8R8, Direct3D.Pool.Managed,
					Direct3D.Filter.Linear, Direct3D.Filter.Linear, this.MaskColor.ToArgb()
				);

				//Chop this sprite sheet up into frames
				//Create a blank bitmap and use it as a template for creating a texture
				System.Drawing.Bitmap SingleFrameBackground = new System.Drawing.Bitmap(this.Width, this.Height);
				
				uint[,] DestinationPixels;
				uint[,] SourcePixels = (uint[,])SpriteSheet.LockRectangle(typeof(uint), 0, new Rectangle(0, 0, this.SheetHeight, this.SheetWidth), Direct3D.LockFlags.None, this.SheetHeight, this.SheetWidth);
				this.textures = new Direct3D.Texture[this.AnimCount];

				//For each frame of animation
				for(f=0; f<this.AnimCount; f++)
				{
					//Get this frame's starting y
					if(f >= AnimsPerSheetWidth)
						SourceYStart = (uint)(f / AnimsPerSheetWidth);

					//Get this frame's starting x
					SourceXStart = (uint)(f - (SourceYStart * AnimsPerSheetWidth));

					SourceYStart *= (uint)this.Height;
					SourceXStart *= (uint)this.Width;

					DestinationY = 0;
					DestinationX = 0;

					//Create the texture's background
					this.textures[f] = new Direct3D.Texture(this.ParentDevice, SingleFrameBackground, Direct3D.Usage.None, Direct3D.Pool.Managed);
					DestinationPixels = (uint[,])this.textures[f].LockRectangle(typeof(uint), 0, new Rectangle(0, 0, this.Height, this.Width), Direct3D.LockFlags.None, this.Height, this.Width);

					//For this frame's x
					for(x=SourceXStart; x<SourceXStart+this.Width; x++)
					{
						//For this frame's y
						for(y=SourceYStart; y<SourceYStart+this.Height; y++)
						{
							DestinationPixels[DestinationY, DestinationX] = SourcePixels[y, x];
							DestinationY++;
						}
						DestinationX++;
						DestinationY = 0;
					}

					this.textures[f].UnlockRectangle(0);
				}

				SpriteSheet.UnlockRectangle(0);
				SpriteSheet.Dispose();

				//Create masks for each frame of animation
				this.CollisionRects = new CollisionRectAccess(this.ParentDevice, this.textures, this.Verticies);
			}
		}

		public void SetInputDevices(InputDeviceManagerAccess NewParentInputDeviceManager, InputDeviceManagerAccess.ControllerIdType NewControllerId)
		{
			this.ParentInputDeviceManager = NewParentInputDeviceManager;
			this.ControllerId = NewControllerId;
		}

		public static void DrawSpriteArray(SpriteAccess[] Sprites)
		{
			Direct3D.Device CurrDevice = null;

			//For each associated sprite on the screen
			for(int i=0; i<Sprites.Length; i++)
			{		
				if(CurrDevice != Sprites[i].ParentDevice)
				{
					CurrDevice = Sprites[i].ParentDevice;
					CurrDevice.VertexFormat = Direct3D.CustomVertex.PositionTextured.Format;
				}

				CurrDevice.Transform.World = Matrix.RotationYawPitchRoll(Sprites[i].RotationX / (float)Math.PI, Sprites[i].RotationY / (float)Math.PI * 2.0f, Sprites[i].RotationZ / (float)Math.PI / 4.0f) * Matrix.Translation(Sprites[i].X, Sprites[i].Y, Sprites[i].Z) * SpaceAndTime.ScaleStandard;
				
				CurrDevice.SetTexture(0, Sprites[i].textures[Sprites[i].Frame]);
				CurrDevice.DrawUserPrimitives(Direct3D.PrimitiveType.TriangleStrip, 2, Sprites[i].Verticies);
			}
		}

		public void Draw()
		{
			this.ParentDevice.VertexFormat = Direct3D.CustomVertex.PositionTextured.Format;

			this.ParentDevice.Transform.World = Matrix.RotationYawPitchRoll(this.RotationX / (float)Math.PI, this.RotationY / (float)Math.PI * 2.0f, this.RotationZ / (float)Math.PI / 4.0f) * Matrix.Translation(this.X, this.Y, this.Z) * SpaceAndTime.ScaleStandard;
			
			this.ParentDevice.SetTexture(0, this.textures[this.Frame]);
			this.ParentDevice.DrawUserPrimitives(Direct3D.PrimitiveType.TriangleStrip, 2, this.Verticies);
		}

		/// <summary> Designates where a sprite will live </summary>
		public enum LocationMode : int
		{
			NotSet /// <summary> The default value. Must be changed </summary>
			,Local /// <summary> Sprite lives on the local machine </summary>
			,Remote /// <summary> Sprite lives on a remote machine </summary>
			,Any /// <summary> Let the program decide where it should live. Will be reset to local or remote. </summary>
		}

		public LocationMode Location
		{
			get { return this._Location; }
			set
			{
				//Only let the value be set once
				if(this._Location != LocationMode.NotSet)
					throw new Exception("The Location value for a SpriteLocationAccess object has already been set. It cannot be changed once set.");

				this._Location = value;
				//Have sprite live on local machine
				if(this._Location == LocationMode.Local)
				{
					this.SpriteInfo = new SpriteInformation();
					ServerManager.Local.Add(this.UniqueSpriteIdentifier, this.SpriteInfo);
				}
					// Have sprite live on remote machine
				else if(this._Location == LocationMode.Remote)
				{
					this.SpriteInfo = null;
				}
					//Have the sprite live on this machine if it is not already on the remote machine
				else if(this._Location == LocationMode.Any)
				{
					if(ServerManager.Remote.WaitForRemoteMachine(0)==false || ServerManager.Remote.Contains(this.UniqueSpriteIdentifier)==false)
					{
						this.SpriteInfo = new SpriteInformation();
						ServerManager.Local.Add(this.UniqueSpriteIdentifier, this.SpriteInfo);
						this._Location = LocationMode.Local;
					}
					else
					{
						this.SpriteInfo = null;
						this._Location = LocationMode.Remote;
					}
				}
			}
		}

		/// <summary> Updates the sprite info depending on if the Location is remote or local </summary>
		protected void UpdateSpriteInfo()
		{
			if(this._Location == LocationMode.Remote)
			{
				if(this.SpriteInfo == null)
					this.SpriteInfo = ServerManager.Remote.GetRemoteSprite(this.UniqueSpriteIdentifier);

				//Copy the entire remote SpriteInfo object into a local clone.
				// It should be faster to transmit this one request
				// rather than getting each property individually.
				SpriteInformation CloneInfo = (SpriteInformation) this.SpriteInfo.Clone();
				this.X = CloneInfo.X;
				this.Y = CloneInfo.Y;
				this.Z = CloneInfo.Z;
				this.Frame = CloneInfo.Frame;
				this.FaceLeft = CloneInfo.FaceLeft;
			}
			else if(this._Location == LocationMode.Local)
			{
				this.SpriteInfo.X = this.X;
				this.SpriteInfo.Y = this.Y;
				this.SpriteInfo.Z = this.Z;
				this.SpriteInfo.FaceLeft = this._FaceLeft;
				this.SpriteInfo.Frame = this.Frame;
			}
			else
			{
				throw new Exception("The sprite Location has not been set to Local or Remote.");
			}
		}
	}
}
