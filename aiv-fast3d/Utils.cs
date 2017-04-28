using System;
using OpenTK;

namespace Aiv.Fast3D
{
	public static class Utils
	{
		public static Vector3 EulerRotationToDirection(Vector3 axis)
		{
			// convert to radians
			return RotationToDirection(axis * (float)(Math.PI / 180f));
		}


		public static Vector3 RotationToDirection(Vector3 axis)
		{
			Quaternion q = Quaternion.FromEulerAngles(axis);
			return (q * Vector3.UnitZ).Normalized();
		}

		public static Vector3 QuaternionToRotation(OpenTK.Quaternion q)
		{
			Vector3 v = Vector3.Zero;

			v.Y = (float)Math.Atan2
						(
							2 * q.Y * q.W - 2 * q.X * q.Z,
							   1 - 2 * Math.Pow(q.Y, 2) - 2 * Math.Pow(q.Z, 2)
						);

			v.X = (float)Math.Asin
			(
				2 * q.X * q.Y + 2 * q.Z * q.W

			);

			v.Z = (float)Math.Atan2
						(
							2 * q.X * q.W - 2 * q.Y * q.Z,
							1 - 2 * Math.Pow(q.X, 2) - 2 * Math.Pow(q.Z, 2)
						);

			if (q.X * q.Y + q.Z * q.W == 0.5)
			{
				v.Y = (float)(2 * Math.Atan2(q.X, q.W));
				v.Z = 0;
			}

			else if (q.X * q.Y + q.Z * q.W == -0.5)
			{
				v.Y = (float)(-2 * Math.Atan2(q.X, q.W));
				v.Z = 0;
			}

			return v;
		}

		public static Vector3 QuaternionToEulerRotation(OpenTK.Quaternion q)
		{
			return QuaternionToRotation(q) * 180f / (float)Math.PI;
		}

	}
}
