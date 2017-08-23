using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using DirectX = Microsoft.DirectX;
using Direct3D = Microsoft.DirectX.Direct3D;


namespace GameEngine
{
	public enum ShaderLevel : short
	{
		NoShaders = 0,
		Pixel_1_4 = 1,
		Pixel_2_0 = 2,
		Pixel_2_b = 3,
		Pixel_3_0 = 4,
	}

	public class ScreenAccess : System.Windows.Forms.Form
	{
		//Default width and backbuffer size for full screen
		private int _ScreenWidth = 800;
		private int _ScreenHeight = 600;
		public ShaderLevel CardShader = GameEngine.ShaderLevel.NoShaders;
		private Direct3D.TextureFilter PreferedTextureFilter;
		public Microsoft.DirectX.Direct3D.Device device = null;
		private CameraAccess Camera = null;
		private GooAccess Goo = null;
		public ParticleAccess Particles = null;
		private WallAccess Wall = null;
		private bool _HasFocus = false;

		public int ScreenWidth
		{
			get { return(this._ScreenWidth); }
		}

		public int ScreenHeight
		{
			get { return(this._ScreenHeight); }
		}

		public bool HasFocus
		{
			get { return (_HasFocus); }
			set { this._HasFocus = value; }
		}

		public ScreenAccess()
		{			
			try
			{
				Direct3D.PresentParameters presentParams = new Direct3D.PresentParameters();
				presentParams.Windowed = true;
				presentParams.SwapEffect = Direct3D.SwapEffect.Discard;
				presentParams.EnableAutoDepthStencil = true;
				presentParams.AutoDepthStencilFormat = Direct3D.DepthFormat.D16;

				Direct3D.Caps hardware = Direct3D.Manager.GetDeviceCaps(0, Direct3D.DeviceType.Hardware);
				Direct3D.CreateFlags flags = Direct3D.CreateFlags.SoftwareVertexProcessing;

				// Search for the highest possible shader support and define the device.
				if(hardware.VertexShaderVersion >= new Version(2, 0))
				{
					if (hardware.DeviceCaps.SupportsHardwareTransformAndLight)
						flags = Direct3D.CreateFlags.HardwareVertexProcessing;

					if (hardware.DeviceCaps.SupportsPureDevice)
						flags |= Direct3D.CreateFlags.PureDevice;

					if(hardware.PixelShaderVersion >= new Version(3, 0))
						CardShader = GameEngine.ShaderLevel.Pixel_3_0;
					else if(hardware.PixelShaderVersion >= new Version(2, 2))
						CardShader = GameEngine.ShaderLevel.Pixel_2_b;
					else if(hardware.PixelShaderVersion >= new Version(2, 0))
						CardShader = GameEngine.ShaderLevel.Pixel_2_0;
					else if(hardware.PixelShaderVersion >= new Version(1, 4))
						CardShader = GameEngine.ShaderLevel.Pixel_1_4;
					else
						CardShader = GameEngine.ShaderLevel.NoShaders;

					device = new Direct3D.Device(0, Direct3D.DeviceType.Hardware, this, flags, presentParams);
				}
				else if (hardware.VertexShaderVersion >= new Version(1, 1))
				{
					if (hardware.DeviceCaps.SupportsHardwareTransformAndLight)
						flags = Direct3D.CreateFlags.HardwareVertexProcessing;

					if (hardware.DeviceCaps.SupportsPureDevice)
						flags |= Direct3D.CreateFlags.PureDevice;

					if(hardware.PixelShaderVersion >= new Version(1, 4))
						CardShader = GameEngine.ShaderLevel.Pixel_1_4;
					else
						CardShader = GameEngine.ShaderLevel.NoShaders;

					device = new Direct3D.Device(0, Direct3D.DeviceType.Hardware, this, flags, presentParams);
				}
				//TODO: test this on shit cards and fake cards to see if they can use DeviceType.Hardware
				else
				{
					CardShader = GameEngine.ShaderLevel.NoShaders;
					device = new Direct3D.Device(0, Direct3D.DeviceType.Hardware, this, 
						Direct3D.CreateFlags.SoftwareVertexProcessing, presentParams);
				}
				
				//Setup Alpha Blending
				this.device.RenderState.AlphaBlendOperation = Direct3D.BlendOperation.Add;
				this.device.RenderState.AlphaTestEnable = true;
				this.device.RenderState.ReferenceAlpha = 0x08;
				this.device.RenderState.AlphaFunction = Direct3D.Compare.GreaterEqual;		
				
				this.device.SetTextureStageState(0, Direct3D.TextureStageStates.AlphaOperation, true);

				//Get the best texture filtering available from the card
				if(this.device.DeviceCaps.TextureFilterCaps.SupportsMinifyAnisotropic == true &&
					this.device.DeviceCaps.TextureFilterCaps.SupportsMagnifyAnisotropic == true)
				{
					this.PreferedTextureFilter = Direct3D.TextureFilter.Anisotropic;
				}
				else if(this.device.DeviceCaps.TextureFilterCaps.SupportsMinifyLinear == true &&
					this.device.DeviceCaps.TextureFilterCaps.SupportsMagnifyLinear == true)
				{
					this.PreferedTextureFilter = Direct3D.TextureFilter.Linear;
				}
				else
				{
					this.PreferedTextureFilter = Direct3D.TextureFilter.None;
				}

				//Add window events to hardware screen device
				this.DeligateEventsToDevices();
			}
			catch (Exception err)
			{
				throw new Exception(err.StackTrace);
			}
		}

