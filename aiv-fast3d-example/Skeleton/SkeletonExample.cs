using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast3D;
using Aiv.Fast2D;
using OpenTK;

namespace Aiv.Fast3D.Example
{
    class SkeletonExample
    {

        static Bone FindBoneByName(Mesh3 mesh, string name)
        {
            foreach (Bone bone in mesh.Skeleton)
            {
                if (bone.Name == name)
                    return bone;
            }
            return null;
        }

        public static void Run()
        {

            Window window = new Window(1024, 768, "Aiv.Fast3D Skeleton Test", false, 24, 0);
            window.SetDefaultViewportOrthographicSize(10);

            window.SetClearColor(new Vector4(0, 0, 1, 1));

            window.EnableDepthTest();

            PerspectiveCamera camera = new PerspectiveCamera(new Vector3(0, 3, 5), new Vector3(0, 180, 0), 90, 0.01f, 1000);

            Mesh3[] aj = SceneImporter.LoadMesh("Perspective/Assets/RunningMotion.fbx", new Vector3(0.03f));

            DirectionalLight light = new DirectionalLight(new Vector3(0, 0, -1));

            Texture[] textures = SceneImporter.LoadTextures("Perspective/Assets/RunningMotion.fbx");

            SkeletalAnimation[] animations = SceneImporter.LoadSkeletalAnimations("Perspective/Assets/RunningMotion.fbx");

            Material ajMaterial = new Material();
            ajMaterial.Ambient = new Vector3(0.1f);
            ajMaterial.Diffuse = textures[0];
            ajMaterial.SpecularMap = textures[1];
            ajMaterial.NormalMap = textures[2];
            ajMaterial.Shininess = 0.1f;
            ajMaterial.Lights = new Light[] { light };

            Material ajFaceMaterial = new Material();
            ajFaceMaterial.Ambient = new Vector3(0.1f);
            ajFaceMaterial.Diffuse = textures[3];
            ajFaceMaterial.Lights = new Light[] { light };

            Sprite diffuse = new Sprite(2, 2);
            diffuse.Camera = new Camera();

            float rot = 0.0f;

            foreach (Mesh3 mesh in aj)
            {


                foreach (string jointName in animations[0].KeyFrames.Keys)
                {
                    Bone bone = FindBoneByName(mesh, jointName);
                    if (bone == null)
                        continue;
                    bone.Rotation = animations[0].KeyFrames[jointName][0].Rotation;
                    bone.Position = animations[0].KeyFrames[jointName][0].Position;
                    bone.Scale = animations[0].KeyFrames[jointName][0].Scale;
                }

            }

            while (window.IsOpened)
            {

                if (window.GetKey(KeyCode.Space))
                    rot += 1 * window.DeltaTime;


                diffuse.position = new Vector2(0, 0);
                diffuse.DrawTexture(textures[0]);
                diffuse.position = new Vector2(0, 2);
                diffuse.DrawTexture(textures[1]);
                diffuse.position = new Vector2(0, 4);
                diffuse.DrawTexture(textures[2]);
                diffuse.position = new Vector2(0, 6);
                diffuse.DrawTexture(textures[3]);

                if (window.GetKey(KeyCode.S))
                {
                    ajMaterial.SpecularMap = null;
                }
                else
                {
                    ajMaterial.SpecularMap = textures[1];
                }

                if (window.GetKey(KeyCode.N))
                {
                    ajMaterial.NormalMap = null;
                }
                else
                {
                    ajMaterial.NormalMap = textures[2];
                }


                aj[0].Rotation3 = new Vector3(0, rot, 0);
                aj[0].DrawPhong(ajMaterial);
                aj[1].Rotation3 = aj[0].Rotation3;
                aj[1].DrawPhong(ajFaceMaterial);
                aj[2].Rotation3 = aj[0].Rotation3;
                aj[2].DrawPhong(ajFaceMaterial);
                aj[3].Rotation3 = aj[0].Rotation3;
                aj[3].DrawPhong(ajFaceMaterial);

                window.Update();
            }
        }
    }
}
