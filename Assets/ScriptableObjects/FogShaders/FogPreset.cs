using UnityEngine;

[CreateAssetMenu(fileName = "New FogPreset", menuName = "FogPreset")]
public class FogPreset : ScriptableObject
{
    [Header("Spawn Settings")]
    public float spawnSize = 120f;
    public float particleSize = 5f;
    public float killRadius = 10f;
    public float singleBurstAmount = 20000f;

    [Header("Distance & Edges")]
    public Vector2 smoothStepEdges = new Vector2(4f, 5f);
    public Vector2 outRemap = new Vector2(0f, 0f);

    [Header("Procedural Noise")]
    public float noiseScale = 10f;
    public float noiseStrength = 4.52f;
    public float noiseSpeed = 1f;
    public Vector2 noiseTiling = new Vector2(1f, -0.2f);
    [Range(0f, 1f)] public float noiseSmoothing = 0.73f;

    [Header("Elemental Weights")]
    [Range(0f, 1f)] public float gradientStrength = 0.266f;
    [Range(0f, 1f)] public float simpleStrength = 0f;
    [Range(0f, 1f)] public float voronoiStrength = 1f;
    [ColorUsage(true, true)] public Color fogTint = Color.white;
    [Range(0f, 1f)] public float tintIntensity = 1f;

    [Header("Texture Displacement")]
    public Texture2D fogTexture;
    public Vector2 textureTiling = new Vector2(-0.5f, 0.5f);
    public float textureSpeed1 = -0.05f;
    public float textureSpeed2 = 0.1f;
    public float gridSize = 25f;
}