using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;
using Direct3D = Microsoft.DirectX.Direct3D;
using DirectInput = Microsoft.DirectX.DirectInput;

namespace GameEngine
{
	public class PlayableCharacterAccess : GameEngine.SpriteAccess
	{
		private PlayerState _State;
		private bool IsOnGround;
		private CollisionRectAccess.HitSide _HurtSide;
		private int ItemCount = 0;
		private ItemType _Item;
		private uint _DeathCount = 0;

		public SoundAccess Sounds;

		public ItemType Item
		{
			set
			{
				//Assign the player the item and give them
				// the correct ammount of usage for this item
				// in their inventory.
				this._Item = value;

				//Items that the player can use
				if(this._Item == ItemType.Grenade || this._Item == ItemType.Mine || this._Item == ItemType.SlideMine || this._Item == ItemType.Invisible)
					this.ItemCount = 1;
				else if(this._Item == ItemType.Gun)
					this.ItemCount = 3;
				else if(this._Item == ItemType.Hook || this._Item == ItemType.parachute || this._Item == ItemType.SuperJump)
					this.ItemCount = -1;

				//Items that affect the player instantly
				if(this._Item == ItemType.Death || this._Item == ItemType.Lightning)
				{
					this.ItemCount = 0;
				}
				else if(this._Item == ItemType.Warp)
				{
					const float WALL_RIGHT = -3.1f;
					const float WALL_LEFT = 3.1f;
					const float WALL_UP = 2.3f;
					const float WALL_DOWN = -2.0f;
					this.ItemCount = 0;
					this.X = SpaceAndTime.RandomPercent * WALL_RIGHT;
					this.Y = SpaceAndTime.RandomPercent * WALL_DOWN;
					this.X += SpaceAndTime.RandomPercent * WALL_LEFT;
					this.Y += SpaceAndTime.RandomPercent * WALL_UP;
				}
			}
		}

		public uint DeathCount
		{
			get { return this._DeathCount; }
		}

		public PlayerState State
		{
			set { this._State = value; }
		}

		public CollisionRectAccess.HitSide HurtSdie
		{
			get { return this._HurtSide; }
		}

		public enum PlayerState : int
		{
			StandRight,
			StandLeft,
			RunRight,
			RunLeft,
			JumpRightUp,
			JumpLeftUp,
			JumpRightMoving,
			JumpLeftMoving,
			SuperJumpMoving,
			SuperJumpUp,
			Land,
			Duck,
			FallDown,
			FallMoving,
			RollBack,
			RollForward,
			Drown,
			Explode,
			Parachute,
			Shoot,
			Grenade,
			Mine,
			SlideMine,
			Hook,
			Dying,
			Dead,
			ClimbUp,
			ClimbDown,
			ClimbIdle,
		}

		//Sprite that refernces another sprites texture
		public PlayableCharacterAccess(SpriteAccess ParentSprite, float NewX, float NewY, float NewZ)
			: base(ParentSprite, NewX, NewY, NewZ)
		{
			this.Initialize();
		}

		//Sprite with it's own texture
		public PlayableCharacterAccess(Direct3D.Device ParentDevice, string TextureLocation, float NewX, float NewY, float NewZ, int NewWidth, int NewHeight, int NewSheetWidth, int NewSheetHeight, Color NewMaskColor, uint CollisionRectWidth, uint CollisionRectHeight)
			: base(ParentDevice, TextureLocation, NewX, NewY, NewZ, NewWidth, NewHeight, NewSheetWidth, NewSheetHeight, NewMaskColor, CollisionRectWidth, CollisionRectHeight)
		{
			this.Initialize();
		}

		private void Initialize()
		{
			this._State = PlayerState.StandRight;

			//Use as default
			this._Item = ItemType.Grenade;
			this.ItemCount = 0;

			this.Frame = 4; //so we dont have a blank frame
		}

