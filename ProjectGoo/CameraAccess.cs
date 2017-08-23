using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;
using Direct3D = Microsoft.DirectX.Direct3D;


namespace GameEngine
{
	public class CameraAccess
	{
		private Direct3D.Device ParentScreen = null;
		private float AngleX = 0f;
		private float AngleY = 0f;
		private float AngleZ = 0f;
		private float NearRange = 0f;
		private float FarRange = 0f;
		private float ViewAngle = 0f;
		private float AspectRatio = 0f;
		private Vector3 Position;
		private Vector3 Target;
		private Vector3 UpVector;
		public Matrix ProjectionMatrix;
		public Matrix ViewMatrix;

		public CameraAccess(Direct3D.Device NewParentScreen, float NewAspectRatio, float NewViewAngle, float NewNearRange, float NewFarRange)
		{
			if (NewParentScreen == null)
				throw new Exception("There is no device to set the camera on!");

			this.ParentScreen = NewParentScreen;
			this.NearRange = NewNearRange;
			this.FarRange = NewFarRange;
			this.ViewAngle = NewViewAngle;
			this.AspectRatio = NewAspectRatio;
			this.Position = new Vector3(0, 0, 5.0f);
			this.Target = new Vector3();
			this.UpVector = new Vector3(0, 1, 0);

			this.CreateViewPoint();
		}

		public void CreateViewPoint(float NewAspectRatio)
		{
			this.AspectRatio = NewAspectRatio;
			this.CreateViewPoint();
		}

		private void CreateViewPoint()
		{
			// Set Camera Angle
			ViewMatrix = Matrix.LookAtLH(this.Position, this.Target, this.UpVector);
			ProjectionMatrix = Matrix.PerspectiveFovLH(this.ViewAngle, this.AspectRatio, this.NearRange, this.FarRange);
			this.ParentScreen.Transform.View = ViewMatrix;
			this.ParentScreen.Transform.Projection = ProjectionMatrix; 			
		}

		public void Move(Direct3D.Device CurrDevice, float NewX, float NewY, float NewZ)
		{
			this.AngleX += NewX;
			this.AngleY += NewY;
			this.AngleZ += NewZ;
			CurrDevice.Transform.World = Matrix.Translation(-5, -10 * 1 / 3, 0) * Matrix.RotationX(AngleX) * Matrix.RotationY(AngleY) * Matrix.RotationZ(AngleZ);
		}
	}
}
