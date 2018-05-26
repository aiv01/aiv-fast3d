using System;
using Aiv.Fast2D;
using OpenTK;
using System.Collections.Generic;

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
layout(location = 4) in vec3 vtgt;

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
uniform float use_cel;
uniform float use_normal_map;

out vec2 uvout;
out vec4 vertex_color;
out vec3 normal_from_view;
out vec3 light_direction;
out vec3 vertex_position;

out float lambert;

uniform float use_shadow_map;
out vec4 shadow_position;
out mat3 tbn;

void main(){

		vec4 new_vertex = vec4(vertex.xyz, 1);
		vec4 new_normal = vec4(vn.xyz, 0);


        uvout = uv;
        vertex_color = vc;

		if (use_depth > 0.0) {
			gl_Position = depth_vp * model * new_vertex;
			return;
		}

        gl_Position = mvp * new_vertex;
		
		if (use_shadow_map > 0.0) {
			shadow_position = depth_vp * model * new_vertex;
		}
	
		if (use_gouraud > 0.0) {
			// compute the vertex in camera space
			vec3 vertex_from_view = (mv * new_vertex).xyz;
			// compute the light in camera space
			vec3 light_from_view = (view * vec4(light_vector, 0)).xyz;
			// compute the normal in camera space
			vec3 normal_from_eye = normalize(mv * new_normal).xyz;

			light_direction = normalize(light_from_view);
			// get the lambert cosine
			lambert = clamp(dot(normal_from_eye, -light_direction), 0.0, 1.0);
		}
		else if (use_phong > 0.0 || use_cel > 0.0) {
			// compute the vertex in camera space
			vec3 vertex_from_view = (mv * new_vertex).xyz;
			// compute the light in camera space
			//vec3 light_from_view = (view * vec4(light_position, 1.0)).xyz;
			vec3 light_from_view = (view * vec4(light_vector, 0)).xyz;
			// compute the normal in camera space
			normal_from_view = normalize(mv * new_normal).xyz;
		
			light_direction = normalize(light_from_view);
			vertex_position = (model * new_vertex).xyz;

            if (use_normal_map > 0.0)
            {
                vec3 tbn_t = normalize(vec3(mv * vec4(vtgt, 0.0)));
                vec3 tbn_n = normalize(vec3(mv * vec4(vn, 0.0)));
                vec3 tbn_b = cross(tbn_n, tbn_t);
                tbn = mat3(tbn_t, tbn_b, tbn_n);
            }
		}
}";
        private static string simpleFragmentShader3 = @"
#version 330 core

precision highp float;

uniform vec4 color;

uniform mat4 view;

uniform float use_texture;
uniform float use_wireframe;

uniform sampler2D tex;


uniform float use_shadow_map;
uniform sampler2D shadow_map_tex;

uniform float use_gouraud;
uniform float use_phong;
uniform float use_depth;
uniform float use_cel;

uniform float use_specular_map;
uniform sampler2D specular_tex;
uniform float use_normal_map;
uniform sampler2D normal_tex;

uniform float threshold;

uniform float shadow_bias;

uniform vec3 ambient;
uniform float shininess;

in vec2 uvout;
in vec4 vertex_color;
in float lambert;

in vec3 normal_from_view;
in vec3 light_direction;
in vec3 vertex_position;

out vec4 out_color;
in vec4 shadow_position;

uniform vec3 light_color;

