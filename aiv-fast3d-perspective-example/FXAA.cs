using Aiv.Fast2D;

namespace Aiv.Fast3D.Perspective.Example
{

	public class FXAA : PostProcessingEffect
	{
		private static string fragmentShader = @"
#version 330 core
precision highp float;
in vec2 uv;
uniform sampler2D tex;
uniform sampler2D depth_tex;
out vec4 color;

uniform float width;
uniform float height;
uniform float fxaa_span_max;
uniform float fxaa_reduce_mul;
uniform float fxaa_reduce_min;

void main() {
	// get the size of a fragment unit
	float x = 1.0 / width;
	float y = 1.0 / height;

	// get diagonal neighbour pixels
	vec4 orig_color = texture(tex, uv);

	vec3 rgb_center = orig_color.rgb;
	vec3 rgb_top_right = texture(tex, uv + vec2(x, y)).rgb;
	vec3 rgb_top_left = texture(tex, uv + vec2(-x, y)).rgb;
	vec3 rgb_bottom_right = texture(tex, uv + vec2(x, -y)).rgb;
	vec3 rgb_bottom_left = texture(tex, uv + vec2(-x, -y)).rgb;

	// calculate luminance factor of each pixel
	vec3 luma = vec3(0.299, 0.587, 0.114);
	float luma_center = dot(rgb_center, luma);
	float luma_top_right = dot(rgb_top_right, luma);
	float luma_top_left = dot(rgb_top_left, luma);
	float luma_bottom_right = dot(rgb_bottom_right, luma);
	float luma_bottom_left = dot(rgb_bottom_left, luma);

	// get min and max luma values
	float luma_min = min(luma_center, min(min(luma_top_right, luma_top_right), min(luma_bottom_right, luma_bottom_left)));
	float luma_max = max(luma_center, max(max(luma_top_right, luma_top_right), max(luma_bottom_right, luma_bottom_left)));


	vec2 dir = vec2(-((luma_top_right + luma_top_left) - (luma_bottom_right + luma_bottom_left)), (luma_top_right + luma_bottom_right) - (luma_top_left + luma_bottom_left));

	float dir_reduce = max((luma_top_right + luma_top_left + luma_bottom_right + luma_bottom_left) * (0.25 * fxaa_reduce_mul), fxaa_reduce_min);

	float rcp_dir_min = 1.0 / (min(abs(dir.x), abs(dir.y)) + dir_reduce);

	dir = min(vec2(fxaa_span_max, fxaa_span_max), max(vec2(-fxaa_span_max, -fxaa_span_max), dir * rcp_dir_min)) * vec2(x, y);

	vec3 rgb_first_item0 = texture(tex, uv + dir * (1.0 / 3.0 - 0.5)).rgb;
	vec3 rgb_first_item1 = texture(tex, uv + dir * (2.0 / 3.0 - 0.5)).rgb;
	vec3 rgb_first = 0.5 * (rgb_first_item0 + rgb_first_item1);

	vec3 rgb_second_item0 = texture(tex, uv + dir * - 0.5).rgb;
	vec3 rgb_second_item1 = texture(tex, uv + dir * 0.5).rgb;
	vec3 rgb_second = rgb_first * 0.5 + 0.25 * (rgb_second_item0 + rgb_second_item1);

	float luma_final = dot(rgb_second, luma);

	if (luma_final < luma_min || luma_final > luma_max) {
		color = vec4(rgb_first, 1);//orig_color.a);
	}
	else {
		color = vec4(rgb_second, 1);//orig_color.a);
	}
}
";

		public FXAA() : base(fragmentShader, null, true)
		{
		}

		public override void Update(Window window)
		{
			screenMesh.shader.SetUniform("width", (float)window.ScaledWidth);
			screenMesh.shader.SetUniform("height", (float)window.ScaledHeight);
			screenMesh.shader.SetUniform("fxaa_span_max", 8.0f);
			screenMesh.shader.SetUniform("fxaa_reduce_mul", 1.0f / 8.0f);
			screenMesh.shader.SetUniform("fxaa_reduce_min", 1.0f / 128.0f);
		}
	}

}
