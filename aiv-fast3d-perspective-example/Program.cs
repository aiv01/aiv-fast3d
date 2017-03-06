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
			window.SetDefaultOrthographicSize(10);
			//Window window = new Window("Aiv.Fast3D Perspective Test", 24, 4);

			window.EnableDepthTest();
			window.CullBackFaces();

			window.SetCursor(false);

			PerspectiveCamera camera = new PerspectiveCamera(new Vector3(0, 3, 30), new Vector3(0, 0, 180f), 60, 0.01f, 1000);

			Texture crate = new Texture("Assets/crate.jpg");

			Texture floorTexture = new Texture("Assets/floor.jpg");
			floorTexture.SetRepeatX();
			floorTexture.SetRepeatY();

			Texture stormTrooperTexture = new Texture("Assets/Stormtrooper.png");
			stormTrooperTexture.SetRepeatX();
			stormTrooperTexture.SetRepeatY();

			Cube cube = new Cube();

			Cube floor = new Cube();
			floor.Scale3 = new Vector3(50, 50f, 50);
			floor.Position3 = new Vector3(0, -25, 0);

			// tiling texture
			for (int i = 0; i < floor.uv.Length; i++)
			{
				floor.uv[i] *= 10;
			}
			floor.UpdateUV();

			Mesh3 stormTrooper = ObjLoader.Load("Assets/Stormtrooper.obj", Vector3.One)[0];

			//stormTrooper.RegenerateNormals();

			Pyramid pyramid = new Pyramid();

			Texture logoAiv = new Texture("Assets/LogoAIV.png");
			Sprite logo = new Sprite(1 * logoAiv.Ratio, 1);

			Camera hudCamera = new Camera();
			logo.Camera = hudCamera;

			Mesh3 skySphere = ObjLoader.Load("Assets/SM_SkySphere.OBJ", Vector3.One)[0];

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
					camera.EulerRotation3 += new Vector3(pitch, yaw, 0) * window.deltaTime;
				}

				if (window.GetKey(KeyCode.Right))
					camera.EulerRotation3 += new Vector3(0, 90 + 45, 0) * window.deltaTime;

				if (window.GetKey(KeyCode.Left))
					camera.EulerRotation3 -= new Vector3(0, 90 + 45, 0) * window.deltaTime;

				window.DisableCullFaces();
				skySphere.Scale3 = new Vector3(100, 100, 100);
				skySphere.DrawColor(new Vector4(1, 1, 1, 1));
				window.CullBackFaces();

				floor.DrawPhong(floorTexture, new Vector3(50, 50, 20), new Vector3(1, 1, 1), new Vector3(0.2f, 0.2f, 0.2f));

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

				logo.DrawTexture(logoAiv);

				window.Update();
			}
		}
	}
}
