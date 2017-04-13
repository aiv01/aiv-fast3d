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

		private static void FbxIndexToFloatList(int faceIndex, Assimp.Mesh mesh, Face face, List<float> vList, List<float> vtList, List<float> vnList, Vector3 multiplier)
		{
			int index = face.Indices[faceIndex];

			Vector3D v = mesh.Vertices[index];
			vList.Add(v.X * multiplier.X);
			vList.Add(v.Y * multiplier.Y);
			vList.Add(v.Z * multiplier.Z);

			if (mesh.HasTextureCoords(0))
			{
				Vector3D uv = mesh.TextureCoordinateChannels[0][index];
				vtList.Add(uv.X);
				vtList.Add(1 - uv.Y);
			}

			if (mesh.HasNormals)
			{
				Vector3D n = mesh.Normals[index];
				vnList.Add(n.X);
				vnList.Add(n.Y);
				vnList.Add(n.Z);
			}
		}

		private static Mesh3 FbxToMesh3(Assimp.Mesh mesh, Vector3 multiplier)
		{
			List<float> vList = new List<float>();
			List<float> vtList = new List<float>();
			List<float> vnList = new List<float>();

			foreach (Face face in mesh.Faces)
			{
				if (face.IndexCount == 3 || face.IndexCount == 4)
				{


					for (int i = 0; i < 3; i++)
					{
						FbxIndexToFloatList(i, mesh, face, vList, vtList, vnList, multiplier);
					}

					// manage quads
					if (face.IndexCount == 4)
					{
						
						FbxIndexToFloatList(3, mesh, face, vList, vtList, vnList, multiplier);
                        FbxIndexToFloatList(0, mesh, face, vList, vtList, vnList, multiplier);
                        FbxIndexToFloatList(2, mesh, face, vList, vtList, vnList, multiplier);
					}
				}
				else
				{
					throw new FbxLoader.InvalidPolygon();
				}

			}

			Aiv.Fast3D.Mesh3 mesh3 = new Mesh3();
			mesh3.v = vList.ToArray();
			mesh3.uv = vtList.ToArray();
			mesh3.vn = vnList.ToArray();
			mesh3.Update();
			mesh3.UpdateNormals();
			return mesh3;
		}

		private static void BuildMesh(Scene scene, Node node, List<Mesh3> meshes, Vector3 multiplier)
		{
			if (node.HasMeshes)
			{
				foreach (int index in node.MeshIndices)
				{
					Assimp.Mesh mesh = scene.Meshes[index];

					meshes.Add(FbxToMesh3(mesh, multiplier));
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

	}
}
