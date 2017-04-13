using System;
using OpenTK;

namespace Aiv.Fast3D
{
	public static class Utils
	{
		public static Vector3 EulerRotationToDirection(Vector3 axis)
		{
			// convert to radians
			axis *= (float)(Math.PI / 180f);
			Matrix4 m = Matrix4.CreateRotationX(axis.X) *
							   Matrix4.CreateRotationY(axis.Y) *
							   Matrix4.CreateRotationZ(axis.Z);

			return (m * new Vector4(Vector3.UnitZ, 0)).Xyz.Normalized();
		}

		public static Vector3 RotationToDirection(Vector3 axis)
		{
			Matrix4 m = Matrix4.CreateRotationX(axis.X) *
							   Matrix4.CreateRotationY(axis.Y) *
							   Matrix4.CreateRotationZ(axis.Z);

			return (m * new Vector4(Vector3.UnitZ, 0)).Xyz.Normalized();
		}

	}
}
