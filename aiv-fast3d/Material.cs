using System;
using OpenTK;
using Aiv.Fast2D;

namespace Aiv.Fast3D
{
    public class Material
    {
        public Vector4 DiffuseColor;
        public Texture Diffuse;

        public float Shininess;
        public Texture SpecularMap;

        public Texture NormalMap;

        public Texture EmissiveMap;

        public Light[] Lights;

        public DepthTexture ShadowMap;
        public float ShadowBias;

        public Vector3 Ambient;

        public Material()
        {
            ShadowBias = 0.005f;
            Lights = new Light[8];
        }
    }
}
