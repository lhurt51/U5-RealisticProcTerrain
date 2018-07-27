using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LODInfo
{
    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int LOD;
    public float visibleDstThreshold;

    public float sqrVisibleDstThreshold
    {
        get { return visibleDstThreshold * visibleDstThreshold; }
    }

}

public class TerrainGenerator : MonoBehaviour {

    const float viewerMoveThresholdForUpdate = 25.0f;
    const float sqrViewerMoveThresholdForUpdate = viewerMoveThresholdForUpdate * viewerMoveThresholdForUpdate;

    public int colliderLODIndex;
    public LODInfo[] LODSettings;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;

    public Transform viewer;
    public Material mapMaterial;

    Vector2 viewerPos;
    Vector2 oldViewerPos;

    float meshWorldSize;
    int chunksVisisbleInViewDst;

    Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

}
