using System;
using Aiv.Fast2D;
using OpenTK.Graphics.OpenGL;

namespace Aiv.Fast3D
{
	public class DepthTexture : RenderTexture
	{
		
		public DepthTexture(int width, int height, int depthSize) : base(width, height)
		{
			this.textureId = Graphics.NewTexture();
			this.Bind();
			// fix texture type
			Graphics.TextureDepth(width, height, depthSize);
			// this leaks a framebuffer object, but unfortunately
			// it is the best solution for now
			frameBuffer = Graphics.NewFrameBuffer();
			Graphics.BindFrameBuffer(frameBuffer);

			Graphics.FrameBufferTextureDepth(this.Id);
			this.SetNearest();
			this.SetRepeatX(false);
			this.SetRepeatY(false);

			Graphics.BindFrameBuffer(Graphics.GetDefaultFrameBuffer());
		}
	}
}
