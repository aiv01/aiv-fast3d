using Aiv.Fast2D;
using OpenTK;

namespace Aiv.Fast3D.Example.Perspective
{

	public class DepthViewer : PostProcessingEffect
	{
		private static string fragmentShader = @"
#version 330 core
precision highp float;
in vec2 uv;
uniform sampler2D tex;
uniform sampler2D depth_tex;
out vec4 color;


float zfar = 100;
float znear = 0.01;

float linearize(float depth)
{
	return (2.0 * znear) / (zfar + znear - depth * (zfar - znear));
}

void main() {
	float depth = linearize(texture(depth_tex, uv).r);

	color = vec4(depth, depth, depth, 1);
}
";


		public DepthViewer() : base(fragmentShader, null, true, 32)
		{
			
		}
	}

}
