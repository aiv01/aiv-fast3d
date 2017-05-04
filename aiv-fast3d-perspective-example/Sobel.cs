using Aiv.Fast2D;

namespace Aiv.Fast3D.Perspective.Example
{

	public class Sobel : PostProcessingEffect
	{
		private static string fragmentShader = @"
#version 330 core
precision highp float;
in vec2 uv;
uniform sampler2D tex;
out vec4 color;

uniform float width;
uniform float height;
uniform float multiplier;

void main() {
	// get the size of a fragment unit
	float x = 1.0 / width;
	float y = 1.0 / height;

	

	vec4 horizontal = vec4(0.0);

	// left bottom
	horizontal -= texture(tex, vec2(uv.x - x, uv.y - y));
	
	// left * 2
	horizontal -= texture(tex, vec2(uv.x - x, uv.y)) * 2.0;

	// left top
	horizontal -= texture(tex, vec2(uv.x - x, uv.y + y));

	// right bottom
	horizontal += texture(tex, vec2( uv.x + x, uv.y - y));

	// right * 2
	horizontal += texture(tex, vec2( uv.x + x, uv.y)) * 2.0;

	// right top
	horizontal += texture(tex, vec2( uv.x + x, uv.y + y));



	vec4 vertical = vec4(0.0);

	// left bottom
	vertical -= texture(tex, vec2( uv.x - x, uv.y - y));

	// bottom * 2
	vertical -= texture(tex, vec2( uv.x, uv.y - y)) * 2.0;

	// right bottom
	vertical -= texture(tex, vec2( uv.x + x, uv.y - y));

	// left top
	vertical += texture(tex, vec2( uv.x - x, uv.y + y));

	// top * 2
	vertical += texture(tex, vec2( uv.x, uv.y + y)) * 2.0;

	// right top
	vertical += texture(tex, vec2( uv.x + x, uv.y + y));


	// pythagora
	vec3 edge = sqrt((horizontal.rgb * horizontal.rgb) + (vertical.rgb * vertical.rgb));

	vec4 orig_color = texture( tex, uv );

	color = vec4(orig_color.rgb - (edge.rgb * multiplier), orig_color.a );

}
";

		public Sobel() : base(fragmentShader, null, true)
		{

		}

		public override void Update(Window window)
		{
			screenMesh.shader.SetUniform("width", (float)window.ScaledWidth);
			screenMesh.shader.SetUniform("height", (float)window.ScaledHeight);
			screenMesh.shader.SetUniform("multiplier", 0.15f);
		}
	}

}
