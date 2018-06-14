using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {

	public static float fBM(float x, float y, int oct, float perscistance, float lacunarity = 2.0f)
    {
        // Total heigth value
        float total = 0;
        // How close the waves are together
        float frequency = 1;
        // How tall the waves become
        float amplitude = 1;
        // The addition of each amplitude
        float maxValue = 0;

        for (int i = 0; i < oct; i++)
        {
            // Simple perlin noise witha calculated amplitude and frequency
            total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
            // Increase maxValue by amplitude (for avging)
            maxValue += amplitude;
            // Increasing amplitude by persistance (to reduce the amplitude every octave)
            amplitude *= perscistance;
            // Increasing the frequency by 2 (to randomize the seed of smaller noises)
            frequency *= lacunarity;
        }

        // Avg of the total to keep it within range
        return total / maxValue;
    }

    public static float Map(float val, float ogMin, float ogMax, float targetMin, float targetMax)
    {
        return (val - ogMin) * (targetMax - targetMin) / (ogMax - ogMin) + targetMin;
    }

    // Fisher-Yates Shuffle
    public static System.Random r = new System.Random();
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;

        while (n-- > 1)
        {
            int k = r.Next(n + 1);

            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

}
