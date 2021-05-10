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

layout(location = 5) in ivec4 influences0;
layout(location = 6) in ivec4 influences1;
layout(location = 7) in vec4 weights0;
layout(location = 8) in vec4 weights1;

uniform float use_skeleton;

uniform mat4 bones[80];

uniform mat4 model;

uniform mat4 mvp;

uniform mat4 mv;

uniform mat4 view;

uniform mat4 depth_vp;

uniform vec3 light_vector[8];
uniform float light_strength[8];
uniform int num_lights;



uniform float use_gouraud;
uniform float use_phong;
uniform float use_depth;
uniform float use_cel;
uniform float use_normal_map;

out vec2 uvout;
out vec4 vertex_color;
out vec3 normal_from_view;
out vec3 light_direction[8];
out float light_attenuation[8];
out vec3 vertex_position;

out float lambert;

uniform float use_shadow_map;
out vec4 shadow_position;
out mat3 tbn;

vec4 to_bone_space(vec4 v)
{
    if (use_skeleton < 0.1)
    {
        return v;
    }
    mat4 bones_matrix = bones[influences0.x] * weights0.x;
    bones_matrix += bones[influences0.y] * weights0.y;
    bones_matrix += bones[influences0.z] * weights0.z;
    bones_matrix += bones[influences0.w] * weights0.w;
    bones_matrix += bones[influences1.x] * weights1.x;
    bones_matrix += bones[influences1.y] * weights1.y;
    bones_matrix += bones[influences1.z] * weights1.z;
    bones_matrix += bones[influences1.w] * weights1.x;

    return bones_matrix * v;

    /*vec4 vbone = (bones[influences0.x] * v) * weights0.x;
    vbone += (bones[influences0.y] * v) * weights0.y;
    vbone += (bones[influences0.z] * v) * weights0.z;
    vbone += (bones[influences0.w] * v) * weights0.w;
    vbone += (bones[influences1.x] * v) * weights1.x;
    vbone += (bones[influences1.y] * v) * weights1.y;
    vbone += (bones[influences1.z] * v) * weights1.z;
    vbone += (bones[influences1.w] * v) * weights1.w;
    return vbone;*/
}

