using System;
using Aiv.Fast2D;
using OpenTK.Graphics.OpenGL;

namespace Aiv.Fast3D
{
	public class DepthTexture : RenderTexture
	{

		public DepthTexture(int width, int height, int depthSize, bool clampToBorder = true) : base(width, height, false, depthSize, true)
		{
			if (clampToBorder)
			{
				Graphics.TextureClampToBorderX(1f, 1f, 1f, 1f);
				Graphics.TextureClampToBorderY(1f, 1f, 1f, 1f);
			}
		}
	}
}
