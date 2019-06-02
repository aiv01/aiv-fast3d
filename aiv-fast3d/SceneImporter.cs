using Assimp;
using OpenTK;
using System.Collections.Generic;
using Aiv.Fast2D;
using System.IO;

namespace Aiv.Fast3D
{
    public static class SceneImporter
    {

        private static OpenTK.Quaternion EvaluateAnimationNodeQuaternion(NodeAnimationChannel channel, float t)
        {
            OpenTK.Quaternion previous = OpenTK.Quaternion.Identity;
            float previousT = 0.0f;
            foreach (QuaternionKey qk in channel.RotationKeys)
            {
                Assimp.Matrix4x4 matrix = new Assimp.Matrix4x4(qk.Value.GetMatrix());
                OpenTK.Quaternion current = GetOpenTKMatrix(matrix).ExtractRotation();
                if (qk.Time > t)
                {
                    return OpenTK.Quaternion.Slerp(previous, current, (t - previousT) / (((float)(qk.Time)) - previousT));
                }
                else if (qk.Time == t)
                {
                    return current;
                }
                previous = current;
                previousT = (float)qk.Time;
            }

            return previous;
        }

        private static OpenTK.Vector3 EvaluateAnimationNodeTranslation(NodeAnimationChannel channel, float t)
        {
            OpenTK.Vector3 previous = OpenTK.Vector3.Zero;
            float previousT = 0.0f;
            foreach (VectorKey vk in channel.PositionKeys)
            {
                OpenTK.Vector3 current = new OpenTK.Vector3(vk.Value.X, vk.Value.Y, vk.Value.Z);
                if (vk.Time > t)
                {
                    return OpenTK.Vector3.Lerp(previous, current, (t - previousT) / (((float)(vk.Time)) - previousT));
                }
                else if (vk.Time == t)
                {
                    return current;
                }
                previous = current;
                previousT = (float)vk.Time;
            }

            return previous;
        }

        private static OpenTK.Vector3 EvaluateAnimationNodeScale(NodeAnimationChannel channel, float t)
        {
            OpenTK.Vector3 previous = OpenTK.Vector3.Zero;
            float previousT = 0.0f;
            foreach (VectorKey vk in channel.ScalingKeys)
            {
                OpenTK.Vector3 current = new OpenTK.Vector3(vk.Value.X, vk.Value.Y, vk.Value.Z);
                if (vk.Time > t)
                {
                    return OpenTK.Vector3.Lerp(previous, current, (t - previousT) / (((float)(vk.Time)) - previousT));
                }
                else if (vk.Time == t)
                {
                    return current;
                }
                previous = current;
                previousT = (float)vk.Time;
            }

            return previous;
        }

        static OpenTK.Matrix4 GetOpenTKMatrix(Assimp.Matrix4x4 m)
        {
            m.Transpose();
            return new Matrix4(m.A1, m.A2, m.A3, m.A4,
                    m.B1, m.B2, m.B3, m.B4,
                    m.C1, m.C2, m.C3, m.C4,
                    m.D1, m.D2, m.D3, m.D4);
        }

        public static SkeletalAnimation[] LoadSkeletalAnimations(string fileName, float fps = 30.0f)
        {

            AssimpContext context = new AssimpContext();
            Scene scene = context.ImportFile(fileName);

            List<SkeletalAnimation> animations = new List<SkeletalAnimation>();

            for (int i = 0; i < scene.AnimationCount; i++)
            {
                Animation animation = scene.Animations[i];

                float Duration = (float)(animation.DurationInTicks / animation.TicksPerSecond);

                int nFrames = (int)(fps * Duration);

                SkeletalAnimation skelAnimation = new SkeletalAnimation(animation.Name, nFrames, fps);

                foreach (NodeAnimationChannel channel in animation.NodeAnimationChannels)
                {
                    for (int j = 0; j < nFrames; j++)
                    {
                        float t = j * (1.0f / fps);
                        // first rotations
                        OpenTK.Quaternion rotation = EvaluateAnimationNodeQuaternion(channel, t);
                        OpenTK.Vector3 translation = EvaluateAnimationNodeTranslation(channel, t);
                        OpenTK.Vector3 scale = EvaluateAnimationNodeScale(channel, t);
                        skelAnimation.SetKeyFrame(channel.NodeName, j, translation, rotation, scale);
                    }
                }

                animations.Add(skelAnimation);
            }

            return animations.ToArray();
        }

