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
				return (rotationMatrix * new Vector4(-Vector3.UnitZ)).Xyz;
			}
		}

		public Vector3 Right
		{
			get
			{
				return (rotationMatrix * new Vector4(-Vector3.UnitX)).Xyz;
			}
		}

		public Vector3 Up
		{
			get
			{
				return (rotationMatrix * new Vector4(-Vector3.UnitY)).Xyz;
			}
		}

		private Vector3 rotation3;
		public Vector3 EulerRotation3
		{
			get
			{
				return rotation3 * (float)(180f / Math.PI);
			}
			set
			{
				rotation3 = value * (float)(Math.PI / 180f);
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

		public PerspectiveCamera(Vector3 position, Vector3 rotation, float fov, float zNear, float zFar, float aspectRatio = 0)
		{
			this.position3 = position;
			this.EulerRotation3 = rotation;
			this.fov = fov * (float)(Math.PI / 180f);
			this.zNear = zNear;
			this.zFar = zFar;
			this.aspectRatio = aspectRatio;
			if (this.aspectRatio == 0)
			{
				this.aspectRatio = Window.Current.aspectRatio;
			}
		}

		private Matrix4 rotationMatrix
		{
			get
			{
				return Matrix4.CreateRotationX(rotation3.X) *
					Matrix4.CreateRotationY(rotation3.Y) *
					Matrix4.CreateRotationZ(rotation3.Z);
			}
		}

		public override Matrix4 Matrix()
		{
			return Matrix4.LookAt(position3, position3 + this.Forward, Vector3.UnitY);

		}

		public override Matrix4 ProjectionMatrix()
		{
			float fovY = fov / aspectRatio;
			return Matrix4.CreatePerspectiveFieldOfView(fovY, aspectRatio, zNear, zFar);
		}
	}
}
