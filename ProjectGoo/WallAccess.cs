using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using DirectX = Microsoft.DirectX;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace GameEngine
{
	public class WallAccess
	{
		private Direct3D.Device GameDevice;
		private GameEngine.ShaderLevel CardShaderLevel;
		private DirectX.Matrix ViewMatrix;
		private DirectX.Matrix ProjectionMatrix;
		private DirectX.Matrix WallWorldMatrix;
		private DirectX.Matrix PipeWorldMatrix;
		private Direct3D.Effect WallEffect;
		private Direct3D.Effect PipeEffect;
		private Direct3D.Mesh WallMesh;
		private Direct3D.Material[] WallMaterials;
		private Direct3D.Texture[] WallTextures;
		private Direct3D.Mesh PipeMesh;
		private Direct3D.Material[] PipeMaterials;
		private Direct3D.Texture[] PipeTextures;
		private Direct3D.ExtendedMaterial[] WallMtrl;
		private Direct3D.ExtendedMaterial[] PipeMtrl;

		public WallAccess(Direct3D.Device NewGameDevice, 
						  GameEngine.ShaderLevel NewCardShaderLevel, 
						  DirectX.Matrix NewViewMatrix, 
						  DirectX.Matrix NewProjectionMatrix)
		{
			GameDevice = NewGameDevice;
			CardShaderLevel = NewCardShaderLevel;
			ViewMatrix = NewViewMatrix;
			ProjectionMatrix = NewProjectionMatrix;

			// Load Shader Effects From Files
			WallEffect = Direct3D.Effect.FromFile(GameDevice, GameConfig.Files.WallFx, null, null, Direct3D.ShaderFlags.None, null);
			WallEffect.Technique = "WallLight_1_1";
			
			PipeEffect = Direct3D.Effect.FromFile(GameDevice, GameConfig.Files.PipeFx, null, null, Direct3D.ShaderFlags.None, null);
			PipeEffect.Technique = "PipeLight_1_1";

			// Load Mesh From File
			WallMesh = Direct3D.Mesh.FromFile(GameConfig.Files.WallMesh, Direct3D.MeshFlags.Managed, GameDevice, out WallMtrl);
			PipeMesh = Direct3D.Mesh.FromFile(GameConfig.Files.PipeMesh, Direct3D.MeshFlags.Managed, GameDevice, out PipeMtrl);
			
			// Load Wall Textures
			if ((WallMtrl != null) && (WallMtrl.Length > 0))
			{
				WallMaterials = new Direct3D.Material[WallMtrl.Length];
				WallTextures = new Direct3D.Texture[WallMtrl.Length];

				for (int i = 0; i < WallMtrl.Length; i++)
				{
					WallMaterials[i] = WallMtrl[i].Material3D;
					if ((WallMtrl[i].TextureFilename != null) && (WallMtrl[i].TextureFilename != string.Empty))
					{
						WallTextures[i] = Direct3D.TextureLoader.FromFile(GameDevice, @"..\..\Resources\" + WallMtrl[i].TextureFilename);
					}
				}
			}

			// Load Pipe Textures
			if ((PipeMtrl != null) && (PipeMtrl.Length > 0))
			{
				PipeMaterials = new Direct3D.Material[PipeMtrl.Length];
				PipeTextures = new Direct3D.Texture[PipeMtrl.Length];

				for (int i = 0; i < PipeMtrl.Length; i++)
				{
					PipeMaterials[i] = PipeMtrl[i].Material3D;
					if ((PipeMtrl[i].TextureFilename != null) && (PipeMtrl[i].TextureFilename != string.Empty))
					{
						PipeTextures[i] = Direct3D.TextureLoader.FromFile(GameDevice, @"..\..\Resources\" + PipeMtrl[i].TextureFilename);
					}
				}
			}

			// Set wall mesh location
			WallWorldMatrix = DirectX.Matrix.RotationYawPitchRoll(3.12f,0.0f,0.0f) * DirectX.Matrix.Translation(15, -75 , -425);			
			PipeWorldMatrix = DirectX.Matrix.RotationYawPitchRoll(3.20f,-0.1f,0.0f) * DirectX.Matrix.Translation(-145, 15 , -375);			
			
			// Set Wall Shader Parameters
			DirectX.Matrix WorldViewProjMatrix = WallWorldMatrix * ViewMatrix * ProjectionMatrix;
			WallEffect.SetValue("WorldViewProj", WorldViewProjMatrix);
			WallEffect.SetValue("WorldMatrix", WallWorldMatrix);
			WallEffect.SetValue("DiffuseDirection", new DirectX.Vector4(1.0f, 1.0f, 1.0f, 0.0f));

			// Set Pipe Shader Parameters
			WorldViewProjMatrix = PipeWorldMatrix * ViewMatrix * ProjectionMatrix;
			PipeEffect.SetValue("WorldViewProj", WorldViewProjMatrix);
			PipeEffect.SetValue("WorldMatrix", PipeWorldMatrix);
			PipeEffect.SetValue("DiffuseDirection", new DirectX.Vector4(1.0f, 1.0f, 1.0f, 0.0f));

		}

		public void DrawWall()
		{		
			// Disable alpha blend setting
			GameDevice.RenderState.AlphaBlendEnable = false;
			
			// Apply shaders to wall
			int numPasses = WallEffect.Begin(0);
			for (int iPass = 0; iPass < numPasses; iPass++)
			{
				WallEffect.BeginPass(iPass);
				for (int i = 0; i < WallMaterials.Length; i++)
				{
					// Set texture tiling properties for parts of the wall.
					if(i == 6) // Drain Texture
					{
						WallEffect.SetValue("xMultiply", 20.0f);
						WallEffect.SetValue("yMultiply", 1.0f);
					}
					else if(i == 4) // Wall Texture
					{
						WallEffect.SetValue("xMultiply", 5.0f);
						WallEffect.SetValue("yMultiply", 5.0f);
					}
					else if(i == 7) // Catwalk Texture
					{
						WallEffect.SetValue("xMultiply", 3.0f);
						WallEffect.SetValue("yMultiply", 3.0f);
					}
					else
					{
						WallEffect.SetValue("xMultiply", 3.0f);
						WallEffect.SetValue("yMultiply", 3.0f);
					}
					WallEffect.CommitChanges();
					GameDevice.SetTexture(0, WallTextures[i]);
					WallMesh.DrawSubset(i);
				}
				WallEffect.EndPass();
			}
			WallEffect.End();

			numPasses = PipeEffect.Begin(0);
			for (int iPass = 0; iPass < numPasses; iPass++)
			{
				PipeEffect.BeginPass(iPass);
				// Set texture tiling properties for parts of the pipe.
				for (int i = 0; i < PipeMaterials.Length; i++)
				{
					if(i == 6) // Bars Texture
					{
						PipeEffect.SetValue("xMultiply", 1.0f);
						PipeEffect.SetValue("yMultiply", 1.0f);
					}
					else if(i == 5) // Outside pipe
					{
						PipeEffect.SetValue("xMultiply", 1.0f);
						PipeEffect.SetValue("yMultiply", 2.0f);
					}
					else if(i == 3) // Inside pipe
					{
						PipeEffect.SetValue("xMultiply", 3.0f);
						PipeEffect.SetValue("yMultiply", 3.0f);
					}
					else
					{
						PipeEffect.SetValue("xMultiply", 1.0f);
						PipeEffect.SetValue("yMultiply", 1.0f);
					}
					PipeEffect.CommitChanges();
					GameDevice.SetTexture(0, PipeTextures[i]);
					PipeMesh.DrawSubset(i);
				}
				PipeEffect.EndPass();
			}
			PipeEffect.End();

			// Restore alpha blend setting
			GameDevice.RenderState.AlphaBlendEnable = true;
		}

	}
}
