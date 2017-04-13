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

uniform mat4 model;

uniform mat4 mvp;

uniform mat4 mv;

uniform mat4 view;

uniform mat4 depth_vp;

uniform vec3 light_vector;
uniform vec3 light_position;


uniform float use_gouraud;
uniform float use_phong;
uniform float use_depth;

out vec2 uvout;
out vec4 vertex_color;
out vec3 normal_from_view;
out vec3 light_direction;

out float lambert;

uniform float use_shadow_map;
out vec4 shadow_position;

void main(){

		if (use_depth > 0.0) {
			gl_Position = depth_vp * model * vec4(vertex.xyz, 1.0);
			return;
		}

        gl_Position = mvp * vec4(vertex.xyz, 1.0);
		
        uvout = uv;
        vertex_color = vc;

		if (use_shadow_map > 0.0) {
			shadow_position = depth_vp * model * vec4(vertex.xyz, 1.0);
		}
	
		if (use_gouraud > 0.0) {
			// compute the vertex in camera space
			vec3 vertex_from_view = (mv * vec4(vertex.xyz, 1.0)).xyz;
			// compute the light in camera space
			vec3 light_from_view = (view * vec4(light_vector, 0)).xyz;
			// compute the normal in camera space
			vec3 normal_from_view = normalize(mv * vec4(vn.xyz, 0)).xyz;

			light_direction = normalize(light_from_view);
			// get the lambert cosine
			lambert = clamp(dot(normal_from_view, -light_direction), 0.0, 1.0);
		}
		else if (use_phong > 0.0) {
			// compute the vertex in camera space
			vec3 vertex_from_view = (mv * vec4(vertex.xyz, 1.0)).xyz;
			// compute the light in camera space
			//vec3 light_from_view = (view * vec4(light_position, 1.0)).xyz;
			vec3 light_from_view = (view * vec4(light_vector, 0)).xyz;
			// compute the normal in camera space
			normal_from_view = normalize(mv * vec4(vn.xyz, 0)).xyz;
		
			light_direction = normalize(light_from_view);
		}
}";
		private static string simpleFragmentShader3 = @"
#version 330 core

precision highp float;

uniform vec4 color;

uniform float use_texture;
uniform float use_wireframe;

uniform sampler2D tex;


uniform float use_shadow_map;
uniform sampler2D shadow_map_tex;

uniform float use_gouraud;
uniform float use_phong;
uniform float use_depth;

uniform float shadow_bias;

uniform vec3 ambient;

in vec2 uvout;
in vec4 vertex_color;
in float lambert;

in vec3 normal_from_view;
in vec3 light_direction;

out vec4 out_color;
in vec4 shadow_position;

