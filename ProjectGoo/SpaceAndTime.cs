using System;

namespace GameEngine
{
	//Used to convert from 3d to 2d or from 2d to 3d
	public class SpaceAndTime
	{
		public static float PixelToFloat = 0.00865f; // should be the screen width in floats / width in pixels
		public const float SpriteZLocation = -95.0f;
		public const float SecondsPerAnimation = 0.1f;
		private static float TimeLastDraw = 0.0f;

		public static bool TimeToUpdate
		{
			get
			{	
				bool RetVal = false;
				float TimeNow = DXUtil.Timer(DirectXTimer.GetApplicationTime);

				if(TimeNow - SpaceAndTime.TimeLastDraw > SpaceAndTime.SecondsPerAnimation)
				{
					SpaceAndTime.TimeLastDraw = TimeNow;
					RetVal = true;
				}

				return(RetVal);
			}
		}

		/// <summary>
		/// Returns a random float between 1 and 0. This uses
		/// System.Security.Cryptography to get a random seed
		/// that it generates a random number from. This will
		/// assure that all numbers are random even when fetched
		/// on the same system time.
		/// </summary>
		public static float RandomPercent
		{
			get
			{
				System.Security.Cryptography.RandomNumberGenerator Crypt = System.Security.Cryptography.RandomNumberGenerator.Create();
				byte[] RandomCryptByteArray = new byte[8];
				int seed;
				System.Random RandomPicker;
				float NewRandomFloat;
				
				//Get random numbers from the crypt system and turn
				// it into a seed for the random number generator
				Crypt.GetBytes(RandomCryptByteArray);
				seed = System.BitConverter.ToInt16(RandomCryptByteArray, 0);

				//Seed the random number generator and 
				// get a random float betweed 1 and 0
				RandomPicker = new System.Random(seed);
				NewRandomFloat = (float) RandomPicker.NextDouble();

				return(NewRandomFloat);
			}
		}

		public static float LengthFrom2DTo3D(float Length)
		{
			return(SpaceAndTime.PixelToFloat * Length);
		}

		public static Microsoft.DirectX.Matrix ScaleStandard
		{
			get { return(Microsoft.DirectX.Matrix.Scaling(17.0f, 17.0f, 1.0f)); }
		}
	}
}
