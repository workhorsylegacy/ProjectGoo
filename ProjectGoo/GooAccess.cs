using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using DirectX = Microsoft.DirectX;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace GameEngine
{
	public class GooAccess
	{
		private Direct3D.Device GameDevice;
		private GameEngine.ShaderLevel CardShaderLevel;
		private DirectX.Matrix ViewMatrix;
		private DirectX.Matrix ProjectionMatrix;
		private DirectX.Matrix WorldMatrix;
		private Direct3D.Effect GooEffect;
		private Direct3D.Mesh GooMesh;
		private Direct3D.Texture ColorTexture;
		private Direct3D.Texture NormalTexture;
		private Direct3D.Texture HeightTexture;

		public GooAccess(Direct3D.Device NewGameDevice, 
						 GameEngine.ShaderLevel NewCardShaderLevel, 
						 DirectX.Matrix NewViewMatrix, 
						 DirectX.Matrix NewProjectionMatrix)
		{
			GameDevice = NewGameDevice;
			CardShaderLevel = NewCardShaderLevel;
			ViewMatrix = NewViewMatrix;
			ProjectionMatrix = NewProjectionMatrix;

			// Load Shader Effect From File
			GooEffect = Direct3D.Effect.FromFile(GameDevice, GameConfig.Files.GooFx, null, null, Direct3D.ShaderFlags.None, null);

			// Choose shader technique based on shader level.
			if(CardShaderLevel == GameEngine.ShaderLevel.Pixel_3_0)
				GooEffect.Technique = "Goo_Parallax_3_0";
			else if(CardShaderLevel == GameEngine.ShaderLevel.Pixel_2_b)
				GooEffect.Technique = "Goo_Parallax_2_b";
			else if(CardShaderLevel == GameEngine.ShaderLevel.Pixel_2_0)
				GooEffect.Technique = "Goo_Parallax_2";
			else if(CardShaderLevel == GameEngine.ShaderLevel.Pixel_1_4)
				GooEffect.Technique = "Goo_Bump_1_4";

			// Load Mesh From File
			GooMesh = Direct3D.Mesh.FromFile(GameConfig.Files.GooMesh, Direct3D.MeshFlags.Managed, GameDevice);

			// Load Textures From File
			ColorTexture = Direct3D.TextureLoader.FromFile(GameDevice, GameConfig.Files.GooColor);
			NormalTexture = Direct3D.TextureLoader.FromFile(GameDevice, GameConfig.Files.GooNormal);
			HeightTexture = Direct3D.TextureLoader.FromFile(GameDevice, GameConfig.Files.GooHeight);
			
			// Load Textures into Effect
			GooEffect.SetValue("ColorTexture", ColorTexture);
			GooEffect.SetValue("NormalsTexture", NormalTexture);
			GooEffect.SetValue("HeightTexture", HeightTexture);

            // Set Parallax and Bump Intensity
			GooEffect.SetValue("ParallaxAmount", .5f);
			GooEffect.SetValue("BumpAmount", 1.5f);            			
		}

		public void DrawGoo()
		{
			float CurrTime = DXUtil.Timer(DirectXTimer.GetApplicationTime);
			float fTime =(float)(( (CurrTime % 180.0f) / 180.0f) * (2*Math.PI));

			WorldMatrix = DirectX.Matrix.RotationYawPitchRoll((float)((CurrTime % 240)/240*Math.PI),0,0) * DirectX.Matrix.Translation(0, -45 , -400);
			
			DirectX.Matrix WorldViewMatrix = WorldMatrix * ViewMatrix;
			DirectX.Matrix WorldViewProjMatrix = WorldMatrix * ViewMatrix * ProjectionMatrix;

			GooEffect.SetValue("fMixTime", fTime);
			GooEffect.SetValue("WorldView", WorldViewMatrix);
			GooEffect.SetValue("WorldViewProjection", WorldViewProjMatrix);
			
			GameDevice.RenderState.AlphaBlendEnable = false;
			int numPasses = GooEffect.Begin(0);            
			for (int i = 0; i < numPasses; i++)
			{	
				GooEffect.BeginPass(i);
				GooMesh.DrawSubset(0);				
				GooEffect.EndPass();
			}
			GooEffect.End();

			// Restore alpha blend setting
			GameDevice.RenderState.AlphaBlendEnable = true;
		}


	}
}
