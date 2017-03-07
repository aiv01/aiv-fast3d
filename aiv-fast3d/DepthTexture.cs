using System;
using Aiv.Fast2D;
using OpenTK.Graphics.OpenGL;

namespace Aiv.Fast3D
{
	public class DepthTexture : RenderTexture
	{
		
		public DepthTexture(int width, int height, int depthSize) : base(width, height)
		{
			
			// this leaks a framebuffer object, but unfortunately
			// it is the best solution for now
			frameBuffer = Graphics.NewFrameBuffer();
			Graphics.BindFrameBuffer(frameBuffer);
			// fix texture type
			Graphics.TextureDepth(width, height, depthSize);
			Graphics.FrameBufferTextureDepth(this.Id);

			Graphics.BindFrameBuffer(Graphics.GetDefaultFrameBuffer());
		}
	}
}