		public void ChangeState(BackgroundAccess ForeGround)
		{
			if(this.Location == SpriteAccess.LocationMode.Local)
			{
				this.ChangeStateLocal(ForeGround);
			}

			this.UpdateSpriteInfo();
		}

		private void ChangeStateLocal(BackgroundAccess Foreground)
		{
			//Have the device manager give me a ref to the device
			// that way I can poll and get keys from it directly
			InputDeviceAccess CurrInputDevice = this.ParentInputDeviceManager.GetInputDeviceRef(this.ControllerId);
			CurrInputDevice.PollInput();


			//Make player fall if he is jumping and moving down
			if(this.VelocityY < 0 && this._State != PlayerState.Parachute && 
				this._State != PlayerState.ClimbDown && this._State != PlayerState.ClimbUp && 
				this._State != PlayerState.ClimbIdle && this._State != PlayerState.Drown &&
				this._HurtSide == CollisionRectAccess.HitSide.None)
			{
				if(this._State == PlayerState.JumpLeftMoving || this._State == PlayerState.JumpRightMoving || this._State == PlayerState.FallMoving)
					this._State = PlayerState.FallMoving;
				else
					this._State = PlayerState.FallDown;
			}

			//Make player fall off laders if they move right or left
			if(
				(this._State == PlayerState.ClimbDown || this._State == PlayerState.ClimbUp || this._State == PlayerState.ClimbIdle) &&
				(CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Left, this.ControllerId) || CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Right, this.ControllerId))
				)
			{
				this.FaceLeft = CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Left, this.ControllerId);
				this._State = PlayerState.FallMoving;
			}


			//Synchronous animations
			// Respawn if the player sank into the goo or died
			if(this.Y < -2.2f || this._State == PlayerState.Dead)
			{
				this.ActionRespawn();
			}
				// Die
			else if(this._State == PlayerState.Dying || this._Item == ItemType.Death)
			{
				this.ActionDie();
			}
				// Drown
			else if(this.Y <= -1.6815 || this._State == PlayerState.Drown)
			{
				this.ActionDrown();
			}
				// Explode
			else if(this._State == PlayerState.Explode)
			{
				this.ActionExplode();
			}
				//Hurt
			else if(this._HurtSide != CollisionRectAccess.HitSide.None)
			{
				//TODO:
				//if ishurt and standing and they are pressing they key twords the
				// enemy then do is ramming and turn off hurt
				// otherwise do hurt
				this.ActionHurt(Foreground);
			}
				//				//Hook
				//			else if(this._Item == ItemType.Hook &&
				//				(
				//				this._State == PlayerState.Hook || 
				//				CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Attack, this.ControllerId)==true &&
				//				(this._State == PlayerState.FallDown || this._State == PlayerState.FallMoving || 
				//				this._State == PlayerState.JumpLeftMoving || this._State == PlayerState.JumpRightMoving || 
				//				this._State == PlayerState.JumpLeftUp || this._State == PlayerState.JumpRightUp)
				//				)
				//				)
				//			{
				//				this.ActionHook(Foreground);
				//			}	
				//Parachute
			else if(
				this._Item == ItemType.parachute && 
				(CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Attack, this.ControllerId)==true || this._State == PlayerState.Parachute) &&
				(this._State == PlayerState.FallDown || this._State == PlayerState.FallMoving || this._State == PlayerState.Parachute)
				)
			{
				if(CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Right, this.ControllerId))
					this.FaceLeft = false;
				else if(CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Left, this.ControllerId))
					this.FaceLeft = true;
				this.ActionParachute(Foreground);
			}
				//Landing
			else if(this.IsOnGround && (this._State == PlayerState.Land || this._State == PlayerState.FallDown || this._State == PlayerState.FallMoving))
			{
				this.ActionLand();
			}
				//Climbing up poll
			else if(CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Up, this.ControllerId) && 
				Foreground.CanGrabPoll(this) && (this.IsOnGround || this._State == PlayerState.ClimbUp || this._State == PlayerState.ClimbDown || this._State == PlayerState.ClimbIdle))
			{
				this.ActionClimbUp(Foreground);
			}
				//Climbing down poll
			else if(CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Down, this.ControllerId) && 
				Foreground.CanGrabPoll(this) && (this.IsOnGround || this._State == PlayerState.ClimbUp || this._State == PlayerState.ClimbDown || this._State == PlayerState.ClimbIdle))
			{
				this.ActionClimbDown(Foreground);
			}
				//Idle on poll
			else if(this._State == PlayerState.ClimbUp || this._State == PlayerState.ClimbDown || this._State == PlayerState.ClimbIdle)
			{
				this.ActionClimbIdle(Foreground);
			}
				//Falling Moving and Falling Down
			else if(this.IsOnGround == false && this._State != PlayerState.Parachute && 
				this._State != PlayerState.JumpLeftMoving && this._State != PlayerState.JumpRightMoving && 
				this._State != PlayerState.JumpLeftUp && this._State != PlayerState.JumpRightUp &&
				this._State != PlayerState.SuperJumpMoving && this._State != PlayerState.SuperJumpUp)
			{
				if(this._State != PlayerState.FallDown)
					this.ActionFallMoving(Foreground);
				else
					this.ActionFallDown(Foreground);
			}
				//Jumping moving
			else if(
				(this._State == PlayerState.JumpRightMoving || this._State == PlayerState.JumpLeftMoving) ||
				(CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Up, this.ControllerId)==true && 
				(CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Left, this.ControllerId)==true || CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Right, this.ControllerId)==true) && 
				this.IsOnGround==true)
				)
			{
				//Set a direction for the initial frame
				if(this._State != PlayerState.JumpRightMoving && this._State != PlayerState.JumpLeftMoving)
					this.FaceLeft = CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Left, this.ControllerId);
				this.ActionJumpMoving(Foreground);
			}
				//Jumping up
			else if(
				(this._State==PlayerState.JumpRightUp || this._State==PlayerState.JumpLeftUp) ||
				CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Up, this.ControllerId)==true && 
				this.IsOnGround==true
				)
			{
				this.ActionJumpUp(Foreground);
			}
				//Super Jump moving
			else if(
				this._State == PlayerState.SuperJumpMoving ||
				(CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Attack, this.ControllerId)==true &&
				this._Item == ItemType.SuperJump &&
				(CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Left, this.ControllerId)==true || CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Right, this.ControllerId)==true) && 
				this.IsOnGround==true)
				)
			{
				//Set a direction for the initial frame
				if(this._State != PlayerState.JumpRightMoving && this._State != PlayerState.JumpLeftMoving)
					this.FaceLeft = CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Left, this.ControllerId);
				this.ActionSuperJumpMoving(Foreground);
			}
				//Super Jump up
			else if(
				this._State==PlayerState.SuperJumpUp ||
				(this._Item == ItemType.SuperJump &&
				CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Attack, this.ControllerId)==true && 
				this.IsOnGround==true)
				)
			{
				this.ActionSuperJumpUp(Foreground);
			}
				//Rolling backwords
			else if(this._State == PlayerState.RollBack)
			{
				this.ActionRollBack(Foreground);
			}
				//Rolling Forward
			else if(
				this._State == PlayerState.RollForward ||
				(
				(this._State == PlayerState.RunRight || this._State == PlayerState.RunLeft) &&
				CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Down, this.ControllerId)==true &&
				(CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Left, this.ControllerId)==true || CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Right, this.ControllerId)==true)
				)
				)
			{
				this.ActionRollForward(Foreground);
			}
				//Use weapon
			else if((CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Attack, this.ControllerId) == true || this._State == PlayerState.Shoot || this._State == PlayerState.Grenade || this._State == PlayerState.Mine || this._State == PlayerState.SlideMine) && this.ItemCount > 0)
			{
				if(this._Item == ItemType.Gun)
					this.ActionGun();
				else if(this._Item == ItemType.Grenade)
					this.ActionGrenade();
				else if(this._Item == ItemType.SlideMine)
					this.ActionSlideMine();
				else if(this._Item == ItemType.Mine)
					this.ActionMine();
			}
				//Asynchronous animations
				//Ducking
			else if(CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Down, this.ControllerId) == true)
			{
				this.ActionDuck();
			}
				//Running Right
			else if(CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Right, this.ControllerId) == true)
			{
				this.ActionRunRight(Foreground);
			}
				//Running Left
			else if(CurrInputDevice.GetKey(InputDeviceAccess.GameKeys.Left, this.ControllerId) == true)
			{
				this.ActionRunLeft(Foreground);
			}
				//Idle animations
			else if(this._State == PlayerState.Duck)
			{
				this.ActionLand();
			}
			else
			{
				if(this.FaceLeft == false)
					this.ActionStandRight();
				else
					this.ActionStandLeft();
			}

			//Move player
			if(this.VelocityX != 0.0f)
				this.MoveHorizontal(Foreground);
			if(this.VelocityY > 0.0f)
				this.MoveUp(Foreground);
			else if(this.VelocityY < 0.0f)
				this.MoveDown(Foreground);
		}

		private void ApplyNormalGravity()
		{
			//Apply normal gravity only if another gravity has not been applied
			if(this.VelocityY > - SpaceAndTime.LengthFrom2DTo3D(36))
			{
				this.VelocityY -= SpaceAndTime.LengthFrom2DTo3D(9);
			}
		}

		private void ActionClimbUp(BackgroundAccess Foreground)
		{
			//climb 26 to 28
			this._State = PlayerState.ClimbUp;

			this.X = Foreground.GetPollCenter(this);
			this.VelocityY = 0.1f;
			this.VelocityX = 0;

			if(this.Frame < 26 || this.Frame >= 28)
				this.Frame = 26;
			else
				this.Frame++;
		}

		private void ActionClimbDown(BackgroundAccess Foreground)
		{
			//climb 26 to 28
			this._State = PlayerState.ClimbDown;

			this.X = Foreground.GetPollCenter(this);
			this.VelocityY = - 0.1f;
			this.VelocityX = 0;

			if(this.Frame < 26 || this.Frame >= 28)
				this.Frame = 26;
			else
				this.Frame++;
		}

		private void ActionClimbIdle(BackgroundAccess Foreground)
		{
			//climb 26 to 28
			this._State = PlayerState.ClimbIdle;

			this.VelocityY = 0;
			this.VelocityX = 0;

			this.Frame = 27;
		}

		private void ActionRunRight(BackgroundAccess Foreground)
		{
			//walk 32 to 37

			this._State = PlayerState.RunRight;
			this.FaceLeft = false;

			if(this.Frame == 34 || this.Frame == 37)
				Sounds.PlayStep();

			//Reset to first frame if this animation has not started
			if (this.Frame < 32 || this.Frame >= 37)
				this.Frame = 32;
			else
				this.Frame++; //Move to next frame

			this.VelocityX = - SpaceAndTime.LengthFrom2DTo3D(16);
			this.ApplyNormalGravity();
		}

		private void ActionRunLeft(BackgroundAccess Foreground)
		{
			//walk 32 to 37

			this._State = PlayerState.RunLeft;
			this.FaceLeft = true;

			if(this.Frame == 34 || this.Frame == 37)
				Sounds.PlayStep();

			//Reset to first frame if this animation has not started
			if (this.Frame < 32 || this.Frame >= 37)
				this.Frame = 32;
			else
				this.Frame++; //Move to next frame

			this.VelocityX = SpaceAndTime.LengthFrom2DTo3D(16);
			this.ApplyNormalGravity();
		}

		private void ActionRollBack(BackgroundAccess Foreground)
		{
			//roll 8 to 5

			this._State = PlayerState.RollBack;

			if(this.Frame < 5 || this.Frame > 8)
				this.Frame = 8;
			else if(this.Frame==5)
			{
				if(this.FaceLeft == true)
					this._State = PlayerState.StandLeft;
				else
					this._State = PlayerState.StandRight;
			}
			else
				this.Frame--;

			this.ApplyNormalGravity();
		}

		private void ActionRollForward(BackgroundAccess Foreground)
		{
			//roll 5 to 8

			this._State = PlayerState.RollForward;

			if(this.Frame < 5 || this.Frame > 8)
				this.Frame = 5;
			else if(this.Frame==8)
			{
				if(this.FaceLeft == true)
					this._State = PlayerState.StandLeft;
				else
					this._State = PlayerState.StandRight;
			}
			else
				this.Frame++;

			if(this.FaceLeft == true)
				this.VelocityX = SpaceAndTime.LengthFrom2DTo3D(16);
			else
				this.VelocityX = - SpaceAndTime.LengthFrom2DTo3D(16);

			this.ApplyNormalGravity();
		}

		private void ActionStandRight()
		{
			//Stand 29

			this._State = PlayerState.StandRight;
			this.Frame = 29;

			this.VelocityX = 0;

			this.ApplyNormalGravity();
		}

		private void ActionStandLeft()
		{
			//Stand 29

			this._State = PlayerState.StandLeft;
			this.Frame = 29;
			this.VelocityX = 0;

			this.ApplyNormalGravity();
		}

		private void ActionFallDown(BackgroundAccess Foreground)
		{
			//Fall 12
			if(this.IsOnGround == true)
				this._State = PlayerState.Land;
			else
			{
				this._State = PlayerState.FallDown;
				this.Frame = 12;
			}

			this.VelocityX = 0.0f;

			this.ApplyNormalGravity();
		}

		private void ActionFallMoving(BackgroundAccess Foreground)
		{
			//Fall 12
			if(this.IsOnGround == true)
				this._State = PlayerState.Land;
			else
			{
				this._State = PlayerState.FallMoving;
				this.Frame = 12;
			}

			if(this.FaceLeft)
				this.VelocityX = SpaceAndTime.LengthFrom2DTo3D(16);
			else
				this.VelocityX = - SpaceAndTime.LengthFrom2DTo3D(16);

			this.ApplyNormalGravity();
		}

		private void ActionLand()
		{
			//Land 4 then 32
			this._State = PlayerState.Land;

			if (this.Frame != 4 && this.Frame != 32)
				this.Frame = 4;
			else if(this.Frame == 32)
				this._State = PlayerState.StandRight;
			else
				this.Frame = 32;

			this.VelocityX = 0;
			this.ApplyNormalGravity();
		}

		private void ActionDuck()
		{
			//duck 4
			this._State = PlayerState.Duck;

			//Reset to first frame if this animation has not started
			if (this.Frame != 4)
				this.Frame = 4;

			this.VelocityX = 0.0f;
			this.VelocityY = 0.0f;

			this.ApplyNormalGravity();
		}

		private void ActionHurt(BackgroundAccess Foreground)
		{
			//hurt 7

			float BigMovement = SpaceAndTime.LengthFrom2DTo3D(30);

			if(this.Frame != 7)
			{
				this.Frame = 7;

				//First cancel all movement
				this.VelocityX = 0;
				this.VelocityY = 0;

				//Now get the new movement
				switch(this._HurtSide)
				{
					case(CollisionRectAccess.HitSide.TopRight):
						this.VelocityX = BigMovement;
						this.VelocityY = - BigMovement;
						break;
					case(CollisionRectAccess.HitSide.TopLeft):
						this.VelocityX = - BigMovement;
						this.VelocityY = - BigMovement;
						break;
					case(CollisionRectAccess.HitSide.BottomRight):
						this.VelocityX = BigMovement;
						this.VelocityY = BigMovement;
						break;
					case(CollisionRectAccess.HitSide.BottomLeft):
						this.VelocityX = - BigMovement;
						this.VelocityY = BigMovement;
						break;
				}
			}
			else if(this.VelocityX == 0 || this.IsOnGround)
			{
				this._HurtSide = CollisionRectAccess.HitSide.None;
				this._State = PlayerState.RollBack;
			}

			this.ApplyNormalGravity();
		}

		private void ActionExplode()
		{
			//explode 5

			if(this.Y < 5)
			{
				this.VelocityY += 0.6f;
				this.Frame = 5;
			}
			else
				this.State = PlayerState.Dying;

			this.ApplyNormalGravity();
		}

		private void ActionSlideMine()
		{
			if(this.Frame < 40 || this.Frame > 42)
			{
				this.State = PlayerState.SlideMine;
				this.Frame = 40;
			}
			else if(this.Frame == 41)
			{
				this.Frame++;
				WeaponManagerAccess.CreateWeapon(this.ParentDevice, WeaponType.SlideMine, this.X, this.Y, this.Z, this.FaceLeft);
			}
			else if(this.Frame == 42)
			{
				this.Frame = 29;
				if(this.FaceLeft == true)
					this.State = PlayerState.StandLeft;
				else
					this.State = PlayerState.StandRight;

				this.ItemCount--;
			}
			else
				this.Frame++;
		}

		private void ActionMine()
		{
			if(this.Frame < 40 || this.Frame > 42)
			{
				this.State = PlayerState.Mine;
				this.Frame = 40;
			}
			else if(this.Frame == 41)
			{
				this.Frame++;
				WeaponManagerAccess.CreateWeapon(this.ParentDevice, WeaponType.Mine, this.X, this.Y, this.Z, this.FaceLeft);
			}
			else if(this.Frame == 42)
			{
				this.Frame = 29;
				if(this.FaceLeft == true)
					this.State = PlayerState.StandLeft;
				else
					this.State = PlayerState.StandRight;

				this.ItemCount--;
			}
			else
				this.Frame++;

			this.VelocityX = 0.0f;

			this.ApplyNormalGravity();
		}

		private void ActionGun()
		{
			//Gun 18 to 20
			this._State = PlayerState.Shoot;

			if(this.Frame < 18 || this.Frame > 20)
				this.Frame = 18;
			else if(this.Frame == 20)
			{
				this.Frame = 29;
				if(this.FaceLeft == true)
					this.State = PlayerState.StandLeft;
				else
					this.State = PlayerState.StandRight;

				this.ItemCount--;
			}
			else
			{
				this.Frame++;
				if(this.Frame == 19)
				{					
					WeaponManagerAccess.CreateWeapon(this.ParentDevice, WeaponType.Bullet, this.X, this.Y, this.Z, this.FaceLeft);
					this.Sounds.PlayGunShot();
				}
			}

			this.VelocityX = 0.0f;

			this.ApplyNormalGravity();
		}

		private void ActionGrenade()
		{
			//Gun 21 to 22
			this._State = PlayerState.Grenade;

			if(this.Frame < 21 || this.Frame > 22)
				this.Frame = 21;
			else if(this.Frame == 21)
			{
				this.Frame = 22;

				this.ItemCount--;
				WeaponManagerAccess.CreateWeapon(this.ParentDevice, WeaponType.Grenade, this.X, this.Y, this.Z, this.FaceLeft);
			}
			else
			{
				this.Frame = 29;
				if(this.FaceLeft == true)
					this.State = PlayerState.StandLeft;
				else
					this.State = PlayerState.StandRight;
			}

			this.VelocityX = 0.0f;
			this.ApplyNormalGravity();
		}

		private void ActionDie()
		{
			//Die 48 to 50
			this._State = PlayerState.Dying;

			if(this.Frame < 48 || this.Frame > 50)
			{
				this._DeathCount++;
				this.Frame = 48;
			}
			else if(this.Frame == 50)
				this._State = PlayerState.Dead;
			else
				this.Frame++;

			this.ApplyNormalGravity();
		}

		private void ActionRespawn()
		{
			this.X = 0;
			this.Y = 2;
			this.VelocityX = 0;
			this.VelocityY = 0;
			this.VelocityZ = 0;
			this._State = PlayerState.FallDown;
			this._Item = ItemType.Grenade;
			this.ItemCount = 0;
			this.FaceLeft = false;
			this._HurtSide = CollisionRectAccess.HitSide.None;
			this.RotationY = 0;
			this.RotationX = 0;
		}

		private void ActionDrown()
		{
			//Make player drown if they are not already drowning
			this.RotationX += 2.0f;
			if(this._State != PlayerState.Drown)
			{
				this._DeathCount++;
				this._State = PlayerState.Drown;
				this.VelocityY = - SpaceAndTime.LengthFrom2DTo3D(1);
				this.VelocityX = 0.0f;
			}
		}

		private void ActionParachute(BackgroundAccess Foreground)
		{
			//parachute 17
			this._State = PlayerState.Parachute;
			this.Frame = 17;

			if(this.IsOnGround == true)
				this._State = PlayerState.Land;

			//Set velocity from gravity
			this.VelocityY -= SpaceAndTime.LengthFrom2DTo3D(2);
			if(this.VelocityY < SpaceAndTime.LengthFrom2DTo3D(8))
				this.VelocityY = - SpaceAndTime.LengthFrom2DTo3D(8);

			if(this.FaceLeft == true)
				this.VelocityX = SpaceAndTime.LengthFrom2DTo3D(16);
			else
				this.VelocityX = - SpaceAndTime.LengthFrom2DTo3D(16);
		}

		private void ActionHook(BackgroundAccess Foreground)
		{
			//Hook 24 to 25
			this._State = PlayerState.Hook;

			if(this.Frame < 24 || this.Frame > 25)
				this.Frame = 24;
			else if(this.Frame == 25)
			{
				if(this.FaceLeft == true)
					this._State = PlayerState.JumpLeftUp;
				else
					this._State = PlayerState.JumpRightUp;
			}
			else
				this.Frame++;

			this.ApplyNormalGravity();
		}

		private void ActionJumpUp(BackgroundAccess Foreground)
		{
			// jump up 8 to 12
			if(this.FaceLeft == true)
				this._State = PlayerState.JumpLeftUp;
			else
				this._State = PlayerState.JumpRightUp;

			if(this.Frame < 8 || this.Frame > 12)
			{
				this.Frame = 8;
				this.VelocityY = SpaceAndTime.LengthFrom2DTo3D(32);
			}
			else if(this.Frame != 12)
				this.Frame++;

			this.ApplyNormalGravity();
		}

		private void ActionJumpMoving(BackgroundAccess Foreground)
		{
			// 13 to 16
			if(this.VelocityY < 0.0f)
				this._State = PlayerState.FallMoving;
			else if(this.FaceLeft == true)
			{
				this._State = PlayerState.JumpLeftMoving;
				this.VelocityX = SpaceAndTime.LengthFrom2DTo3D(16);
			}
			else
			{
				this._State = PlayerState.JumpRightMoving;
				this.VelocityX = - SpaceAndTime.LengthFrom2DTo3D(16);
			}

			if(this.Frame < 13 || this.Frame > 16)
			{
				this.Frame = 13;
				this.VelocityY = SpaceAndTime.LengthFrom2DTo3D(32);
			}
			else if(this.Frame != 16)
				this.Frame++;

			this.ApplyNormalGravity();
		}

		private void ActionSuperJumpMoving(BackgroundAccess Foreground)
		{
			// 13 to 16
			if(this.VelocityY < 0.0f)
				this._State = PlayerState.FallMoving;
			else
				this._State = PlayerState.SuperJumpMoving;

			if(this.Frame < 13 || this.Frame > 16)
			{
				this.Frame = 13;
				this.VelocityY = SpaceAndTime.LengthFrom2DTo3D(50);
				if(this.FaceLeft)
					this.VelocityX = SpaceAndTime.LengthFrom2DTo3D(16);
				else
					this.VelocityX = - SpaceAndTime.LengthFrom2DTo3D(16);
			}
			else if(this.Frame != 16)
				this.Frame++;

			this.ApplyNormalGravity();
		}

		private void ActionSuperJumpUp(BackgroundAccess Foreground)
		{
			// jump up 8 to 12
			if(this.VelocityY < 0.0f)
				this._State = PlayerState.FallMoving;
			else
				this._State = PlayerState.SuperJumpUp;

			if(this.Frame < 8 || this.Frame > 12)
			{
				this.Frame = 8;
				this.VelocityY = SpaceAndTime.LengthFrom2DTo3D(50);
				this.VelocityX = 0.0f;
			}
			else if(this.Frame != 12)
				this.Frame++;

			this.ApplyNormalGravity();
		}

		private void MoveHorizontal(BackgroundAccess Foreground)
		{
			this.X += this.VelocityX;
		}


		private void MoveDown(BackgroundAccess Foreground)
		{
			float NewX = 0;
			float NewY = 0;

			if(this._State != PlayerState.ClimbDown)
			{
				if(Foreground.CheckPlatformCollision(this, 0.0f, this.VelocityY, ref NewX, ref NewY))
				{
					this.IsOnGround = true;
					this.VelocityY = 0.0f;
				}
				else
				{
					this.IsOnGround = false;
				}
				this.X = NewX;
				this.Y = NewY;
			}
			else
			{
				this.Y += this.VelocityY;
			}
		}

		private void MoveUp(BackgroundAccess Foreground)
		{
//			float NewX = 0;
//			float NewY = 0;
//
//			Foreground.CheckPlatformCollision(this, 0.0f, this.VelocityY, ref NewX, ref NewY);
//			this.X = NewX;
//			this.Y = NewY;
			
			this.IsOnGround = false;
			this.Y += this.VelocityY;
		}

		//Check this sprite against other sprites. If it
		// is hitting them it will change its state accordingly
		public void CheckEnemyCollision(PlayableCharacterAccess[] PlayersToCheck, uint IndexToSkip)
		{
			if(this._HurtSide == CollisionRectAccess.HitSide.None)
			{
				for(uint i=0; i<PlayersToCheck.Length; i++)
				{
					//Skip the index of IndexToSkip
					if(i != IndexToSkip)
					{
						//Set this collision
						this._HurtSide = this.CollisionRects.CheckObjectRectAgainst(PlayersToCheck[i], this.Frame, this.X, this.Y);

						//Set the other collision
						switch(this._HurtSide)
						{
							case(CollisionRectAccess.HitSide.TopRight):
								PlayersToCheck[i]._HurtSide = CollisionRectAccess.HitSide.BottomLeft;
								break;
							case(CollisionRectAccess.HitSide.TopLeft):
								PlayersToCheck[i]._HurtSide = CollisionRectAccess.HitSide.BottomRight;
								break;
							case(CollisionRectAccess.HitSide.BottomRight):
								PlayersToCheck[i]._HurtSide = CollisionRectAccess.HitSide.TopLeft;
								break;
							case(CollisionRectAccess.HitSide.BottomLeft):
								PlayersToCheck[i]._HurtSide = CollisionRectAccess.HitSide.TopRight;
								break;
						}

						break;
					}
				}
			}
		}
	}
}

