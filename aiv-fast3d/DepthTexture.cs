using System;
using Aiv.Fast2D;
using OpenTK.Graphics.OpenGL;

namespace Aiv.Fast3D
{
    public class DepthTexture : RenderTexture
    {
        //public bool Flipped { get; set; }
        /*
        public void Bind(int unit = 0)
        {   
            Graphics.BindTextureToUnit(TextureId, unit);
        }
        */

        public virtual float[] DownloadFloat(int mipMap = 0)
        {
            float[] data = new float[this.Width * this.Height * sizeof(float)];
            Bind();
            Graphics.BindTextureToUnit(TextureId, 0);
            Graphics.DepthTextureGetPixels(-1, mipMap, data);
            return data;
        }

        public DepthTexture(int width, int height, int depthSize, bool clampToBorder = true)
            : base(width, height, false, depthSize, true)
        {
            if (clampToBorder)
            {
                Graphics.TextureClampToBorderX(1f, 1f, 1f, 1f);
                Graphics.TextureClampToBorderY(1f, 1f, 1f, 1f);
            }
            //Flipped = true;
        }
    }
}
