using System;
using Aiv.Fast3D;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using System.Globalization;
using Assimp;


namespace Aiv.Fast3D
{
	public static class FbxLoader
	{

		public class InvalidPolygon : Exception
		{
			public InvalidPolygon() : base("Only triangles and quads are supported") { }
		}

		private class FbxMeshData
		{
			public List<float> vList;
			public List<float> vtList;
			public List<float> vnList;

			public List<float> bonesMappingList;
			public List<float> bonesWeightList;

			public Vector3 multiplier;
			public Dictionary<int, List<FbxBoneInfluence>> bonesInfluences;

			public FbxMeshData(Vector3 multiplier)
			{
				vList = new List<float>();
				vtList = new List<float>();
				vnList = new List<float>();

				bonesMappingList = new List<float>();
				bonesWeightList = new List<float>();

				bonesInfluences = new Dictionary<int, List<FbxBoneInfluence>>();

				this.multiplier = multiplier;
			}
		}


		private static void FbxIndexToFloatList(int faceIndex, Assimp.Mesh mesh, Face face, FbxMeshData fbxData)
		{
			int index = face.Indices[faceIndex];

			Vector3D v = mesh.Vertices[index];
			fbxData.vList.Add(v.X * fbxData.multiplier.X);
			fbxData.vList.Add(v.Y * fbxData.multiplier.Y);
			fbxData.vList.Add(v.Z * fbxData.multiplier.Z);

			if (mesh.HasTextureCoords(0))
			{
				Vector3D uv = mesh.TextureCoordinateChannels[0][index];
				fbxData.vtList.Add(uv.X);
				fbxData.vtList.Add(1 - uv.Y);
			}

			if (mesh.HasNormals)
			{
				Vector3D n = mesh.Normals[index];
				fbxData.vnList.Add(n.X);
				fbxData.vnList.Add(n.Y);
				fbxData.vnList.Add(n.Z);
			}

			if (mesh.HasBones)
			{
				float[] bonesMapping = { -1, -1, -1, -1 };
				float[] bonesWeight = { -1, -1, -1, -1 };
				if (fbxData.bonesInfluences.ContainsKey(index))
				{

					for (int i = 0; i < Math.Min(fbxData.bonesInfluences[index].Count, 4); i++)
					{
						int boneId = mesh.Bones.IndexOf(fbxData.bonesInfluences[index][i].bone);
						bonesMapping[i] = boneId;
						bonesWeight[i] = fbxData.bonesInfluences[index][i].weight;
					}
				}
				fbxData.bonesMappingList.Add(bonesMapping[0]);
				fbxData.bonesMappingList.Add(bonesMapping[1]);
				fbxData.bonesMappingList.Add(bonesMapping[2]);
				fbxData.bonesMappingList.Add(bonesMapping[3]);

				fbxData.bonesWeightList.Add(bonesWeight[0]);
				fbxData.bonesWeightList.Add(bonesWeight[1]);
				fbxData.bonesWeightList.Add(bonesWeight[2]);
				fbxData.bonesWeightList.Add(bonesWeight[3]);
			}

		}


		private class FbxBoneInfluence
		{
			public Bone bone;
			public float weight;
		}