void main(){

		vec4 new_vertex = to_bone_space(vec4(vertex.xyz, 1));
		vec4 new_normal = to_bone_space(vec4(vn.xyz, 0));


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
			vec3 light_from_view = (view * vec4(light_vector[0], 0)).xyz;
			// compute the normal in camera space
			vec3 normal_from_eye = normalize(mv * new_normal).xyz;

			light_direction[0] = normalize(light_from_view);
			// get the lambert cosine
			lambert = clamp(dot(normal_from_eye, -light_direction[0]), 0.0, 1.0);
		}
		else if (use_phong > 0.0 || use_cel > 0.0) {
			// compute the vertex in camera space
			vec3 vertex_from_view = (mv * new_vertex).xyz;
			
			// compute the normal in camera space
			normal_from_view = normalize(mv * new_normal).xyz;
		
            // compute the light in camera space
            for(int i=0;i<int(num_lights);i++)
            {
                if (light_strength[i] > 0.0)
                {
                    vec3 light_from_view = (view * normalize(vec4((model * new_vertex).xyz - light_vector[i], 0))).xyz;
                    light_direction[i] = normalize(light_from_view);
                    float light_distance = length((model * new_vertex).xyz - light_vector[i]);
                    light_attenuation[i] = clamp(1.0 - (1.0/light_strength[i]) * min(light_distance, light_strength[i]), 0.0, 1.0);
                }
                else
                {
                    vec3 light_from_view = (view * vec4(light_vector[i], 0)).xyz;
			        light_direction[i] = normalize(light_from_view);
                    light_attenuation[i] = 1.0;
                }
            }
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
uniform float use_emissive_map;
uniform sampler2D emissive_tex;

uniform float threshold;

uniform float shadow_bias;

uniform vec3 ambient;
uniform float shininess;

in vec2 uvout;
in vec4 vertex_color;
in float lambert;

in vec3 normal_from_view;

in vec3 vertex_position;

out vec4 out_color;
in vec4 shadow_position;

in vec3 light_direction[8];
in float light_attenuation[8];

uniform vec3 light_color[8];
uniform float light_strength[8];
uniform int num_lights;

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
            //see issue: https://github.com/aiv01/aiv-fast3d/issues/22
            //out_color = vec4(0, 0, 0, 0);    
            discard;
        }
        return;
    }
    else {
        out_color = vertex_color;
    }
    out_color += color;

    float specular = 0;

	if (use_gouraud > 0.0) {
		out_color = vec4(out_color.xyz * light_color[0] * lambert, out_color.w);
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

        vec3 light_multiplier = vec3(0, 0, 0);

        // manage lights
        for(int i=0;i<int(num_lights);i++)
        {
		    float diffuse = clamp(dot(normal_eye, -light_direction[i]), 0.0, 1.0);

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
		    vec3 light_reflection = reflect(light_direction[i], normal_eye);
		    // direction from vertex to camera
		    vec3 vertex_to_camera = normalize(camera_position - vertex_position);

            
		    float specular_shininess = shininess;

		    if (use_specular_map > 0.0) {
			    specular_shininess += texture(specular_tex, uvout).x;
		    }

		    specular = pow(max(0.0, dot(vertex_to_camera, light_reflection)), 32) * specular_shininess;
	
            if (light_attenuation[i] > 0.001)
            {
		        light_multiplier += (light_color[i] * diffuse) / light_attenuation[i];
            }
        }

        out_color = vec4(out_color.xyz * (light_multiplier + ambient) + specular, out_color.w);

        if (use_emissive_map > 0.0)
        {
            out_color += texture(emissive_tex, uvout);
        }
        
        
        // apply shadow
		if (use_shadow_map > 0.0) {
			vec3 shadow_projection = shadow_position.xyz / shadow_position.w;
			shadow_projection = shadow_projection * 0.5 + 0.5;
			if ( texture(shadow_map_tex, shadow_projection.xy).x < shadow_projection.z-shadow_bias && shadow_projection.z <= 1.0) {
				out_color = vec4(out_color.xyz * ambient * 0.5, out_color.w) ;
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

        public int[] influences0;
        private int influences0BufferId;
        public int[] influences1;
        private int influences1BufferId;

        public float[] weights0;
        private int weights0BufferId;
        public float[] weights1;
        private int weights1BufferId;

        public string Name;

        public Bone[] Skeleton;

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

            this.influences0BufferId = Graphics.NewBuffer();
            this.influences1BufferId = Graphics.NewBuffer();
            this.weights0BufferId = Graphics.NewBuffer();
            this.weights1BufferId = Graphics.NewBuffer();


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

        public void RebuildSkeleton()
        {
            this.Bind();
            Graphics.MapBufferToIntArray(this.influences0BufferId, 5, 4);
            Graphics.MapBufferToIntArray(this.influences1BufferId, 6, 4);
            Graphics.MapBufferToArray(this.weights0BufferId, 7, 4);
            Graphics.MapBufferToArray(this.weights1BufferId, 8, 4);
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

        public void UpdateInfluencesAndWeights()
        {
            if (this.Skeleton == null)
                return;
            this.RebuildSkeleton();
            Graphics.BufferData(this.influences0BufferId, this.influences0);
            Graphics.BufferData(this.influences1BufferId, this.influences1);
            Graphics.BufferData(this.weights0BufferId, this.weights0);
            Graphics.BufferData(this.weights1BufferId, this.weights1);
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

            if (Skeleton != null)
            {
                this.shader.SetUniform("use_skeleton", 1.0f);
                for (int i = 0; i < Math.Min(Skeleton.Length, 80); i++)
                {
                    Matrix4 boneMatrix = Skeleton[i].BindPoseMatrix * Skeleton[i].GlobalMatrix;
                    this.shader.SetUniform(string.Format("bones[{0}]", i), boneMatrix);
                }

            }
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
            this.SetDefaultLight(light);
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
            this.SetDefaultLight(light);
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
            this.SetDefaultLight(light);
            this.shader.SetUniform("ambient", ambientColor);
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
            this.SetDefaultLight(light);
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
            this.SetDefaultLight(light);
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
            this.SetDefaultLight(light);
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

        public void DrawPhong(Material material)
        {
            this.Bind();
            this.shader.SetUniform("use_phong", 1f);
            this.shader.SetUniform("color", material.DiffuseColor);
            this.shader.SetUniform("shininess", material.Shininess);
            int numLights = 0;
            for (int i = 0; i < material.Lights.Length; i++)
            {
                if (material.Lights[i] != null)
                {
                    this.shader.SetUniform(string.Format("light_vector[{0}]", i), material.Lights[i].Vector);
                    this.shader.SetUniform(string.Format("light_color[{0}]", i), material.Lights[i].Color);
                    this.shader.SetUniform(string.Format("light_strength[{0}]", i), material.Lights[i].Strength);
                    numLights++;
                }
            }
            this.shader.SetUniform("num_lights", numLights);

            this.shader.SetUniform("ambient", material.Ambient);
            this.shader.SetUniform("shadow_bias", material.ShadowBias);
            if (material.ShadowMap != null)
            {
                this.shader.SetUniform("use_shadow_map", 1f);
                material.ShadowMap.Bind(1);
                this.shader.SetUniform("shadow_map_tex", 1);
                this.shader.SetUniform("depth_vp", material.Lights[0].ShadowProjection);
            }
            if (material.SpecularMap != null)
            {
                this.shader.SetUniform("use_specular_map", 1f);
                material.SpecularMap.Bind(2);
                this.shader.SetUniform("specular_tex", 2);
            }
            if (material.NormalMap != null)
            {
                this.shader.SetUniform("use_normal_map", 1f);
                material.NormalMap.Bind(3);
                this.shader.SetUniform("normal_tex", 3);
            }
            if (material.EmissiveMap != null)
            {
                this.shader.SetUniform("use_emissive_map", 1f);
                material.EmissiveMap.Bind(4);
                this.shader.SetUniform("emissive_tex", 4);
            }

            if (material.Diffuse != null)
            {
                this.DrawTexture(material.Diffuse);
            }
            else
            {
                this.Draw();
            }
            this.ResetUniforms();
        }

        public void DrawCel(Texture texture, Light light, Vector3 ambientColor, float threshold = 0.75f, DepthTexture shadowMapTexture = null, float shadowBias = 0.005f, Texture normalMapTexture = null)
        {
            this.Bind();
            this.shader.SetUniform("use_cel", 1f);
            this.SetDefaultLight(light);
            this.shader.SetUniform("ambient", ambientColor);
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

        protected void SetDefaultLight(Light light)
        {
            this.shader.SetUniform("num_lights", 1);
            this.shader.SetUniform("light_vector[0]", light.Vector);
            this.shader.SetUniform("light_color[0]", light.Color);
            this.shader.SetUniform("light_strength[0]", light.Strength);
        }

        public void ResetUniforms()
        {
            this.shader.SetUniform("use_gouraud", -1f);
            this.shader.SetUniform("use_phong", -1f);
            this.shader.SetUniform("use_cel", -1f);
            this.shader.SetUniform("use_depth", -1f);
            this.shader.SetUniform("use_texture", -1f);
            this.shader.SetUniform("use_specular_map", -1f);
            this.shader.SetUniform("use_normal_map", -1f);
            this.shader.SetUniform("use_emissive_map", -1f);
            this.shader.SetUniform("use_shadow_map", -1f);

            this.shader.SetUniform("num_lights", 1);

            this.shader.SetUniform("shininess", 0f);
            this.shader.SetUniform("color", Vector4.Zero);

            this.shader.SetUniform("use_skeleton", 0f);
        }


        public override string ToString()
        {
            return string.Format("[Mesh3: Name={0}, Position3={1}, Rotation3={2}, Scale3={3}]", Name, Position3, Rotation3, Scale3);
        }
    }
}
