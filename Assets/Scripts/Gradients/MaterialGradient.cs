using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MaterialGradient {

    public bool bRandomizeTint;

    [System.Serializable]
    public class MaterialLevel
    {
        [SerializeField]
        Texture2D texture;
        [SerializeField]
        Color tint;
        [SerializeField]
        float tintStrength;
        [SerializeField]
        float minHeight;
        [SerializeField]
        float maxHeight;
        [SerializeField]
        float minSlope;
        [SerializeField]
        float maxSlope;
        [SerializeField]
        float blendStrength;
        [SerializeField]
        Vector2 tileOffset;
        [SerializeField]
        Vector2 tileScale;
        [SerializeField]
        Vector2 splatNoiseVScale;
        [SerializeField]
        float splatNoiseScaler;

        public MaterialLevel(Texture2D text, Color tint, float tintStrength, float minHeight, float maxHeight, float minSlope, float maxSlope, float blendStrength, Vector2 tileOffset, Vector2 tileScale, Vector2 splatNoiseVScale, float splatNoiseScaler)
        {
            this.texture = text;
            this.tint = tint;
            this.tintStrength = tintStrength;
            this.maxHeight = maxHeight;
            this.minHeight = minHeight;
            this.maxSlope = maxSlope;
            this.minSlope = minSlope;
            this.blendStrength = blendStrength;
            this.tileOffset = tileOffset;
            this.tileScale = tileScale;
            this.splatNoiseVScale = splatNoiseVScale;
            this.splatNoiseScaler = splatNoiseScaler;
        }

        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        public Color Tint
        {
            get { return tint; }
            set { tint = (value != null) ? value : Color.white; }
        }

        public float TintStrength
        {
            get { return tintStrength; }
            set { tintStrength = Mathf.Clamp01(value); }
        }

        public float MinHeight
        {
            get { return minHeight; }
            set { minHeight = Mathf.Clamp01((value < maxHeight) ? value : maxHeight - 0.1f);  }
        }

        public float MaxHeight
        {
            get { return maxHeight; }
            set { maxHeight = Mathf.Clamp01((value > minHeight) ? value : minHeight + 0.1f); }
        }

        public float MinSlope
        {
            get { return minSlope; }
            set { minSlope = Mathf.Clamp((value < maxSlope) ? value : maxSlope - 15.0f, 0.0f, 90.0f); }
        }

        public float MaxSlope
        {
            get { return maxSlope; }
            set { maxSlope = Mathf.Clamp((value > minSlope) ? value : minSlope + 15.0f, 0.0f, 90.0f); }
        }

        public float BlendStrength
        {
            get { return blendStrength; }
            set { blendStrength = Mathf.Clamp01(value); }
        }

        public Vector2 TileOffset
        {
            get { return tileOffset; }
            set { tileOffset = value; }
        }

        public Vector2 TileScale
        {
            get { return tileScale; }
            set { tileScale = new Vector2((value.x > 0.0f) ? value.x : 0.01f, (value.y > 0.0f) ? value.y : 0.01f); }
        }

        public Vector2 SplatNoiseVScale
        {
            get { return splatNoiseVScale; }
            set { splatNoiseVScale = new Vector2((value.x > 0.0f) ? value.x : 0.01f, (value.y > 0.0f) ? value.y : 0.01f); }

        }

        public float SplatNoiseScaler
        {
            get { return splatNoiseScaler; }
            set { splatNoiseScaler = Mathf.Max(value, 0.001f); }
        }
    }

    [SerializeField]
    List<MaterialLevel> mats = new List<MaterialLevel>();

    public int NumMats
    {
        get { return mats.Count;  }
    }

    public MaterialLevel GetMatLevel(int i)
    {
        return mats[i];
    }

    public List<MaterialLevel> GetAllMatLevels()
    {
        return mats;
    }

    public int AddMat(MaterialLevel newMat)
    {
        // If newmat is null return -1
        if (newMat == null) return -1;

        // Looping through the list looking for its position
        for (int i = 0; i < NumMats; i++)
        {
            if (newMat.MinHeight < mats[i].MinHeight)
            {
                mats.Insert(i, newMat);
                return i;
            }
        }

        // If this is the biggest height add to the end
        mats.Add(newMat);

        return NumMats - 1;
    }

    public int AddMat(Color color, float minHeight, Texture2D text = null, float tintStrength = 1.0f, float maxHeight = 0.0f, float minSlope = 0.0f, float maxSlope = 90.0f, float blendStrength = 0.1f, Vector2 tileOffset = default(Vector2), Vector2 tileScale = default(Vector2), Vector2 splatNoiseVScale = default(Vector2), float splatNoiseScaler = 0.1f)
    {
        // Setting all the parameters if they do not have a value
        if (maxHeight < minHeight) maxHeight = minHeight + 0.1f;
        if (tileScale == default(Vector2)) tileScale = new Vector2(50.0f, 50.0f);
        if (splatNoiseVScale == default(Vector2)) splatNoiseVScale = new Vector2(0.01f, 0.01f);

        // Creating a new Material Level
        MaterialLevel newMat = new MaterialLevel(text, color, tintStrength, minHeight, maxHeight, minSlope, maxSlope, blendStrength, tileOffset, tileScale, splatNoiseVScale, splatNoiseScaler);

        // Looping through the list looking for its position
        for (int i = 0; i < NumMats; i++)
        {
            if (newMat.MinHeight < mats[i].MinHeight)
            {
                mats.Insert(i, newMat);
                return i;
            }
        }

        // If this is the biggest height add to the end
        mats.Add(newMat);

        return NumMats - 1;
    }

    public void RemoveMat(int index, bool bOverride = false)
    {
        if (bOverride || mats.Count >= 2) mats.RemoveAt(index);
    }

    public int UpdateMatMinHeight(int i, float minHeight)
    {
        // MaterialLevel newMat = new MaterialLevel(mats[index].Texture, mats[index].Tint, mats[index].TintStrength, minHeight, mats[index].MaxHeight, mats[index].MinSlope, mats[index].MaxSlope, mats[index].BlendStrength, mats[index].TileOffset, mats[index].TileScale, mats[index].SplatNoiseVScale, mats[index].SplatNoiseScaler);
        MaterialLevel newMat = mats[i];
        newMat.MinHeight = minHeight;

        RemoveMat(i, true);

        return AddMat(newMat);
    }

    public void UpdateMatMaxHeight(int i, float maxHeight)
    {
        if (mats[i] != null) mats[i].MaxHeight = maxHeight;
    }

    public void UpdateMatTexture(int i, Texture2D tex)
    {
        if (mats[i] != null) mats[i].Texture = tex;
    }

    public void UpdateMatTint(int i, Color col)
    {
        if (mats[i] != null) mats[i].Tint = col;
    }

    public void UpdateMatTintStrength(int i, float tintStrength)
    {
        if (mats[i] != null) mats[i].TintStrength = tintStrength;
    }

    public void UpdateMatMinSlope(int i, float minSlope)
    {
        if (mats[i] != null) mats[i].MinSlope = minSlope;
    }

    public void UpdateMatMaxSlope(int i, float maxSlope)
    {
        if (mats[i] != null) mats[i].MaxSlope = maxSlope;
    }

    public void UpdateMatBlendStrength(int i, float blendStrength)
    {
        if (mats[i] != null) mats[i].BlendStrength = blendStrength;
    }

    public void UpdateMatTileOffset(int i, Vector2 tileOffset)
    {
        if (mats[i] != null) mats[i].TileOffset = tileOffset;
    }

    public void UpdateMatTileScale(int i, Vector2 tileScale)
    {
        if (mats[i] != null) mats[i].TileScale = tileScale;
    }

    public void UpdateMatSplatNoiseVScale(int i, Vector2 splatNoiseVScale)
    {
        if (mats[i] != null) mats[i].SplatNoiseVScale = splatNoiseVScale;
    }

    public void UpdateMatSplatNoiseScaler(int i, float splatNoiseScaler)
    {
        if (mats[i] != null) mats[i].SplatNoiseScaler = splatNoiseScaler;
    }

    // Need to work on this eval for the gradient more!
    public Color Eval(float height)
    {
        Color color = Color.white;
        List<Color> keysColor = new List<Color>();
        List<float> keyStrength = new List<float>();

        // Need to fix this so that the window will work
        for (int i = 0; i < NumMats; i++)
        {
            if (mats[i].MinHeight <= height && mats[i].MaxHeight >= height)
            {
                keysColor.Add(mats[i].Tint);
                keyStrength.Add(Utils.Map(height, mats[i].MinHeight, mats[i].MaxHeight, -1, 1));
            }
        }

        if (keysColor.Count > 1)
        {
            for (int i = 0; i < keysColor.Count - 1; i++)
            {
                if (keysColor[i + 1] != null)
                {
                    float strength = Mathf.Abs(keyStrength[i + 1] + keyStrength[i]);
                    color = Color.Lerp(keysColor[i + 1], (i == 0) ? keysColor[i] : color, strength);
                }
            }
        }
        else if (keysColor.Count == 1) color = keysColor[0];
        else color = Color.black;

        return color;
    }

    public Texture2D GetTexture(int width)
    {
        Texture2D texture = new Texture2D(width, 1);
        Color[] colors = new Color[width];

        for (int i = 0; i < width; i++) colors[i] = Eval((float)i / (width - 1));

        texture.SetPixels(colors);
        texture.Apply();

        return texture;
    }

    public MaterialGradient()
    {
        AddMat(Color.white, 0.0f);
        AddMat(Color.black, 1.0f);
    }

}
