using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;
using DirectSound = Microsoft.DirectX.DirectSound;

namespace GameEngine
{
	public class SoundAccess
	{
		public DirectSound.Device device = null;
		//private DirectSound.SecondaryBuffer BackgroundMusic = null;
		private DirectSound.SecondaryBuffer ExplosionSound = null;
		private DirectSound.SecondaryBuffer GunSound = null;
		private DirectSound.SecondaryBuffer StepSound = null;

		public SoundAccess(ScreenAccess ParentForm)
		{
			try
			{
				device = new DirectSound.Device();
				device.SetCooperativeLevel(ParentForm, DirectSound.CooperativeLevel.Normal);				
//				BackgroundMusic = new DirectSound.SecondaryBuffer(GameFiles.BgMusic, device);
//				
//				BackgroundMusic.Play(0, DirectSound.BufferPlayFlags.Looping);				
			}
			catch (Exception err)
			{
				throw err;
			}
		}

		public void PlayExplosion()
		{	
			if(ExplosionSound == null)
				ExplosionSound = new DirectSound.SecondaryBuffer(GameConfig.Files.SoundExplosion, device);	
			ExplosionSound.SetCurrentPosition(0);
			ExplosionSound.Play(0,DirectSound.BufferPlayFlags.Default);
		}

		public void PlayGunShot()
		{
			if(GunSound == null)
				GunSound = new DirectSound.SecondaryBuffer(GameConfig.Files.SoundGun, device);	
			GunSound.SetCurrentPosition(0);
			GunSound.Play(0,DirectSound.BufferPlayFlags.Default);
		}

		public void PlayStep()
		{
			if(StepSound == null)
				StepSound = new DirectSound.SecondaryBuffer(GameConfig.Files.SoundStep, device);	
			StepSound.SetCurrentPosition(0);
			StepSound.Play(0,DirectSound.BufferPlayFlags.Default);
		}
	}
}
