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
out vec2 uvout;
out vec4 vertex_color;
out vec3 vnout;
void main(){
        gl_Position = mvp * vec4(vertex.xyz, 1.0);
        uvout = uv;
        vertex_color = vc;
        vnout = vn;
}";
        private static string simpleFragmentShader3 = @"
#version 330 core

precision highp float;

uniform vec4 color;
uniform float use_texture;
uniform float use_wireframe;
uniform sampler2D tex;
in vec2 uvout;
in vec4 vertex_color;
in vec3 vnout;
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
}";

        private static string simpleVertexShaderObsolete3 = @"
attribute vec3 vertex;
attribute vec2 uv;
attribute vec4 vc;
attribute vec3 vn;
uniform mat4 mvp;
varying vec2 uvout;
varying vec4 vertex_color;
varying vec3 vnout;
void main(){
        gl_Position = mvp * vec4(vertex.xyz, 1.0);
        uvout = uv;
        vertex_color = vc;
        vnout = vn;
}";
        private static string simpleFragmentShaderObsolete3 = @"
precision mediump float;
uniform vec4 color;
uniform float use_texture;
uniform float use_wireframe;
uniform sampler2D tex;
varying vec2 uvout;
varying vec4 vertex_color;
varying vec3 vnout;
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

        private int normalsBuffer;
        public float[] vn;


        public Mesh3() : base(simpleShader3, 3)
        {
            normalsBuffer = NewFloatBuffer(3, 3, new float[] { }, 1);

            scale3 = Vector3.One;
            position3 = Vector3.Zero;
            rotation3 = Vector3.Zero;
        }

        public void UpdateNormals()
        {
            UpdateFloatBuffer(normalsBuffer, vn);
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



            if (this.Camera != null)
            {
                m *= this.Camera.Matrix();
            }
            else if (Window.Current.CurrentCamera != null)
            {
                m *= Window.Current.CurrentCamera.Matrix();
            }

            Matrix4 mvp = m * Window.Current.OrthoMatrix;

            // pass the matrix to the shader
            this.shader.SetUniform("mvp", mvp);
        }
    }
}
