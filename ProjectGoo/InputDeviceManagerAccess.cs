using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using DirectInput = Microsoft.DirectX.DirectInput;

namespace GameEngine
{
	public class InputDeviceManagerAccess
	{
		private ScreenAccess ParentForm = null;
		private ArrayList Devices = null;
		private ArrayList ControllerIds = null; //holds the an array of ControllerIdType

		public struct ControllerIdType
		{
			public System.Guid DeviceGuid;
			public uint DeviceUserId;
		}

		public InputDeviceManagerAccess(ScreenAccess NewParentForm)
		{
			this.ParentForm = NewParentForm;
			this.Devices = new ArrayList();
			this.ControllerIds = new ArrayList();
		}

		public ControllerIdType GetNewKeyboardInstance(DirectInput.Key KeyUp, DirectInput.Key KeyDown, DirectInput.Key KeyLeft, DirectInput.Key KeyRight, DirectInput.Key KeyAttack)
		{
			System.Collections.Hashtable KeyTable = new System.Collections.Hashtable();
			KeyTable[InputDeviceAccess.GameKeys.Up] = KeyUp;
			KeyTable[InputDeviceAccess.GameKeys.Down] = KeyDown;
			KeyTable[InputDeviceAccess.GameKeys.Left] = KeyLeft;
			KeyTable[InputDeviceAccess.GameKeys.Right] = KeyRight;
			KeyTable[InputDeviceAccess.GameKeys.Attack] = KeyAttack;

			InputDeviceAccess.TypeOfDevices TypeOfDevice = InputDeviceAccess.TypeOfDevices.Keyboard;

			return(this.InitializeDevice(TypeOfDevice, KeyTable));
		}

		public ControllerIdType GetNewGamepadInstance(int KeyUp, int KeyDown, int KeyLeft, int KeyRight, int KeyAttack)
		{
			System.Collections.Hashtable KeyTable = new System.Collections.Hashtable();
			KeyTable[InputDeviceAccess.GameKeys.Up] = KeyUp;
			KeyTable[InputDeviceAccess.GameKeys.Down] = KeyDown;
			KeyTable[InputDeviceAccess.GameKeys.Left] = KeyLeft;
			KeyTable[InputDeviceAccess.GameKeys.Right] = KeyRight;
			KeyTable[InputDeviceAccess.GameKeys.Attack] = KeyAttack;

			InputDeviceAccess.TypeOfDevices TypeOfDevice = InputDeviceAccess.TypeOfDevices.Gamepad;

			return(this.InitializeDevice(TypeOfDevice, KeyTable));
		}

		private ControllerIdType InitializeDevice(InputDeviceAccess.TypeOfDevices TypeOfDevice, System.Collections.Hashtable NewKeyTable)
		{
			InputDeviceAccess CurrDevice = null;
			ControllerIdType NewControllerId = new ControllerIdType();

			//If the desired device is a keyboard and another keyboard has available users
			// assign this user to the same keyboard.
			if(TypeOfDevice == InputDeviceAccess.TypeOfDevices.Keyboard)
			{
				foreach(InputDeviceAccess ExistingDevice in this.Devices)
				{
					if(ExistingDevice.TypeOfDevice == InputDeviceAccess.TypeOfDevices.Keyboard && 
						ExistingDevice.DeviceUserCount < ExistingDevice.DeviceUserMax)
					{
						CurrDevice = ExistingDevice;
					}
				}
			}

			//create device
			if(CurrDevice == null)
				CurrDevice = new InputDeviceAccess(this.ParentForm, TypeOfDevice);
			
			//controller info: guid & device user id
			NewControllerId.DeviceGuid = CurrDevice.Guid;
			NewControllerId.DeviceUserId = CurrDevice.GetNextUserId();

			//Save ControllerId and Device
			this.ControllerIds.Add(NewControllerId);
			this.Devices.Add(CurrDevice);

			//Send device new keytable
			CurrDevice.SetKeyTable(NewKeyTable, NewControllerId.DeviceUserId);

			//Poll the device to give it some keys to check
			CurrDevice.PollInput();

			return(NewControllerId);
		}

		public InputDeviceAccess GetInputDeviceRef(ControllerIdType ControllerWithGuidOfDeviceToReturn)
		{
			InputDeviceAccess DeviceToReturn = null;

			//Find the device with the guid and returns a reference to it
			foreach(InputDeviceAccess CurrInputDevice in this.Devices)
			{
				if(CurrInputDevice.Guid == ControllerWithGuidOfDeviceToReturn.DeviceGuid)
					DeviceToReturn = CurrInputDevice;
			}
			
			//Throw if the device was not found
			if(DeviceToReturn == null)
				throw new System.Exception("the device manager was instructed to return a reference to a device with the Guid " + ControllerWithGuidOfDeviceToReturn.DeviceGuid.ToString() + " which is not an existing device");

			return(DeviceToReturn);
		}

//		public void PollInput(ControllerIdType ControllerToPoll)
//		{
//			//Find the device with the guid and have it poll
//			// pass in the device it self and look for it in the manager
//			foreach(InputDeviceAccess CurrInputDevice in this.Devices)
//			{
//				if(CurrInputDevice.Guid == ControllerToPoll.DeviceGuid)
//					CurrInputDevice.PollInput();
//			}
//		}
//
//		public bool GetKey(InputDeviceAccess.KeyNames KeyToCheck, uint ControllerIdToCheck)
//		{
//
//		}
	}
}
