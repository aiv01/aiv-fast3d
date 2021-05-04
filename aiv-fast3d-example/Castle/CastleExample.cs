using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Aiv.Fast2D;
using Aiv.Fast3D;

namespace Aiv.Fast3D.Example
{
    class CastleExample
    {
        public static void Run()
        {

            Window window = new Window(1024, 576, "castle");
            window.EnableDepthTest();
            Mesh3[] meshes = SceneImporter.LoadMesh("Castle/Assets/Castle.obj", Vector3.One * 5f);
           
            PerspectiveCamera camera = new PerspectiveCamera(new Vector3(0, 250, 750), new Vector3(10, 180, 0), 60, 0.1f, 1000);

            Console.WriteLine(meshes.Length);

            DirectionalLight sun = new DirectionalLight(new Vector3(-0.5f, 0, -1).Normalized());

            float rot = 0;

            while (window.IsOpened)
            {

                rot += 10 * window.deltaTime;

                foreach (Mesh3 mesh in meshes)
                {
                    mesh.Position3 = new Vector3(0f, 300, 0f);
                    mesh.EulerRotation3 = new Vector3(0, rot, 0);
                    //mesh.DrawWireframe(new Vector4(1, 0, 0, 1));
                    mesh.DrawPhong(new Vector4(1, 0, 0, 1), sun, new Vector3(0.1f, 0.1f, 0.1f));
                }

                window.Update();
            }
        }
    }
}
