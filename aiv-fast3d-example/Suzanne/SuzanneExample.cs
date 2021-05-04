using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D;
using Aiv.Fast3D;
using OpenTK;

namespace Aiv.Fast3D.Example
{
	class SuzanneExample
	{
		public static void Run()
		{
			Window window = new Window(1024, 768, "3D Suzanne Test");
			window.SetDefaultOrthographicSize(10);
			window.SetZNearZFar(-300, 300);

			window.EnableDepthTest();
			window.CullBackFaces();


			Mesh3 suzanne = SceneImporter.LoadMesh("Orthographic/Assets/suzanne.obj", new Vector3(2, -2, 2))[0];
			suzanne.Position3 = new Vector3(window.OrthoWidth/2.0f, window.OrthoHeight/2.0f, 0);


			//PerspectiveCamera camera = new PerspectiveCamera(new Vector3(0, 6, 30), new Vector3(-10, 180, 0), 60, 0.01f, 1000);
			//suzanne.Camera = camera;
			
			suzanne.vc = new float[suzanne.v.Length / 3 * 4];
			int vcPos = 0;
			for (int i = 0; i < suzanne.v.Length; i += 3)
			{
				suzanne.vc[vcPos++] = suzanne.v[i] / 2f;
				suzanne.vc[vcPos++] = suzanne.v[i + 1] / -2f;
				suzanne.vc[vcPos++] = suzanne.v[i + 2] / 2f;
				suzanne.vc[vcPos++] = 0.5f;
			}
			suzanne.UpdateVertexColor();
			

			while (window.opened) {
				suzanne.Draw();

				suzanne.EulerRotation3 += new Vector3(0, 10, 0) * window.deltaTime;
				window.Update();
			}
		}
	}
}
