using UnityEngine;

using System.Collections.Generic;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    VertexColorTest.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.Example
{
    [ExecuteInEditMode, RequireComponent(typeof(MeshFilter))]
    public class VertexColorTest : MonoBehaviour
    {
        [SerializeField] private Color _colorA;
        [SerializeField] private Color _colorB;
        [SerializeField] private Color _colorC;

        private void Awake()
        {
            GenerateVertexColors();
        }

        [ContextMenu("Refresh Vertex Colors")]
        public void GenerateVertexColors()
        {
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            int[] triangles = mesh.triangles;
            Vector3[] vertices = mesh.vertices;
            Color[] colors = new Color[vertices.Length];

            List<int>[] neighbors = new List<int>[vertices.Length];
            for (int i = 0; i < vertices.Length; i++) neighbors[i] = new List<int>();

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int v1 = triangles[i];
                int v2 = triangles[i + 1];
                int v3 = triangles[i + 2];

                AddEdge(neighbors, v1, v2);
                AddEdge(neighbors, v2, v3);
                AddEdge(neighbors, v3, v1);
            }

            Color[] palette = { _colorA, _colorB, _colorC };

            for (int i = 0; i < vertices.Length; i++)
            {
                bool[] usedColors = new bool[palette.Length];

                foreach (int neighbor in neighbors[i])
                {
                    for (int c = 0; c < palette.Length; c++)
                    {
                        if (colors[neighbor] == palette[c])
                            usedColors[c] = true;
                    }
                }

                int chosenColor = 0;
                for (int c = 0; c < usedColors.Length; c++)
                {
                    if (!usedColors[c])
                    {
                        chosenColor = c;
                        break;
                    }
                }
                colors[i] = palette[chosenColor];
            }

            mesh.colors = colors;
        }

        private void AddEdge(List<int>[] neighbors, int a, int b)
        {
            if (!neighbors[a].Contains(b)) neighbors[a].Add(b);
            if (!neighbors[b].Contains(a)) neighbors[b].Add(a);
        }
    }
}
