using System;
using OpenTK;
using System.Collections.Generic;

namespace Aiv.Fast3D
{
    public class Sphere : Mesh3
    {
        public Sphere(int segments = 32)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            float pi = (float)Math.PI;
            float pi2 = pi * 2;
            float radius = 0.5f;

            int verticalSteps = segments + 2;
            int horizontalSteps = verticalSteps * 2;

            for (int i = 0; i <= verticalSteps; i++)
            {
                float verticalStep = (float)i / (float)verticalSteps;
                float verticalAngle = verticalStep * pi;

                for (int j = 0; j <= horizontalSteps; j++)
                {
                    float horizontalStep = (float)j / (float)horizontalSteps;
                    float horizontalAngle = horizontalStep * pi2;

                    Vector3 v = Matrix3.CreateRotationZ(-verticalAngle) * Vector3.UnitY;
                    v = Matrix3.CreateRotationY(horizontalAngle) * v;

                    vertices.Add(v * -radius);

                    normals.Add(-v.Normalized());

                    uvs.Add(new Vector2(horizontalStep, 1-verticalStep));
                }
            }

            int vpos = 0;
            int npos = 0;
            int uvpos = 0;

            this.v = new float[vertices.Count * 3 * 3 * 2];
            this.vn = new float[vertices.Count * 3 * 3 * 2];
            this.uv = new float[vertices.Count * 2 * 3 * 2];

            for (int i = 0; i <= verticalSteps; i++)
            {
                int index = (i * verticalSteps + 1) * (horizontalSteps + 1);
                for (; (index + horizontalSteps + 2) < vertices.Count; index++)
                {
                    // vertices [1]
                    this.v[vpos++] = vertices[index + 1].X;
                    this.v[vpos++] = vertices[index + 1].Y;
                    this.v[vpos++] = vertices[index + 1].Z;

                    this.v[vpos++] = vertices[index].X;
                    this.v[vpos++] = vertices[index].Y;
                    this.v[vpos++] = vertices[index].Z;

                    this.v[vpos++] = vertices[index + horizontalSteps + 1].X;
                    this.v[vpos++] = vertices[index + horizontalSteps + 1].Y;
                    this.v[vpos++] = vertices[index + horizontalSteps + 1].Z;

                    // normals [1]
                    this.vn[npos++] = normals[index + 1].X;
                    this.vn[npos++] = normals[index + 1].Y;
                    this.vn[npos++] = normals[index + 1].Z;

                    this.vn[npos++] = normals[index].X;
                    this.vn[npos++] = normals[index].Y;
                    this.vn[npos++] = normals[index].Z;

                    this.vn[npos++] = normals[index + horizontalSteps + 1].X;
                    this.vn[npos++] = normals[index + horizontalSteps + 1].Y;
                    this.vn[npos++] = normals[index + horizontalSteps + 1].Z;

                    // uvs [1]
                    this.uv[uvpos++] = uvs[index + 1].X;
                    this.uv[uvpos++] = uvs[index + 1].Y;

                    this.uv[uvpos++] = uvs[index].X;
                    this.uv[uvpos++] = uvs[index].Y;

                    this.uv[uvpos++] = uvs[index + horizontalSteps + 1].X;
                    this.uv[uvpos++] = uvs[index + horizontalSteps + 1].Y;

                    // vertices [2]
                    this.v[vpos++] = vertices[index + 1].X;
                    this.v[vpos++] = vertices[index + 1].Y;
                    this.v[vpos++] = vertices[index + 1].Z;

                    this.v[vpos++] = vertices[index + horizontalSteps + 1].X;
                    this.v[vpos++] = vertices[index + horizontalSteps + 1].Y;
                    this.v[vpos++] = vertices[index + horizontalSteps + 1].Z;

                    this.v[vpos++] = vertices[index + horizontalSteps + 2].X;
                    this.v[vpos++] = vertices[index + horizontalSteps + 2].Y;
                    this.v[vpos++] = vertices[index + horizontalSteps + 2].Z;

                    // normals [2]
                    this.vn[npos++] = normals[index + 1].X;
                    this.vn[npos++] = normals[index + 1].Y;
                    this.vn[npos++] = normals[index + 1].Z;

                    this.vn[npos++] = normals[index + horizontalSteps + 1].X;
                    this.vn[npos++] = normals[index + horizontalSteps + 1].Y;
                    this.vn[npos++] = normals[index + horizontalSteps + 1].Z;

                    this.vn[npos++] = normals[index + horizontalSteps + 2].X;
                    this.vn[npos++] = normals[index + horizontalSteps + 2].Y;
                    this.vn[npos++] = normals[index + horizontalSteps + 2].Z;

                    // uvs [2]
                    this.uv[uvpos++] = uvs[index + 1].X;
                    this.uv[uvpos++] = uvs[index + 1].Y;

                    this.uv[uvpos++] = uvs[index + horizontalSteps + 1].X;
                    this.uv[uvpos++] = uvs[index + horizontalSteps + 1].Y;

                    this.uv[uvpos++] = uvs[index + horizontalSteps + 2].X;
                    this.uv[uvpos++] = uvs[index + horizontalSteps + 2].Y;

                }
            }

            this.Update();
            this.UpdateNormals();
        }
    }
}
