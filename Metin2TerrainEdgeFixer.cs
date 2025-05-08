using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class Metin2TerrainEdgeFixer : EditorWindow
{
    private List<Terrain> selectedTerrains = new List<Terrain>();
    private float blendDistance = 1f;
    private bool blendHeightmaps = true;
    private bool blendTextures = true;

    [Header("Social Links")]
    [SerializeField] private string iconsFolderPath = "Assets/Tools/Icons";
    [SerializeField] private Texture2D GitHubIcon;
    [SerializeField] private Texture2D InstagramIcon;
    [SerializeField] private Texture2D DiscordIcon;
    [SerializeField] private Texture2D YouTubeIcon;
    [SerializeField] private Texture2D Metin2DownloadsIcon;
    [SerializeField] private Texture2D M2DevIcon;
    [SerializeField] private Texture2D TurkmmoIcon;
    private readonly string GitHubURL = "https://github.com/ProjectMetin2Avi";
    private readonly string instagramURL = "https://www.instagram.com/metin2.avi/";
    private readonly string discordURL = "https://discord.gg/WZMzMgPp38";
    private readonly string youtubeURL = "https://www.youtube.com/@project_avi";
    private readonly string Metin2DownloadsURL = "https://www.metin2downloads.to/cms/user/30621-metin2avi/";
    private readonly string M2DevURL = "https://metin2.dev/profile/53064-metin2avi/";
    private readonly string TurkmmoURL = "https://forum.turkmmo.com/uye/165187-trmove/";

    [MenuItem("Tools/Metin2 Terrain Edge Fixer - @Metin2Avi")]
    public static void ShowWindow()
    {
        GetWindow<Metin2TerrainEdgeFixer>("Metin2 Terrain Edge Fixer - @Metin2Avi");
    }

    private void OnGUI()
    {
        DrawSocialLinks();
        GUILayout.Label("Metin2 Terrain Edge Fixer - @Metin2Avi", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("1. Select terrains\n2. Click 'Fix Edges'", MessageType.Info);

        blendDistance = EditorGUILayout.Slider("Blend Distance", blendDistance, 0.1f, 10f);
        blendHeightmaps = EditorGUILayout.Toggle("Blend Heightmaps", blendHeightmaps);
        blendTextures = EditorGUILayout.Toggle("Blend Textures", blendTextures);

        if (GUILayout.Button("Get Selected Terrains"))
        {
            GetSelectedTerrains();
        }

        EditorGUILayout.Space();

        if (selectedTerrains.Count > 0)
        {
            EditorGUILayout.LabelField($"Selected Terrains: {selectedTerrains.Count}");
        }

        GUI.enabled = selectedTerrains.Count > 0;
        if (GUILayout.Button("Fix Terrain Edges"))
        {
            FixTerrainEdges();
        }
        GUI.enabled = true;
    }

    private void DrawSocialLinks()
    {
        EditorGUILayout.Space(20);
        GUILayout.Label("Follow/Contact", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (DiscordIcon && GUILayout.Button(new GUIContent(GitHubIcon, "GitHub"), GUILayout.Width(40), GUILayout.Height(40)))
            Application.OpenURL(GitHubURL);

        if (DiscordIcon && GUILayout.Button(new GUIContent(InstagramIcon, "Instagram"), GUILayout.Width(40), GUILayout.Height(40)))
            Application.OpenURL(instagramURL);

        if (DiscordIcon && GUILayout.Button(new GUIContent(DiscordIcon, "Discord"), GUILayout.Width(40), GUILayout.Height(40)))
            Application.OpenURL(discordURL);

        if (YouTubeIcon && GUILayout.Button(new GUIContent(YouTubeIcon, "YouTube"), GUILayout.Width(40), GUILayout.Height(40)))
            Application.OpenURL(youtubeURL);

        if (Metin2DownloadsIcon && GUILayout.Button(new GUIContent(Metin2DownloadsIcon, "Metin2Downloads"), GUILayout.Width(40), GUILayout.Height(40)))
            Application.OpenURL(Metin2DownloadsURL);

        if (M2DevIcon && GUILayout.Button(new GUIContent(M2DevIcon, "M2Dev"), GUILayout.Width(40), GUILayout.Height(40)))
            Application.OpenURL(M2DevURL);

        if (TurkmmoIcon && GUILayout.Button(new GUIContent(TurkmmoIcon, "Turkmmo"), GUILayout.Width(40), GUILayout.Height(40)))
            Application.OpenURL(TurkmmoURL);

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void GetSelectedTerrains()
    {
        selectedTerrains.Clear();
        foreach (GameObject obj in Selection.gameObjects)
        {
            Terrain terrain = obj.GetComponent<Terrain>();
            if (terrain != null) selectedTerrains.Add(terrain);
        }
    }

    private void FixTerrainEdges()
    {
        List<Object> undoObjects = new List<Object>();
        foreach (Terrain terrain in selectedTerrains)
        {
            undoObjects.Add(terrain);
            undoObjects.Add(terrain.terrainData);
        }
        Undo.RegisterCompleteObjectUndo(undoObjects.ToArray(), "Terrain Edge Fix");

        foreach (Terrain currentTerrain in selectedTerrains)
        {
            TerrainData terrainData = currentTerrain.terrainData;
            Vector3 terrainPos = currentTerrain.transform.position;

            Terrain[] neighbors = new Terrain[4];
            foreach (Terrain t in selectedTerrains)
            {
                if (t == currentTerrain) continue;
                Vector3 pos = t.transform.position;
                float size = terrainData.size.x;

                if (pos.x + size == terrainPos.x && pos.z == terrainPos.z) neighbors[0] = t; // Left
                else if (pos.x - size == terrainPos.x && pos.z == terrainPos.z) neighbors[1] = t; // Right
                else if (pos.x == terrainPos.x && pos.z - size == terrainPos.z) neighbors[2] = t; // Top
                else if (pos.x == terrainPos.x && pos.z + size == terrainPos.z) neighbors[3] = t; // Bottom
            }

            if (blendHeightmaps)
            {
                Undo.RegisterCompleteObjectUndo(terrainData, "Heightmap Changes");
                float[,] heights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
                BlendHeightmaps(terrainData, neighbors, ref heights);
                terrainData.SetHeights(0, 0, heights);
                EditorUtility.SetDirty(terrainData);
            }

            if (blendTextures)
            {
                Undo.RegisterCompleteObjectUndo(terrainData, "Alphamap Changes");
                float[,,] alphamap = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
                BlendTextures(terrainData, neighbors, ref alphamap);
                terrainData.SetAlphamaps(0, 0, alphamap);
                EditorUtility.SetDirty(terrainData);
            }

            EditorUtility.SetDirty(currentTerrain);
        }
    }

    private void BlendHeightmaps(TerrainData terrainData, Terrain[] neighbors, ref float[,] heights)
    {
        int resolution = terrainData.heightmapResolution;
        int blendPixels = Mathf.RoundToInt(blendDistance * resolution / terrainData.size.x);

        // Left neighbor
        if (neighbors[0] != null)
        {
            float[,] neighborHeights = neighbors[0].terrainData.GetHeights(neighbors[0].terrainData.heightmapResolution - blendPixels, 0, blendPixels, resolution);
            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < blendPixels; x++)
                {
                    float blend = (float)x / blendPixels;
                    heights[z, x] = Mathf.Lerp(neighborHeights[z, x], heights[z, x], blend);
                }
            }
        }

        // Right neighbor
        if (neighbors[1] != null)
        {
            float[,] neighborHeights = neighbors[1].terrainData.GetHeights(0, 0, blendPixels, resolution);
            for (int z = 0; z < resolution; z++)
            {
                for (int x = resolution - blendPixels; x < resolution; x++)
                {
                    float blend = (float)(resolution - x) / blendPixels;
                    int nx = x - (resolution - blendPixels);
                    heights[z, x] = Mathf.Lerp(neighborHeights[z, nx], heights[z, x], blend);
                }
            }
        }

        // Top neighbor
        if (neighbors[2] != null)
        {
            float[,] neighborHeights = neighbors[2].terrainData.GetHeights(0, 0, resolution, blendPixels);
            for (int z = resolution - blendPixels; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float blend = (float)(resolution - z) / blendPixels;
                    int nz = z - (resolution - blendPixels);
                    heights[z, x] = Mathf.Lerp(neighborHeights[nz, x], heights[z, x], blend);
                }
            }
        }

        // Bottom neighbor
        if (neighbors[3] != null)
        {
            float[,] neighborHeights = neighbors[3].terrainData.GetHeights(0, neighbors[3].terrainData.heightmapResolution - blendPixels, resolution, blendPixels);
            for (int z = 0; z < blendPixels; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float blend = (float)z / blendPixels;
                    heights[z, x] = Mathf.Lerp(neighborHeights[z, x], heights[z, x], blend);
                }
            }
        }
    }

    private void BlendTextures(TerrainData terrainData, Terrain[] neighbors, ref float[,,] alphamap)
    {
        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;
        int layers = terrainData.alphamapLayers;
        int blendPixels = Mathf.RoundToInt(blendDistance * width / terrainData.size.x);

        // Left edge
        if (neighbors[0] != null)
        {
            TerrainData neighborTerrainData = neighbors[0].terrainData;
            int neighborWidth = neighborTerrainData.alphamapWidth;
            float[,,] neighborAlphamap = neighborTerrainData.GetAlphamaps(neighborWidth - blendPixels, 0, blendPixels, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < blendPixels; x++)
                {
                    float blend = (float)x / blendPixels;
                    for (int l = 0; l < Mathf.Min(layers, neighborTerrainData.alphamapLayers); l++)
                    {
                        alphamap[y, x, l] = Mathf.Lerp(neighborAlphamap[y, x, l], alphamap[y, x, l], blend);
                    }
                    NormalizeAlphamapRow(ref alphamap, y, x, layers);
                }
            }
        }

        // Right edge
        if (neighbors[1] != null)
        {
            TerrainData neighborTerrainData = neighbors[1].terrainData;
            float[,,] neighborAlphamap = neighborTerrainData.GetAlphamaps(0, 0, blendPixels, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = width - blendPixels; x < width; x++)
                {
                    float blend = (float)(width - x) / blendPixels;
                    int nx = x - (width - blendPixels);
                    for (int l = 0; l < Mathf.Min(layers, neighborTerrainData.alphamapLayers); l++)
                    {
                        alphamap[y, x, l] = Mathf.Lerp(neighborAlphamap[y, nx, l], alphamap[y, x, l], blend);
                    }
                    NormalizeAlphamapRow(ref alphamap, y, x, layers);
                }
            }
        }

        // Top edge
        if (neighbors[2] != null)
        {
            TerrainData neighborTerrainData = neighbors[2].terrainData;
            float[,,] neighborAlphamap = neighborTerrainData.GetAlphamaps(0, 0, width, blendPixels);

            for (int y = height - blendPixels; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float blend = (float)(height - y) / blendPixels;
                    int ny = y - (height - blendPixels);
                    for (int l = 0; l < Mathf.Min(layers, neighborTerrainData.alphamapLayers); l++)
                    {
                        alphamap[y, x, l] = Mathf.Lerp(neighborAlphamap[ny, x, l], alphamap[y, x, l], blend);
                    }
                    NormalizeAlphamapRow(ref alphamap, y, x, layers);
                }
            }
        }

        // Bottom edge
        if (neighbors[3] != null)
        {
            TerrainData neighborTerrainData = neighbors[3].terrainData;
            int neighborHeight = neighborTerrainData.alphamapHeight;
            float[,,] neighborAlphamap = neighborTerrainData.GetAlphamaps(0, neighborHeight - blendPixels, width, blendPixels);

            for (int y = 0; y < blendPixels; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float blend = (float)y / blendPixels;
                    for (int l = 0; l < Mathf.Min(layers, neighborTerrainData.alphamapLayers); l++)
                    {
                        alphamap[y, x, l] = Mathf.Lerp(neighborAlphamap[y, x, l], alphamap[y, x, l], blend);
                    }
                    NormalizeAlphamapRow(ref alphamap, y, x, layers);
                }
            }
        }
    }

    private void NormalizeAlphamapRow(ref float[,,] alphamap, int y, int x, int layers)
    {
        float total = 0f;

        for (int l = 0; l < layers; l++)
        {
            total += alphamap[y, x, l];
        }

        if (total > 0f && total != 1f)
        {
            for (int l = 0; l < layers; l++)
            {
                alphamap[y, x, l] /= total;
            }
        }
        else if (total <= 0f)
        {
            // If total is zero, set the first layer to 1
            if (layers > 0)
            {
                alphamap[y, x, 0] = 1f;
                for (int l = 1; l < layers; l++)
                {
                    alphamap[y, x, l] = 0f;
                }
            }
        }
    }
}
