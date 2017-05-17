﻿using System;
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

            Window window = new Window(1024, 768, "Aiv.Fast3D Perspective Test", false, 24, 0);
            window.SetDefaultOrthographicSize(10);
            //Window window = new Window("Aiv.Fast3D Perspective Test", 24, 4);



            window.EnableDepthTest();
            //window.CullBackFaces();

            window.SetCursor(false);

            Vector3 cameraEuler = new Vector3(-10, 180, 0);
            PerspectiveCamera camera = new PerspectiveCamera(new Vector3(0, 6, 30), cameraEuler, 60, 0.01f, 1000);

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

            Mesh3 stormTrooper = ObjLoader.Load("Assets/Stormtrooper.obj", Vector3.One)[0];

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

            Mesh3[] botMesh = FbxLoader.Load("Assets/running.fbx", new Vector3(0.02f, 0.02f, 0.02f));

            SkeletalAnimation[] animations = FbxLoader.LoadAnimations("Assets/running.fbx", new Vector3(0.02f, 0.02f, 0.02f));


            int numFrames = 0;

            Mesh3 movingTrooper = ObjLoader.Load("Assets/Stormtrooper.obj", Vector3.One)[0];
            movingTrooper.Position3 = new Vector3(0, 0, 2);
            Vector3 movingTrooperRotation = new Vector3();

            foreach (SkeletalAnimation animation in animations)
            {
                Console.WriteLine(animation.Name);
                foreach (string subject in animation.KeyFrames.Keys)
                {
                    Console.WriteLine(subject);
                    int currentFrames = 0;
                    foreach (SkeletalAnimation.KeyFrame frame in animation.KeyFrames[subject])
                    {
                        Console.WriteLine(frame.Time + " " + frame.Position + " " + frame.Rotation + " " + frame.Scale);
                        currentFrames++;
                    }
                    if (currentFrames > numFrames)
                        numFrames = currentFrames;
                }
            }

            float neckRotation = 0;

            int animationIndex = 0;
            float animationTimer = 1f / animations[0].Fps;

            movingTrooper.SetParent(cube);

            Mesh3[] tank = ObjLoader.Load("Assets/T34.obj", Vector3.One);
            Texture tankDiffuse = new Texture("Assets/T-34.png");
            Texture tankSpecular = new Texture("Assets/T-34_S.png");



            //window.AddPostProcessingEffect(new DepthViewer());

            window.AddPostProcessingEffect(new Fog());

            //window.AddPostProcessingEffect(new Sobel());

            Vector3 pyramidRotation = Vector3.Zero;

            PostProcessingEffect fxaa = window.AddPostProcessingEffect(new FXAA());

            Plane plane = new Plane();
            Vector3 planeEuler = new Vector3();

            Vector3 shadowOffset = new Vector3(0, 0, 0.22f);

            Sphere sphere = new Sphere();
            Vector3 sphereEuler = new Vector3();
            Texture world = new Texture("Assets/world.jpg");

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
                    cameraEuler += new Vector3(0, 90 + 45, 0) * window.deltaTime;

                if (window.GetKey(KeyCode.Left))
                    cameraEuler -= new Vector3(0, 90 + 45, 0) * window.deltaTime;

                if (window.GetKey(KeyCode.P))
                    cameraEuler += new Vector3(90 + 45, 0, 0) * window.deltaTime;

                if (window.GetKey(KeyCode.L))
                    cameraEuler -= new Vector3(90 + 45, 0, 0) * window.deltaTime;

                camera.SetEulerRotation(cameraEuler);

                //fxaa.enabled = window.GetKey(KeyCode.X);

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

                animationTimer -= window.deltaTime;
                if (animationTimer <= 0)
                {
                    animationIndex++;
                    if (animationIndex >= numFrames)
                        animationIndex = 0;
                    animationTimer = 1f / animations[0].Fps;
                }

                foreach (string subject in animations[0].KeyFrames.Keys)
                {
                    if (botMesh[0].HasBone(subject))
                    {
                        Mesh3.Bone bone = botMesh[0].GetBone(subject);
                        if (animationIndex < animations[0].KeyFrames[subject].Count)
                        {
                            //bone.Position = animations[0].KeyFrames[subject][animationIndex].Position;
                            //bone.Rotation = animations[0].KeyFrames[subject][animationIndex].Rotation;
                        }
                    }
                }

                pyramidRotation += new Vector3(0, 10, 0) * window.deltaTime;
                neckRotation += window.deltaTime;

                string boneName = "mixamorig:Neck";
                if (botMesh[0].HasBone(boneName))
                {
                    Mesh3.Bone bone = botMesh[0].GetBone(boneName);
                    bone.Rotation = Quaternion.FromEulerAngles(new Vector3(0, (float)Math.Sin(neckRotation) / 2, 0));
                }

                if (botMesh[1].HasBone(boneName))
                {
                    Mesh3.Bone bone = botMesh[1].GetBone(boneName);
                    bone.Rotation = Quaternion.FromEulerAngles(new Vector3(0, (float)Math.Sin(neckRotation) / 2, 0));
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
                pyramid.SetEulerRotation(pyramidRotation);
                pyramid.Position3 = new Vector3(-6, 2, 10) + shadowOffset;
                pyramid.DrawShadowMap(directionalLight);

                pyramid.Scale3 = new Vector3(1, 2, 1);
                pyramid.SetRotation(Vector3.Zero);
                pyramid.Position3 = new Vector3(-30, 2, 10) + shadowOffset;
                pyramid.DrawShadowMap(directionalLight);


                botMesh[0].Position3 = new Vector3(6, 0, 10) + shadowOffset;
                botMesh[0].DrawShadowMap(directionalLight);
                botMesh[1].Position3 = new Vector3(6, 0, 10) + shadowOffset;
                botMesh[1].DrawShadowMap(directionalLight);

                stormTrooper.Position3 = new Vector3(0, 0, 5) + shadowOffset;
                stormTrooper.SetRotation(Vector3.Zero);
                stormTrooper.DrawShadowMap(directionalLight);
                stormTrooper.Position3 = new Vector3(-5, 0, 5) + shadowOffset;
                stormTrooper.SetRotation(Vector3.Zero);
                stormTrooper.DrawShadowMap(directionalLight);
                stormTrooper.Position3 = new Vector3(7, 0, 5) + shadowOffset;
                stormTrooper.SetRotation(Utils.LookAt((camera.Position3 - stormTrooper.Position3).Normalized()));
                stormTrooper.DrawShadowMap(directionalLight);
                cube.SetEulerRotation(new Vector3(0, crateRotation, 0));
                cube.Position3 = new Vector3(0, 7, 0) + shadowOffset;
                cube.DrawShadowMap(directionalLight);
                cube.SetEulerRotation(Vector3.Zero);
                cube.Position3 = new Vector3(5, 1, 5) + shadowOffset;
                cube.DrawShadowMap(directionalLight);


                foreach (Mesh3 item in tank)
                {
                    item.Position3 = new Vector3(-10, 0, 20) + shadowOffset;
                    item.DrawShadowMap(directionalLight);
                }

                window.DisableCullFaces();
                window.RenderTo(null);

                window.CullBackFaces();
                //window.DisableCullFaces();

                floor.DrawPhong(floorTexture, directionalLight, new Vector3(0.5f, 0.5f, 0.5f), 0, shadowTexture);

                pyramid.Scale3 = new Vector3(1, 2, 1);
                pyramid.SetEulerRotation(pyramidRotation);
                pyramid.Position3 = new Vector3(-6, 2, 10);
                pyramid.DrawGouraud(new Vector4(1, 0, 0, 1), directionalLight, shadowTexture);

                pyramid.Scale3 = new Vector3(1, 2, 1);
                pyramid.SetEulerRotation(Vector3.Zero);
                pyramid.Position3 = new Vector3(-30, 2, 10);
                pyramid.DrawGouraud(new Vector4(1, 1, 0, 1), directionalLight, shadowTexture);

                botMesh[0].Position3 = new Vector3(6, 0, 10);
                botMesh[0].DrawGouraud(new Vector4(0, 0.8f, 1, 1), directionalLight, shadowTexture, 0.01f);
                botMesh[1].Position3 = new Vector3(6, 0, 10);
                botMesh[1].DrawGouraud(new Vector4(1, 0, 0, 1), directionalLight, shadowTexture, 0.01f);

                stormTrooper.Position3 = new Vector3(0, 0, 5);
                stormTrooper.SetRotation(Vector3.Zero);
                stormTrooper.DrawGouraud(stormTrooperTexture, directionalLight, shadowTexture);

                stormTrooper.Position3 = new Vector3(-5, 0, 5);
                stormTrooper.SetRotation(Vector3.Zero);
                stormTrooper.DrawPhong(stormTrooperTexture, directionalLight, new Vector3(0, 0.1f, 0), 0.75f, shadowTexture);

                stormTrooper.Position3 = new Vector3(7, 0, 5);
                stormTrooper.SetRotation(Utils.LookAt((camera.Position3 - stormTrooper.Position3).Normalized()));
                stormTrooper.DrawCel(stormTrooperTexture, directionalLight, new Vector3(0, 0.1f, 0), 0.75f, shadowTexture);


                //cube.DrawColor(new Vector4(1, 0, 0, 1));
                cube.SetEulerRotation(new Vector3(0, crateRotation, 0));
                cube.Position3 = new Vector3(0, 7, 0);
                cube.DrawTexture(crate);

                if (window.GetKey(KeyCode.Space))
                    movingTrooper.Position3 += movingTrooper.Forward * window.deltaTime * 2;

                if (window.GetKey(KeyCode.G))
                    movingTrooperRotation += new Vector3(0, 90, 0) * window.deltaTime;

                if (window.GetKey(KeyCode.H))
                    movingTrooperRotation -= new Vector3(0, 90, 0) * window.deltaTime;
                movingTrooper.SetEulerRotation(movingTrooperRotation);

                movingTrooper.DrawGouraud(new Vector4(1, 0, 0, 1), directionalLight);

                cube.SetEulerRotation(Vector3.Zero);
                cube.Position3 = new Vector3(5, 1, 5);
                cube.DrawGouraud(new Vector4(0, 0, 1, 1), directionalLight, shadowTexture);


                foreach (Mesh3 item in tank)
                {
                    item.Position3 = new Vector3(-10, 0, 20);
                    item.DrawPhong(tankDiffuse, directionalLight, new Vector3(0.1f, 0.1f, 0.1f), tankSpecular);

                }

                plane.Position3 = new Vector3(-13, 5, 0);
                planeEuler += Vector3.UnitY * 30 * window.deltaTime;
                plane.DrawColor(new Vector4(1, 1, 0, 1));

                sphere.Position3 = new Vector3(-5, 2, 20);
                sphereEuler += Vector3.UnitY * 10 * window.deltaTime;
                sphere.SetEulerRotation(sphereEuler);
                sphere.Scale3 = new Vector3(3);
                sphere.DrawPhong(world, directionalLight, new Vector3(0.2f, 0.2f, 0.2f));
                //sphere.DrawColor(new Vector4(1, 0, 0, 1));

                logo.DrawTexture(logoAiv);

                shadow.DrawTexture(shadowTexture);

                // this ensure postprocessing works
                window.DisableCullFaces();

                plane.Position3 = new Vector3(-10, 5, 0);
                planeEuler += Vector3.UnitY * 30 * window.deltaTime;
                plane.SetEulerRotation(planeEuler);
                plane.DrawColor(new Vector4(0, 1, 1, 1));

                window.Update();
            }
        }
    }
}
