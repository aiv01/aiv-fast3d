using System;
using Aiv.Fast2D;
using OpenTK;

namespace Aiv.Fast3D
{
	public class Mesh3 : Mesh
	{
		private static string simpleVertexShader3 = @"
#version 330 core
layout(location = 0) in vec3 vertex;
layout(location = 1) in vec2 uv;
layout(location = 2) in vec4 vc;
layout(location = 3) in vec3 vn;


uniform mat4 mvp;

uniform mat4 mv;

uniform mat4 view;

uniform vec4 directional_light;

uniform float use_gouraud;

out vec2 uvout;
out vec4 vertex_color;
out vec3 vertex_normal;

out float lambert;

void main(){
        gl_Position = mvp * vec4(vertex.xyz, 1.0);
		
        uvout = uv;
        vertex_color = vc;
		vertex_normal = vn;

		if (use_gouraud > 0.0) {
			// compute the light position in camera space
			vec3 light_position = (view * directional_light).xyz;
			// compute the normal in camera space
			vec3 normal_for_view = normalize(mv * vec4(vn.xyz, 0)).xyz;
			// compute the vertex in camera space
			vec3 vertex_position = (mv * vec4(vertex.xyz, 1.0)).xyz;
			// get the direction from vertex to light
			vec3 light_direction = normalize(light_position - vertex_position);
			lambert = clamp(dot(normal_for_view, light_direction), 0.0, 1.0);
		}
}";
		private static string simpleFragmentShader3 = @"
#version 330 core

precision highp float;

uniform vec4 color;

uniform float use_texture;
uniform float use_wireframe;

uniform sampler2D tex;

uniform float use_gouraud;

uniform vec4 ambient;

in vec2 uvout;
in vec4 vertex_color;
in float lambert;

out vec4 out_color;

void main(){
    if (use_texture > 0.0) {
		out_color = texture(tex, uvout);
        out_color += vec4(vertex_color.xyz * out_color.a, vertex_color.a);
    }
    else if (use_wireframe > 0.0) {
        if(any(lessThan(vertex_color.xyz, vec3(use_wireframe)))) {
            out_color = color;
        }
        else {
            out_color = vec4(0, 0, 0, 0);    
        }
        return;
    }
    else {
        out_color = vertex_color;
    }
    out_color += color;

	if (use_gouraud > 0.0) {
		out_color = vec4(out_color.xyz * lambert, out_color.w);
	}
}";

		private static string simpleVertexShaderObsolete3 = @"
attribute vec3 vertex;
attribute vec2 uv;
attribute vec4 vc;
uniform mat4 mvp;
uniform vec3 directional_light;
varying vec2 uvout;
varying vec4 vertex_color;
varying vec3 vnout;
void main(){
        gl_Position = mvp * vec4(vertex.xyz, 1.0);
        uvout = uv;
        vertex_color = vc;
}";
		private static string simpleFragmentShaderObsolete3 = @"
precision mediump float;
uniform vec4 color;
uniform float use_texture;
uniform float use_wireframe;
uniform sampler2D tex;
varying vec2 uvout;
varying vec4 vertex_color;
void main(){
    if (use_texture > 0.0) {
        gl_FragColor = texture2D(tex, uvout);
        gl_FragColor += vec4(vertex_color.xyz * gl_FragColor.a, vertex_color.a);
    }
    else if (use_wireframe > 0.0) {
        if(any(lessThan(vertex_color.xyz, vec3(use_wireframe)))) {
            gl_FragColor = color;
        }
        else {
            gl_FragColor = vec4(0, 0, 0, 0);    
        }
        return;
    }
    else {
        gl_FragColor = vertex_color;
    }
    gl_FragColor += color;
}";

		private static Shader simpleShader3 = new Shader(simpleVertexShader3, simpleFragmentShader3, simpleVertexShaderObsolete3, simpleFragmentShaderObsolete3, new string[] { "vertex", "uv", "vc", "vn" });

		private Vector3 rotation3;
		public Vector3 Rotation3
		{
			get
			{
				return rotation3;
			}
			set
			{
				rotation3 = value;
			}
		}

