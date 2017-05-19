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
    }
}
