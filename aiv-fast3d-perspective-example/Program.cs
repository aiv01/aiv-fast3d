using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D;
using Aiv.Fast3D;
using OpenTK;

namespace Aiv.Fast3D.Perspective.Example
{
	class Program
	{
		static float oldX;
		static float oldY;

		static float MouseX(Window window)
		{
			float value = 0;
			float newX = window.RawMouseX;
			if (newX > oldX)
				value = 1;
			if (newX < oldX)
				value = -1;
			oldX = newX;
			return value;
		}

		static float MouseY(Window window)
		{
			float value = 0;
			float newY = window.RawMouseY;
			if (newY > oldY)
				value = 1;
			if (newY < oldY)
				value = -1;
			oldY = newY;
			return value;
		}


		static void Main(string[] args)
		{
			Window window = new Window(1024, 768, "Aiv.Fast3D Perspective Test", false, 24, 4);
			//Window window = new Window("Aiv.Fast3D Perspective Test", 24, 4);
			window.SetPerspective(60);
			window.SetZNearZFar(0.01f, 1000);
			window.EnableDepthTest();
			window.CullBackFaces();

			window.SetCursor(false);

			PerspectiveCamera camera = new PerspectiveCamera(new Vector3(0, 3, 30), new Vector3(0, 0, 180f));

			Texture crate = new Texture("Assets/crate.jpg");

			Texture stormTrooperTexture = new Texture("Assets/Stormtrooper.png");
			stormTrooperTexture.SetRepeatX();
			stormTrooperTexture.SetRepeatY();

			Cube cube = new Cube();

			Cube floor = new Cube();
			floor.Scale3 = new Vector3(10, 0.1f, 10);
			floor.Position3 = new Vector3(0, -1, 0);

			Mesh3 stormTrooper = ObjLoader.Load("Assets/Stormtrooper.obj", Vector3.One)[0];

			//stormTrooper.RegenerateNormals();

			Pyramid pyramid = new Pyramid();

			while (window.IsOpened)
			{

				if (window.GetKey(KeyCode.Esc))
					break;

				if (window.GetKey(KeyCode.W))
					camera.Position3 += camera.Forward * 10 * window.deltaTime;

				if (window.GetKey(KeyCode.S))
					camera.Position3 += -camera.Forward * 10 * window.deltaTime;

				if (window.GetKey(KeyCode.D))
					camera.Position3 += camera.Right * 10 * window.deltaTime;

				if (window.GetKey(KeyCode.A))
					camera.Position3 += -camera.Right * 10 * window.deltaTime;

				if (window.GetKey(KeyCode.Up))
					camera.Position3 += camera.Up * 10 * window.deltaTime;

				if (window.GetKey(KeyCode.Down))
					camera.Position3 += -camera.Up * 10 * window.deltaTime;

				if (window.HasFocus)
				{
					float yaw = MouseX(window) * (90 + 45f);
					float pitch = MouseY(window) * 90f;
					pitch = 0;
					camera.EulerRotation3 += new Vector3(pitch, yaw, 0) * window.deltaTime;
				}


				floor.DrawColor(new Vector4(0, 1, 0, 1));

				pyramid.Scale3 = new Vector3(1, 2, 1);
				pyramid.Position3 = new Vector3(-6, 2, 10);
				pyramid.DrawGouraud(new Vector4(1, 0, 0, 1), new Vector3(50, 50, 20), new Vector3(1, 1, 1));

				stormTrooper.Position3 = new Vector3(0, 0, 5);
				stormTrooper.DrawGouraud(stormTrooperTexture, new Vector3(50, 50, 20), new Vector3(1, 1, 1));

				stormTrooper.Position3 = new Vector3(-5, 0, 5);
				stormTrooper.DrawPhong(stormTrooperTexture, new Vector3(50, 50, 20), new Vector3(1, 1, 1), new Vector3(0, 0.1f, 0));

				//cube.DrawColor(new Vector4(1, 0, 0, 1));

				cube.Position3 = new Vector3(0, 7, 0);
				cube.DrawTexture(crate);

				cube.Position3 = new Vector3(5, 1, 5);
				cube.DrawGouraud(new Vector4(0, 0, 1, 1), new Vector3(50, 50, 20), new Vector3(1, 1, 1));

				window.Update();
			}
		}
	}
}
