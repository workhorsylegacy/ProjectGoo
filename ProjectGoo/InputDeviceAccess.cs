using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;
using DirectInput = Microsoft.DirectX.DirectInput;

namespace GameEngine
{
	public class InputDeviceAccess
	{
		private uint _DeviceUserMax;
		private uint _DeviceUserCount;
		private TypeOfDevices _TypeOfDevice;
		private DirectInput.Device device = null;
		private DirectInput.Key[] PressedKeys = null; //Holds the keys that were polled
		private System.Collections.Hashtable[] KeyTable;

		private static ArrayList AssignedGuids = new ArrayList(); //Holds the Guids of devices that are assigned to a player

		public uint DeviceUserCount
		{
			get { return(this._DeviceUserCount); }
		}

		public uint DeviceUserMax
		{
			get { return(this._DeviceUserMax); }
		}

		public TypeOfDevices TypeOfDevice
		{
			get { return(this._TypeOfDevice); }
		}

		public System.Guid Guid
		{
			get { return(this.device.DeviceInformation.InstanceGuid); }
		}

		public enum TypeOfDevices : uint
		{
			Keyboard,
			Mouse,
			Gamepad,
		}

		//keys in the game that can be assigned to a button
		public enum GameKeys : uint
		{
			Up, 
			Down,
			Left,
			Right,
			Attack,
		}

		//the value of a key for an input device
		public struct Buttons
		{
			public static int One = 0;
			public static int Two = 1;
			public static int Three = 2;
			public static int Four = 3;
			public static int Five = 4;
			public static int Six = 5;
			public static int Seven = 6;
			public static int Eight = 7;
			public static int Nine = 8;
			public static int Ten = 9;
			public static int Eleven = 10;
			public static int Twelve = 11;
			public static int AxisUp = 0;
			public static int AxisDown = 1;
			public static int AxisRight = 2;
			public static int AxisLeft = 3;
			public static int HatUp = 0;
			public static int HatDown = 1;
			public static int HatLeft = 2;
			public static int HatRight = 3;
			public static int AnyUp = 0;
			public static int AnyDown = 1;
			public static int AnyLeft = 2;
			public static int AnyRight = 3;
		}

		private static bool IsFreeDeviceGuid(System.Guid GuidToCheck)
		{
			bool RetVal = true;
			
			foreach(System.Guid AssignedGuid in InputDeviceAccess.AssignedGuids)
			{
				if(AssignedGuid == GuidToCheck)
					RetVal = false;
			}

			return(RetVal);
		}

		public InputDeviceAccess(ScreenAccess ParentForm, InputDeviceAccess.TypeOfDevices TypeOfDevice)
		{
			if(TypeOfDevice == InputDeviceAccess.TypeOfDevices.Keyboard)
				this.InitializeAsKeyboard(ParentForm, TypeOfDevice);
			else if(TypeOfDevice == InputDeviceAccess.TypeOfDevices.Gamepad)
				this.InitializeAsGamePad(ParentForm, TypeOfDevice);
			else if(TypeOfDevice == InputDeviceAccess.TypeOfDevices.Mouse)
				 throw new System.Exception("mouse not yet added to InputDeviceAccess");
		}

		private void InitializeAsKeyboard(ScreenAccess ParentForm, InputDeviceAccess.TypeOfDevices NewTypeOfDevice)
		{
			try
			{
				this.device = new DirectInput.Device(DirectInput.SystemGuid.Keyboard);
				this.device.SetCooperativeLevel
				(
					ParentForm, 
					DirectInput.CooperativeLevelFlags.Background | 
					DirectInput.CooperativeLevelFlags.NonExclusive
				);
				device.Acquire();

				this._TypeOfDevice = NewTypeOfDevice;

				//Determine how many users are suited for this device
				this._DeviceUserMax = 2;

				// Get space in the keytable for the number of device users
				this.KeyTable = new System.Collections.Hashtable[this._DeviceUserMax];

				//record guid as used
				InputDeviceAccess.AssignedGuids.Add(this.device.DeviceInformation.InstanceGuid);
			}
			catch(Exception err)
			{
				throw err;
			}
		}

