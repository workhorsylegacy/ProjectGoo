using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;


using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using DI = Microsoft.DirectX.DirectInput;


namespace GameEngine
{
	public class GameAccess
	{
		private ScreenAccess Screen = null;
		private SoundAccess sound = null;
		private InputDeviceManagerAccess InputDeviceManager = null;
		private WriterAccess Writer = null;
		private PlayableCharacterAccess[] PlayableCharacters = null;
		private BackgroundAccess Background = null;
		private BubbleManagerAccess BubbleManager = null;
		
		///<summary> Is true if a script is being run by RunFromScript() </summary>
		private bool IsRunningScript = false;

		/// <summary> True if the game is runnning </summary>
		private bool IsRunningGame = false;


		public GameAccess()
		{
			this.Screen = new ScreenAccess();
			this.sound = new SoundAccess(this.Screen);
			this.InputDeviceManager = new InputDeviceManagerAccess(this.Screen);
			this.Writer = new WriterAccess(this.Screen);

			//Check the App.config to see if it is correct
			GameConfig.ValidateConfigFile();
		}

		public void Run()
		{
			this.IsRunningGame = true;
			
			//Start game loop
			while(this.IsRunningGame == true)
			{
				//Determine if we are playing a remote or local game
				bool RemoteGame = GameConfig.Locations.IsRemoteGame();

				//Activate servers if we are playing a remote game
				if(RemoteGame == true)
				{
					ServerManager.Remote.Activate();
					ServerManager.Local.Activate();
				}

				//Load all the game objects
				this.LoadScript();

				//Sync to the remote machine if we are playing a remote game
				if(RemoteGame == true)
				{
					if(ServerManager.Remote.WaitForRemoteMachine(10) == false)
					{
						this.IsRunningScript = false;
						this.IsRunningGame = false;
					}
				}

				//Start Level
				while(this.IsRunningScript == true)
				{
					this.Draw();
					this.Animate();
					Application.DoEvents();
					System.GC.Collect(System.GC.MaxGeneration);

					//Stop the game if the screen is not created
					if(Screen.Created == false)
					{
						this.IsRunningScript = false;
						this.IsRunningGame = false;
					}
				}

				//Shut down the server
				ServerManager.Local.Deactivate();
				ServerManager.Remote.Deactivate();
			}
		}

		private void LoadScript()
		{
			this.IsRunningScript = true;

			//Setup the screen
			this.Screen.StartDrawing(640, 480, (float)Math.PI / 4, 1.0f, 1000f);

			//Setup players
			this.PlayableCharacters = new PlayableCharacterAccess[2];
			this.PlayableCharacters[0] = new PlayableCharacterAccess(this.Screen.device, GameConfig.Files.SpriteA, -2.3f, -1.0f, SpaceAndTime.SpriteZLocation, 128, 128, 1024, 1024, Color.FromArgb(0xFF, 0x00, 0xFF, 0x00), 20, 0);
			this.PlayableCharacters[1] = new PlayableCharacterAccess(this.Screen.device, GameConfig.Files.SpriteB, 2.6f, -1.0f, SpaceAndTime.SpriteZLocation, 128, 128, 1024, 1024, Color.FromArgb(0xFF, 0x00, 0xFF, 0x00), 20, 0);

			this.PlayableCharacters[0].Location = GameConfig.Locations.PlayerOne;
			this.PlayableCharacters[1].Location = GameConfig.Locations.PlayerTwo;

			this.PlayableCharacters[0].Sounds = this.sound;
			this.PlayableCharacters[1].Sounds = this.sound;

			//Get a device for the user
			// tell the manager what type of device and the key map.
			// the ControllerId uniquly matches the user to a device or
			// part of a device.
			InputDeviceManagerAccess.ControllerIdType ControllerId;
			ControllerId = this.InputDeviceManager.GetNewKeyboardInstance(DI.Key.I, DI.Key.K, DI.Key.J, DI.Key.L, DI.Key.U);
			//ControllerId = this.InputDeviceManager.GetNewGamepadInstance(InputDeviceAccess.Buttons.AnyUp, InputDeviceAccess.Buttons.AnyDown, InputDeviceAccess.Buttons.AnyLeft, InputDeviceAccess.Buttons.AnyRight, InputDeviceAccess.Buttons.One);
			this.PlayableCharacters[0].SetInputDevices(this.InputDeviceManager, ControllerId);

			ControllerId = this.InputDeviceManager.GetNewKeyboardInstance(DI.Key.W, DI.Key.S, DI.Key.A, DI.Key.D, DI.Key.E);
			//ControllerId = this.InputDeviceManager.GetNewGamepadInstance(InputDeviceAccess.Buttons.AnyUp, InputDeviceAccess.Buttons.AnyDown, InputDeviceAccess.Buttons.AnyLeft, InputDeviceAccess.Buttons.AnyRight, InputDeviceAccess.Buttons.One);
			this.PlayableCharacters[1].SetInputDevices(this.InputDeviceManager, ControllerId);


			this.Background = new BackgroundAccess(this.Screen.device);

			//Create Bubbles
			this.BubbleManager = new BubbleManagerAccess(this.Screen.device);
		}

		//Animate the game objects if the screen has focus
		private void Animate()
		{
			//Animate only if enough time has passed
			if(SpaceAndTime.TimeToUpdate)
			{
				if(/*this.Screen.HasFocus*/ true)
				{
					//Move Players
					for (uint i = 0; i < this.PlayableCharacters.Length; i++)
					{
						//Change the state of each sprite
						this.PlayableCharacters[i].ChangeState(this.Background);
					}

					// Move weapons
					WeaponManagerAccess.MoveWeapons(this.Background);

					// Move Bubbles
					this.BubbleManager.AnimateBubbles();
					this.BubbleManager.MoveBubbles();

					// Player to weapon collisions
					WeaponManagerAccess.CheckPlayerCollisions(this.PlayableCharacters, this.Screen.Particles, this.sound);

					// Player to bubble collisions
					this.BubbleManager.CheckPlayerCollisions(this.PlayableCharacters);

					// Player to player collisions
					for (uint i = 0; i < this.PlayableCharacters.Length; i++)
					{
						//Only check for a collision if the player is local
						// the remote machine can check for its own collisions
						if(this.PlayableCharacters[i].Location == SpriteAccess.LocationMode.Local)
						{
							//Check each sprite for collisions with other sprites
							this.PlayableCharacters[i].CheckEnemyCollision(this.PlayableCharacters, i);
						}
					}
				}
				else
				{
					//The game has lost focus. sleep
					System.Threading.Thread.Sleep(1000);
				}
			}
		}

		private void Draw()
		{
			this.Screen.StartDraw();

			this.Background.Draw();
			this.Screen.device.RenderState.CullMode = Microsoft.DirectX.Direct3D.Cull.None;
			SpriteAccess.DrawSpriteArray(this.PlayableCharacters);

			WeaponManagerAccess.DrawWeapons();

			this.BubbleManager.DrawBubbles();

			this.Screen.Particles.Draw();

			this.Writer.WriteScoreLeft("Player One Score: " + this.PlayableCharacters[1].DeathCount.ToString());
			this.Writer.WriteScoreRight("Player Two Score: " + this.PlayableCharacters[0].DeathCount.ToString());

			this.Screen.EndDraw();
		}
	}
}