		private Vector3 position3;
		public Vector3 Position3
		{
			get
			{
				return position3;
			}
			set
			{
				position3 = value;
			}
		}

		private Vector3 scale3;
		public Vector3 Scale3
		{
			get
			{
				return scale3;
			}
			set
			{
				scale3 = value;
			}
		}

		private Vector3 pivot3;
		public Vector3 Pivot3
		{
			get
			{
				return pivot3;
			}
			set
			{
				pivot3 = value;
			}
		}

		public Vector3 EulerRotation3
		{
			get
			{
				return this.rotation3 * 180f / (float)Math.PI;
			}
			set
			{
				this.rotation3 = value * (float)Math.PI / 180f;
			}
		}

		public float[] vn;
		private int vnBufferId;

		public Mesh3() : base(simpleShader3, 3)
		{

			scale3 = Vector3.One;
			position3 = Vector3.Zero;
			rotation3 = Vector3.Zero;

			this.vnBufferId = Graphics.NewBuffer();
			Graphics.MapBufferToArray(this.vnBufferId, 3, 3);

			// ensure normals are loaded
			this.shaderSetupHook += (mesh) =>
			{
				if (((Mesh3)mesh).vn == null)
				{
					this.vn = new float[this.v.Length];
					((Mesh3)mesh).UpdateNormals();
				}
			};
		}

		public void UpdateNormals()
		{
			if (this.vn == null)
				return;
			Graphics.BufferData(this.vnBufferId, this.vn);
		}


		protected override void ApplyMatrix()
		{
			if (this.noMatrix)
				return;

			// WARNING !!! OpenTK uses row-major while OpenGL uses column-major
			Matrix4 m =
				Matrix4.CreateTranslation(-this.pivot3.X, -this.pivot3.Y, -this.pivot3.Z) *
#if !__MOBILE__
				Matrix4.CreateScale(this.scale3.X, this.scale3.Y, this.scale3.Z) *
#else
                Matrix4.Scale(this.scale3.X, this.scale3.Y, this.scale3.Z) *
#endif
				Matrix4.CreateRotationX(this.rotation3.X) *
				Matrix4.CreateRotationY(this.rotation3.Y) *
				Matrix4.CreateRotationZ(this.rotation3.Z) *
				// here we do not re-add the pivot, so translation is pivot based too
				Matrix4.CreateTranslation(this.position3.X, this.position3.Y, this.position3.Z);

			// camera space
			if (this.Camera != null)
			{
				this.shader.SetUniform("view", this.Camera.Matrix());
				m *= this.Camera.Matrix();
			}
			else if (Window.Current.CurrentCamera != null)
			{
				this.shader.SetUniform("view", Window.Current.CurrentCamera.Matrix());
				m *= Window.Current.CurrentCamera.Matrix();
			}
			else
			{
				this.shader.SetUniform("view", Matrix4.Identity);
			}

			// for 3d shader we need to model+view transformation matrix for computing lights
			this.shader.SetUniform("mv", m);

			Matrix4 mvp = m * Window.Current.ProjectionMatrix;

			// pass the matrix to the shader
			this.shader.SetUniform("mvp", mvp);
		}

		public void RegenerateNormals()
		{

		}

		public void DrawGouraud(Vector4 color, Vector3 directionalLight, Vector3 directionalLightColor, Vector3 ambientColor)
		{
			this.Bind();
			this.shader.SetUniform("use_gouraud", 1f);
			this.shader.SetUniform("directional_light", directionalLight);
			this.DrawColor(color.X, color.Y, color.Z, color.W);
			this.shader.SetUniform("use_gouraud", 0f);
		}

		public void DrawPhong(Vector4 color, Vector3 light, Vector3 lightColor, Vector3 ambientColor)
		{

		}

		public void DrawGouraud(Texture texture, Vector3 light, Vector3 lightColor, Vector3 ambientColor)
		{

		}

		public void DrawPhong(Texture texture, Vector3 light, Vector3 lightColor, Vector3 ambientColor)
		{

		}
	}
}
