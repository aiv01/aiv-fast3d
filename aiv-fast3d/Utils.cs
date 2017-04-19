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

		// taken from wikipedia
		public static Vector3 QuaternionToRotation(OpenTK.Quaternion q)
		{
			double ySqr = q.Y * q.Y;

			// pitch
			double t2 = +2.0 * (q.W * q.Y - q.Z * q.X);
			t2 = t2 > 1.0 ? 1.0 : t2;
			t2 = t2 < -1.0 ? -1.0 : t2;
			float pitch = (float)Math.Asin(t2);

			// yaw
			double t0 = +2.0 * (q.W * q.X + q.Y * q.Z);
			double t1 = +1.0 - 2.0 * (q.X * q.X + ySqr);
			float yaw = (float)Math.Atan2(t0, t1);

			// roll
			double t3 = +2.0 * (q.W * q.Z + q.X * q.Y);
			double t4 = +1.0 - 2.0 * (ySqr + q.Z * q.Z);
			float roll = (float)Math.Atan2(t3, t4);

			return new Vector3(pitch, yaw, roll);
		}
	}
}
