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
    class Program
    {
        static void Main(string[] args)
        {
            Window window = new Window(1024, 768, "3D Test");
            window.SetDefaultOrthographicSize(200);
            window.SetZNearZFar(-300, 300);

            window.EnableDepthTest();

            window.SetClearColor(0f, 1f, 1f, 1f);

            Texture backgroundTexture = new Texture("Assets/airadventurelevel1.png");

            Sprite background = new Sprite(window.OrthoWidth, window.OrthoHeight);


            Mesh3 mesh3 = new Mesh3();

            Obj suzanne = new Obj("Assets/suzanne.obj", new Vector3(50, -50, 50));
            suzanne.Position3 = new Vector3(75, 100, 100);

            suzanne.vc = new float[suzanne.v.Length / 3 * 4];
            int vcPos = 0;
            for (int i = 0; i < suzanne.v.Length; i += 3)
            {
                suzanne.vc[vcPos++] = suzanne.v[i] / 50f;
                suzanne.vc[vcPos++] = suzanne.v[i + 1] / -50f;
                suzanne.vc[vcPos++] = suzanne.v[i + 2] / 50f;
                suzanne.vc[vcPos++] = 1f;
            }
            suzanne.UpdateVertexColor();

            Obj stormTrooper = new Obj("Assets/Stormtrooper.obj", new Vector3(50, -50, 50));
            stormTrooper.Position3 = new Vector3(200, 200, 100);

            Texture texture = new Texture("Assets/Stormtrooper.png");

            mesh3.v = new float[]
            {
                // front face
                0, 0, 0,
                100, 0, 0,
                0, 100, 0,

                100, 0, 0,
                100, 100, 0,
                0, 100, 0,

                // back face
                0, 0, -100,
                100, 0, -100,
                0, 100, -100,

                100, 0, -100,
                100, 100, -100,
                0, 100, -100,

                // bottom face
                0, 100, 0,
                0, 100, -100,
                100, 100, 0,

                0, 100, -100,
                100, 100, -100,
                100, 100, 0,

                // right face
                100, 0, 0,
                100, 0, -100,
                100, 100, 0,

                100, 0, -100,
                100, 100, -100,
                100, 100, 0,

                // left face
                0, 0, 0,
                0, 0, -100,
                0, 100, 0,

                0, 0, -100,
                0, 100, -100,
                0, 100, 0,

                
                // top face
                0, 0, 0,
                0, 0,- 100,
                100, 0, 0,

                0, 0, -100,
                100, 0, -100,
                100, 0, 0,


            };
            mesh3.UpdateVertex();

            mesh3.Pivot3 = new Vector3(50, 50, -50);

            mesh3.Position3 = new Vector3(130, 130, 100);

            mesh3.Scale3 = new Vector3(0.5f, 0.5f, 0.5f);

            while (window.opened)
            {
                if (window.GetKey(KeyCode.Esc))
                    break;

                if (window.GetKey(KeyCode.Up))
                {
                    mesh3.EulerRotation3 += new Vector3(-20 * window.deltaTime, 0, 0);
                    suzanne.EulerRotation3 += new Vector3(0, -30 * window.deltaTime, 0);
                    stormTrooper.EulerRotation3 += new Vector3(0, 30 * window.deltaTime, 0);
                }

                mesh3.EulerRotation3 += new Vector3(0, -10 * window.deltaTime, 0);

                background.DrawTexture(backgroundTexture);

                mesh3.DrawWireframe(1f, 0f, 0f, 1f);

                suzanne.Draw();

                stormTrooper.DrawTexture(texture);

                window.Update();
            }
        }
    }
}