		private void InitializeAsGamePad(ScreenAccess ParentForm, InputDeviceAccess.TypeOfDevices NewTypeOfDevice)
		{
			//Check to see if this type of device is available and try to get it for this user
			try
			{
				//Grab first attached gamepad that isnt already assigned
				foreach(DirectInput.DeviceInstance CurrDeviceInstance in DirectInput.Manager.GetDevices(DirectInput.DeviceClass.GameControl, DirectInput.EnumDevicesFlags.AttachedOnly))
				{
					if(InputDeviceAccess.IsFreeDeviceGuid(CurrDeviceInstance.InstanceGuid) == true)
					{
						this.device = new DirectInput.Device(CurrDeviceInstance.InstanceGuid);
						
						//If this device is dead throw an exception
						if(this.device == null)
							throw new Exception("found a gamepad GUID, but when we tried to make a device from it it was null");
						else
							break;
					}
				}

				//throw an exception if there is no device
				if(this.device == null)
					throw new Exception("A gamepad was assigned as an input device, but none were found.");

				//Setup device
				this._TypeOfDevice = NewTypeOfDevice;
				this.device.SetDataFormat(DirectInput.DeviceDataFormat.Joystick);
				this.device.SetCooperativeLevel(ParentForm, DirectInput.CooperativeLevelFlags.Background | DirectInput.CooperativeLevelFlags.NonExclusive);
				this.device.Properties.AxisModeAbsolute = true;
				this.device.Acquire();

				//Determine how many users are suited for this device
				this._DeviceUserMax = 1;

				// Get space in the keytable for the number of device users
				this.KeyTable = new System.Collections.Hashtable[this._DeviceUserMax];

				//record guid as used
				InputDeviceAccess.AssignedGuids.Add(this.device.DeviceInformation.InstanceGuid);

				//Get the keys just so PressedKeys wont be null
				this.PollInput();
			}
			catch(Exception err)
			{
				throw err;
			}
		}

		/// <summary>
		/// Gets the next free user id for this device. Will return 0
		/// if there are no more ids available.
		/// </summary>
		/// <returns></returns>
		public uint GetNextUserId()
		{
			uint RetVal = 0;
			if(this._DeviceUserCount + 1 <= this._DeviceUserMax)
			{
				this._DeviceUserCount++;
				RetVal = this._DeviceUserCount;
			}
			return(RetVal);
		}

		/// <summary>
		/// Used to define what keys will be used
		/// for what in-game actions.
		/// </summary>
		/// <param name="NewKeyTable"></param>
		/// <param name="CurrUserId"></param>
		public void SetKeyTable(System.Collections.Hashtable NewKeyTable, uint CurrUserId)
		{
			this.KeyTable[CurrUserId-1] = new System.Collections.Hashtable();
			this.KeyTable[CurrUserId-1] = (System.Collections.Hashtable) NewKeyTable.Clone();
		}

		//Gets all the keys that are pressed. All keys that were polled will be saved
		// and used in all other called methods
		public void PollInput()
		{
			if(this._TypeOfDevice == InputDeviceAccess.TypeOfDevices.Keyboard)
				this.PressedKeys = this.device.GetPressedKeys();
//			else
//				this.device.Poll();
			//TODO:
			//Does this device need to be polled?
		}

		//TODO:
		//make functions for IsAnyKeysPressed() and IsAllKeysPressed()
		//they should accept params of keys
		public bool GetKey(GameKeys KeyToFind, InputDeviceManagerAccess.ControllerIdType CurrControllerId)
		{
			bool RetVal = false;

			if(this._TypeOfDevice == InputDeviceAccess.TypeOfDevices.Keyboard)
			{
				foreach (DirectInput.Key a in this.PressedKeys)
				{
					if ((DirectInput.Key) this.KeyTable[CurrControllerId.DeviceUserId-1][KeyToFind] == a)
					{
						RetVal = true;
						break;
					}
				}
			}
			else if(this._TypeOfDevice == InputDeviceAccess.TypeOfDevices.Gamepad)
			{
				byte[] ButtonStatus;
				int[] HatStatus;

				//Check hats and axis
				if(KeyToFind >= InputDeviceAccess.GameKeys.Up && KeyToFind <= InputDeviceAccess.GameKeys.Right)
				{
					HatStatus = this.device.CurrentJoystickState.GetPointOfView();
					int PressedKeyValue = (int)(this.KeyTable[CurrControllerId.DeviceUserId-1][KeyToFind]);
					switch(KeyToFind)
					{
						case(InputDeviceAccess.GameKeys.Up):
							if(HatStatus[0]==0 || HatStatus[0]==4500 || HatStatus[0]==31500)
								RetVal = true;
							break;
						case(InputDeviceAccess.GameKeys.Left):
							if(HatStatus[0]==27000 || HatStatus[0]==31500 || HatStatus[0]==22500)
								RetVal = true;
							break;
						case(InputDeviceAccess.GameKeys.Right):
							if(HatStatus[0]==9000 || HatStatus[0]==4500 || HatStatus[0]==13500)
								RetVal = true;
							break;
						case(InputDeviceAccess.GameKeys.Down):
							if(HatStatus[0]==18000 || HatStatus[0]==22500 || HatStatus[0]==13500)
								RetVal = true;
							break;
					}
				}
				//Check buttons
				else if(KeyToFind == InputDeviceAccess.GameKeys.Attack)
				{
					ButtonStatus = this.device.CurrentJoystickState.GetButtons();
					int PressedKeyValue = (int)(this.KeyTable[CurrControllerId.DeviceUserId-1][KeyToFind]);
					if(ButtonStatus[PressedKeyValue] > 0)
					{
						RetVal = true;
					}
				}

			}

			return (RetVal);
		}
	}
}
