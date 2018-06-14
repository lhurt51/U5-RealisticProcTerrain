using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class HeightMapSettings : UpdatableData {

    public PerlinNoiseGenerator.NormalizeMode normalizeMode;

    public List<PerlinParameters> perlinParams;

    public float heightMultiplier;
    public AnimationCurve heightCurve;

    public bool useFalloff;
    public bool useMultiPerlinNoise;

    public float minHeight
    {
        get { return heightMultiplier * heightCurve.Evaluate(0); }
    }

    public float maxHeight
    {
        get { return heightMultiplier * heightCurve.Evaluate(1); }
    }

    public void AddPerlinParam()
    {
        PerlinNoiseGenerator.AddNewPerlin(perlinParams);
    }

    public void RemovePerlinParam()
    {
        PerlinNoiseGenerator.RemovePerlin(perlinParams);
    }

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        foreach (PerlinParameters p in perlinParams) p.ValidateValues();
        base.OnValidate();
    }

#endif

}
