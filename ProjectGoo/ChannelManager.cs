using System;
using System.Collections;
using System.Data;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;

namespace GameEngine
{
	public abstract class ChannelManager
	{
		private static string LocalUri;
		private static string LocalIP;
		private static uint LocalPort;

		public static string RemoteChannel;

		public static void CreateLocalChannel(string NewUri, string NewIP, uint NewPort, System.Type ObjectToBeServed, WellKnownObjectMode ObjectMode)
		{
			ChannelManager.LocalUri = NewUri;
			ChannelManager.LocalIP = NewIP;
			ChannelManager.LocalPort = NewPort;

			//Create the channel if it doesn't exist
			if(ChannelManager.DoesChannelExist(NewUri) == false)
			{
				System.Collections.Specialized.ListDictionary ChannelProperties = new System.Collections.Specialized.ListDictionary();
				ChannelProperties.Add("port", (int)ChannelManager.LocalPort);
				ChannelProperties.Add("name", ChannelManager.LocalUri);
				ChannelProperties.Add("bindTo", ChannelManager.LocalIP);

				HttpChannel chnl = new HttpChannel(ChannelProperties, new SoapClientFormatterSinkProvider(), new SoapServerFormatterSinkProvider());
				ChannelServices.RegisterChannel(chnl);

				if(DoesWellKnowServiceExist(ObjectToBeServed.FullName) == false)
				{
					RemotingConfiguration.RegisterWellKnownServiceType(
						ObjectToBeServed,
						ChannelManager.LocalUri,
						ObjectMode);
				}
			}

			//Throw a fatal exception if the channel was not created
			if(ChannelManager.DoesChannelExist(ChannelManager.LocalUri) == false)
				throw new Exception(String.Format("Created the channel '{0}', but it didn't show up as a registered channel", ChannelManager.LocalUri));
		}

		/// <summary> Provides an object instance to the network. All objects will be omnipresent. </summary>
		public static void ServeOmnipresentObject(string NewUri, string NewIP, uint NewPort, MarshalByRefObject ObjectToServe)
		{
			ChannelManager.LocalUri = NewUri;
			ChannelManager.LocalIP = NewIP;
			ChannelManager.LocalPort = NewPort;

			//Create the channel if it doesn't exist
			if(ChannelManager.DoesChannelExist(NewUri) == false)
			{
				System.Collections.Specialized.ListDictionary ChannelProperties = new System.Collections.Specialized.ListDictionary();
				ChannelProperties.Add("port", (int)ChannelManager.LocalPort);
				ChannelProperties.Add("name", ChannelManager.LocalUri);
				ChannelProperties.Add("bindTo", ChannelManager.LocalIP);

				HttpChannel chnl = new HttpChannel(ChannelProperties, new SoapClientFormatterSinkProvider(), new SoapServerFormatterSinkProvider());
				ChannelServices.RegisterChannel(chnl);

				//Need a way to see if this object is already marshaled?
				RemotingServices.Marshal(ObjectToServe, ChannelManager.LocalUri);
			}

			//Throw a fatal exception if the channel was not created
			if(ChannelManager.DoesChannelExist(ChannelManager.LocalUri) == false)
				throw new Exception(String.Format("Created the channel '{0}', but it didn't show up as a registered channel", ChannelManager.LocalUri));
		}

		public static void CreateRemoteChannel(string NewChannelName)
		{
			ChannelManager.RemoteChannel = NewChannelName;

			//Create the channel if it doesn't exist
			if(ChannelManager.DoesChannelExist(ChannelManager.RemoteChannel) == false)
			{
				System.Collections.Specialized.ListDictionary ChannelProperties = new System.Collections.Specialized.ListDictionary();
				ChannelProperties.Add("name", ChannelManager.RemoteChannel);
				HttpChannel chnl = new HttpChannel(ChannelProperties, new SoapClientFormatterSinkProvider(), new SoapServerFormatterSinkProvider());
				ChannelServices.RegisterChannel(chnl);
			}

			//Throw a fatal exception if the channel was not created
			if(ChannelManager.DoesChannelExist(ChannelManager.RemoteChannel) == false)
				throw new Exception(String.Format("Created the channel '{0}', but it didn't show up as a registered channel", ChannelManager.LocalUri));
		}

		public static bool DoesChannelExist(string ChannelName)
		{
			bool RetVal = false;

			System.Runtime.Remoting.Channels.IChannel OldChannel = ChannelServices.GetChannel(ChannelName);
			if(OldChannel != null)
				RetVal = true;

			return RetVal;
		}

		public static bool DoesWellKnowServiceExist(string ServiceName)
		{
			bool RetVal = false;

			WellKnownServiceTypeEntry[] ExistingServices = RemotingConfiguration.GetRegisteredWellKnownServiceTypes();
			foreach(WellKnownServiceTypeEntry Service in ExistingServices)
			{
				if(Service.ObjectUri == ServiceName)
				{
					RetVal = true;
					break;
				}
			}

			return RetVal;
		}
	}
}
