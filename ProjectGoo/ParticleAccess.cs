using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using DirectX = Microsoft.DirectX;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace GameEngine
{
	public class Particle
	{
		public float TimeCreated;
		public Direct3D.CustomVertex.PositionTextured[] Verticies = null;
		public float Gravity;
		public DirectX.Vector3 Positions;
		public DirectX.Vector3 Velocities;
		public float Width = 2;
		public float Height = 2;

		public Particle(float newGravity, DirectX.Vector3 newPositions, DirectX.Vector3 newVelocities, float newTimeCreated)
		{
			this.TimeCreated = newTimeCreated;
			this.Gravity = newGravity;
			this.Positions = newPositions;
			this.Velocities = newVelocities;
			
			this.Verticies = new Direct3D.CustomVertex.PositionTextured[4];	
			GenerateVerticies();
		}

		public void GenerateVerticies()
		{
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
		}
	}

	public class Flash
	{
		public float TimeCreated;
		public DirectX.Vector3 Positions;
		public Direct3D.CustomVertex.PositionTextured[] Verticies = null;
		public float Width = 1;
		public float Height = 1;

		public Flash(DirectX.Vector3 newPositions, float newTimeCreated)
		{
			this.TimeCreated = newTimeCreated;
			this.Positions = newPositions;
			
			this.Verticies = new Direct3D.CustomVertex.PositionTextured[4];	
			GenerateVerticies();
		}

		public void GenerateVerticies()
		{
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
		}
	}
	
	public class Explosion
	{
		private Direct3D.Device CurrDevice;

		public DirectX.Vector3 ExplosionPosition;

		private Direct3D.Texture ExplosionTexture;
		private Direct3D.Texture LightTexture;
		
		private float LastUpdateTime;

		private Random Rand;

		private float UniversalGravity;
		private float ParticleCount;
		private ArrayList Particles;
		private Flash ExplosionFlash;

		public bool NeedsDelete;

		public Explosion(Direct3D.Device NewDevice, DirectX.Vector3 NewPosition)
		{
			this.CurrDevice = NewDevice;

			this.ExplosionTexture = Direct3D.TextureLoader.FromFile(CurrDevice, GameConfig.Files.ExplosionPoint);
			this.LightTexture = Direct3D.TextureLoader.FromFile(CurrDevice, GameConfig.Files.ExplosionLight);

			this.ExplosionPosition = NewPosition;
			
			this.Rand = new Random();

			this.UniversalGravity = .50f;
			this.ParticleCount = 100;
			this.NeedsDelete = false;

			this.Particles = new ArrayList();
			for(int i=0; i < this.ParticleCount; i++)
			{
				Particles.Add(new Particle(this.UniversalGravity, 
											ExplosionPosition,  
											new DirectX.Vector3((float)(Rand.NextDouble() * 0.75f - 0.375f), (float)(Rand.NextDouble() * 0.5f - 0.1f), 0.0f),
											DXUtil.Timer(DirectXTimer.GetApplicationTime)));
			}
			ExplosionFlash = new Flash( ExplosionPosition, DXUtil.Timer(DirectXTimer.GetApplicationTime));
			this.LastUpdateTime = DXUtil.Timer(DirectXTimer.GetApplicationTime);
		}

		public void Draw()
		{
			Particle tmpParticle;
			float CurrTime;
			float TimeDiff;
			if(this.Particles.Count < 1)
			{
				NeedsDelete = true;
			}
			CurrTime = DXUtil.Timer(DirectXTimer.GetApplicationTime);
			TimeDiff = CurrTime - LastUpdateTime;
			for(int i=0; i < Particles.Count; i++)
			{
				tmpParticle = (Particle)Particles[i];
				tmpParticle.Velocities.Y -= tmpParticle.Gravity * TimeDiff;
				tmpParticle.Positions.X += tmpParticle.Velocities.X * TimeDiff;
				tmpParticle.Positions.Y += tmpParticle.Velocities.Y * TimeDiff;
				tmpParticle.Positions.Z += tmpParticle.Velocities.Z * TimeDiff;
				
				if(CurrTime - tmpParticle.TimeCreated <= 1.5f)
				{
					tmpParticle.Height = 1.5f * (1.5f-(CurrTime - tmpParticle.TimeCreated));
					tmpParticle.Width = 1.5f * (1.5f-(CurrTime - tmpParticle.TimeCreated));
					tmpParticle.GenerateVerticies();
				}
			
				this.CurrDevice.Transform.World = DirectX.Matrix.RotationYawPitchRoll(0,0,0) * DirectX.Matrix.Translation(tmpParticle.Positions.X, tmpParticle.Positions.Y, tmpParticle.Positions.Z) * SpaceAndTime.ScaleStandard;
				this.CurrDevice.SetTexture(0, ExplosionTexture);
				CurrDevice.DrawUserPrimitives(Direct3D.PrimitiveType.TriangleStrip, 2, tmpParticle.Verticies);
				if(tmpParticle.Positions.Y < -2.0f)
				{
					this.Particles.Remove(tmpParticle);
				}
			}
			if(CurrTime - ExplosionFlash.TimeCreated <= .25)
			{
				ExplosionFlash.Height = 256 * (CurrTime - ExplosionFlash.TimeCreated);
				ExplosionFlash.Width = 256 * (CurrTime - ExplosionFlash.TimeCreated);
				ExplosionFlash.GenerateVerticies();
			}
			else
			{
				ExplosionFlash.Height = 0.0f;
				ExplosionFlash.Width = 0.0f;
				ExplosionFlash.GenerateVerticies();
			}
			this.CurrDevice.Transform.World = DirectX.Matrix.RotationYawPitchRoll(0,0,0) * DirectX.Matrix.Translation(ExplosionFlash.Positions.X, ExplosionFlash.Positions.Y, ExplosionFlash.Positions.Z) * SpaceAndTime.ScaleStandard;
			this.CurrDevice.SetTexture(0, LightTexture);
			CurrDevice.DrawUserPrimitives(Direct3D.PrimitiveType.TriangleStrip, 2, ExplosionFlash.Verticies);
			this.LastUpdateTime = DXUtil.Timer(DirectXTimer.GetApplicationTime);
		}
	}

	public class ParticleAccess
	{
		private ArrayList ExplosionEffects;
		private Direct3D.Device CurrDevice;

		public ParticleAccess(Direct3D.Device NewDevice)
		{
			CurrDevice = NewDevice;
			this.ExplosionEffects = new ArrayList();
		}

		public void Draw()
		{
			Explosion tmpExplosion;

			for(int i=0; i < this.ExplosionEffects.Count; i++)
			{
				tmpExplosion = (Explosion)ExplosionEffects[i];
				tmpExplosion.Draw();
				if(tmpExplosion.NeedsDelete == true)
				{
					this.ExplosionEffects.Remove(tmpExplosion);
				}
			}
		}

		public void AddExplosion(DirectX.Vector3 ExplosionPosition)
		{
			this.ExplosionEffects.Add(new Explosion(CurrDevice, ExplosionPosition));
		}

		public int ParticleCount
		{
			get{return ExplosionEffects.Count;}
		}
	}


}
