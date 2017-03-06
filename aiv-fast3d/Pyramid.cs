using System;
namespace Aiv.Fast3D
{
	public class Pyramid : Mesh3
	{
		public Pyramid()
		{
			this.v = new float[]
			{
                // bottom face
				-1, -1, 1,
				1, -1,-1,
				1, -1, 1,

				-1, -1, 1,
				-1, -1, -1,
				1, -1, -1,

                // front face
				0, 1, 0,
				-1, -1, 1,
				1, -1, 1,

				// back face
				0, 1, 0,
				1, -1, -1,
				-1, -1, -1,

				// right face
				0, 1, 0,
				1, -1, 1,
				1, -1, -1,

				// left face
				0, 1, 0,
				-1, -1, -1,
				-1, -1, 1,
			};

			this.uv = new float[]{

				// bottom face
				0, 0,
				1, 0,
				0, 1,
				1, 0,
				1, 1,
				0, 1,

				// front face
				0.5f, 0.5f,
				0, 1,
				1, 1,

				// back face
				0.5f, 0.5f,
				0, 0,
				1, 0,

				// right face
				0.5f, 0.5f,
				1, 1,
				1, 0,

				// left face
				0.5f, 0.5f,
				0, 0,
				0, 1,

			};

			this.Update();
			this.RegenerateNormals();
		}
	}
}
