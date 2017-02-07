using System;
using Aiv.Fast3D;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using System.Globalization;


namespace Aiv.Fast3D
{
    public class Obj : Mesh3
    {

        private List<Vector3> objVertices;
        private List<Vector2> objUVs;
        private List<Vector3> objNormals;

        private Vector3 multiplier;

        private List<float> vList;
        private List<float> vtList;
        private List<float> vnList;

        public Obj(string fileName) : this(fileName, Vector3.One)
        {
        }

        public Obj(string fileName, Vector3 multiplier) : base()
        {
            objVertices = new List<Vector3>();
            objUVs = new List<Vector2>();
            objNormals = new List<Vector3>();

            vList = new List<float>();
            vtList = new List<float>();
            vnList = new List<float>();

            this.multiplier = multiplier;

            string[] lines = File.ReadAllLines(fileName);
            foreach (string line in lines)
            {
                string[] items = line.Split(' ');

                if (items[0] == "v")
                {
                    Vector3 v = new Vector3(ParseFloat(items[1]), ParseFloat(items[2]), ParseFloat(items[3]));
                    Console.WriteLine(v);
                    objVertices.Add(v);
                }
                else if (items[0] == "vt")
                {
                    Vector2 vt = new Vector2(ParseFloat(items[1]), ParseFloat(items[2]));
                    objUVs.Add(vt);
                }
                else if (items[0] == "vn")
                {
                    Vector3 vn = new Vector3(ParseFloat(items[1]), ParseFloat(items[2]), ParseFloat(items[3]));
                    objNormals.Add(vn);
                }
                else if (items[0] == "f")
                {
                    AddFace(items);
                }
            }

            this.v = vList.ToArray();
            this.uv = vtList.ToArray();

            this.Update();
            this.UpdateFloatBuffer(3, vnList.ToArray());
        }

        private float ParseFloat(string value)
        {
            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        private void AddVertex(string[] indices)
        {
            Vector3 vItem = objVertices[int.Parse(indices[0]) - 1];
            vList.Add(vItem.X * multiplier.X);
            vList.Add(vItem.Y * multiplier.Y);
            vList.Add(vItem.Z * multiplier.Z);

            Vector2 vtItem = objUVs[int.Parse(indices[1]) - 1];
            vtList.Add(vtItem.X);
            // by default textures are y-reversed
            vtList.Add(1f - vtItem.Y);

            Vector3 vnItem = objNormals[int.Parse(indices[2]) - 1];
            vnList.Add(vnItem.X);
            vnList.Add(vnItem.Y);
            vnList.Add(vnItem.Z);
        }

        private void AddFace(string[] items)
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
        }
    }
}