		private static Mesh3 FbxToMesh3(Scene scene, Assimp.Mesh mesh, Vector3 multiplier)
		{


			FbxMeshData fbxData = new FbxMeshData(multiplier);

			if (mesh.HasBones)
			{
				foreach (Bone bone in mesh.Bones)
				{
					foreach (VertexWeight vertexWeight in bone.VertexWeights)
					{
						if (!fbxData.bonesInfluences.ContainsKey(vertexWeight.VertexID))
						{
							fbxData.bonesInfluences[vertexWeight.VertexID] = new List<FbxBoneInfluence>();
						}
						fbxData.bonesInfluences[vertexWeight.VertexID].Add(new FbxBoneInfluence() { bone = bone, weight = vertexWeight.Weight });
					}
				}
			}

			foreach (Face face in mesh.Faces)
			{
				if (face.IndexCount == 3 || face.IndexCount == 4)
				{


					for (int i = 0; i < 3; i++)
					{
						FbxIndexToFloatList(i, mesh, face, fbxData);
					}

					// manage quads
					if (face.IndexCount == 4)
					{

						FbxIndexToFloatList(3, mesh, face, fbxData);
						FbxIndexToFloatList(0, mesh, face, fbxData);
						FbxIndexToFloatList(2, mesh, face, fbxData);
					}
				}
				else
				{
					throw new FbxLoader.InvalidPolygon();
				}

			}

			Aiv.Fast3D.Mesh3 mesh3 = new Mesh3();
			mesh3.v = fbxData.vList.ToArray();
			mesh3.uv = fbxData.vtList.ToArray();
			mesh3.vn = fbxData.vnList.ToArray();
			if (fbxData.bonesMappingList.Count > 0)
			{
				mesh3.bonesMapping = fbxData.bonesMappingList.ToArray();
			}
			if (fbxData.bonesWeightList.Count > 0)
			{
				mesh3.bonesWeight = fbxData.bonesWeightList.ToArray();
			}
			mesh3.Update();
			mesh3.UpdateNormals();
			mesh3.UpdateBones();

			if (mesh.HasBones)
			{
				for (int i = 0; i < mesh.BoneCount; i++)
				{
					mesh3.AddBone(i, mesh.Bones[i].Name);
				}

				Console.WriteLine("Bones count = " + mesh3.BonesCount);

				// fixup bones
				FixBoneParenting(scene.RootNode, mesh3, null);
			}

			return mesh3;
		}

		private static string SanitizeBoneName(string name)
		{
			string assimpMagic = "_$AssimpFbx$";
			if (name.Contains(assimpMagic))
			{
				int index = name.IndexOf(assimpMagic, StringComparison.Ordinal);
				if (index >= 0)
					name = name.Remove(index);
			}
			return name;
		}

		private static void FixBoneParenting(Node node, Mesh3 mesh, Mesh3.Bone parentBone)
		{

			string nodeName = SanitizeBoneName(node.Name);

			if (mesh.HasBone(nodeName))
			{
				if (parentBone == null || (parentBone != null && parentBone.Name != nodeName))
				{
					Mesh3.Bone currentBone = mesh.GetBone(nodeName);
					currentBone.SetParent(parentBone);
					parentBone = currentBone;
				}
			}


			for (int i = 0; i < node.ChildCount; i++)
			{
				Node childNode = node.Children[i];
				FixBoneParenting(childNode, mesh, parentBone);
			}


		}

		private static void BuildMesh(Scene scene, Node node, List<Mesh3> meshes, Vector3 multiplier)
		{

			if (node.HasMeshes)
			{
				foreach (int index in node.MeshIndices)
				{
					Assimp.Mesh mesh = scene.Meshes[index];

					meshes.Add(FbxToMesh3(scene, mesh, multiplier));
				}
			}

			for (int i = 0; i < node.ChildCount; i++)
			{
				BuildMesh(scene, node.Children[i], meshes, multiplier);
			}
		}

		public static Mesh3[] Load(string fileName, Vector3 multiplier)
		{

			AssimpContext importer = new AssimpContext();
			Scene scene = importer.ImportFile(fileName);

			List<Mesh3> meshes = new List<Mesh3>();

			BuildMesh(scene, scene.RootNode, meshes, multiplier);

			return meshes.ToArray();
		}

