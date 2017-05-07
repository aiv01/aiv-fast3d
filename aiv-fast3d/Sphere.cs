using System;
using OpenTK;
using System.Collections.Generic;

namespace Aiv.Fast3D
{
	public class Sphere : Mesh3
	{
		public Sphere(int horizontal, int vertical)
		{
			List<Vector3> vertices = new List<Vector3>();

			float pi = (float)Math.PI;
			float pi2 = pi * 2;
			float radius = 0.5f;

			// top vertex
			vertices.Add(Vector3.UnitY * radius);

			for (int i = 0; i < vertical; i++)
			{
				float a1 = pi * (i + 1) / (vertical + 1);
				float sin1 = (float)Math.Sin(a1);
				float cos1 = (float)Math.Cos(a1);

				for (int j = 0; j < horizontal; j++)
				{

					float a2 = pi2 * j / horizontal;
					float sin2 = (float)Math.Sin(a2);
					float cos2 = (float)Math.Cos(a2);

					vertices.Add(new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius);
				}
			}

			// bottom vertex
			vertices.Add(Vector3.UnitY * -radius);

			this.v = new float[horizontal * 3 * 3];
			this.vn = new float[this.v.Length];
			this.uv = new float[(this.v.Length / 3) * 2];

			int index = 1;
			int pos = 0;
			for (int j = 0; j < horizontal; j++)
			{
				// top
				this.v[pos++] = vertices[0].X;
				this.v[pos++] = vertices[0].Y;
				this.v[pos++] = vertices[0].Z;

				int rightIndex = index + 1;
				if (rightIndex >= horizontal - 1)
					rightIndex = 1;

				// right
				this.v[pos++] = vertices[rightIndex].X;
				this.v[pos++] = vertices[rightIndex].Y;
				this.v[pos++] = vertices[rightIndex].Z;

				// left
				this.v[pos++] = vertices[index].X;
				this.v[pos++] = vertices[index].Y;
				this.v[pos++] = vertices[index].Z;

				index++;
			}

			// normals
			for (int i = 0; i < this.vn.Length; i += 3)
			{
				Vector3 normalized = new Vector3(this.v[i], this.v[i + 1], this.v[i + 2]).Normalized();
				this.vn[i] = normalized.X;
				this.vn[i + 1] = normalized.Y;
				this.vn[i + 2] = normalized.Z;
			}

			this.Update();
			this.UpdateNormals();
		}
	}
}
