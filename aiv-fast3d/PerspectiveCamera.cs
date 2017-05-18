using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D;
using OpenTK;

namespace Aiv.Fast3D
{
	public class PerspectiveCamera : Camera
	{

		private Vector3 position3;
		public Vector3 Position3
		{
			get
			{
				return this.position3;
			}
			set
			{
				this.position3 = value;
			}
		}

		public Vector3 Forward
		{
			get
			{
				return Quaternion * Vector3.UnitZ;
			}
		}

		public Vector3 Right
		{
			get
			{
				return Vector3.Cross(Forward, Up);
			}
		}

		public Vector3 Up
		{
			get
			{
				return Quaternion * Vector3.UnitY;
			}
		}

		private Quaternion internalRotation;
        public Quaternion Quaternion
		{
			get
			{
                return internalRotation;
			}
            set
            {
                internalRotation = value;
            }
		}

        public Vector3 Rotation3
        {
            set
            {
                internalRotation = (Matrix3.CreateRotationX(value.X) * Matrix3.CreateRotationZ(value.Z) * Matrix3.CreateRotationY(value.Y)).ExtractRotation();
            }
        }

        public Vector3 EulerRotation3
        {
            set
            {
                Rotation3 = value * (float)Math.PI / 180f;
            }
        }

        private float fov;
		private float zNear;
		private float zFar;
		private float aspectRatio;

		public override bool HasProjection
		{
			get
			{
				return true;
			}
		}

		public PerspectiveCamera(Vector3 position, Vector3 eulerRotation, float fov, float zNear, float zFar, float aspectRatio = 0)
		{
			this.position3 = position;
            this.SetEulerRotation(eulerRotation);
			this.fov = fov * (float)(Math.PI / 180f);
			this.zNear = zNear;
			this.zFar = zFar;
			this.aspectRatio = aspectRatio;
			if (this.aspectRatio == 0)
			{
				this.aspectRatio = Window.Current.aspectRatio;
			}
		}

		public override Matrix4 Matrix()
		{
			return Matrix4.LookAt(position3, position3 + this.Forward, this.Up);

		}

		public override Matrix4 ProjectionMatrix()
		{
			float fovY = fov / aspectRatio;
			return Matrix4.CreatePerspectiveFieldOfView(fovY, aspectRatio, zNear, zFar);
		}

        public void SetEulerRotation(Vector3 pitchYawRoll)
        {
            float degToRad = (float)Math.PI / 180f;
            pitchYawRoll *= degToRad;
            internalRotation = (Matrix3.CreateRotationX(pitchYawRoll.X) * Matrix3.CreateRotationZ(pitchYawRoll.Z) * Matrix3.CreateRotationY(pitchYawRoll.Y)).ExtractRotation();
        }

        public void SetEulerRotation(float pitch, float yaw, float roll)
        {
            float degToRad = (float)Math.PI / 180f;
            internalRotation = (Matrix3.CreateRotationX(pitch * degToRad) * Matrix3.CreateRotationZ(roll * degToRad) * Matrix3.CreateRotationY(yaw * degToRad)).ExtractRotation();
        }

        public void SetRotation(Vector3 pitchYawRoll)
        {
            internalRotation = (Matrix3.CreateRotationX(pitchYawRoll.X) * Matrix3.CreateRotationZ(pitchYawRoll.Z) * Matrix3.CreateRotationY(pitchYawRoll.Y)).ExtractRotation();
        }

        public void SetRotation(float pitch, float yaw, float roll)
        {
            internalRotation = (Matrix3.CreateRotationX(pitch) * Matrix3.CreateRotationZ(roll) * Matrix3.CreateRotationY(yaw)).ExtractRotation();
        }
	}
}
