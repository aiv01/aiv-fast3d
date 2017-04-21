using System;
using Aiv.Fast3D;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using System.Globalization;


namespace Aiv.Fast3D
{
	public static class ObjLoader
	{
		private static List<Vector3> objVertices;
		private static List<Vector2> objUVs;
		private static List<Vector3> objNormals;

		private class ObjMesh
		{


			public List<float> vList;
			public List<float> vtList;
			public List<float> vnList;

			Vector3 multiplier;

			public ObjMesh(Vector3 multiplier)
			{


				vList = new List<float>();
				vtList = new List<float>();
				vnList = new List<float>();

				this.multiplier = multiplier;
			}

			public void AddFace(string[] items)
			{
				// first vertex
				string[] indices = items[1].Split('/');
				AddVertex(indices);

				// second vertex
				indices = items[2].Split('/');
				AddVertex(indices);

				// third vertex
				indices = items[3].Split('/');
				AddVertex(indices);

				if (items.Length > 4)
				{
					// fourth vertex
					indices = items[4].Split('/');
					AddVertex(indices);

					// first vertex
					indices = items[1].Split('/');
					AddVertex(indices);

					// second vertex
					indices = items[3].Split('/');
					AddVertex(indices);
				}
			}

			private void AddVertex(string[] indices)
			{
				Vector3 vItem = objVertices[int.Parse(indices[0]) - 1];
				vList.Add(vItem.X * multiplier.X);
				vList.Add(vItem.Y * multiplier.Y);
				vList.Add(vItem.Z * multiplier.Z);

				if (indices.Length > 1)
				{
					if (indices[1] != "")
					{
						Vector2 vtItem = objUVs[int.Parse(indices[1]) - 1];
						vtList.Add(vtItem.X);
						// by default textures are y-reversed
						vtList.Add(1f - vtItem.Y);
					}

					if (indices.Length > 2)
					{
						if (indices[2] != "")
						{
							Vector3 vnItem = objNormals[int.Parse(indices[2]) - 1];
							vnList.Add(vnItem.X);
							vnList.Add(vnItem.Y);
							vnList.Add(vnItem.Z);
						}
					}
				}
			}
		}

		public static Mesh3[] Load(string fileName, Vector3 multiplier)
		{

			List<ObjMesh> meshes = new List<ObjMesh>();

			objVertices = new List<Vector3>();
			objUVs = new List<Vector2>();
			objNormals = new List<Vector3>();

			ObjMesh currentMesh = null;

			string[] lines = File.ReadAllLines(fileName);
			foreach (string line in lines)
			{
				string[] items = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (items.Length == 0)
					continue;
				if (items[0] == "o" || items[0] == "g")
				{
					currentMesh = new ObjMesh(multiplier);
					meshes.Add(currentMesh);
				}
				else if (items[0] == "v")
				{
					if (currentMesh == null)
					{
						currentMesh = new ObjMesh(multiplier);
						meshes.Add(currentMesh);
					}
					Vector3 v = new Vector3(ParseFloat(items[1]), ParseFloat(items[2]), ParseFloat(items[3]));
					objVertices.Add(v);
				}
				else if (items[0] == "vt")
				{
					Vector2 vt = new Vector2(ParseFloat(items[1]), ParseFloat(items[2]));
					objUVs.Add(vt);
				}
				else if (items[0] == "vn")
				{
					Vector3 v = new Vector3(ParseFloat(items[1]), ParseFloat(items[2]), ParseFloat(items[3]));
					objNormals.Add(v);
				}
				else if (items[0] == "f")
				{
					currentMesh.AddFace(items);
				}
			}

			Mesh3[] finalMeshes = new Mesh3[meshes.Count];
			for (int i = 0; i < finalMeshes.Length; i++)
			{
				finalMeshes[i] = new Mesh3();
				finalMeshes[i].v = meshes[i].vList.ToArray();
				finalMeshes[i].uv = meshes[i].vtList.ToArray();
				finalMeshes[i].vn = meshes[i].vnList.ToArray();
				finalMeshes[i].Update();
				// check if normals are available
				if (meshes[i].vnList.Count == 0)
				{
					finalMeshes[i].RegenerateNormals();
				}
				// check for invalid UVs
				if (meshes[i].vtList.Count == 0)
				{
					// this will trigger uv regeneration
					finalMeshes[i].uv = null;
				}
				finalMeshes[i].UpdateNormals();
			}
			return finalMeshes;
		}

		private static float ParseFloat(string value)
		{
			return float.Parse(value, CultureInfo.InvariantCulture);
		}

	}
}