		// taken from wikipedia
		private static Vector3 QuaternionToVector3(OpenTK.Quaternion q)
		{
			double ySqr = q.Y * q.Y;

			// roll
			double t0 = +2.0 * (q.W * q.X + q.Y * q.Z);
			double t1 = +1.0 - 2.0 * (q.X * q.X + ySqr);
			float roll = (float)Math.Atan2(t0, t1);

			// pitch
			double t2 = +2.0 * (q.W * q.Y - q.Z * q.X);
			t2 = t2 > 1.0 ? 1.0 : t2;
			t2 = t2 < -1.0 ? -1.0 : t2;
			float pitch = (float)Math.Asin(t2);

			// yaw
			double t3 = +2.0 * (q.W * q.Z + q.X * q.Y);
			double t4 = +1.0 - 2.0 * (ySqr + q.Z * q.Z);
			float yaw = (float)Math.Atan2(t3, t4);

			return new Vector3(pitch, yaw, roll);
		}

		public static SkeletalAnimation[] LoadAnimations(string fileName, Vector3 multiplier)
		{

			AssimpContext importer = new AssimpContext();
			Scene scene = importer.ImportFile(fileName);

			List<SkeletalAnimation> skeletalAnimations = new List<SkeletalAnimation>();

			if (scene.HasAnimations)
			{

				foreach (Animation animation in scene.Animations)
				{
					if (animation.HasNodeAnimations)
					{
						SkeletalAnimation skeletalAnimation = new SkeletalAnimation(animation.Name, (float)animation.TicksPerSecond);
						foreach (NodeAnimationChannel channel in animation.NodeAnimationChannels)
						{
							Vector3 lastPosition = Vector3.Zero;
							Vector3 lastRotation = Vector3.Zero;
							Vector3 lastScale = Vector3.One;
							string nodeName = SanitizeBoneName(channel.NodeName);
							if (channel.NodeName.Contains("_$AssimpFbx$_Translation"))
							{
								foreach (VectorKey key in channel.PositionKeys)
								{
									lastPosition = new Vector3(key.Value.X, key.Value.Y, key.Value.Z) * multiplier;
									skeletalAnimation.AddKeyFrame(nodeName, (float)key.Time, lastPosition, lastRotation, lastScale);
								}
							}
							else if (channel.NodeName.Contains("_$AssimpFbx$_Rotation"))
							{
								foreach (QuaternionKey key in channel.RotationKeys)
								{
									OpenTK.Quaternion q = new OpenTK.Quaternion(key.Value.X, key.Value.Y, key.Value.Z, key.Value.W);
									lastRotation = QuaternionToVector3(q);
									skeletalAnimation.AddKeyFrame(nodeName, (float)key.Time, lastPosition, lastRotation, lastScale);
								}
							}
							else if (channel.NodeName.Contains("_$AssimpFbx$_Scaling"))
							{
								foreach (VectorKey key in channel.ScalingKeys)
								{
									lastScale = new Vector3(key.Value.X, key.Value.Y, key.Value.Z);
									skeletalAnimation.AddKeyFrame(nodeName, (float)key.Time, lastPosition, lastRotation, lastScale);
								}
							}
							else
							{
								for (int i = 0; i < channel.PositionKeyCount; i++)
								{
									VectorKey keyT = channel.PositionKeys[i];
									QuaternionKey keyR = channel.RotationKeys[i];
									VectorKey keyS = channel.ScalingKeys[i];

									lastPosition = new Vector3(keyT.Value.X, keyT.Value.Y, keyT.Value.Z) * multiplier;

									OpenTK.Quaternion q = new OpenTK.Quaternion(keyR.Value.X, keyR.Value.Y, keyR.Value.Z, keyR.Value.W);
									lastRotation = QuaternionToVector3(q);

									lastScale = new Vector3(keyS.Value.X, keyS.Value.Y, keyS.Value.Z);

									skeletalAnimation.AddKeyFrame(nodeName, (float)keyT.Time, lastPosition, lastRotation, lastScale);
								}
							}

						}
						skeletalAnimations.Add(skeletalAnimation);
					}
				}

			}

			return skeletalAnimations.ToArray();
		}

	}
}
