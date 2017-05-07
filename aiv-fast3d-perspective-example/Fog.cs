using Aiv.Fast2D;
using OpenTK;

namespace Aiv.Fast3D.Perspective.Example
{

	public class Fog : PostProcessingEffect
	{
		private static string fragmentShader = @"
#version 330 core
precision highp float;
in vec2 uv;
uniform sampler2D tex;
uniform sampler2D depth_tex;
out vec4 color;

uniform float start;
uniform float end;

float znear = 0.01;
float zfar = 1000; 

float fog_linear(float z) {
	return clamp((end - z) / (end - start), 0.0, 1.0);
}

float fog_exp(float z, float density) {
	return 1.0 - clamp(exp(-density * z), 0.0, 1.0);
}

float fog_exp2(float z, float density) {
	const float l2 = -1.442695;
	float d = density * z;
  	return 1.0 - clamp(exp2(d * d * l2), 0.0, 1.0);
}

float get_distance_by_depth() {
	// [-1, 1] to [0, 1]
	float depth = 2.0 * texture(depth_tex, uv).r - 1.0;
	return 2.0 * znear * zfar / (zfar + znear - depth * (zfar - znear));
}

void main() {
	float distance = get_distance_by_depth();

	vec4 orig_color = texture(tex, uv);
	float fog_amount = fog_exp2(distance, 0.075);

	// poison
	//vec3 fog_color = vec3(0.1, 0.5, 0);

	// snow
	vec3 fog_color = vec3(1, 1, 1);

	// cold fog
	//color = vec4(mix(orig_color.rgb, fog_color, fog_amount), orig_color.a);

	// a dark-horror fog
	color = vec4(orig_color.rgb * fog_linear(distance), orig_color.a);
}
";


		public Fog() : base(fragmentShader, null, true, 32)
		{

		}

		public override void Update(Window window)
		{
			screenMesh.shader.SetUniform("start", 10f);
			screenMesh.shader.SetUniform("end", 30f);
		}
	}

}
