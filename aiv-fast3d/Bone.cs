using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Aiv.Fast3D
{
    public class Bone
    {
        public string Name;
        public int Index;

        public Bone Parent;

        public Vector3 BindPosePosition;
        public Vector3 BindPoseScale;
        public Quaternion BindPoseRotation;

        public Vector3 Position;
        public Vector3 Scale;
        public Quaternion Rotation;

        public Bone()
        {
            BindPosePosition = Vector3.Zero;
            BindPoseRotation = Quaternion.Identity;
            BindPoseScale = Vector3.One;
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }

        public Matrix4 LocalMatrix
        {
            get
            {
                return Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position);
            }
        }

        public Matrix4 BindPoseMatrix
        {
            get
            {
                return Matrix4.CreateScale(BindPoseScale) * Matrix4.CreateFromQuaternion(BindPoseRotation) * Matrix4.CreateTranslation(BindPosePosition);
            }
        }

        public Matrix4 GlobalMatrix
        {
            get
            {
                Matrix4 matrix = LocalMatrix;
                Bone parent = Parent;
                while (parent != null)
                {
                    matrix *= parent.LocalMatrix;
                    parent = parent.Parent;
                }
                return matrix;
            }
        }
    }
}
