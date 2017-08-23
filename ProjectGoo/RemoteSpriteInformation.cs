using System;
using System.Collections;


namespace GameEngine
{
	public interface IRemoteSpriteInformation
	{
		/// <summary> X coordinate </summary>
		float X { set; get; }

		/// <summary> Y coordinate </summary>
		float Y { set; get; }

		/// <summary> Z coordinate </summary>
		float Z { set; get; }

		/// <summary> Frame of animation </summary>
		int Frame { set; get; }

		/// <summary> Direction the sprite is facing </summary>
		bool FaceLeft { set; get; }

		/// <summary> Transmits all the values in a clone copy
		/// This should be transmitted faster than using multiple properties.
		/// </summary>
		object Clone();
	}

	/// <summary> Holds a sprite's information. Is visable to remote and local machines</summary>
	public class SpriteInfoManager : MarshalByRefObject
	{
		/// <summary> Holds an Hashtable of UniqueId:SpriteInformation </summary>
		private Hashtable _RemoteSpriteInfo = new Hashtable();
		
		public void Add(uint UniqueId, SpriteInformation SpriteToAdd)
		{
			this._RemoteSpriteInfo.Add(UniqueId, SpriteToAdd);
		}

		public void Remove(uint UniqueId)
		{
			this._RemoteSpriteInfo.Remove(UniqueId);
		}

		public bool Contains(uint UniqueId)
		{
			return this._RemoteSpriteInfo.Contains(UniqueId);
		}

		public SpriteInformation GetInfoFromId(uint UniqueId)
		{
			SpriteInformation RetVal = null;

			if(Contains(UniqueId) == false)
				throw new Exception(string.Format("The sprite id {0} was not found on the remote machine", UniqueId));

			System.Collections.IDictionaryEnumerator SpriteEnum = this._RemoteSpriteInfo.GetEnumerator();
			while(SpriteEnum.MoveNext())
			{
				if((uint) SpriteEnum.Key == UniqueId)
				{
					RetVal = (SpriteInformation) SpriteEnum.Value;
					break;
				}
			}

			return RetVal;
		}
	}

	/// <summary> Holds sprite information that is sent between machines. </summary>
	public class SpriteInformation : MarshalByRefObject , IRemoteSpriteInformation, ICloneable
	{
		private float _X;
		private float _Y;
		private float _Z;
		private int _Frame;
		private bool _FaceLeft;

		public float X
		{
			set{this._X = value;}
			get{return(this._X);}
		}

		public float Y
		{
			set{this._Y = value;}
			get{return(this._Y);}
		}

		public float Z
		{
			set{this._Z = value;}
			get{return(this._Z);}
		}

		public int Frame
		{
			set{this._Frame = value;}
			get{return(this._Frame);}
		}

		public bool FaceLeft
		{
			set{this._FaceLeft = value;}
			get{return(this._FaceLeft);}
		}

		#region ICloneable Members

		public object Clone()
		{
			SpriteInformation NewSpriteInfo = new SpriteInformation();
			NewSpriteInfo._X = this._X;
			NewSpriteInfo._Y = this._Y;
			NewSpriteInfo._Z = this._Z;
			NewSpriteInfo._Frame =  this._Frame;
			NewSpriteInfo._FaceLeft = this._FaceLeft;
			
			return NewSpriteInfo;
		}

		#endregion
	}
}