using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace GameEngine
{
	#region WeaponAccess
	public class WeaponAccess : SpriteAccess
	{
		protected WeaponType _TypeOfWeapon;
		protected bool _IsDead;

		public bool IsDead
		{
			get { return(this._IsDead); }
			set { this._IsDead = value; }
		}

		public WeaponType TypeOfWeapon
		{
			get {return(this._TypeOfWeapon);}
		}

		public WeaponAccess(SpriteAccess ParentSprite, WeaponType NewTypeOfWeapon, float SourceX, float SourceY, float SourceZ, bool SourceFaceLeft)
			: base(ParentSprite, SourceX, SourceY, SourceZ)
		{
			this._TypeOfWeapon = NewTypeOfWeapon;
			this._FaceLeft = SourceFaceLeft;
		}

		//this weapon doesn't move but it's children will
		public virtual void Move(BackgroundAccess Foreground)
		{

		}
	}
	#endregion

	#region GrenadeAccess
	class GrenadeAccess : WeaponAccess
	{
		public GrenadeAccess(SpriteAccess ParentSprite, float SourceX, float SourceY, float SourceZ, bool SourceFaceLeft)
			: base(ParentSprite, WeaponType.Grenade, SourceX, SourceY, SourceZ, SourceFaceLeft)
		{
			//Get offset for weapon x, y, and z
			if(SourceFaceLeft == true)
				this.X = SourceX + 0.1f;
			else
				this.X = SourceX - 0.1f;

			this.Y = SourceY;
			this.Z = SourceZ;

			//Get velocity
			this.VelocityX = 0.0f;
			this.VelocityY = 0.1f;
		}

		public override void Move(BackgroundAccess Foreground)
		{
			float NewX = 0;
			float NewY = 0;

			if(this._FaceLeft)
				this.VelocityX = 0.1f;
			else
				this.VelocityX = -0.1f;

			if(Foreground.CheckPlatformCollisionAndKillPlatforms(this, this.VelocityX, this.VelocityY, ref NewX, ref NewY) == false)
			{
				this.X = NewX;
				this.Y = NewY;
			}
			else
			{
				this._IsDead = true;
			}

			//Decrease velocity acording to gravity
			if(this.VelocityY > -0.3f)
				this.VelocityY -= 0.02f;
		}
	}
	#endregion

	#region SlideMineAccess
	class SlideMineAccess : WeaponAccess
	{
		public SlideMineAccess(SpriteAccess ParentSprite, float SourceX, float SourceY, float SourceZ, bool SourceFaceLeft)
			: base(ParentSprite, WeaponType.SlideMine, SourceX, SourceY, SourceZ, SourceFaceLeft)
		{
			//Get offset for weapon x, y, and z
			if(SourceFaceLeft == true)
				this.X = SourceX + 0.2f;
			else
				this.X = SourceX - 0.2f;

			this.Y = SourceY;
			this.Z = SourceZ;

			//Get velocity
			this.VelocityX = 0.0f;
			this.VelocityY = 0.0f;
		}

		public override void Move(BackgroundAccess Foreground)
		{
			float NewX = 0;
			float NewY = 0;

			if(this._FaceLeft)
				this.VelocityX = 0.2f;
			else
				this.VelocityX = - 0.2f;

			if(Foreground.CheckPlatformCollision(this, this.VelocityX, 0, ref NewX, ref NewY) == false)
				this.X = NewX;

			if(Foreground.CheckPlatformCollision(this, 0, this.VelocityY, ref NewX, ref NewY) == false)
				this.Y = NewY;
			else
				this.VelocityY = 0.0f;

			//Decrease velocity acording to gravity
			if(this.VelocityY > -0.3f)
				this.VelocityY -= 0.02f;
		}
	}
	#endregion

	#region MineAccess
	class MineAccess : WeaponAccess
	{
		public MineAccess(SpriteAccess ParentSprite, float SourceX, float SourceY, float SourceZ, bool SourceFaceLeft)
			: base(ParentSprite, WeaponType.Mine, SourceX, SourceY, SourceZ, SourceFaceLeft)
		{
			//Get offset for weapon x, y, and z
			if(SourceFaceLeft == true)
				this.X = SourceX + 0.3f;
			else
				this.X = SourceX - 0.3f;

			this.Y = SourceY;
			this.Z = SourceZ;

			//Get velocity
			this.VelocityX = 0.0f;
			this.VelocityY = 0.0f;
		}

		public override void Move(BackgroundAccess Foreground)
		{
			float NewX = 0;
			float NewY = 0;

			if(Foreground.CheckPlatformCollision(this, 0, this.VelocityY, ref NewX, ref NewY) == false)
				this.Y = NewY;
			else
				this.VelocityY = 0.0f;

			//Decrease velocity acording to gravity
			if(this.VelocityY > -0.3f)
				this.VelocityY -= 0.02f;
		}
	}
	#endregion

	#region BulletAccess
	public class BulletAccess : WeaponAccess
	{
		public BulletAccess(SpriteAccess ParentSprite, float SourceX, float SourceY, float SourceZ, bool SourceFaceLeft)
			: base(ParentSprite, WeaponType.Bullet, SourceX, SourceY, SourceZ, SourceFaceLeft)
		{
			//Get offset for weapon x, y, and z
			if(SourceFaceLeft == true)
				this.X = SourceX + 0.3f;
			else
				this.X = SourceX - 0.3f;

			this.Y = SourceY;
			this.Z = SourceZ;

			//Get velocity
			this.VelocityX = 1.0f;
			this.VelocityY = 0.0f;
		}

		public override void Move(BackgroundAccess Foreground)
		{
			if(this._FaceLeft)
				this.VelocityX = 1.0f;
			else
				this.VelocityX = -1.0f;

			this.X += this.VelocityX;

			if(this.X > 5 || this.X < -5)
				this._IsDead = true;
		}
	}
	#endregion

	public enum WeaponType : int
	{
		Grenade,
		SlideMine,
		Mine,
		Bullet,
	}

	public class WeaponManagerAccess
	{
		private static SpriteAccess MasterGrenadeSprite = null;
		private static SpriteAccess MasterSlideMineSprite = null;
		private static SpriteAccess MasterMineSprite = null;
		private static SpriteAccess MasterBulletSprite = null;
		public static ArrayList AllWeapons = new ArrayList(); //holds refs to all weapons

		//These are used to manage all the weapons in the game
		public static void CreateWeapon(Direct3D.Device ParentDevice, WeaponType NewTypeOfWeapon, float SourceX, float SourceY, float SourceZ, bool SourceFaceLeft)
		{
			//Create master sprites if they dont exist
			if(WeaponManagerAccess.MasterGrenadeSprite == null)
				WeaponManagerAccess.MasterGrenadeSprite = new SpriteAccess(ParentDevice, GameConfig.Files.Grenade, 0, 0, 0, 32, 32, 64, 64, Color.FromArgb(0xFF, 0x00, 0x00, 0xFF), 0, 0);
			if(WeaponManagerAccess.MasterSlideMineSprite == null)
				WeaponManagerAccess.MasterSlideMineSprite = new SpriteAccess(ParentDevice, GameConfig.Files.SlideMine, 0, 0, 0, 32, 32, 64, 64, Color.FromArgb(0xFF, 0x00, 0x00, 0xFF), 0, 0);
			if(WeaponManagerAccess.MasterMineSprite == null)
				WeaponManagerAccess.MasterMineSprite = new SpriteAccess(ParentDevice, GameConfig.Files.Mine, 0, 0, 0, 32, 32, 64, 64, Color.FromArgb(0xFF, 0x00, 0x00, 0xFF), 0, 0);
			if(WeaponManagerAccess.MasterBulletSprite == null)
				WeaponManagerAccess.MasterBulletSprite = new SpriteAccess(ParentDevice, GameConfig.Files.Bullet, 0, 0, 0, 256, 256, 256, 256, Color.FromArgb(0xFF, 0x00, 0x00, 0xFF), 0, 0);

			//Create new weapon
			WeaponAccess NewWeapon = null;
			if(NewTypeOfWeapon == WeaponType.Grenade)
				NewWeapon = new GrenadeAccess(WeaponManagerAccess.MasterGrenadeSprite, SourceX, SourceY, SourceZ, SourceFaceLeft);
			else if(NewTypeOfWeapon == WeaponType.Mine)
				NewWeapon = new MineAccess(WeaponManagerAccess.MasterMineSprite, SourceX, SourceY, SourceZ, SourceFaceLeft);
			else if(NewTypeOfWeapon == WeaponType.SlideMine)
				NewWeapon = new SlideMineAccess(WeaponManagerAccess.MasterSlideMineSprite, SourceX, SourceY, SourceZ, SourceFaceLeft);
			else if(NewTypeOfWeapon == WeaponType.Bullet)
				NewWeapon = new BulletAccess(WeaponManagerAccess.MasterBulletSprite, SourceX, SourceY, SourceZ, SourceFaceLeft);

			//Save ref to new weapon
			WeaponManagerAccess.AllWeapons.Add(NewWeapon);
		}

		public static void MoveWeapons(BackgroundAccess Foreground)
		{
			for(int i=0; i<WeaponManagerAccess.AllWeapons.Count; i++)
			{
				//move
				((WeaponAccess) WeaponManagerAccess.AllWeapons[i]).Move(Foreground);

				//kill item if it hit a platform
				if(((WeaponAccess) WeaponManagerAccess.AllWeapons[i]).IsDead == true)
					WeaponManagerAccess.AllWeapons.RemoveAt(i);
			}
		}

		public static void CheckPlayerCollisions(SpriteAccess[] OtherPlayers, ParticleAccess CurrParticles, SoundAccess Sounds)
		{
			//for each player
			foreach(PlayableCharacterAccess CurrPlayer in OtherPlayers)
			{
				//for each weapon
				foreach(WeaponAccess CurrWeapon in WeaponManagerAccess.AllWeapons)
				{
					//Check collisions against players
					if(CurrWeapon.TypeOfWeapon != WeaponType.Grenade)
					{
						if(CurrWeapon.CollisionRects.CheckObjectRectAgainst(CurrPlayer, CurrWeapon.Frame, CurrWeapon.X, CurrWeapon.Y) != CollisionRectAccess.HitSide.None)
						{
							//remove weapon
							//kill player
							CurrPlayer.State = PlayableCharacterAccess.PlayerState.Explode;
							CurrWeapon.IsDead = true;

							//Add explosion
							if(CurrWeapon.TypeOfWeapon == WeaponType.SlideMine || CurrWeapon.TypeOfWeapon == WeaponType.Mine)
							{
								//CurrParticles.AddExplosion(new Vector3(CurrPlayer.X, CurrPlayer.Y, SpaceAndTime.SpriteZLocation));
								CurrParticles.AddExplosion(new Vector3(CurrPlayer.X/17, CurrPlayer.Y/17, 0.0f));
								Sounds.PlayExplosion();
							}
						}
					}
				}
			}
		}

		public static void DrawWeapons()
		{
			foreach(WeaponAccess CurrWeapon in WeaponManagerAccess.AllWeapons)
				CurrWeapon.Draw();
		}
	}
}