void main(){

	if (use_depth > 0.0) {
		return;
	}

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
		if (use_shadow_map > 0.0) {
			vec3 shadow_projection = shadow_position.xyz / shadow_position.w;
			shadow_projection = shadow_projection * 0.5 + 0.5;
			if ( texture(shadow_map_tex, shadow_projection.xy).x < shadow_projection.z-shadow_bias) {
				out_color = vec4(0, 0, 0, 1);
			}
		}
	}
	else if (use_phong > 0.0) {
		float diffuse = clamp(dot(normal_from_view, -light_direction), 0.0, 1.0);
		vec3 base_color = out_color.xyz;
		out_color = vec4(base_color * diffuse + base_color * ambient, out_color.w);
		if (use_shadow_map > 0.0) {
			vec3 shadow_projection = shadow_position.xyz / shadow_position.w;
			shadow_projection = shadow_projection * 0.5 + 0.5;
			if ( texture(shadow_map_tex, shadow_projection.xy).x < shadow_projection.z-shadow_bias) {
				out_color = vec4(base_color.xyz * ambient, out_color.w) ;
			}
		}
	}
}";

		private static Shader simpleShader3 = new Shader(simpleVertexShader3, simpleFragmentShader3, null, null, null);

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

			this.shader.SetUniform("model", m);

			Matrix4 projectionMatrix = Window.Current.ProjectionMatrix;

			// camera space
			if (this.Camera != null)
			{
				this.shader.SetUniform("view", this.Camera.Matrix());
				m *= this.Camera.Matrix();
				if (this.Camera.HasProjection)
				{
					projectionMatrix = this.Camera.ProjectionMatrix();
				}
			}
			else if (Window.Current.CurrentCamera != null)
			{
				this.shader.SetUniform("view", Window.Current.CurrentCamera.Matrix());
				m *= Window.Current.CurrentCamera.Matrix();
				if (Window.Current.CurrentCamera.HasProjection)
				{
					projectionMatrix = Window.Current.CurrentCamera.ProjectionMatrix();
				}
			}
			else
			{
				this.shader.SetUniform("view", Matrix4.Identity);
			}

			// for 3d shader we need to model+view transformation matrix for computing lights
			this.shader.SetUniform("mv", m);

			Matrix4 mvp = m * projectionMatrix;

			// pass the matrix to the shader
			this.shader.SetUniform("mvp", mvp);
		}

		public void RegenerateNormals()
		{
			this.vn = new float[this.v.Length];
			for (int i = 0; i < this.v.Length; i += 9)
			{
				float x = this.v[i];
				float y = this.v[i + 1];
				float z = this.v[i + 2];
				Vector3 v0 = new Vector3(x, y, z);
				x = this.v[i + 3];
				y = this.v[i + 4];
				z = this.v[i + 5];
				Vector3 v1 = new Vector3(x, y, z);
				x = this.v[i + 6];
				y = this.v[i + 7];
				z = this.v[i + 8];
				Vector3 v2 = new Vector3(x, y, z);

				Vector3 vn0 = Vector3.Cross(v2 - v0, v1 - v0).Normalized() * -1;
				Vector3 vn1 = Vector3.Cross(v0 - v1, v2 - v1).Normalized() * -1;
				Vector3 vn2 = Vector3.Cross(v1 - v2, v0 - v2).Normalized() * -1;

				this.vn[i] = vn0.X;
				this.vn[i + 1] = vn0.Y;
				this.vn[i + 2] = vn0.Z;

				this.vn[i + 3] = vn1.X;
				this.vn[i + 4] = vn1.Y;
				this.vn[i + 5] = vn1.Z;

				this.vn[i + 6] = vn2.X;
				this.vn[i + 7] = vn2.Y;
				this.vn[i + 8] = vn2.Z;
			}

			UpdateNormals();
		}

		public void DrawGouraud(Vector4 color, Light light, DepthTexture shadowMapTexture = null, float shadowBias = 0.005f)
		{
			this.Bind();
			this.shader.SetUniform("use_gouraud", 1f);
			this.shader.SetUniform("light_vector", light.Vector);
			this.shader.SetUniform("shadow_bias", shadowBias);
			if (shadowMapTexture != null)
			{
				this.shader.SetUniform("use_shadow_map", 1f);
				shadowMapTexture.Bind(1);
				this.shader.SetUniform("shadow_map_tex", 1);
				this.shader.SetUniform("depth_vp", light.ShadowProjection);
			}
			this.DrawColor(color.X, color.Y, color.Z, color.W);
			this.shader.SetUniform("use_gouraud", -1f);
			this.shader.SetUniform("use_shadow_map", -1f);
		}

		public void DrawPhong(Vector4 color, Light light, Vector3 ambientColor, DepthTexture shadowMapTexture = null, float shadowBias = 0.005f)
		{
			this.Bind();
			this.shader.SetUniform("use_phong", 1f);
			this.shader.SetUniform("light_vector", light.Vector);
			this.shader.SetUniform("ambient", ambientColor);
			this.shader.SetUniform("shadow_bias", shadowBias);
			if (shadowMapTexture != null)
			{
				this.shader.SetUniform("use_shadow_map", 1f);
				shadowMapTexture.Bind(1);
				this.shader.SetUniform("shadow_map_tex", 1);
				this.shader.SetUniform("depth_vp", light.ShadowProjection);
			}
			this.DrawColor(color.X, color.Y, color.Z, color.W);
			this.shader.SetUniform("use_phong", -1f);
			this.shader.SetUniform("use_shadow_map", -1f);
		}

		public void DrawGouraud(Texture texture, Light light, DepthTexture shadowMapTexture = null, float shadowBias = 0.005f)
		{
			this.Bind();
			this.shader.SetUniform("use_gouraud", 1f);
			this.shader.SetUniform("light_vector", light.Vector);
			this.shader.SetUniform("shadow_bias", shadowBias);
			if (shadowMapTexture != null)
			{
				this.shader.SetUniform("use_shadow_map", 1f);
				shadowMapTexture.Bind(1);
				this.shader.SetUniform("shadow_map_tex", 1);
				this.shader.SetUniform("depth_vp", light.ShadowProjection);
			}
			this.DrawTexture(texture);
			this.shader.SetUniform("use_gouraud", -1f);
			this.shader.SetUniform("use_shadow_map", -1f);
		}

		public void DrawPhong(Texture texture, Light light, Vector3 ambientColor, DepthTexture shadowMapTexture = null, float shadowBias = 0.005f)
		{
			this.Bind();
			this.shader.SetUniform("use_phong", 1f);
			this.shader.SetUniform("light_vector", light.Vector);
			this.shader.SetUniform("ambient", ambientColor);
			this.shader.SetUniform("shadow_bias", shadowBias);
			if (shadowMapTexture != null)
			{
				this.shader.SetUniform("use_shadow_map", 1f);
				shadowMapTexture.Bind(1);
				this.shader.SetUniform("shadow_map_tex", 1);
				this.shader.SetUniform("depth_vp", light.ShadowProjection);
			}
			this.DrawTexture(texture);
			this.shader.SetUniform("use_phong", -1f);
			this.shader.SetUniform("use_shadow_map", -1f);
		}

		public void DrawShadowMap(Light light)
		{
			this.Bind();
			this.shader.SetUniform("use_depth", 1f);
			this.shader.SetUniform("depth_vp", light.ShadowProjection);
			this.Draw();
			this.shader.SetUniform("use_depth", -1f);
		}
	}
}
