using System;
using OpenTK;

namespace Aiv.Fast3D
{
	public class DirectionalLight : Light
	{

		private Vector3 direction;
		public override Vector3 Vector
		{
			get
			{
				return direction;
			}
		}

		private Matrix4 m;

		public override Matrix4 ShadowProjection
		{
			get
			{
				return m * ortho;
			}
		}

		public DirectionalLight(Vector3 direction)
		{
			UpdateDirection(direction);
		}

		public void UpdateDirection(Vector3 direction)
		{
			this.direction = direction;
			m = Matrix4.LookAt(Vector3.Zero, direction, Vector3.UnitY);
		}

		private Matrix4 ortho;
		public void SetShadowProjection(float left, float right, float bottom, float top, float zNear, float zFar)
		{
			ortho = Matrix4.CreateOrthographicOffCenter(left, right, bottom, top, zNear, zFar);
		}
	}
}
