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
			Console.WriteLine(axis);
			//Quaternion q = Quaternion.FromEulerAngles(axis);
			//Quaternion q = Quaternion.FromAxisAngle(new Vector3(1, 0, 0), -60 * ((float)Math.PI / 180f));
			Quaternion quat = Quaternion.FromEulerAngles(axis);
			Console.WriteLine(QuaternionToEulerRotation(quat));
			Vector3 v = quat * Vector3.UnitZ;
			return v;
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

		// taken from Unity
		public static Quaternion LookAt(Vector3 forward, Vector3 up)
		{
			forward = Vector3.Normalize(forward);
			Vector3 right = Vector3.Normalize(Vector3.Cross(up, forward));
			up = Vector3.Cross(forward, right);
			var m00 = right.X;
			var m01 = right.Y;
			var m02 = right.Z;
			var m10 = up.X;
			var m11 = up.Y;
			var m12 = up.Z;
			var m20 = forward.X;
			var m21 = forward.Y;
			var m22 = forward.Z;


			float num8 = (m00 + m11) + m22;
			var quaternion = new Quaternion();
			if (num8 > 0f)
			{
				var num = (float)Math.Sqrt(num8 + 1f);
				quaternion.W = num * 0.5f;
				num = 0.5f / num;
				quaternion.X = (m12 - m21) * num;
				quaternion.Y = (m20 - m02) * num;
				quaternion.Z = (m01 - m10) * num;
				return quaternion;
			}
			if ((m00 >= m11) && (m00 >= m22))
			{
				var num7 = (float)Math.Sqrt(((1f + m00) - m11) - m22);
				var num4 = 0.5f / num7;
				quaternion.X = 0.5f * num7;
				quaternion.Y = (m01 + m10) * num4;
				quaternion.Z = (m02 + m20) * num4;
				quaternion.W = (m12 - m21) * num4;
				return quaternion;
			}
			if (m11 > m22)
			{
				var num6 = (float)Math.Sqrt(((1f + m11) - m00) - m22);
				var num3 = 0.5f / num6;
				quaternion.X = (m10 + m01) * num3;
				quaternion.Y = 0.5f * num6;
				quaternion.Z = (m21 + m12) * num3;
				quaternion.W = (m20 - m02) * num3;
				return quaternion;
			}
			var num5 = (float)Math.Sqrt(((1f + m22) - m00) - m11);
			var num2 = 0.5f / num5;
			quaternion.X = (m20 + m02) * num2;
			quaternion.Y = (m21 + m12) * num2;
			quaternion.Z = 0.5f * num5;
			quaternion.W = (m01 - m10) * num2;
			return quaternion;
		}

		public static Quaternion LookAt(Vector3 direction)
		{
			return LookAt(direction, Vector3.UnitY);
		}
	}
}