		public void StartDraw()
		{
			this.device.Clear(Direct3D.ClearFlags.Target | Direct3D.ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

			this.device.BeginScene();

			// Draw Goo				
			this.Goo.DrawGoo();

			// Draw Wall
			this.Wall.DrawWall();

			// Draw Particles
			//Particles have to be drawn after and over everything else
			// So they should be drawn just before the EndDraw() call
//			this.Particles.Draw();
		}

		public void EndDraw()
		{
			this.device.EndScene();
			this.device.Present();
		}

		private void ResetSettings()
		{
			this.device.RenderState.Lighting = false;	
			//Turn on alpha blending
			this.device.RenderState.AlphaBlendEnable = true;
			this.device.RenderState.AlphaBlendOperation = Direct3D.BlendOperation.Add;
			this.device.RenderState.SourceBlend = Direct3D.Blend.SourceAlpha;
			this.device.RenderState.DestinationBlend = Direct3D.Blend.InvSourceAlpha;
			this.device.VertexFormat = Direct3D.CustomVertex.PositionTextured.Format;

			//Set texturing filtering to the best available.
			this.device.SamplerState[0].MinFilter = this.PreferedTextureFilter;
			this.device.SamplerState[0].MagFilter = this.PreferedTextureFilter;
		}

		//Creates a camera viewpoint and starts drawing the screen
		public void StartDrawing(int NewWidth, int NewHeight, float ViewAngle, float NearRange, float FarRange)
		{
			this._ScreenWidth = NewWidth;
			this._ScreenHeight = NewHeight;
			
			//Make form
			this.ClientSize = new System.Drawing.Size(this._ScreenWidth, this._ScreenHeight);
			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);

			this.Show();
			this.Camera = new CameraAccess(this.device, this._ScreenWidth / this._ScreenHeight, ViewAngle, NearRange, FarRange);

			this.Goo = new GooAccess(this.device, this.CardShader, this.Camera.ViewMatrix, this.Camera.ProjectionMatrix);
			this.Wall = new WallAccess(this.device, this.CardShader, this.Camera.ViewMatrix, this.Camera.ProjectionMatrix);

			this.Particles = new ParticleAccess(this.device);

			//Tell screen there is an active world to draw
			// and reset the parameters such as lighting, culling, blending
			this.ResetSettings();
		}

//		//Stop Drawing
//		public void EndDrawing()
//		{
//			this.Camera = null;
//		}

		// --------------------------------------------------------------------
		// Device Event Handlers
		// --------------------------------------------------------------------
		protected virtual void InvalidateDeviceObjects(object sender, EventArgs e)
		{
		}

		protected virtual void RestoreDeviceObjects(object sender, EventArgs e)
		{
			//Reset screen settings if there is an active world to draw
			this.ResetSettings();
		}

		protected virtual void DeleteDeviceObjects(object sender, EventArgs e)
		{
		}

		protected virtual void EnvironmentResizing(object sender, CancelEventArgs e)
		{
			this._ScreenWidth = this.ClientSize.Width;
			this._ScreenHeight = this.ClientSize.Height;
			this.Invalidate();
			//e.Cancel = true;
		}

		private void DeligateEventsToDevices()
		{
			this.device.DeviceLost += new EventHandler(this.InvalidateDeviceObjects);
			this.device.DeviceReset += new EventHandler(this.RestoreDeviceObjects);
			this.device.Disposing += new EventHandler(this.DeleteDeviceObjects);
			this.device.DeviceResizing += new CancelEventHandler(this.EnvironmentResizing);
		}

		// --------------------------------------------------------------------
		// Handle windows events
		// --------------------------------------------------------------------
		//Only when the form surface is redrawn as
		// resizing and moving the form.
		protected override void OnPaint(PaintEventArgs e)
		{
			this.Camera.CreateViewPoint(((float)(this.ClientSize.Width) / ((float)this.ClientSize.Height)));
			//this.Invalidate();
		}

		//When another program gets the focus
		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			this._HasFocus = false;
		}

		private void InitializeComponent()
		{
			// 
			// ScreenAccess
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.Name = "ScreenAccess";
		}

		//When this program recieves the focus
		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			this._HasFocus = true;
		}
	}
}
