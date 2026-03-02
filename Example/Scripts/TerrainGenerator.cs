using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:   TerrainGenerator.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.Example
{
    [ExecuteInEditMode]
    internal class TerrainGenerator : MonoBehaviour
    {
        [SerializeField] private Terrain _terrain;

        [Header("Terrain Dimensions")]
        [SerializeField] private int _depth = 30;
        [SerializeField] private int _width = 500;
        [SerializeField] private int _length = 500;

        [Header("Noise Parameters")]
        [SerializeField, Range(1, 8)] int _octaves = 5;
        [SerializeField] float _scale = 25f;
        [SerializeField, Range(0, 1)] float _persistence = 0.5f;
        [SerializeField] float _lacunarity = 2.0f;
        [SerializeField] float _offsetX = 100f;
        [SerializeField] float _offsetY = 100f;

        [Header("Forest Settings")]
        [Range(0, 5000)] public int _treeCount = 1000;
        [SerializeField] float _minHeight = 5f;
        [SerializeField] float _minSpacing = 4f;
        [SerializeField] int _treePrototypeIndex = 0;

        [Header("Editor Settings")]
        [SerializeField] bool _autoUpdate = true;

        private void OnValidate() 
        { 
            if (_autoUpdate) RequestTerrainUpdate(); 
        }

        [ContextMenu("Terrain Update")]
        public void RequestTerrainUpdate()
        {
            if (!_terrain) _terrain = GetComponent<Terrain>();
            if (_terrain == null || _terrain.terrainData == null) return;

            GenerateHeightmap();
            SpawnTrees();
        }

        private void GenerateHeightmap()
        {
            _terrain.terrainData.heightmapResolution = 513;
            _terrain.terrainData.size = new Vector3(_width, _depth, _length);
            _terrain.terrainData.SetHeights(0, 0, GenerateHeights());
        }

        private float[,] GenerateHeights()
        {
            int res = _terrain.terrainData.heightmapResolution;
            float[,] heights = new float[res, res];
            for (int x = 0; x < res; x++)
                for (int y = 0; y < res; y++)
                    heights[x, y] = CalculatefBm(x, y, res);
            return heights;
        }

        private float CalculatefBm(int x, int y, int res)
        {
            float total = 0, freq = 1, amp = 1, maxV = 0;
            for (int i = 0; i < _octaves; i++)
            {
                float xC = (float)x / res * _scale * freq + _offsetX;
                float yC = (float)y / res * _scale * freq + _offsetY;
                total += Mathf.PerlinNoise(xC, yC) * amp;
                maxV += amp; amp *= _persistence; freq *= _lacunarity;
            }
            return total / maxV;
        }

        private void SpawnTrees()
        {
            TerrainData data = _terrain.terrainData;
            data.treeInstances = new TreeInstance[0];

            List<TreeInstance> instances = new List<TreeInstance>();
            Random.InitState((int)(_offsetX + _offsetY));

            int maxAttempts = _treeCount * 10;
            int attempts = 0;

            while (instances.Count < _treeCount && attempts < maxAttempts)
            {
                attempts++;
                float x = Random.value;
                float z = Random.value;

                float worldHeight = data.GetInterpolatedHeight(x, z);

                if (worldHeight >= _minHeight)
                {
                    Vector3 currentPos = new Vector3(x * _width, 0, z * _length);

                    if (IsSpaceAvailable(currentPos, instances))
                    {
                        float normalizedHeight = worldHeight / data.size.y;
                        TreeInstance tree = new TreeInstance();
                        tree.position = new Vector3(x, normalizedHeight, z);

                        tree.prototypeIndex = _treePrototypeIndex;
                        tree.widthScale = Random.Range(0.8f, 1.2f);
                        tree.heightScale = Random.Range(0.8f, 1.2f);
                        tree.rotation = Random.value * Mathf.PI * 2f;
                        tree.color = Color.white;
                        tree.lightmapColor = Color.white;

                        instances.Add(tree);
                    }
                }
            }

            data.treeInstances = instances.ToArray();
            _terrain.Flush();
        }

        private bool IsSpaceAvailable(Vector3 pos, List<TreeInstance> existingTrees)
        {
            foreach (TreeInstance tree in existingTrees)
            {
                Vector3 existingTreeWorldPos = new Vector3(tree.position.x * _width, 0, tree.position.z * _length);

                if (Vector3.Distance(pos, existingTreeWorldPos) < _minSpacing)
                {
                    return false;
                }
            }
            return true;
        }
    }
}