        public static Texture[] LoadTextures(string fileName)
        {


            AssimpContext context = new AssimpContext();

            Scene scene = context.ImportFile(fileName);

            Texture[] textures = new Texture[scene.TextureCount];

            for (int i = 0; i < textures.Length; i++)
            {
                EmbeddedTexture eTexture = scene.Textures[i];
                if (eTexture.IsCompressed)
                {
                    MemoryStream stream = new MemoryStream(eTexture.CompressedData);
                    textures[i] = new Texture(stream);
                }
                else
                {

                    textures[i] = new Texture(eTexture.Width, eTexture.Height);
                    int pixelIndex = 0;
                    foreach (Texel texel in eTexture.NonCompressedData)
                    {
                        textures[i].Bitmap[pixelIndex++] = texel.R;
                        textures[i].Bitmap[pixelIndex++] = texel.G;
                        textures[i].Bitmap[pixelIndex++] = texel.B;
                        textures[i].Bitmap[pixelIndex++] = texel.A;
                    }
                    textures[i].Update();
                }
            }


            // over-engineering (just to be sure...)
            scene.Clear();
            System.GC.Collect();

            return textures;
        }

        public static Mesh3[] LoadMesh(string fileName, Vector3 multiplier, bool flipUV = false)
        {
            AssimpContext context = new AssimpContext();

            Scene scene = context.ImportFile(fileName, PostProcessSteps.Triangulate | PostProcessSteps.GenerateSmoothNormals);
            Mesh3[] meshes = new Mesh3[scene.MeshCount];

            for (int i = 0; i < meshes.Length; i++)
            {
                Assimp.Mesh currentMesh = scene.Meshes[i];
                meshes[i] = new Mesh3();
                meshes[i].Name = currentMesh.Name;
                List<float> vertices = new List<float>();
                List<float> normals = new List<float>();
                List<float> uvs = new List<float>();
                List<int> influences0 = new List<int>();
                List<int> influences1 = new List<int>();
                List<float> weights0 = new List<float>();
                List<float> weights1 = new List<float>();

                Dictionary<int, List<int>> influences = new Dictionary<int, List<int>>();
                Dictionary<int, List<float>> weights = new Dictionary<int, List<float>>();

                if (currentMesh.HasBones)
                {

                    Dictionary<string, Aiv.Fast3D.Bone> bonesMapping = new Dictionary<string, Aiv.Fast3D.Bone>();
                    foreach (Assimp.Bone bone in currentMesh.Bones)
                    {
                        BuildSkeleton(scene, bone.Name, bone.OffsetMatrix, bonesMapping);
                    }

                    meshes[i].Skeleton = FixSkeleton(scene, bonesMapping);

                    // now we can get influences and weights

                    foreach (Assimp.Bone bone in currentMesh.Bones)
                    {
                        foreach (VertexWeight vw in bone.VertexWeights)
                        {
                            if (!influences.ContainsKey(vw.VertexID))
                            {
                                influences.Add(vw.VertexID, new List<int>());
                                weights.Add(vw.VertexID, new List<float>());
                            }

                            // no more than 8 influences
                            if (influences[vw.VertexID].Count < 8)
                            {
                                influences[vw.VertexID].Add(bonesMapping[bone.Name].Index);
                                weights[vw.VertexID].Add(vw.Weight);
                            }
                        }
                    }
                }

                foreach (Face face in currentMesh.Faces)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        int index = face.Indices[j];
                        Vector3D vertex = currentMesh.Vertices[index];
                        vertices.Add(vertex.X * multiplier.X);
                        vertices.Add(vertex.Y * multiplier.Y);
                        vertices.Add(vertex.Z * multiplier.Z);
                        if (currentMesh.HasNormals)
                        {
                            Vector3D normal = currentMesh.Normals[index];
                            normals.Add(normal.X);
                            normals.Add(normal.Y);
                            normals.Add(normal.Z);
                        }
                        if (currentMesh.HasTextureCoords(0))
                        {
                            Vector3D uv = currentMesh.TextureCoordinateChannels[0][index];
                            uvs.Add(uv.X);
                            if (!flipUV)
                                uvs.Add(1 - uv.Y);
                            else
                                uvs.Add(uv.Y);
                        }

                        if (meshes[i].Skeleton != null)
                        {
                            for (int k = 0; k < 4; k++)
                            {

                                influences0.Add(influences[index].Count > k ? influences[index][k] : 0);
                                weights0.Add(weights[index].Count > k ? weights[index][k] : 0);
                            }
                            for (int k = 0; k < 4; k++)
                            {
                                influences1.Add(influences[index].Count > (k + 4) ? influences[index][k + 4] : 0);
                                weights1.Add(weights[index].Count > (k + 4) ? weights[index][k + 4] : 0);
                            }
                        }

                    }
                }




                meshes[i].v = vertices.ToArray();
                meshes[i].vn = normals.ToArray();
                meshes[i].uv = uvs.ToArray();
                meshes[i].Update();
                // check if normals are not available (PostProcessSteps.GenerateSmoothNormals should do its job)
                if (normals.Count == 0)
                {
                    meshes[i].RegenerateNormals();
                }
                // check for invalid UVs
                if (uvs.Count == 0)
                {
                    // this will trigger uv regeneration
                    meshes[i].uv = null;
                }
                meshes[i].UpdateNormals();