in mat3 tbn;

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
		out_color = vec4(out_color.xyz * light_color * lambert, out_color.w);
		if (use_shadow_map > 0.0) {
			vec3 shadow_projection = shadow_position.xyz / shadow_position.w;
			shadow_projection = shadow_projection * 0.5 + 0.5;
			if ( texture(shadow_map_tex, shadow_projection.xy).x < shadow_projection.z-shadow_bias && shadow_projection.z <= 1.0) {
				out_color = vec4(0, 0, 0, 1);
			}
		}
	}
	else if (use_phong > 0.0 || use_cel > 0.0) {
        vec3 normal_eye = normal_from_view;
        if (use_normal_map > 0.0)
        {
            normal_eye = normalize(texture(normal_tex, uvout).rgb * 2 - 1);
            normal_eye = normalize(tbn * normal_eye);
        }
		float diffuse = clamp(dot(normal_eye, -light_direction), 0.0, 1.0);

		if (use_cel > 0.0) {
			if (diffuse >= threshold) {
				diffuse = 1;
			}
			else if (diffuse >= threshold/2) {
				diffuse = 0.5;
			}
			else {
				diffuse = 0;
			}
		}

		vec3 camera_position = -view[3].xyz;
		// reflection vector from light
		vec3 light_reflection = reflect(light_direction, normal_eye);
		// direction from vertex to camera
		vec3 vertex_to_camera = normalize(camera_position - vertex_position);

		float specular_shininess = shininess;

		if (use_specular_map > 0.0) {
			specular_shininess = texture(specular_tex, uvout).x;
		}

		float specular = 0;

		if (specular_shininess > 0.0) {
			specular = pow(max(0.0, dot(vertex_to_camera, light_reflection)), specular_shininess);
		}
	
		vec3 base_color = out_color.xyz;
		out_color = vec4(base_color * light_color * (diffuse + ambient + specular), out_color.w);
		if (use_shadow_map > 0.0) {
			vec3 shadow_projection = shadow_position.xyz / shadow_position.w;
			shadow_projection = shadow_projection * 0.5 + 0.5;
			if ( texture(shadow_map_tex, shadow_projection.xy).x < shadow_projection.z-shadow_bias && shadow_projection.z <= 1.0) {
				out_color = vec4(base_color.xyz * ambient, out_color.w) ;
			}
		}
	}
}";

        private static Shader simpleShader3 = new Shader(simpleVertexShader3, simpleFragmentShader3, null, null, null);

        public enum RotationMode
        {
            XYZ,
            XZY,
            YXZ,
            YZX,
            ZXY,
            ZYX
        }

        private Dictionary<RotationMode, RotationFunc> rotationModesMapping;

        public void SetRotationMode(RotationMode mode)
        {
            rotationGenerator = rotationModesMapping[mode];
        }

        public void SetRotationMode(RotationFunc func)
        {
            rotationGenerator = func;
        }

        private Vector3 internalRotation;

        public Vector3 Rotation3
        {
            get
            {
                return internalRotation;
            }
            set
            {
                internalRotation = value;
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

        public delegate Matrix4 RotationFunc();

        private RotationFunc rotationGenerator;


        public Quaternion Quaternion
        {
            get
            {
                return rotationGenerator().ExtractRotation();

            }
        }

        public Vector3 EulerRotation3
        {
            get
            {
                return this.Rotation3 * 180f / (float)Math.PI;
            }
            set
            {
                this.Rotation3 = value * (float)Math.PI / 180f;
            }
        }

        public Vector3 Forward
        {
            get
            {
                return Quaternion * Vector3.UnitZ;
            }
        }

        public Vector3 Right
        {
            get
            {
                return Vector3.Cross(Forward, Up);
            }
        }

        public Vector3 Up
        {
            get
            {
                return Quaternion * Vector3.UnitY;
            }
        }

        public float[] vn;
        private int vnBufferId;

        public float[] vtgt;
        private int vtgtBufferId;


        public string Name;

        public Mesh3() : base(simpleShader3, 3)
        {

            scale3 = Vector3.One;
            position3 = Vector3.Zero;
            internalRotation = Vector3.Zero;

            Name = string.Empty;

            rotationModesMapping = new Dictionary<RotationMode, RotationFunc>();
            rotationModesMapping[RotationMode.XYZ] = () => Matrix4.CreateRotationX(internalRotation.X) * Matrix4.CreateRotationY(internalRotation.Y) * Matrix4.CreateRotationZ(internalRotation.Z);
            rotationModesMapping[RotationMode.XZY] = () => Matrix4.CreateRotationX(internalRotation.X) * Matrix4.CreateRotationZ(internalRotation.Z) * Matrix4.CreateRotationY(internalRotation.Y);
            rotationModesMapping[RotationMode.YXZ] = () => Matrix4.CreateRotationY(internalRotation.Y) * Matrix4.CreateRotationX(internalRotation.X) * Matrix4.CreateRotationZ(internalRotation.Z);
            rotationModesMapping[RotationMode.YZX] = () => Matrix4.CreateRotationY(internalRotation.Y) * Matrix4.CreateRotationZ(internalRotation.Z) * Matrix4.CreateRotationX(internalRotation.X);
            rotationModesMapping[RotationMode.ZXY] = () => Matrix4.CreateRotationZ(internalRotation.Z) * Matrix4.CreateRotationX(internalRotation.X) * Matrix4.CreateRotationY(internalRotation.Y);
            rotationModesMapping[RotationMode.ZYX] = () => Matrix4.CreateRotationZ(internalRotation.Z) * Matrix4.CreateRotationY(internalRotation.Y) * Matrix4.CreateRotationX(internalRotation.X);


            SetRotationMode(RotationMode.YZX);

            this.vnBufferId = Graphics.NewBuffer();
            Graphics.MapBufferToArray(this.vnBufferId, 3, 3);

            this.vtgtBufferId = Graphics.NewBuffer();
            Graphics.MapBufferToArray(this.vtgtBufferId, 4, 3);

            // ensure normals are loaded
            this.shaderSetupHook += (mesh) =>
            {
                if (((Mesh3)mesh).vn == null)
                {
                    this.vn = new float[this.v.Length];
                    ((Mesh3)mesh).UpdateNormals();
                }
                if (((Mesh3)mesh).uv == null)
                {
                    this.uv = new float[this.v.Length];
                    ((Mesh3)mesh).UpdateUV();
                }
                if (((Mesh3)mesh).vtgt == null)
                {
                    ((Mesh3)mesh).RegenerateTangents();
                    ((Mesh3)mesh).UpdateTangents();
                }
            };
        }

        public void RegenerateTangents()
        {
            if (this.vn == null || this.uv == null)
                return;
            this.vtgt = new float[this.v.Length];
            int j = 0;
            for (int i = 0; i < this.v.Length; i += 9)
            {
                float x = this.v[i];
                float y = this.v[i + 1];
                float z = this.v[i + 2];
                float uvU = this.uv[j++];
                float uvV = this.uv[j++];
                Vector3 v0 = new Vector3(x, y, z);
                Vector2 uv0 = new Vector2(uvU, uvV);

                x = this.v[i + 3];
                y = this.v[i + 4];
                z = this.v[i + 5];
                uvU = this.uv[j++];
                uvV = this.uv[j++];
                Vector3 v1 = new Vector3(x, y, z);
                Vector2 uv1 = new Vector2(uvU, uvV);

                x = this.v[i + 6];
                y = this.v[i + 7];
                z = this.v[i + 8];
                uvU = this.uv[j++];
                uvV = this.uv[j++];
                Vector3 v2 = new Vector3(x, y, z);
                Vector2 uv2 = new Vector2(uvU, uvV);

                Vector3 edge0 = v1 - v0;
                Vector3 edge1 = v2 - v1;

                Vector2 deltaUV0 = uv1 - uv0;
                Vector2 deltaUV1 = uv2 - uv1;

                float f = 1.0f / (deltaUV0.X * deltaUV1.Y - deltaUV1.X * deltaUV0.Y);

                Vector3 tgt;
                tgt.X = f * (deltaUV1.Y * edge0.X - deltaUV0.Y * edge1.X);
                tgt.Y = f * (deltaUV1.Y * edge0.Y - deltaUV0.Y * edge1.Y);
                tgt.Z = f * (deltaUV1.Y * edge0.Z - deltaUV0.Y * edge1.Z);

                this.vtgt[i] = tgt.X;
                this.vtgt[i + 1] = tgt.Y;
                this.vtgt[i + 2] = tgt.Z;

                this.vtgt[i + 3] = tgt.X;
                this.vtgt[i + 4] = tgt.Y;
                this.vtgt[i + 5] = tgt.Z;

                this.vtgt[i + 6] = tgt.X;
                this.vtgt[i + 7] = tgt.Y;
                this.vtgt[i + 8] = tgt.Z;
            }
        }

        public void UpdateNormals()
        {
            if (this.vn == null)
                return;
            Graphics.BufferData(this.vnBufferId, this.vn);
        }

        public void UpdateTangents()
        {
            if (this.vtgt == null)
                return;
            Graphics.BufferData(this.vtgtBufferId, this.vtgt);
        }

        private Mesh3 parent;
        public Mesh3 Parent
        {
            get
            {
                return this.parent;
            }
        }

        public void SetParent(Mesh3 parent)
        {
            this.parent = parent;
        }

        public Matrix4 Matrix
        {
            get
            {
                Matrix4 m = Matrix4.CreateTranslation(-this.pivot3.X, -this.pivot3.Y, -this.pivot3.Z) *
#if !__MOBILE__
                Matrix4.CreateScale(this.scale3.X, this.scale3.Y, this.scale3.Z) *
#else
                Matrix4.Scale(this.scale3.X, this.scale3.Y, this.scale3.Z) *
#endif
                                   rotationGenerator() *
                // here we do not re-add the pivot, so translation is pivot based too
                Matrix4.CreateTranslation(this.position3.X, this.position3.Y, this.position3.Z);

                if (this.parent != null)
                {
                    m *= this.parent.Matrix;
                }

                return m;
            }
        }

        protected override void ApplyMatrix()
        {
            if (this.noMatrix)
                return;

            // WARNING !!! OpenTK uses row-major while OpenGL uses column-major
            Matrix4 m = this.Matrix;

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
            this.shader.SetUniform("light_color", light.Color);
            this.shader.SetUniform("shadow_bias", shadowBias);
            if (shadowMapTexture != null)
            {
                this.shader.SetUniform("use_shadow_map", 1f);
                shadowMapTexture.Bind(1);
                this.shader.SetUniform("shadow_map_tex", 1);
                this.shader.SetUniform("depth_vp", light.ShadowProjection);
            }
            this.DrawColor(color.X, color.Y, color.Z, color.W);
            this.ResetUniforms();
        }

        public void DrawPhong(Vector4 color, Light light, Vector3 ambientColor, float shininess = 0, DepthTexture shadowMapTexture = null, float shadowBias = 0.005f, Texture normalMapTexture = null)
        {
            this.Bind();
            this.shader.SetUniform("use_phong", 1f);
            this.shader.SetUniform("light_vector", light.Vector);
            this.shader.SetUniform("light_color", light.Color);
            this.shader.SetUniform("ambient", ambientColor);
            this.shader.SetUniform("shadow_bias", shadowBias);
            if (shadowMapTexture != null)
            {
                this.shader.SetUniform("use_shadow_map", 1f);
                shadowMapTexture.Bind(1);
                this.shader.SetUniform("shadow_map_tex", 1);
                this.shader.SetUniform("depth_vp", light.ShadowProjection);
            }
            if (normalMapTexture != null)
            {
                this.shader.SetUniform("use_normal_map", 1f);
                normalMapTexture.Bind(3);
                this.shader.SetUniform("normal_tex", 3);
            }
            this.DrawColor(color.X, color.Y, color.Z, color.W);
            this.ResetUniforms();
        }

        public void DrawCel(Vector4 color, Light light, Vector3 ambientColor, float threshold = 0.75f, DepthTexture shadowMapTexture = null, float shadowBias = 0.005f, Texture normalMapTexture = null)
        {
            this.Bind();
            this.shader.SetUniform("use_cel", 1f);
            this.shader.SetUniform("light_vector", light.Vector);
            this.shader.SetUniform("light_color", light.Color);
            this.shader.SetUniform("ambient", ambientColor);
            this.shader.SetUniform("shininess", 0f);
            this.shader.SetUniform("threshold", threshold);
            this.shader.SetUniform("shadow_bias", shadowBias);
            if (shadowMapTexture != null)
            {
                this.shader.SetUniform("use_shadow_map", 1f);
                shadowMapTexture.Bind(1);
                this.shader.SetUniform("shadow_map_tex", 1);
                this.shader.SetUniform("depth_vp", light.ShadowProjection);
            }
            if (normalMapTexture != null)
            {
                this.shader.SetUniform("use_normal_map", 1f);
                normalMapTexture.Bind(3);
                this.shader.SetUniform("normal_tex", 3);
            }
            this.DrawColor(color.X, color.Y, color.Z, color.W);
            this.ResetUniforms();
        }

        public void DrawGouraud(Texture texture, Light light, DepthTexture shadowMapTexture = null, float shadowBias = 0.005f)
        {
            this.Bind();
            this.shader.SetUniform("use_gouraud", 1f);
            this.shader.SetUniform("light_vector", light.Vector);
            this.shader.SetUniform("light_color", light.Color);
            this.shader.SetUniform("shadow_bias", shadowBias);
            if (shadowMapTexture != null)
            {
                this.shader.SetUniform("use_shadow_map", 1f);
                shadowMapTexture.Bind(1);
                this.shader.SetUniform("shadow_map_tex", 1);
                this.shader.SetUniform("depth_vp", light.ShadowProjection);
            }
            this.DrawTexture(texture);
            this.ResetUniforms();
        }

        public void DrawPhong(Texture texture, Light light, Vector3 ambientColor, float shininess = 0, DepthTexture shadowMapTexture = null, float shadowBias = 0.005f, Texture normalMapTexture = null)
        {
            this.Bind();
            this.shader.SetUniform("use_phong", 1f);
            this.shader.SetUniform("light_vector", light.Vector);
            this.shader.SetUniform("light_color", light.Color);
            this.shader.SetUniform("ambient", ambientColor);
            this.shader.SetUniform("shininess", shininess);
            this.shader.SetUniform("shadow_bias", shadowBias);
            if (shadowMapTexture != null)
            {
                this.shader.SetUniform("use_shadow_map", 1f);
                shadowMapTexture.Bind(1);
                this.shader.SetUniform("shadow_map_tex", 1);
                this.shader.SetUniform("depth_vp", light.ShadowProjection);
            }
            if (normalMapTexture != null)
            {
                this.shader.SetUniform("use_normal_map", 1f);
                normalMapTexture.Bind(3);
                this.shader.SetUniform("normal_tex", 3);
            }
            this.DrawTexture(texture);
            this.ResetUniforms();
        }

        public void DrawPhong(Texture texture, Light light, Vector3 ambientColor, Texture specularMap, DepthTexture shadowMapTexture = null, float shadowBias = 0.005f, Texture normalMapTexture = null)
        {
            this.Bind();
            this.shader.SetUniform("use_phong", 1f);
            this.shader.SetUniform("light_vector", light.Vector);
            this.shader.SetUniform("light_color", light.Color);
            this.shader.SetUniform("ambient", ambientColor);
            this.shader.SetUniform("shadow_bias", shadowBias);
            if (shadowMapTexture != null)
            {
                this.shader.SetUniform("use_shadow_map", 1f);
                shadowMapTexture.Bind(1);
                this.shader.SetUniform("shadow_map_tex", 1);
                this.shader.SetUniform("depth_vp", light.ShadowProjection);
            }
            if (specularMap != null)
            {
                this.shader.SetUniform("use_specular_map", 1f);
                specularMap.Bind(2);
                this.shader.SetUniform("specular_tex", 2);
            }
            if (normalMapTexture != null)
            {
                this.shader.SetUniform("use_normal_map", 1f);
                normalMapTexture.Bind(3);
                this.shader.SetUniform("normal_tex", 3);
            }
            this.DrawTexture(texture);
            this.ResetUniforms();
        }

        public void DrawCel(Texture texture, Light light, Vector3 ambientColor, float threshold = 0.75f, DepthTexture shadowMapTexture = null, float shadowBias = 0.005f, Texture normalMapTexture = null)
        {
            this.Bind();
            this.shader.SetUniform("use_cel", 1f);
            this.shader.SetUniform("light_vector", light.Vector);
            this.shader.SetUniform("light_color", light.Color);
            this.shader.SetUniform("ambient", ambientColor);
            this.shader.SetUniform("shininess", 0f);
            this.shader.SetUniform("threshold", threshold);
            this.shader.SetUniform("shadow_bias", shadowBias);
            if (shadowMapTexture != null)
            {
                this.shader.SetUniform("use_shadow_map", 1f);
                shadowMapTexture.Bind(1);
                this.shader.SetUniform("shadow_map_tex", 1);
                this.shader.SetUniform("depth_vp", light.ShadowProjection);
            }
            if (normalMapTexture != null)
            {
                this.shader.SetUniform("use_normal_map", 1f);
                normalMapTexture.Bind(3);
                this.shader.SetUniform("normal_tex", 3);
            }
            this.DrawTexture(texture);
            this.ResetUniforms();
        }

        public void DrawShadowMap(Light light)
        {
            this.Bind();
            this.shader.SetUniform("use_depth", 1f);
            this.shader.SetUniform("depth_vp", light.ShadowProjection);
            this.Draw();
            this.ResetUniforms();
        }

        public void ResetUniforms()
        {
            this.shader.SetUniform("use_gouraud", -1f);
            this.shader.SetUniform("use_phong", -1f);
            this.shader.SetUniform("use_cel", -1f);
            this.shader.SetUniform("use_depth", -1f);
            this.shader.SetUniform("use_specular_map", -1f);
            this.shader.SetUniform("use_normal_map", -1f);
            this.shader.SetUniform("use_shadow_map", -1f);
        }


        public override string ToString()
        {
            return string.Format("[Mesh3: Name={0}, Position3={1}, Rotation3={2}, Scale3={3}]", Name, Position3, Rotation3, Scale3);
        }
    }
}
