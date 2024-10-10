using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


namespace Heaton.WorldGen
{
    public class MapGenerator : MonoBehaviour
    {
        public enum DrawMode
        {
            NoiseMap,
            ColourMap,
            Mesh
        }

        [Header("Flags")]
        [SerializeField] private bool useSeed;
        [SerializeField] private bool generateOnPlay;
        [SerializeField] private DrawMode drawMode;

        [Header("Terrain Parameters")]
        [SerializeField] private int mapWidth;
        [SerializeField] private int mapHeight;
        [SerializeField] private float noiseScale;
        [SerializeField] private int octaves;
        [SerializeField, Range(0f, 1f)] private float persistance;
        [SerializeField] private float lacunarity;
        [SerializeField] private int seed;
        [SerializeField] private Vector2 offset;
        [SerializeField] private float meshHeightMultiplier;
        [SerializeField] private AnimationCurve meshHeightCurve;
        [SerializeField] private TerrainType[] regions;

        private void Start()
        {
            if (generateOnPlay)
                GenerateMap();
        }

        public void GenerateMap()
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, useSeed ? seed : UnityEngine.Random.Range(-1000, 1000), noiseScale, octaves, persistance, lacunarity, offset);

            Color[] colourMap = new Color[mapWidth * mapHeight];

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float currentHeight = noiseMap[x, y];

                    // Find which region this height falls under
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (currentHeight <= regions[i].height)
                        {
                            colourMap[y * mapWidth + x] = regions[i].colour;
                            break;
                        }
                    }
                }
            }

            MapDisplay display = GetComponent<MapDisplay>();

            if (drawMode == DrawMode.NoiseMap)
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
            else if (drawMode == DrawMode.ColourMap)
                display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
            else if (drawMode == DrawMode.Mesh)
            {
                display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve), TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
            }
        }

        private void OnValidate()
        {
            if (mapWidth < 0)
                mapWidth = 1;
            if (mapHeight < 0)
                mapHeight = 1;

            if (lacunarity < 1)
                lacunarity = 1;

            if (octaves < 1)
                octaves = 1;
        }
    }

    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color colour;
    }
}