                if (meshes[i].Skeleton != null)
                {
                    meshes[i].influences0 = influences0.ToArray();
                    meshes[i].influences1 = influences1.ToArray();
                    meshes[i].weights0 = weights0.ToArray();
                    meshes[i].weights1 = weights1.ToArray();
                    meshes[i].UpdateInfluencesAndWeights();
                }

            }



            // over-engineering (just to be sure...)
            scene.Clear();
            System.GC.Collect();

            return meshes;
        }

        public static Mesh3[] LoadMesh(string fileName, bool flipUV = false)
        {
            return LoadMesh(fileName, Vector3.One, flipUV);
        }

        private static void BuildSkeleton(Scene scene, string boneName, Matrix4x4 offsetMatrix, Dictionary<string, Aiv.Fast3D.Bone> bonesMapping)
        {
            Node boneNode = scene.RootNode.FindNode(boneName);
            Aiv.Fast3D.Bone newBone = new Aiv.Fast3D.Bone();
            newBone.Name = boneName;


            OpenTK.Matrix4 matrix = GetOpenTKMatrix(offsetMatrix);

            newBone.BindPosePosition = matrix.ExtractTranslation();
            newBone.BindPoseScale = matrix.ExtractScale();
            newBone.BindPoseRotation = matrix.ExtractRotation();

            matrix = GetOpenTKMatrix(boneNode.Transform);

            newBone.Position = matrix.ExtractTranslation();
            newBone.Scale = matrix.ExtractScale();
            newBone.Rotation = matrix.ExtractRotation();

            bonesMapping.Add(boneName, newBone);
        }

        private static Aiv.Fast3D.Bone[] FixSkeleton(Scene scene, Dictionary<string, Aiv.Fast3D.Bone> bonesMapping)
        {
            List<Aiv.Fast3D.Bone> bones = new List<Bone>(bonesMapping.Values);
            foreach (Aiv.Fast3D.Bone bone in bones)
            {
                Node boneNode = scene.RootNode.FindNode(bone.Name);
                Node parentNode = boneNode.Parent;
                Bone currentBone = bone;

                while (parentNode != null && parentNode != scene.RootNode)
                {
                    if (bonesMapping.ContainsKey(parentNode.Name))
                    {
                        currentBone.Parent = bonesMapping[parentNode.Name];
                    }
                    else
                    {
                        Aiv.Fast3D.Bone newBone = new Aiv.Fast3D.Bone();
                        newBone.Name = parentNode.Name;
                        OpenTK.Matrix4 matrix = GetOpenTKMatrix(parentNode.Transform);

                        newBone.Position = matrix.ExtractTranslation();
                        newBone.Scale = matrix.ExtractScale();
                        newBone.Rotation = matrix.ExtractRotation();

                        bonesMapping.Add(newBone.Name, newBone);
                        currentBone.Parent = newBone;
                    }
                    currentBone = currentBone.Parent;
                    parentNode = parentNode.Parent;
                }
            }

            // now check for multiple roots
            int rootsCounter = 0;
            Aiv.Fast3D.Bone rootBone = null;
            foreach (Aiv.Fast3D.Bone bone in bonesMapping.Values)
            {
                if (bone.Parent == null)
                {
                    rootsCounter++;
                    rootBone = bone;
                }
            }
            if (rootsCounter > 1)
                throw new System.Exception("invalid skeleton for mesh: multiple roots found");

            int index = 0;
            BuildBonesTree(rootBone, bonesMapping, ref index);


            // now creates the bones array
            Aiv.Fast3D.Bone[] finalTree = new Aiv.Fast3D.Bone[index];
            foreach (Aiv.Fast3D.Bone bone in bonesMapping.Values)
            {
                finalTree[bone.Index] = bone;
            }

            return finalTree;
        }

        private static List<Aiv.Fast3D.Bone> GetBoneChildren(Aiv.Fast3D.Bone bone, Dictionary<string, Aiv.Fast3D.Bone> bonesMapping)
        {
            List<Aiv.Fast3D.Bone> children = new List<Bone>();
            foreach (Aiv.Fast3D.Bone childBone in bonesMapping.Values)
            {
                if (childBone.Parent == bone)
                {
                    children.Add(childBone);
                }
            }
            return children;
        }

        private static void BuildBonesTree(Aiv.Fast3D.Bone bone, Dictionary<string, Aiv.Fast3D.Bone> bonesMapping, ref int index)
        {
            bone.Index = index++;
            foreach (Aiv.Fast3D.Bone child in GetBoneChildren(bone, bonesMapping))
            {
                BuildBonesTree(child, bonesMapping, ref index);
            }
        }
    }
}
