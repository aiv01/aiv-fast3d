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

        public virtual float Strength
        {
            get
            {
                return 0;
            }
        }

        public virtual Matrix4 ShadowProjection
        {
            get
            {
                return Matrix4.Identity;
            }
        }

        protected Vector3 color;
        public Vector3 Color
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
            color = Vector3.One;
        }
    }
}
