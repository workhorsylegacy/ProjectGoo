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
	//Takes care of all the bubbles in the game
	public class BubbleManagerAccess
	{
		#region " BubbleAccess "
		private class BubbleAccess
		{
			private ItemType _Item;
			public ItemType Item
			{
				get{return(this._Item);}
			}

			private BubbleState _State;
			public BubbleState State
			{
				get{return(this._State);}
			}

			private float X, Y, Z;

			private float NextX;
			private float NextY;
			private float MoveSpeed = SpaceAndTime.LengthFrom2DTo3D(8.0f);

			private static SpriteAccess MasterBubbleSprite = null;
			private static SpriteAccess MasterItemSprite = null;

			private SpriteAccess BubbleSprite = null;
			private SpriteAccess ItemSprite = null;

			public enum BubbleState : int
			{
				NeedsFreshAngle,
				HitWallUp,
				HitWallDown,
				HitWallRight,
				HitWallLeft,
				Floating,
				Popping,
				Popped,
				Bouncing
			}


			public BubbleAccess(Direct3D.Device NewParentDevice, float NewX, float NewY, float NewZ)
			{
				this.Initialize(NewParentDevice, NewX, NewY, NewZ);
			}

			private void Initialize(Direct3D.Device NewParentDevice, float NewX, float NewY, float NewZ)
			{
				//recreate the master sprites if they dont exist
				if(BubbleAccess.MasterBubbleSprite == null)
					BubbleAccess.MasterBubbleSprite = new SpriteAccess(NewParentDevice, GameConfig.Files.Bubbles, 0, 0, 0, 32, 32, 64, 64, Color.FromArgb(0xFF, 0x00, 0x00, 0xFF), 0, 0);

				if(BubbleAccess.MasterItemSprite == null)
					BubbleAccess.MasterItemSprite = new SpriteAccess(NewParentDevice, GameConfig.Files.Items, 0, 0, 0, 32, 32, 128, 128, Color.FromArgb(0xFF, 0x00, 0x00, 0xFF), 0, 0);

				this.X = NewX;
				this.Y = NewY;
				this.Z = NewZ;

				//have this bubble sprites refer to the master sprites
				this.BubbleSprite = new SpriteAccess(BubbleAccess.MasterBubbleSprite, this.X, this.Y, this.Z);
				this.ItemSprite = new SpriteAccess(BubbleAccess.MasterItemSprite, this.X, this.Y, this.Z);

				this._State = BubbleState.NeedsFreshAngle;

				this._Item = (ItemType) (int)((1.0f + (float)ItemType.Warp) * SpaceAndTime.RandomPercent);
				this.ItemSprite.Frame =  (int)this._Item;
			}

			public void ChangeState()
			{
				if(this._State == BubbleState.NeedsFreshAngle)
					this.ActionGetFirstAngle();
				else if(this._State == BubbleState.Popping)
					this.ActionPop();
				else if(this._State == BubbleState.Bouncing)
					this._State = BubbleState.Floating;
				else if(this._State >= BubbleState.HitWallUp && this._State <= BubbleState.HitWallLeft)
					this.ActionBounceOffWall();
			}

			private void ActionBounceOffWall()
			{
				if(this._State == BubbleState.HitWallDown)
					this.NextY = -this.NextY;
				else if(this._State == BubbleState.HitWallUp)
					this.NextY = -this.NextY;
				else if(this._State == BubbleState.HitWallRight)
					this.NextX = -this.NextX;
				else if(this._State == BubbleState.HitWallLeft)
					this.NextX = -this.NextX;

				this._State = BubbleState.Bouncing;
			}

			private void ActionPop()
			{
				// Pop 0 to 3
				if(this.BubbleSprite.Frame < 0 || this.BubbleSprite.Frame > 3)
					this.BubbleSprite.Frame = 0;
				else if(this.BubbleSprite.Frame == 3)
					this._State = BubbleState.Popped;
				else
					this.BubbleSprite.Frame++;
			}

			private void ActionGetFirstAngle()
			{
				float QuadrantMovement;
				float UpMovementPercentage = 0.0f;
				float DownMovementPercentage = 0.0f;
				float RightMovementPercentage = 0.0f;
				float LeftMovementPercentage = 0.0f;
				float FirstEighth;
				float SecondEighth;
				float MovementAngle;

				MovementAngle = 360.0f * SpaceAndTime.RandomPercent;

				this._State = BubbleState.Floating;

				//Find the degree of movement in this quarter of 360 degrees
				if(MovementAngle <= 90) // North-Western Movement
					QuadrantMovement = MovementAngle;
				else if(MovementAngle <= 180) // North-Eastern Movement
					QuadrantMovement = MovementAngle - 90;
				else if(MovementAngle <= 270) // South-East Movement
					QuadrantMovement = MovementAngle - 180;
				else // South-West Movement
					QuadrantMovement = MovementAngle - 270;

				//For each half of the quarter, find out how much we moved
				// percentage wise (of the total degrees for this quarter). 
				FirstEighth = QuadrantMovement / 90.0f;
				SecondEighth = (90.0f - QuadrantMovement) / 90.0f;

				//Find out how much we moved in each direction
				if(MovementAngle <= 90) // North-Western Movement
				{
					UpMovementPercentage = FirstEighth;
					LeftMovementPercentage = SecondEighth;
				}
				else if(MovementAngle <= 180) // North-Eastern Movement
				{
					UpMovementPercentage = SecondEighth;
					RightMovementPercentage = FirstEighth;
				}
				else if(MovementAngle <= 270) // South-East Movement
				{
					DownMovementPercentage = FirstEighth;
					RightMovementPercentage = SecondEighth;
				}
				else // South-West Movement
				{
					DownMovementPercentage = SecondEighth;
					LeftMovementPercentage = FirstEighth;
				}

				//Set next movement
				if(UpMovementPercentage > 0.0f)
					this.NextY = + (UpMovementPercentage * this.MoveSpeed);
				else if(DownMovementPercentage > 0.0f)
					this.NextY = - (DownMovementPercentage * this.MoveSpeed);
				
				if(RightMovementPercentage > 0.0f)
					this.NextX = - (RightMovementPercentage * this.MoveSpeed);
				else if(LeftMovementPercentage > 0.0f)
					this.NextX = + (LeftMovementPercentage * this.MoveSpeed);
			}

			public void Move()
			{
				const float WALL_RIGHT = -3.1f;
				const float WALL_LEFT = 3.1f;
				const float WALL_UP = 2.3f;
				const float WALL_DOWN = -2.0f;

				//Hit horizontal walls or move
				if(this.X <= WALL_RIGHT && this._State != BubbleState.Bouncing)
					this._State = BubbleState.HitWallRight;
				else if(this.X >= WALL_LEFT && this._State != BubbleState.Bouncing)
					this._State = BubbleState.HitWallLeft;
				else
					this.X += this.NextX;

				//Hit vertical walls or move
				if(this.Y <= WALL_DOWN && this._State != BubbleState.Bouncing)
					this._State = BubbleState.HitWallDown;
				else if(this.Y >= WALL_UP && this._State != BubbleState.Bouncing)
					this._State = BubbleState.HitWallUp;
				else
					this.Y += this.NextY;
			}

			public void Draw()
			{
				this.BubbleSprite.X = this.X;
				this.BubbleSprite.Y = this.Y;
				this.BubbleSprite.Z = this.Z;

				this.ItemSprite.X = this.X;
				this.ItemSprite.Y = this.Y;
				this.ItemSprite.Z = this.Z;

				this.BubbleSprite.Draw();
				this.ItemSprite.Draw();
			}

			public bool CheckPlayerCollisions(PlayableCharacterAccess Player)
			{
				bool RetVal = false;

				//Dont bother checking with bubbles that are popped
				if(this._State != BubbleState.Popped && this._State != BubbleState.Popping)
				{
					if(this.BubbleSprite.CollisionRects.CheckObjectRectAgainst(Player, this.BubbleSprite.Frame, this.X, this.Y) != CollisionRectAccess.HitSide.None)
					{
						this._State = BubbleState.Popping;
						RetVal = true;
					}
				}

				return(RetVal);
			}
		}
		#endregion

		private ArrayList Bubbles;
		private Direct3D.Device ParentDevice;

		public BubbleManagerAccess(Direct3D.Device NewParentDevice)
		{
			this.ParentDevice = NewParentDevice;

			this.Bubbles = new ArrayList();
		}

		public void DrawBubbles()
		{
			foreach(BubbleAccess Bubble in this.Bubbles)
				Bubble.Draw();
		}

		public void AnimateBubbles()
		{
			//Remove any bubbles that have popped
			BubbleAccess CurrBubble;
			for(int i=0; i< this.Bubbles.Count; i++)
			{
				CurrBubble = (BubbleAccess) this.Bubbles[i];
				if(CurrBubble.State == BubbleAccess.BubbleState.Popped)
				{
					this.Bubbles.RemoveAt(i);
				}
			}
			
			// Animate bubbles
			foreach(BubbleAccess Bubble in this.Bubbles)
				Bubble.ChangeState();

			//Create more bubbles if there is less than 3
			if(this.Bubbles.Count < 3)
			{
				float NewX, NewY;
				this.CreateRandomBubbleLocation(out NewX, out NewY);
				this.Bubbles.Add(new BubbleAccess(this.ParentDevice, NewX, NewY, SpaceAndTime.SpriteZLocation));
			}
		}

		public void MoveBubbles()
		{
			foreach(BubbleAccess Bubble in this.Bubbles)
				Bubble.Move();
		}

		public void CheckPlayerCollisions(PlayableCharacterAccess[] PlayableCharacters)
		{
			//For each player
			foreach(PlayableCharacterAccess Player in PlayableCharacters)
			{
				//For each bubble
				foreach(BubbleAccess Bubble in this.Bubbles)
				{
					//Check if the bubble hit a player
					if(Bubble.CheckPlayerCollisions(Player) == true)
					{
						Player.Item = Bubble.Item;
					}
				}
			}
		}

		private void CreateRandomBubbleLocation(out float NewX, out float NewY)
		{
			//Set defaults to new x and y
			NewX = 0;
			NewY = 0;

			//Pick one of the 3 spawn points randomly
			uint SpawnPoint = (uint)(SpaceAndTime.RandomPercent * 3.0f);
			
			//Get ammount x and y will deviate from spawn point
			float DeviationX = SpaceAndTime.RandomPercent * 0.5f;
			float DeviationY = SpaceAndTime.RandomPercent * 0.5f;

			//Get random x and y within spawn point
			switch(SpawnPoint)
			{
				case(0): //left
					NewX = 2.0f - DeviationX; 
					NewY = 0.0f + DeviationY;
					break;
				case(1): //top
					NewX = 0.0f + DeviationX;
					NewY = 2.0f - DeviationY;
					break;
				case(2): //right
					NewX = -2.0f + DeviationX; 
					NewY = 0.0f + DeviationY;
					break;
			}
		}
	}
}





