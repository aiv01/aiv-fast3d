using System;
using OpenTK;
using Aiv.Fast2D;

namespace Aiv.Fast3D
{
    public class PointLight : Light
    {

        public Vector3 Position;
        public override Vector3 Vector
        {
            get
            {
                return Position;
            }
        }

        public float Radius;
        public override float Strength
        {
            get
            {
                return Radius;
            }
        }


        public PointLight(Vector3 position, float radius)
        {
            this.Position = position;
            this.Radius = radius;
        }

    }
}