using System;
using OpenTK;
namespace Aiv.Fast3D
{
	public class Light
	{

		public virtual Vector3 Vector
		{
			get
			{
				return Vector3.Zero;
			}
		}

		public virtual Matrix4 ShadowProjection
		{
			get
			{
				return Matrix4.Identity;
			}
		}

		protected Vector4 color;
		public Vector4 Color
		{
			get
			{
				return color;
			}
			set
			{
				color = value;
			}
		}

		public Light()
		{
		}
	}
}
