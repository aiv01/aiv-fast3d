using System;
namespace Aiv.Fast3D
{
	public class Plane : Mesh3
	{
		public Plane()
		{
			this.v = new float[]
			{
                // front face
                0.5f, 0.5f, 0f,
				-0.5f, 0.5f, 0f,
				-0.5f, -0.5f, 0f,

				0.5f, 0.5f, 0f,
				-0.5f, -0.5f, 0f,
				0.5f, -0.5f, 0
			};

			this.uv = new float[]{
				// front face
				1, 0,
				0, 0,
				0, 1,
				1, 0,
				0, 1,
				1, 1,
			};

			this.Update();
			this.RegenerateNormals();
		}
	}
}
