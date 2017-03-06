using System;
namespace Aiv.Fast3D
{
	public class Cube : Mesh3
	{
		public Cube()
		{
			this.v = new float[]
			{
                // front face
                1, 1, 1,
				-1, 1, 1,
				-1, -1, 1,

				1, 1, 1,
				-1, -1, 1,
				1, -1, 1,

                // back face
				-1, 1, -1,
                1, 1, -1,
				-1, -1, -1,

				-1, -1, -1,
				1, 1, -1,
				1, -1, -1,

                // bottom face
				-1, -1, 1,
				1, -1,-1,
				1, -1, 1,

				-1, -1, 1,
				-1, -1, -1,
				1, -1, -1,

                // right face
				1, 1, -1,
                1, 1, 1,
				1, -1, 1,

				1, -1, -1,
				1, 1, -1,
				1, -1, 1,

                // left face
				-1, 1, 1,
                -1, 1, -1,
				-1, -1, -1,

				-1, -1, 1,
				-1, 1, 1,
				-1, -1, -1,

                
                // top face
				1, 1,-1,
                -1, 1, 1,
				1, 1, 1,

				-1, 1, -1,
				-1, 1, 1,
				1, 1, -1,
			};

			this.uv = new float[]{
				// front face
				1, 0,
				0, 0,
				0, 1,
				1, 1,
				1, 0,
				0, 1,

				// back face
				1, 0,
				0, 0,
				0, 1,
				1, 1,
				1, 0,
				0, 1,

				// bottom face
				0, 0,
				1, 0,
				0, 1,
				1, 0,
				1, 1,
				0, 1,

				// right face
				1, 0,
				0, 0,
				0, 1,
				1, 1,
				1, 0,
				0, 1,

				// left face
				1, 0,
				0, 0,
				0, 1,
				1, 1,
				1, 0,
				0, 1,

				// top face
				1, 0,
				0, 0,
				0, 1,
				1, 1,
				1, 0,
				0, 1
			};

			this.Update();
			this.RegenerateNormals();
		}
	}
}
