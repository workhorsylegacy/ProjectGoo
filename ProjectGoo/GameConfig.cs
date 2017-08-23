using System;
using System.Collections;
using System.Configuration;
using System.Collections.Specialized;

namespace GameEngine
{
	/// <summary> Used to access the config file </summary>
	public class GameConfig
	{
		public class Files
		{
			public static readonly string SpriteA = ConfigurationSettings.AppSettings["SpriteA"];
			public static readonly string SpriteB = ConfigurationSettings.AppSettings["SpriteB"];

			public static readonly string Platform = ConfigurationSettings.AppSettings["Platform"];

			public static readonly string PollTop = ConfigurationSettings.AppSettings["PollTop"];
			public static readonly string PollCenter = ConfigurationSettings.AppSettings["PollCenter"];
			public static readonly string PollBottom = ConfigurationSettings.AppSettings["PollBottom"];

			public static readonly string Bubbles = ConfigurationSettings.AppSettings["Bubbles"];
			public static readonly string Items = ConfigurationSettings.AppSettings["Items"];

			public static readonly string Grenade = ConfigurationSettings.AppSettings["Grenade"];
			public static readonly string SlideMine = ConfigurationSettings.AppSettings["SlideMine"];
			public static readonly string Mine = ConfigurationSettings.AppSettings["Mine"];
			public static readonly string Bullet = ConfigurationSettings.AppSettings["Bullet"];

			public static readonly string ExplosionPoint = ConfigurationSettings.AppSettings["ExplosionPoint"];
			public static readonly string ExplosionLight = ConfigurationSettings.AppSettings["ExplosionLight"];

			public static readonly string GooFx = ConfigurationSettings.AppSettings["GooFx"];
			public static readonly string GooMesh = ConfigurationSettings.AppSettings["GooMesh"];
			public static readonly string GooColor = ConfigurationSettings.AppSettings["GooColor"];
			public static readonly string GooNormal = ConfigurationSettings.AppSettings["GooNormal"];
			public static readonly string GooHeight = ConfigurationSettings.AppSettings["GooHeight"];

			public static readonly string BgMusic = ConfigurationSettings.AppSettings["BgMusic"];
			public static readonly string SoundExplosion = ConfigurationSettings.AppSettings["SoundExplosion"];
			public static readonly string SoundGun = ConfigurationSettings.AppSettings["SoundGun"];
			public static readonly string SoundStep = ConfigurationSettings.AppSettings["SoundStep"];

			public static readonly string WallFx = ConfigurationSettings.AppSettings["WallFx"];
			public static readonly string WallMesh = ConfigurationSettings.AppSettings["WallMesh"];

			public static readonly string PipeFx = ConfigurationSettings.AppSettings["PipeFx"];
			public static readonly string PipeMesh = ConfigurationSettings.AppSettings["PipeMesh"];
		}

		public class Net
		{
			public static readonly string LocalChannel = GetConfigValue("LocalNet/Info", "LocalChannel");
			public static readonly string LocalIP = GetConfigValue("LocalNet/Info", "LocalIP");
			public static readonly string LocalPort = GetConfigValue("LocalNet/Info", "LocalPort");

			public static readonly string RemoteChannel = GetConfigValue("RemoteNet/Info", "RemoteChannel");
			public static readonly string RemoteURI = GetConfigValue("RemoteNet/Info", "RemoteURI");
		}

		public class Locations
		{
			public static readonly SpriteAccess.LocationMode PlayerOne = StringToLocationMode(GetConfigValue("SpriteLocations/Info", "PlayerOne"));
			public static readonly SpriteAccess.LocationMode PlayerTwo = StringToLocationMode(GetConfigValue("SpriteLocations/Info", "PlayerTwo"));
			public static readonly SpriteAccess.LocationMode Bubbles = StringToLocationMode(GetConfigValue("SpriteLocations/Info", "Bubbles"));
			public static readonly SpriteAccess.LocationMode Platforms = StringToLocationMode(GetConfigValue("SpriteLocations/Info", "Platforms"));
			public static readonly SpriteAccess.LocationMode Polls = StringToLocationMode(GetConfigValue("SpriteLocations/Info", "Polls"));
			public static readonly SpriteAccess.LocationMode Weapons = StringToLocationMode(GetConfigValue("SpriteLocations/Info", "Weapons"));

			private static SpriteAccess.LocationMode StringToLocationMode(string LocationString)
			{
				SpriteAccess.LocationMode RetVal = SpriteAccess.LocationMode.NotSet;
				switch (LocationString.ToLower())
				{
					case("remote"):
						RetVal = SpriteAccess.LocationMode.Remote;
						break;
					case("local"):
						RetVal = SpriteAccess.LocationMode.Local;
						break;
					case("any"):
						RetVal = SpriteAccess.LocationMode.Any;
						break;
					default:
						throw new System.Configuration.ConfigurationException("The config file sections SpriteLocations can only have the values 'Remote', 'Local', and 'Any'");
				}

				return RetVal;
			}

			public static bool IsRemoteGame()
			{
				bool RetVal = false;
				SpriteAccess.LocationMode[] Sprites = {PlayerOne, PlayerTwo, Bubbles, Platforms, Polls, Weapons};

				foreach(SpriteAccess.LocationMode Sprite in Sprites)
				{
					if(Sprite == SpriteAccess.LocationMode.Remote || Sprite == SpriteAccess.LocationMode.Any)
					{
						RetVal = true;
						break;
					}
				}

				return RetVal;
			}
		}

		private static string GetConfigValue(string ConfigSection, string KeyName)
		{
			return (string) ((NameValueCollection)ConfigurationSettings.GetConfig(ConfigSection))[KeyName];
		}

		/// <summary> Checks that the App.config file is valid. Will throw an exception if it is missing keys, or the listed files are missing. </summary>
		public static void ValidateConfigFile()
		{
			ValidateSection(new ValidationDelegate(ValidateFiles), typeof(GameConfig.Files));
			ValidateSection(new ValidationDelegate(ValidateNet), typeof(GameConfig.Net));
		}

		private static void ValidateSection(ValidationDelegate Validation, Type TypeToValidate)
		{
			object o = Activator.CreateInstance(TypeToValidate);
			System.Reflection.FieldInfo[] Fields = o.GetType().GetFields();

			//Validate file paths
			string Value = null;
			string Name = null;
			foreach(System.Reflection.FieldInfo Field in Fields)
			{
				Value = (string) Field.GetValue(o);
				Name = Field.Name;

				Validation(Value, Name);
			}
		}

		private delegate void ValidationDelegate(string Value, string Name);

		private static void ValidateFiles(string Value, string Name)
		{
			if(Value == null)
				throw new System.Configuration.ConfigurationException(string.Format("The config file is missing the field '{0}', or it is spelled wrong.", Name));
			else if(System.IO.File.Exists(Value) == false)
				throw new System.Configuration.ConfigurationException(string.Format("The field in the config file '{0}' references the file '{1}' that does not exist.", Name, Value));
		}

		private static void ValidateNet(string Value, string Name)
		{
			if(Value == null)
				throw new System.Configuration.ConfigurationException(string.Format("The config file is missing the field '{0}', or it is spelled wrong.", Name));
			else if(Value.Length==0 || Value.Trim()=="")
				throw new System.Configuration.ConfigurationException(string.Format("The field in the config file '{0}' is blank.", Name, Value));
		}
	}
}
