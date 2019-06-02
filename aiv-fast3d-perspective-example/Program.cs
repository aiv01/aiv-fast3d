using System;
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

            Window window = new Window(1024, 768, "Aiv.Fast3D Perspective Test", false, 24, 0);
            window.SetDefaultOrthographicSize(10);
            //Window window = new Window("Aiv.Fast3D Perspective Test", 24, 4);



            window.EnableDepthTest();
            //window.CullBackFaces();

            //window.SetCursor(false);

            PerspectiveCamera camera = new PerspectiveCamera(new Vector3(0, 6, 30), new Vector3(-10, 180, 0), 60, 0.01f, 1000);

            Texture crate = new Texture("Assets/crate.jpg");

            Texture floorTexture = new Texture("Assets/floor.jpg");
            floorTexture.SetRepeatX();
            floorTexture.SetRepeatY();

            Texture stormTrooperTexture = new Texture("Assets/Stormtrooper.png");
            stormTrooperTexture.SetRepeatX();
            stormTrooperTexture.SetRepeatY();

            Cube cube = new Cube();

            Cube floor = new Cube();
            floor.Scale3 = new Vector3(150, 50f, 150);
            floor.Position3 = new Vector3(0, -25, 0);

            // tiling texture
            for (int i = 0; i < floor.uv.Length; i++)
            {
                floor.uv[i] *= 10;
            }
            floor.UpdateUV();

            Mesh3 stormTrooper = SceneImporter.LoadMesh("Assets/Stormtrooper.obj", Vector3.One)[0];

            //stormTrooper.RegenerateNormals();

            Pyramid pyramid = new Pyramid();

            Texture logoAiv = new Texture("Assets/LogoAIV.png");
            Sprite logo = new Sprite(1 * logoAiv.Ratio, 1);

            Camera hudCamera = new Camera();
            logo.Camera = hudCamera;

            float lightRotation = -30;


            DepthTexture shadowTexture = new DepthTexture(4096, 4096, 24);

            Sprite shadow = new Sprite(5, 5);
            shadow.Camera = hudCamera;

            float shadow_left = -30;
            float shadow_right = 30;
            float shadow_bottom = -30;
            float shadow_top = 30;
            float shadow_near = -30;
            float shadow_far = 30;

            DirectionalLight directionalLight = new DirectionalLight(Utils.EulerRotationToDirection(new Vector3(lightRotation, 180, 0)));
            directionalLight.SetShadowProjection(shadow_left, shadow_right, shadow_bottom, shadow_top, shadow_near, shadow_far);

            //directionalLight.Color = new Vector3(0.5f, 1, 0.5f);

            float crateRotation = 0;



            Mesh3 movingTrooper = SceneImporter.LoadMesh("Assets/Stormtrooper.obj", Vector3.One)[0];
            movingTrooper.Position3 = new Vector3(0, 0, 2);


            movingTrooper.SetParent(cube);

            Mesh3[] tank = SceneImporter.LoadMesh("Assets/T34.obj", Vector3.One);
            foreach (var tankPart in tank)
            {
                Console.WriteLine(tankPart);
            }
            Texture tankDiffuse = new Texture("Assets/T-34.png");
            Texture tankSpecular = new Texture("Assets/T-34_S.png");
            Texture tankNormal = new Texture("Assets/T-34_N.png");


            Mesh3[] ajParts = SceneImporter.LoadMesh("Assets/RunningMotion.fbx", new Vector3(0.03f));



            //window.AddPostProcessingEffect(new DepthViewer());

            window.AddPostProcessingEffect(new Fog());

            window.AddPostProcessingEffect(new Sobel());

            Vector3 pyramidRotation = Vector3.Zero;

            PostProcessingEffect fxaa = window.AddPostProcessingEffect(new FXAA());

            Plane plane = new Plane();

            Vector3 shadowOffset = new Vector3(0, 0, 0.22f);

            Sphere sphere = new Sphere();
            Texture world = new Texture("Assets/world.jpg");

            Cube wall = new Cube();
            Texture bricksNormal = new Texture("Assets/normal_mapping_normal_map.png");

            PointLight pointLight = new PointLight(new Vector3(11, 10, 20), 10f);
            pointLight.Color = new Vector3(0, 1, 0);

            PointLight pointLight2 = new PointLight(new Vector3(17, 3, 20), 10f);
            pointLight2.Color = new Vector3(0, 0, 1);

            PointLight pointLight3 = new PointLight(new Vector3(11, 3, 20), 2.2f);
            pointLight3.Color = new Vector3(1, 0, 0);


            Material wallMaterial = new Material();
            wallMaterial.DiffuseColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
            wallMaterial.EmissiveMap = logoAiv;
            wallMaterial.Lights[0] = directionalLight;
            wallMaterial.Lights[1] = pointLight;
            wallMaterial.Lights[2] = pointLight2;
            wallMaterial.Lights[3] = pointLight3;
            wallMaterial.NormalMap = bricksNormal;




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
                    // currently broken, the mouse is too flaky
                    //float yaw = MouseX(window) * (90 + 45f);
                    //float pitch = MouseY(window) * 90f;
                    //camera.EulerRotation3 += new Vector3(pitch, yaw, 0) * window.deltaTime;
                }

                if (window.GetKey(KeyCode.Right))
                    camera.EulerRotation3 += new Vector3(0, 90 + 45, 0) * window.deltaTime;

                if (window.GetKey(KeyCode.Left))
                    camera.EulerRotation3 -= new Vector3(0, 90 + 45, 0) * window.deltaTime;

                if (window.GetKey(KeyCode.P))
                    camera.EulerRotation3 += new Vector3(90 + 45, 0, 0) * window.deltaTime;

                if (window.GetKey(KeyCode.L))
                    camera.EulerRotation3 -= new Vector3(90 + 45, 0, 0) * window.deltaTime;


                fxaa.enabled = !window.GetKey(KeyCode.X);

                if (window.GetKey(KeyCode.Num1))
                {
                    shadowOffset += Vector3.UnitX * 2 * window.deltaTime;
                    Console.WriteLine(shadowOffset);
                }

                if (window.GetKey(KeyCode.Num2))
                {
                    shadowOffset -= Vector3.UnitX * 2 * window.deltaTime;
                    Console.WriteLine(shadowOffset);
                }

                if (window.GetKey(KeyCode.Num3))
                {
                    shadowOffset += Vector3.UnitY * 2 * window.deltaTime;
                    Console.WriteLine(shadowOffset);
                }

                if (window.GetKey(KeyCode.Num4))
                {
                    shadowOffset -= Vector3.UnitY * 2 * window.deltaTime;
                    Console.WriteLine(shadowOffset);
                }

                if (window.GetKey(KeyCode.Num5))
                {
                    shadowOffset += Vector3.UnitZ * 0.2f * window.deltaTime;
                    Console.WriteLine(shadowOffset);
                }

                if (window.GetKey(KeyCode.Num6))
                {
                    shadowOffset -= Vector3.UnitZ * 2 * window.deltaTime;
                    Console.WriteLine(shadowOffset);
                }



                directionalLight.SetShadowProjection(shadow_left, shadow_right, shadow_bottom, shadow_top, shadow_near, shadow_far);



                pyramidRotation += new Vector3(0, 10, 0) * window.deltaTime;

                if (window.mouseLeft)
                {
                    float x = window.mouseX * (1f / window.OrthoWidth);
                    float y = 1 - (window.mouseY * (1f / window.OrthoHeight));
                    x = (2 * x) - 1;
                    y = (2 * y) - 1;
                    Console.WriteLine(x + "," + y);
                    Vector3 direction = camera.ScreenPointToDirection(x, y);
                    Console.WriteLine(direction);
                }


                crateRotation += 30 * window.deltaTime;

                //lightRotation += 10 * window.deltaTime;
                //directionalLight.UpdateDirection(Utils.EulerRotationToDirection(new Vector3(lightRotation, 180, 0)));

                window.DisableCullFaces();
                // draw shadow map texture
                window.RenderTo(shadowTexture);
                window.CullFrontFaces();
                floor.DrawShadowMap(directionalLight);
                pyramid.Scale3 = new Vector3(1, 2, 1);
                pyramid.EulerRotation3 = pyramidRotation;
                pyramid.Position3 = new Vector3(-6, 2, 10) + shadowOffset;
                pyramid.DrawShadowMap(directionalLight);

                pyramid.Scale3 = new Vector3(1, 2, 1);
                pyramid.Rotation3 = Vector3.Zero;
                pyramid.Position3 = new Vector3(-30, 2, 10) + shadowOffset;
                pyramid.DrawShadowMap(directionalLight);

                stormTrooper.Position3 = new Vector3(0, 0, 5) + shadowOffset;
                stormTrooper.Rotation3 = Vector3.Zero;
                stormTrooper.DrawShadowMap(directionalLight);
                stormTrooper.Position3 = new Vector3(-5, 0, 5) + shadowOffset;
                stormTrooper.Rotation3 = Vector3.Zero;
                stormTrooper.DrawShadowMap(directionalLight);
                stormTrooper.Position3 = new Vector3(7, 0, 5) + shadowOffset;
                stormTrooper.Rotation3 = Utils.LookAt((camera.Position3 - stormTrooper.Position3).Normalized());
                stormTrooper.DrawShadowMap(directionalLight);
                cube.EulerRotation3 = new Vector3(0, crateRotation, 0);
                cube.Position3 = new Vector3(0, 7, 0) + shadowOffset;
                cube.DrawShadowMap(directionalLight);
                cube.EulerRotation3 = Vector3.Zero;
                cube.Position3 = new Vector3(5, 1, 5) + shadowOffset;
                cube.DrawShadowMap(directionalLight);


                foreach (Mesh3 item in tank)
                {
                    item.Position3 = new Vector3(-10, 0, 20) + shadowOffset;
                    item.DrawShadowMap(directionalLight);
                }

                foreach (Mesh3 ajPart in ajParts)
                {
                    ajPart.Position3 = new Vector3(-15, 0, 20) + shadowOffset;
                    ajPart.DrawShadowMap(directionalLight);
                }

                window.DisableCullFaces();
                window.RenderTo(null);

                window.CullBackFaces();
                //window.DisableCullFaces();

                floor.DrawPhong(floorTexture, directionalLight, new Vector3(0.5f, 0.5f, 0.5f), 0, shadowTexture);

                pyramid.Scale3 = new Vector3(1, 2, 1);
                pyramid.EulerRotation3 = pyramidRotation;
                pyramid.Position3 = new Vector3(-6, 2, 10);
                pyramid.DrawGouraud(new Vector4(1, 0, 0, 1), directionalLight, shadowTexture);

                pyramid.Scale3 = new Vector3(1, 2, 1);
                pyramid.EulerRotation3 = Vector3.Zero;
                pyramid.Position3 = new Vector3(-30, 2, 10);
                pyramid.DrawGouraud(new Vector4(1, 1, 0, 1), directionalLight, shadowTexture);


                stormTrooper.Position3 = new Vector3(0, 0, 5);
                stormTrooper.Rotation3 = Vector3.Zero;
                stormTrooper.DrawGouraud(stormTrooperTexture, directionalLight, shadowTexture);

                stormTrooper.Position3 = new Vector3(-5, 0, 5);
                stormTrooper.Rotation3 = Vector3.Zero;
                stormTrooper.DrawPhong(stormTrooperTexture, directionalLight, new Vector3(0, 0.1f, 0), 0.75f, shadowTexture);

                stormTrooper.Position3 = new Vector3(7, 0, 5);
                stormTrooper.Rotation3 = Utils.LookAt((camera.Position3 - stormTrooper.Position3).Normalized());
                stormTrooper.DrawCel(stormTrooperTexture, directionalLight, new Vector3(0, 0.1f, 0), 0.75f, shadowTexture);


                //cube.DrawColor(new Vector4(1, 0, 0, 1));
                cube.EulerRotation3 = new Vector3(0, crateRotation, 0);
                cube.Position3 = new Vector3(0, 7, 0);
                cube.DrawTexture(crate);

                if (window.GetKey(KeyCode.Space))
                    movingTrooper.Position3 += movingTrooper.Forward * window.deltaTime * 2;

                if (window.GetKey(KeyCode.G))
                    movingTrooper.EulerRotation3 += new Vector3(0, 90, 0) * window.deltaTime;

                if (window.GetKey(KeyCode.H))
                    movingTrooper.EulerRotation3 -= new Vector3(0, 90, 0) * window.deltaTime;

                movingTrooper.DrawGouraud(new Vector4(1, 0, 0, 1), directionalLight);

                cube.EulerRotation3 = Vector3.Zero;
                cube.Position3 = new Vector3(5, 1, 5);
                cube.DrawGouraud(new Vector4(0, 0, 1, 1), directionalLight, shadowTexture);


                foreach (Mesh3 item in tank)
                {
                    item.Position3 = new Vector3(-10, 0, 20);
                    item.DrawPhong(tankDiffuse, directionalLight, new Vector3(0.1f, 0.1f, 0.1f), tankSpecular, null, 0, window.GetKey(KeyCode.B) ? tankNormal : null);
                }

                foreach (Mesh3 ajPart in ajParts)
                {
                    ajPart.Position3 = new Vector3(-15, 0, 20) + shadowOffset;
                    ajPart.DrawPhong(new Vector4(1, 1, 0, 1), directionalLight, new Vector3(0.1f, 0.1f, 0.1f));
                }

                plane.Position3 = new Vector3(-13, 5, 0);
                plane.EulerRotation3 += Vector3.UnitY * 30 * window.deltaTime;
                plane.DrawColor(new Vector4(1, 1, 0, 1));

                sphere.Position3 = new Vector3(-5, 2, 20);
                sphere.EulerRotation3 += Vector3.UnitY * 10 * window.deltaTime;
                sphere.Scale3 = new Vector3(3);
                sphere.DrawPhong(world, directionalLight, new Vector3(0.2f, 0.2f, 0.2f));
                //sphere.DrawColor(new Vector4(1, 0, 0, 1));

                wall.Position3 = new Vector3(8, 3, 15);
                wall.Scale3 = new Vector3(3, 3, 1);
                wall.EulerRotation3 += new Vector3(0, 30, 0) * window.deltaTime;
                wall.DrawPhong(new Vector4(0.4f, 0.2f, 0, 1), directionalLight, new Vector3(0.2f, 0.2f, 0.2f), 0, null, 0, bricksNormal);

                pointLight.Position = camera.Position3;
                wall.Position3 = new Vector3(11, 3, 20);
                wallMaterial.Shininess = window.GetKey(KeyCode.K) ? 0.7f : 0;
                wall.DrawPhong(wallMaterial);


                logo.DrawTexture(logoAiv);

                shadow.DrawTexture(shadowTexture);

                // this ensure postprocessing works
                window.DisableCullFaces();

                plane.Position3 = new Vector3(-10, 5, 0);
                plane.EulerRotation3 += Vector3.UnitY * 30 * window.deltaTime;
                plane.DrawColor(new Vector4(0, 1, 1, 1));

                window.Update();
            }
        }
    }
}
