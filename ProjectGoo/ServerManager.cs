using System;

namespace GameEngine
{
	public abstract class ServerManager
	{
		//Local sprites
		public abstract class Local
		{
			private static string Channel = null;
			private static string IP = null;
			private static uint Port;
			private static bool _IsOn;
			private static SpriteInfoManager SpriteInfo = null;

			public static bool IsOn
			{
				get { return _IsOn; }
			}

			public static void Add(uint UniqueId, SpriteInformation SpriteToAdd)
			{
				if(SpriteInfo == null)
					SpriteInfo = new SpriteInfoManager();
				SpriteInfo.Add(UniqueId, SpriteToAdd);
			}

			public static void Remove(uint UniqueId)
			{
				if(SpriteInfo == null)
					SpriteInfo = new SpriteInfoManager();
				SpriteInfo.Remove(UniqueId);
			}

			public static bool Contains(uint UniqueId)
			{
				if(SpriteInfo == null)
					SpriteInfo = new SpriteInfoManager();
				return SpriteInfo.Contains(UniqueId);
			}

			/// <summary> Allows remote machines to access the local machine's sprite data </summary>
			public static void Activate()
			{
				if(Local._IsOn == true)
					return;

				Local.Channel = GameConfig.Net.LocalChannel;
				Local.IP = GameConfig.Net.LocalIP;
				Local.Port = System.Convert.ToUInt32(GameConfig.Net.LocalPort);

				//Create local channel for local player
				if(ChannelManager.DoesChannelExist(Channel)==false)
				{
					ChannelManager.ServeOmnipresentObject(Channel, IP, Port, SpriteInfo);
					//ChannelManager.CreateLocalChannel(Channel, IP, Port, typeof(SpriteInfoManager), System.Runtime.Remoting.WellKnownObjectMode.Singleton);
				}

				Local._IsOn = true;
			}

			public static void Deactivate()
			{
				if(Local._IsOn == false)
					return;

				//TODO:
				// Have it remove:
				// the well known remote service
				// the local channel

				Local._IsOn = false;
			}
		}

		//Remote sprites
		public abstract class Remote
		{
			private static bool _IsOn;

			private static string Channel = null;
			/// <summary> The complete path of the channel that serves sprite information on the remote machine </summary>
			private static string RemoteMachineUri = null;

			public static bool IsOn
			{
				get { return _IsOn; }
			}

			/// <summary> Holds a sprite manager that is marshaled from a remote machine. Info from a remote sprite can be gotten from this.</summary>
			private static SpriteInfoManager RemoteSpriteManager = null;

			public static SpriteInformation GetRemoteSprite(uint UniqueId)
			{	
				Remote.RequireOn();
				SpriteInformation RetVal = null;

				if(RemoteSpriteManager == null)
					RemoteSpriteManager = (SpriteInfoManager) Activator.GetObject(typeof(SpriteInfoManager), Remote.RemoteMachineUri);

//				try
//				{
					RetVal = RemoteSpriteManager.GetInfoFromId(UniqueId);
//				}
//				catch(Exception err)
//				{
//
//				}

				return RetVal;
			}

			public static bool Contains(uint UniqueId)
			{
				Remote.RequireOn();

				if(RemoteSpriteManager == null)
					RemoteSpriteManager = (SpriteInfoManager) Activator.GetObject(typeof(SpriteInfoManager), Remote.RemoteMachineUri);
				return RemoteSpriteManager.Contains(UniqueId);
			}

			/// <summary> Allows the local machine to access sprite data on a remote machine </summary>
			public static void Activate()
			{
				if(Remote._IsOn == true)
					return;

				Remote.Channel = GameConfig.Net.RemoteChannel;
				Remote.RemoteMachineUri = GameConfig.Net.RemoteURI;

				//Create remote channel for remote player
				if(ChannelManager.DoesChannelExist(Channel) == false)
					ChannelManager.CreateRemoteChannel(Channel);

				Remote._IsOn = true;
			}

			public static void Deactivate()
			{
				if(Remote._IsOn == false)
					return;

				//TODO:
				// Have it remove:
				// the remote channel

				Remote._IsOn = false;
			}

			public static bool WaitForRemoteMachine(uint TimeOutCount)
			{
				bool RetVal = false;
				Remote.RequireOn();
			
				SpriteInfoManager TestObject = null;
				TestObject = (SpriteInfoManager) Activator.GetObject(typeof(SpriteInfoManager), Remote.RemoteMachineUri);

				//try grabbing any object from the remote machine until it does not timeout
				while(RetVal == false)
				{
					if(TimeOutCount <= 0)
						break;
					try
					{
						TestObject.Contains(0);
						RetVal = true;
					}
					catch(System.Net.WebException err)
					{
				
					}
					catch(Exception err)
					{
						throw new Exception("While trying to sync the local ServerManager to the remote server, an unexpected type of exception was encountered: " + err.ToString());
					}

					TimeOutCount--;
				}
				
				return RetVal;
			}

			private static void RequireOn()
			{
				if(Remote._IsOn == false)
					throw new Exception("The ServerManager's remote connection is not on");
			}
		}

		/// <summary> Used to get a unique identifier for a sprite </summary>
		public abstract class UniqueIdentifier
		{
			private static uint NewUniqueId;

			public static uint GetNewUniqueId()
			{
				return UniqueIdentifier.NewUniqueId++;
			}
		}
	}
}
