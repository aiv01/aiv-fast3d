﻿using System;
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

			this.vn = new float[] {
				// front face
				0, 0, 1,
				0, 0, 1,
				0, 0, 1,
				0, 0, 1,
				0, 0, 1,
				0, 0, 1,
				// back face
				0, 0, -1,
				0, 0, -1,
				0, 0, -1,
				0, 0, -1,
				0, 0, -1,
				0, 0, -1,
				// bottom face
				0, -1, 0,
				0, -1, 0,
				0, -1, 0,
				0, -1, 0,
				0, -1, 0,
				0, -1, 0,
				// right face
				1, 0, 0,
				1, 0, 0,
				1, 0, 0,
				1, 0, 0,
				1, 0, 0,
				1, 0, 0,
				// left face
				-1, 0, 0,
				-1, 0, 0,
				-1, 0, 0,
				-1, 0, 0,
				-1, 0, 0,
				-1, 0, 0,
				// top face
				0, 1, 0,
				0, 1, 0,
				0, 1, 0,
				0, 1, 0,
				0, 1, 0,
				0, 1, 0
			};

			this.Update();
			this.UpdateNormals();
		}
	}
}