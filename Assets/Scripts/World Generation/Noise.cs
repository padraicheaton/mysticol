using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Heaton.WorldGen
{
    public static class Noise
    {
        // Octaves = how detailed the noise map will be, how many levels of noise to be applied
        // Persistance = how much each new octave will influence the overall level
        // Lacunarity = how much more granular each subsequent octave will be
        public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
        {
            float[,] noiseMap = new float[mapWidth, mapHeight];

            // Ensure that the same map can be regerated at any time using the same seed
            System.Random prng = new System.Random(seed);

            // To ensure that each octave is sampled from a different position, store offsets for each octave
            Vector2[] octaveOffsets = new Vector2[octaves];

            for (int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + offset.x;
                float offsetY = prng.Next(-100000, 100000) + offset.y;

                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            if (scale <= 0)
                scale = 0.0001f;

            // Keep track of the max & min noise values so that the map can be normalised at the end
            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            // Have the samples of the perlin noise be around the center of the map rather than the top right
            float halfWidth = mapWidth / 2f;
            float halfHeight = mapHeight / 2f;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float amplitude = 1; // Reduces the impact on the final noise the more octaves deep
                    float frequency = 1; // Increases the granularity of the noise the more octaves deep
                    float noiseHeight = 0;

                    // Calculate more refined value for each given octave
                    for (int i = 0; i < octaves; i++)
                    {
                        // Figure out where on the perlin noise map to sample from
                        // Important to have non integer values here as integer values result in the same outputs
                        float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                        float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; // Make this in the range -1 to 1, rather than 0 to 1

                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    maxNoiseHeight = Mathf.Max(maxNoiseHeight, noiseHeight);
                    minNoiseHeight = Mathf.Min(minNoiseHeight, noiseHeight);

                    noiseMap[x, y] = noiseHeight;
                }
            }

            // Normalise the noise height values
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
            }

            return noiseMap;
        }
    }
}