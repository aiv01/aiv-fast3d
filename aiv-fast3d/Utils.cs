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
            Quaternion quat = (Matrix3.CreateRotationZ(axis.Z) * Matrix3.CreateRotationY(axis.Y) * Matrix3.CreateRotationX(axis.X)).ExtractRotation();
            Vector3 v = quat * Vector3.UnitZ;
            return v;
        }

        // taken from Unity
        public static Vector3 LookAt(Vector3 forward, Vector3 up)
        {
            float pitch = 0;
            float yaw = (float)Math.Atan2(forward.X, forward.Z);
            float roll = 0;

            return new Vector3(pitch, yaw, roll);
        }

        public static float GetPitchFromDirection(Vector3 forward)
        {
            return (float)Math.Asin(-forward.Y);
        }

        public static float GetYawFromDirection(Vector3 forward)
        {
            return (float)Math.Atan2(forward.X, forward.Z);
        }

        public static Vector3 LookAt(Vector3 direction)
        {
            return LookAt(direction, Vector3.UnitY);
        }

        // from http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/index.htm
        public static Vector3 QuaternionToPitchYawRoll(Quaternion q)
        {
            float yaw, pitch, roll;
            float test = q.X * q.Y + q.Z * q.W;

            // 90 deg roll singularity
            if (test > 0.499f)
            {
                yaw = 2f * (float)Math.Atan2(q.X, q.W);
                roll = MathHelper.PiOver2;
                pitch = 0f;
            }
            // -90 deg roll singularity
            if (test < -0.499f)
            {
                yaw = -2f * (float)Math.Atan2(q.X, q.W);
                roll = -MathHelper.PiOver2;
                pitch = 0f;
            }
            else
            {
                float sqx = q.X * q.X;
                float sqy = q.Y * q.Y;
                float sqz = q.Z * q.Z;
                yaw = (float)Math.Atan2(2 * q.Y * q.W - 2 * q.X * q.Z, 1 - 2 * sqy - 2 * sqz);
                roll = (float)Math.Asin(2 * test);
                pitch = (float)Math.Atan2(2 * q.X * q.W - 2 * q.Y * q.Z, 1 - 2 * sqx - 2 * sqz);
            }

            return new Vector3(pitch, yaw, roll);
        }
    }
}
