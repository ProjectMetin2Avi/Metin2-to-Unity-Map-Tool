using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

public class Metin2TerrainTileImporter : EditorWindow
{
    private string rawFilePath = "";
    private string textureSetPath = "";
    private Terrain targetTerrain;
    private string texturesBasePath = ""; // Path to Ymir Work textures folder
    private bool flipHorizontal = false;
    private bool flipVertical = true;
    // Batch processing options
    private bool batchProcessMode = false;
    private string mapBaseDirectory = "";
    private List<TerrainMapping> terrainMappings = new List<TerrainMapping>();
    private Vector2Int selectedMapCoord = new Vector2Int();
    private Dictionary<Vector2Int, Terrain> coordinateToTerrainMap = new Dictionary<Vector2Int, Terrain>();
    private bool showMapCoordinates = false;
    private Vector2 terrainListScrollPosition;
    private Vector2 mapAreasScrollPosition;
    private List<Terrain> customTerrainOrder = new List<Terrain>();
    private int draggingIndex = -1;
    private Terrain draggedTerrain = null;
    private bool orderByFolder = true;
    private string defaultTextureSetPath = "";
    private bool useGlobalTextureSet = false;
    private string globalTextureSetPath = "";

    // Blend settings
    [SerializeField, Range(0.5f, 5f)]
    private float blendRadius = 5f;
    [SerializeField, Range(1f, 10f)]
    private float blendSharpness = 2f;

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

    [Serializable]
    private class TerrainMapping
    {
        public Terrain terrain;
        public string rawFilePath;
        public string textureSetPath;
        public Vector2Int coordinates;
        public bool processed;

        // Sıralama için tam koordinat değeri
        public string SortKey { get { return $"{coordinates.x:D3}{coordinates.y:D3}"; } }

        public override string ToString()
        {
            return $"{coordinates.x:D3}{coordinates.y:D3} -> {(terrain != null ? terrain.name : "<not assigned>")}";
        }
    }

    [MenuItem("Tools/Metin2 Terrain Tile Importer - @Metin2Avi")]
    public static void ShowWindow()
    {
        GetWindow<Metin2TerrainTileImporter>("Metin2 Terrain Tile Importer - @Metin2Avi");
    }

    private void OnGUI()
    {
        DrawSocialLinks();
        GUILayout.Label("Metin2 Terrain Texture Importer - @Metin2Avi", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Mode selection
        EditorGUILayout.BeginHorizontal();
        bool prevBatchMode = batchProcessMode;
        batchProcessMode = GUILayout.Toggle(batchProcessMode, "Batch Processing Mode", EditorStyles.toolbarButton, GUILayout.Width(150));
        bool standardMode = !GUILayout.Toggle(!batchProcessMode, "Standard Mode", EditorStyles.toolbarButton, GUILayout.Width(150));
        batchProcessMode = batchProcessMode || standardMode;

        if (prevBatchMode != batchProcessMode)
        {
            // Clear mappings when switching modes
            if (batchProcessMode)
                terrainMappings.Clear();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("TextureSet File (.txt):");
        EditorGUILayout.BeginHorizontal();
        defaultTextureSetPath = EditorGUILayout.TextField(defaultTextureSetPath);
        if (GUILayout.Button("Browse", GUILayout.Width(100)))
        {
            string selectedPath = EditorUtility.OpenFilePanel("TextureSet Dosyasını Seç (.txt)", "", "txt");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                defaultTextureSetPath = selectedPath;
                useGlobalTextureSet = true;
                Debug.Log($"Selected TextureSet file: {defaultTextureSetPath}");
            }
        }
        EditorGUILayout.EndHorizontal();

        useGlobalTextureSet = EditorGUILayout.ToggleLeft("Use this TextureSet for all terrains", useGlobalTextureSet);

        EditorGUILayout.Space();

        if (batchProcessMode)
        {
            DrawBatchModeGUI();
        }
        else
        {
            DrawStandardModeGUI();
        }
    }

    private void DrawStandardModeGUI()
    {
        // File paths
        EditorGUILayout.LabelField("Raw Tile File (tile.raw):");
        EditorGUILayout.BeginHorizontal();
        rawFilePath = EditorGUILayout.TextField(rawFilePath);
        if (GUILayout.Button("Browse", GUILayout.Width(100)))
            rawFilePath = EditorUtility.OpenFilePanel("Select tile.raw", "", "raw");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("TextureSet File (.txt):");
        EditorGUILayout.BeginHorizontal();
        textureSetPath = EditorGUILayout.TextField(textureSetPath);
        if (GUILayout.Button("Browse", GUILayout.Width(100)))
            textureSetPath = EditorUtility.OpenFilePanel("Select TextureSet", "", "txt");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Ymir Work Textures Folder:");
        EditorGUILayout.BeginHorizontal();
        texturesBasePath = EditorGUILayout.TextField(texturesBasePath);
        if (GUILayout.Button("Browse", GUILayout.Width(100)))
            texturesBasePath = EditorUtility.OpenFolderPanel("Select Ymir Work Textures Folder", "", "");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Flip options
        EditorGUILayout.BeginHorizontal();
        flipHorizontal = EditorGUILayout.Toggle("Flip Horizontal", flipHorizontal);
        flipVertical = EditorGUILayout.Toggle("Flip Vertical", flipVertical);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Blend controls
        GUILayout.Label("Blend Settings", EditorStyles.boldLabel);
        blendRadius = EditorGUILayout.Slider("Blend Radius", blendRadius, 0.5f, 5f);
        blendSharpness = EditorGUILayout.Slider("Blend Sharpness", blendSharpness, 1f, 10f);

        EditorGUILayout.Space();
        targetTerrain = EditorGUILayout.ObjectField("Target Terrain:", targetTerrain, typeof(Terrain), true) as Terrain;
        EditorGUILayout.Space();

        if (GUILayout.Button("Import Textures"))
        {
            if (ValidateInputs())
                ImportTextures();
        }
    }

    private void DrawBatchModeGUI()
    {
        // Her UI gösteriminde, harita alanlarının doğru sıralanmasını zorla
        if (orderByFolder && terrainMappings.Count > 0)
        {
            SortTerrainMappings();
        }

        // Map base directory selector
        EditorGUILayout.LabelField("Map Base Directory (containing map areas like 003003):");
        EditorGUILayout.BeginHorizontal();
        mapBaseDirectory = EditorGUILayout.TextField(mapBaseDirectory);
        if (GUILayout.Button("Browse", GUILayout.Width(100)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Map Base Directory", "", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                mapBaseDirectory = selectedPath;
                // Automatically detect texture sets and raw files
                ScanForMapAreas();

                // Sort mappings by folder name (numerical order)
                if (orderByFolder)
                {
                    terrainMappings = terrainMappings.OrderBy(m => m.coordinates.y).ThenBy(m => m.coordinates.x).ToList();
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Ymir Work Textures Folder:");
        EditorGUILayout.BeginHorizontal();
        texturesBasePath = EditorGUILayout.TextField(texturesBasePath);
        if (GUILayout.Button("Browse", GUILayout.Width(100)))
            texturesBasePath = EditorUtility.OpenFolderPanel("Select Ymir Work Textures Folder", "", "");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Flip options
        EditorGUILayout.BeginHorizontal();
        flipHorizontal = EditorGUILayout.Toggle("Flip Horizontal", flipHorizontal);
        flipVertical = EditorGUILayout.Toggle("Flip Vertical", flipVertical);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Blend controls
        GUILayout.Label("Blend Settings", EditorStyles.boldLabel);
        blendRadius = EditorGUILayout.Slider("Blend Radius", blendRadius, 0.5f, 5f);
        blendSharpness = EditorGUILayout.Slider("Blend Sharpness", blendSharpness, 1f, 10f);

        EditorGUILayout.Space();

        // Auto-detect terrain objects in scene
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Auto-Detect Terrains", GUILayout.Width(150)))
        {
            AutoDetectTerrains();
        }
        if (GUILayout.Button("Reset Terrain Order", GUILayout.Width(150)))
        {
            ResetTerrainList();
        }
        EditorGUILayout.EndHorizontal();

        // Toggle between folder order and custom order
        bool newOrderByFolder = EditorGUILayout.ToggleLeft("Order Tile.raw Files By Folder Sequence", orderByFolder);
        if (newOrderByFolder != orderByFolder)
        {
            orderByFolder = newOrderByFolder;
            if (orderByFolder)
            {
                // Koordinatları doğru sıralama (000000, 000001, 000002, ...)
                SortTerrainMappings();
            }
        }

        EditorGUILayout.Space();

        GUILayout.Label("Drag and Drop Terrain Assignment", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(@"1. Drag terrains in the desired order
2. The tile.raw files are listed in folder order
3. Each terrain will be mapped to the corresponding tile.raw file in the list", MessageType.Info);

        // First column: Draggable terrain list
        EditorGUILayout.BeginHorizontal();

        // Terrain column
        EditorGUILayout.BeginVertical(GUILayout.Width(300));
        EditorGUILayout.LabelField("Terrains (Drag to Reorder)", EditorStyles.boldLabel);

        terrainListScrollPosition = EditorGUILayout.BeginScrollView(terrainListScrollPosition, GUILayout.Height(200));

        // Make sure we have the right number of terrains in our list
        EnsureTerrainListPopulated();

        Event currentEvent = Event.current;

        // Terrain list with drag and drop
        for (int i = 0; i < customTerrainOrder.Count; i++)
        {
            Rect terrainRect = EditorGUILayout.BeginHorizontal("box");

            // Draw the terrain field
            customTerrainOrder[i] = (Terrain)EditorGUILayout.ObjectField("Terrain " + (i + 1), customTerrainOrder[i], typeof(Terrain), true);

            // Handle dragging and reordering
            if (terrainRect.Contains(currentEvent.mousePosition))
            {
                if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
                {
                    draggingIndex = i;
                    draggedTerrain = customTerrainOrder[i];
                    currentEvent.Use();
                }
                else if (currentEvent.type == EventType.MouseDrag && draggingIndex >= 0)
                {
                    // This just ensures the drag is activated
                    DragAndDrop.PrepareStartDrag();
                    currentEvent.Use();
                }
                else if (currentEvent.type == EventType.MouseUp && draggingIndex >= 0)
                {
                    // Reorder when mouse is released
                    if (draggingIndex != i)
                    {
                        // Remove from old position
                        customTerrainOrder.RemoveAt(draggingIndex);
                        // Insert at new position
                        if (i > draggingIndex)
                            customTerrainOrder.Insert(i - 1, draggedTerrain);
                        else
                            customTerrainOrder.Insert(i, draggedTerrain);

                        GUI.changed = true;
                    }

                    draggedTerrain = null;
                    draggingIndex = -1;
                    currentEvent.Use();
                }
            }

            // Draw handle/grip to indicate draggable
            GUILayout.Label("≡", GUILayout.Width(15));

            EditorGUILayout.EndHorizontal();

            // Visual feedback for dragging
            if (i == draggingIndex)
            {
                GUI.backgroundColor = new Color(0.8f, 0.8f, 1.0f, 0.5f);
                GUI.Box(terrainRect, "");
                GUI.backgroundColor = Color.white;
            }
        }

        // Reset drag state if mouse is released outside
        if (currentEvent.type == EventType.MouseUp && draggingIndex >= 0)
        {
            draggingIndex = -1;
            draggedTerrain = null;
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        // Map areas column
        EditorGUILayout.BeginVertical(GUILayout.Width(300));
        EditorGUILayout.LabelField("Tile.raw Map Areas", EditorStyles.boldLabel);

        // Kullanıcının kaydırması için scroll pozisyonunu takip eden değişkeni kullan
        mapAreasScrollPosition = EditorGUILayout.BeginScrollView(mapAreasScrollPosition, GUILayout.Height(200));

        // Display detected map areas in folder order
        for (int i = 0; i < terrainMappings.Count; i++)
        {
            EditorGUILayout.BeginHorizontal("box");

            // Display the coordinate and raw file name
            EditorGUILayout.LabelField($"{terrainMappings[i].coordinates.x:D3}{terrainMappings[i].coordinates.y:D3}", GUILayout.Width(70));
            EditorGUILayout.LabelField(System.IO.Path.GetFileName(terrainMappings[i].rawFilePath), GUILayout.Width(120));

            // Show the currently assigned terrain (based on index matching)
            string assignedTerrainName = "Not assigned";
            if (i < customTerrainOrder.Count && customTerrainOrder[i] != null)
            {
                assignedTerrainName = customTerrainOrder[i].name;
            }
            EditorGUILayout.LabelField(assignedTerrainName, GUILayout.Width(100));

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Map individual terrains
        if (customTerrainOrder.Count > 0 && terrainMappings.Count > 0)
        {
            if (GUILayout.Button("Apply Current Mapping", GUILayout.Height(25)))
            {
                ApplyCustomMapping();
            }
        }

        EditorGUILayout.Space();

        // Batch import button
        GUI.enabled = terrainMappings.Count > 0 && customTerrainOrder.Count > 0;
        if (GUILayout.Button("Process All Terrains", GUILayout.Height(30)))
        {
            BatchImportAll();
        }
        GUI.enabled = true;
    }

    private void ResetTerrainList()
    {
        customTerrainOrder.Clear();
        EnsureTerrainListPopulated();
    }

    private void EnsureTerrainListPopulated()
    {
        // Make sure our terrain list has appropriate number of entries
        if (customTerrainOrder.Count == 0)
        {
            // Find all terrains in scene
            Terrain[] sceneTerrains = UnityEngine.Object.FindObjectsOfType<Terrain>();

            // Add all terrains from scene
            customTerrainOrder.AddRange(sceneTerrains);

            // Sort by name if possible
            customTerrainOrder = customTerrainOrder.OrderBy(t => t.name).ToList();
        }

        // Ensure we have at least as many slots as map areas
        while (customTerrainOrder.Count < terrainMappings.Count)
        {
            customTerrainOrder.Add(null);
        }
    }

    private void ApplyCustomMapping()
    {
        // Önce harita koordinatlarının doğru sıralandığından emin ol
        if (orderByFolder)
        {
            SortTerrainMappings();
        }

        // Apply the current custom mapping of terrains to tiles
        for (int i = 0; i < terrainMappings.Count && i < customTerrainOrder.Count; i++)
        {
            terrainMappings[i].terrain = customTerrainOrder[i];
        }

        Debug.Log($"Mapping Applied: Applied custom terrain mapping. {customTerrainOrder.Count(t => t != null)} terrains mapped to {terrainMappings.Count} tile areas.");
    }

    private void ScanForMapAreas()
    {
        if (string.IsNullOrEmpty(mapBaseDirectory) || !System.IO.Directory.Exists(mapBaseDirectory))
            return;

        terrainMappings.Clear();

        // Get all subdirectories that match the pattern (e.g., 003002)
        var dirs = System.IO.Directory.GetDirectories(mapBaseDirectory)
            .Where(d => System.Text.RegularExpressions.Regex.IsMatch(System.IO.Path.GetFileName(d), "^\\d{6}$"))
            .OrderBy(d => {
                string name = System.IO.Path.GetFileName(d);
                // Önce ilk 3 karakter (X koordinatı), sonra son 3 karakter (Y koordinatı) ile sırala
                int xValue = int.Parse(name.Substring(0, 3));
                int yValue = int.Parse(name.Substring(3, 3));
                // X * 10000 + Y ile tek bir sayıya dönüştür (örn. 001002 -> 1*10000 + 2 = 10002)
                return xValue * 10000 + yValue;
            })
            .ToList();

        foreach (var dir in dirs)
        {
            string dirName = System.IO.Path.GetFileName(dir);
            string rawFilePath = System.IO.Path.Combine(dir, "tile.raw");
            string textureSetPath = "";

            // Check if tile.raw exists
            if (System.IO.File.Exists(rawFilePath))
            {
                // Look for texture set in the area folder or parent folder
                string[] possibleTextureSetNames = new[] { "TextureSet.txt", "texture.txt", "textureset.txt" };
                foreach (var textureSetName in possibleTextureSetNames)
                {
                    string potentialPath = System.IO.Path.Combine(dir, textureSetName);
                    if (System.IO.File.Exists(potentialPath))
                    {
                        textureSetPath = potentialPath;
                        break;
                    }

                    // Try in parent folder
                    potentialPath = System.IO.Path.Combine(mapBaseDirectory, textureSetName);
                    if (System.IO.File.Exists(potentialPath))
                    {
                        textureSetPath = potentialPath;
                        break;
                    }
                }

                // If we couldn't find a texture set file, look for areadata.txt as fallback
                if (string.IsNullOrEmpty(textureSetPath))
                {
                    string areaDataPath = System.IO.Path.Combine(dir, "areadata.txt");
                    if (System.IO.File.Exists(areaDataPath))
                    {
                        textureSetPath = areaDataPath; // We'll handle conversion later if needed
                    }
                }

                // Extract coordinates from directory name (e.g., "003003" -> Vector2Int(3, 3))
                if (int.TryParse(dirName.Substring(0, 3), out int x) &&
                    int.TryParse(dirName.Substring(3, 3), out int y))
                {
                    Vector2Int coords = new Vector2Int(x, y);
                    terrainMappings.Add(new TerrainMapping
                    {
                        coordinates = coords,
                        rawFilePath = rawFilePath,
                        textureSetPath = textureSetPath,
                        terrain = null,
                        processed = false
                    });
                }
            }
        }

        if (terrainMappings.Count == 0)
        {
            EditorUtility.DisplayDialog("No Map Areas Found",
                "No valid map areas found in the selected directory. Make sure the directory contains subdirectories with names like '003003' containing tile.raw files.",
                "OK");
        }
        else
        {
            // Harita koordinatlarını her zaman doğru sıralama
            SortTerrainMappings();
        }
    }

    // Harita alanlarını her zaman doğru sıralamak için yardımcı metod
    private void SortTerrainMappings()
    {
        // Koordinatları doğru sıralama - önce X, sonra Y (000000, 000001, 000002, ...)
        terrainMappings = terrainMappings.OrderBy(m => m.coordinates.x * 10000 + m.coordinates.y).ToList();
    }
    private void AutoDetectTerrains()
    {
        if (terrainMappings.Count == 0)
        {
            EditorUtility.DisplayDialog("No Map Areas", "Please select a map directory first.", "OK");
            return;
        }

        // Get all terrains in the scene
        Terrain[] terrains = UnityEngine.Object.FindObjectsOfType<Terrain>();
        if (terrains.Length == 0)
        {
            EditorUtility.DisplayDialog("No Terrains", "No terrains found in the scene.", "OK");
            return;
        }

        // Try to match terrains to coordinates based on name or position
        Dictionary<Vector2Int, Terrain> matchedTerrains = new Dictionary<Vector2Int, Terrain>();

        // First try to match by name pattern (e.g., 003003 in the name)
        foreach (var terrain in terrains)
        {
            string name = terrain.name.ToLowerInvariant();
            foreach (var mapping in terrainMappings)
            {
                string coordStr = $"{mapping.coordinates.x:D3}{mapping.coordinates.y:D3}";
                if (name.Contains(coordStr))
                {
                    mapping.terrain = terrain;
                    matchedTerrains[mapping.coordinates] = terrain;
                    break;
                }
            }
        }

        // Next try to match by terrain grid position
        if (matchedTerrains.Count < terrainMappings.Count && terrains.Length >= terrainMappings.Count)
        {
            // Sort terrains by position to create a grid
            var sortedTerrains = terrains.OrderBy(t => t.transform.position.z).ThenBy(t => t.transform.position.x).ToList();

            // Find unique X and Z positions to determine grid dimensions
            var uniqueX = sortedTerrains.Select(t => t.transform.position.x).Distinct().OrderBy(x => x).ToList();
            var uniqueZ = sortedTerrains.Select(t => t.transform.position.z).Distinct().OrderBy(z => z).ToList();

            if (uniqueX.Count > 0 && uniqueZ.Count > 0)
            {
                // Create a map of grid position to terrain
                var gridToTerrain = new Dictionary<Vector2Int, Terrain>();
                foreach (var terrain in terrains)
                {
                    int gridX = uniqueX.IndexOf(terrain.transform.position.x);
                    int gridZ = uniqueZ.IndexOf(terrain.transform.position.z);
                    gridToTerrain[new Vector2Int(gridX, gridZ)] = terrain;
                }

                // Sort mappings by coordinates for easier assignment
                var sortedMappings = terrainMappings.OrderBy(m => m.coordinates.y).ThenBy(m => m.coordinates.x).ToList();

                // Assign terrains to mappings that don't already have a terrain
                for (int i = 0; i < sortedMappings.Count; i++)
                {
                    if (sortedMappings[i].terrain != null) continue;

                    // Find grid coordinates that match the mapping index
                    foreach (var gridPos in gridToTerrain.Keys.OrderBy(pos => pos.y).ThenBy(pos => pos.x))
                    {
                        if (!matchedTerrains.Values.Contains(gridToTerrain[gridPos]))
                        {
                            sortedMappings[i].terrain = gridToTerrain[gridPos];
                            matchedTerrains[sortedMappings[i].coordinates] = gridToTerrain[gridPos];
                            break;
                        }
                    }
                }
            }
        }

        // Update the dictionary for later use
        coordinateToTerrainMap = matchedTerrains;

        if (matchedTerrains.Count == 0)
        {
            EditorUtility.DisplayDialog("No Matches", "Could not match any terrains to map coordinates. Please assign them manually.", "OK");
        }
        else
        {
            Debug.Log($"Auto-Detection Complete: Successfully matched {matchedTerrains.Count} out of {terrainMappings.Count} terrains.");
        }
    }

    private void BatchImportAll()
    {
        int processedCount = 0;
        int errorCount = 0;

        // Apply the current mapping first
        ApplyCustomMapping();

        // Then process all mappings
        foreach (var mapping in terrainMappings)
        {
            if (mapping.terrain != null && !mapping.processed)
            {
                bool success = ImportSingleMapping(mapping);
                if (success)
                {
                    processedCount++;
                    mapping.processed = true;
                }
                else
                {
                    errorCount++;
                }
            }
        }

        Debug.Log($"Batch Import Complete: Processed {processedCount} terrains successfully. {errorCount} errors occurred. {terrainMappings.Count - processedCount - errorCount} terrains skipped (not assigned or already processed).");
    }

    private bool ImportSingleMapping(TerrainMapping mapping)
    {
        try
        {
            if (mapping == null || mapping.terrain == null || string.IsNullOrEmpty(mapping.rawFilePath) || !System.IO.File.Exists(mapping.rawFilePath))
                return false;

            // Setup for import
            rawFilePath = mapping.rawFilePath;
            targetTerrain = mapping.terrain;

            // TextureSet.txt yolunu belirle
            if (useGlobalTextureSet && !string.IsNullOrEmpty(defaultTextureSetPath) && System.IO.File.Exists(defaultTextureSetPath))
            {
                // Kullanıcının seçtiği TextureSet.txt dosyasını kullan
                textureSetPath = defaultTextureSetPath;
                Debug.Log($"Using user-selected TextureSet: {textureSetPath}");
            }
            else
            {
                // Sabit TextureSet.txt yolunu kullan
                textureSetPath = "E:\\winddd\\GF-25.0.5.0\\TextureSet.txt";

                // TextureSet.txt dosyasının var olup olmadığını kontrol et
                if (!System.IO.File.Exists(textureSetPath))
                {
                    Debug.LogError($"TextureSet.txt dosyası bulunamadı: {textureSetPath}");

                    // Yedek olarak mapping TextureSet'i kullan
                    textureSetPath = mapping.textureSetPath;

                    // Hala bulunamazsa harita klasöründe ara
                    if (string.IsNullOrEmpty(textureSetPath) || !System.IO.File.Exists(textureSetPath))
                    {
                        string mapDir = System.IO.Path.GetDirectoryName(mapping.rawFilePath);
                        string mapParentDir = System.IO.Directory.GetParent(mapDir)?.FullName;

                        if (!string.IsNullOrEmpty(mapParentDir))
                        {
                            string parentTextureSet = System.IO.Path.Combine(mapParentDir, "TextureSet.txt");
                            if (System.IO.File.Exists(parentTextureSet))
                            {
                                textureSetPath = parentTextureSet;
                                Debug.Log($"Harita klasöründeki TextureSet.txt kullanılıyor: {textureSetPath}");
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log($"TextureSet.txt dosyası kullanılıyor: {textureSetPath}");
                }
            }

            // Do the import
            ImportTextures();
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error importing mapping {mapping.coordinates}: {ex.Message}");
            return false;
        }
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

    private bool ValidateInputs()
    {
        if (string.IsNullOrEmpty(rawFilePath) || !File.Exists(rawFilePath))
        {
            EditorUtility.DisplayDialog("Error", "Please select a valid tile.raw file!", "OK");
            return false;
        }
        if (string.IsNullOrEmpty(textureSetPath) || !File.Exists(textureSetPath))
        {
            EditorUtility.DisplayDialog("Error", "Please select a valid TextureSet file!", "OK");
            return false;
        }
        if (string.IsNullOrEmpty(texturesBasePath) || !Directory.Exists(texturesBasePath))
        {
            EditorUtility.DisplayDialog("Error", "Please select a valid Ymir Work textures folder!", "OK");
            return false;
        }
        if (targetTerrain == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a target Terrain!", "OK");
            return false;
        }
        return true;
    }

    private class TextureInfo
    {
        public string path;
        public Vector2 tiling;
    }

    private void ImportTextures()
    {
        try
        {
            var textureInfos = ParseTextureSet();
            if (textureInfos.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "No texture information could be read from the TextureSet file!", "OK");
                return;
            }
            Debug.Log("=== REQUIRED TEXTURES ===");
            for (int i = 0; i < textureInfos.Count; i++)
            {
                var name = Path.GetFileNameWithoutExtension(textureInfos[i].path);
                Debug.Log($"{i + 1}. {name}.dds (Tiling: {textureInfos[i].tiling})");
            }
            TerrainLayer[] layers = CreateTerrainLayers(textureInfos);
            if (layers == null || layers.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "Failed to create Terrain Layers!", "OK");
                return;
            }
            float[,,] splatmap = CreateSplatmapFromRaw(textureInfos.Count);
            if (splatmap == null)
            {
                EditorUtility.DisplayDialog("Error", "Failed to create splatmap data!", "OK");
                return;
            }
            targetTerrain.terrainData.terrainLayers = layers;
            targetTerrain.terrainData.SetAlphamaps(0, 0, splatmap);
            Debug.Log("Success: Textures imported successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ImportTextures error details: {e}");
            EditorUtility.DisplayDialog("Error", $"An error occurred: {e.Message}", "OK");
        }
    }

    private List<TextureInfo> ParseTextureSet()
    {
        var textureInfos = new List<TextureInfo>();
        try
        {
            if (string.IsNullOrEmpty(textureSetPath) || !File.Exists(textureSetPath))
            {
                Debug.LogError($"TextureSet file does not exist: {textureSetPath}");
                return TryFallbackTextureSet(textureInfos);
            }

            var lines = File.ReadAllLines(textureSetPath);
            if (lines.Length == 0)
            {
                Debug.LogError("TextureSet file is empty");
                return TryFallbackTextureSet(textureInfos);
            }

            Debug.Log($"Reading texture set file: {textureSetPath} with {lines.Length} lines");

            // Try to determine texture set format based on content
            string firstLine = lines[0].Trim().ToLowerInvariant();
            bool isStandardFormat = firstLine.Contains("texture") || lines.Any(l => l.Trim().StartsWith("Start Texture"));
            bool isAreaDataFormat = firstLine.Contains("areadata") || firstLine.Contains("start object");

            if (isStandardFormat)
            {
                ParseStandardTextureSet(lines, textureInfos);
            }
            else if (isAreaDataFormat)
            {
                Debug.LogWarning("Detected AreaData format instead of TextureSet. Will attempt to use default textures.");
                // Use default textures since areadata doesn't contain texture information
                return CreateDefaultTextureSet();
            }
            else
            {
                // Try general approach that should work for most formats
                ParseGenericTextureSet(lines, textureInfos);
            }

            if (textureInfos.Count == 0)
            {
                Debug.LogWarning("No textures found in the provided file. Will try fallback.");
                return TryFallbackTextureSet(textureInfos);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ParseTextureSet error details: {e}");
            // Try to use default texture set if parsing fails
            return TryFallbackTextureSet(textureInfos);
        }

        return textureInfos;
    }

    private List<TextureInfo> TryFallbackTextureSet(List<TextureInfo> existingList)
    {
        if (existingList.Count > 0) return existingList;

        Debug.Log("Attempting to use default texture set...");

        // Look for TextureSet in project assets
        string[] guids = AssetDatabase.FindAssets("t:TextAsset TextureSet");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            if (!string.IsNullOrEmpty(path))
            {
                string fullPath = System.IO.Path.Combine(Application.dataPath, "..", path);
                textureSetPath = fullPath; // Update path for future reference
                Debug.Log($"Found default TextureSet at {path}");

                try
                {
                    var lines = File.ReadAllLines(fullPath);
                    ParseStandardTextureSet(lines, existingList);
                    if (existingList.Count > 0)
                    {
                        return existingList;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error using default TextureSet: {ex.Message}");
                }
            }
        }

        // If still no results, create hard-coded default set
        return CreateDefaultTextureSet();
    }

    private List<TextureInfo> CreateDefaultTextureSet()
    {
        Debug.Log("Creating default texture set");
        var result = new List<TextureInfo>();

        // Add common Metin2 textures that are likely to exist
        string[] defaultTextures = new[]
        {
            "textureset/fieldset/field.dds",
            "textureset/fieldset/field01.dds",
            "textureset/fieldset/field02.dds",
            "textureset/fieldset/field03.dds",
            "textureset/fieldset/field04.dds",
            "textureset/snow_texture.dds",
            "tileset/metin2_a1_tiles/013001-2.dds"
        };

        foreach (string texPath in defaultTextures)
        {
            result.Add(new TextureInfo
            {
                path = texPath,
                tiling = new Vector2(20, 20)
            });
        }

        return result;
    }

    private void ParseStandardTextureSet(string[] lines, List<TextureInfo> textureInfos)
    {
        TextureInfo current = null;
        foreach (var line in lines)
        {
            var t = line.Trim();
            if (string.IsNullOrEmpty(t)) continue;

            if (t.StartsWith("Start Texture"))
            {
                current = new TextureInfo();
            }
            else if (t.StartsWith("End Texture"))
            {
                // Ensure we have a valid texture and path before adding
                if (current != null && !string.IsNullOrEmpty(current.path) && current.tiling != Vector2.zero)
                {
                    // Make sure both tiling components are set
                    if (current.tiling.y == 0) current.tiling.y = current.tiling.x;
                    textureInfos.Add(current);
                }
                current = null;
            }
            else if (t.StartsWith("\""))
            {
                if (current != null)
                {
                    // Normalize path - handle multiple formats
                    var p = t.Trim('"');

                    // Handle various path formats
                    string[] pathPrefixes = new[] { "d:\\ymir work\\", "ymir work\\", "\\ymir work\\", "data\\", "\\data\\", "d:\\" };
                    foreach (var prefix in pathPrefixes)
                    {
                        if (p.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        {
                            p = p.Substring(prefix.Length);
                            break;
                        }
                    }

                    // Normalize separators
                    p = p.Replace("\\", "/");

                    // Add .dds extension if missing
                    if (!Path.HasExtension(p) || !p.EndsWith(".dds", StringComparison.OrdinalIgnoreCase))
                    {
                        p = Path.ChangeExtension(p, ".dds");
                    }

                    current.path = p;
                }
            }
            else if (current != null && float.TryParse(t, out float val))
            {
                if (current.tiling == Vector2.zero) current.tiling.x = val;
                else if (current.tiling.y == 0)
                {
                    current.tiling.y = val;

                    // If we find a complete texture entry in this format, add it
                    if (!string.IsNullOrEmpty(current.path))
                    {
                        textureInfos.Add(current);
                        current = null;
                    }
                }
            }
        }

        // In case the last texture didn't end properly
        if (current != null && !string.IsNullOrEmpty(current.path) && current.tiling.x > 0)
        {
            if (current.tiling.y == 0) current.tiling.y = current.tiling.x;
            textureInfos.Add(current);
        }
    }

    private void ParseGenericTextureSet(string[] lines, List<TextureInfo> textureInfos)
    {
        // Try to find any lines containing paths to textures
        List<string> potentialPaths = new List<string>();

        foreach (string line in lines)
        {
            string trimmed = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//") || trimmed.StartsWith("#"))
                continue;

            // Look for patterns like texture paths with common extensions
            if ((trimmed.Contains(".dds") || trimmed.Contains(".tga") || trimmed.Contains(".jpg") || trimmed.Contains(".png")) &&
                (trimmed.Contains("texture") || trimmed.Contains("tileset") || trimmed.Contains("map")))
            {
                potentialPaths.Add(trimmed);
            }
            else if (trimmed.Contains("ymir") || trimmed.Contains("data") || trimmed.Contains("terrainmaps"))
            {
                // These might be paths as well, even without extensions
                potentialPaths.Add(trimmed);
            }
        }

        // Process the found paths
        foreach (string path in potentialPaths)
        {
            string processedPath = path;

            // Remove quotes and path prefixes if present
            if (processedPath.StartsWith("\"") && processedPath.EndsWith("\""))
            {
                processedPath = processedPath.Trim('"');
            }

            // Clean up the path
            string[] pathPrefixes = new[] { "d:\\ymir work\\", "ymir work\\", "\\ymir work\\", "data\\", "\\data\\", "d:\\" };
            foreach (var prefix in pathPrefixes)
            {
                if (processedPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    processedPath = processedPath.Substring(prefix.Length);
                    break;
                }
            }

            processedPath = processedPath.Replace("\\", "/");

            // Add to texture list with default tiling
            textureInfos.Add(new TextureInfo
            {
                path = processedPath,
                tiling = new Vector2(20, 20) // Default tiling value
            });
        }
    }

    private TerrainLayer[] CreateTerrainLayers(List<TextureInfo> textureInfos)
    {
        try
        {
            string layersPath = "Assets/TerrainLayers";
            Directory.CreateDirectory(layersPath);
            var layers = new List<TerrainLayer>();
            foreach (var info in textureInfos)
            {
                var name = Path.GetFileNameWithoutExtension(info.path);
                var assetPath = $"Assets/Textures/{name}.png";
                TerrainLayer layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>($"{layersPath}/{name}.terrainlayer");
                if (layer == null)
                {
                    layer = new TerrainLayer();
                    layer.name = name;
                    layer.tileSize = info.tiling;
                    layer.specular = Color.black;
                    layer.metallic = 0f;
                    layer.smoothness = 0f;
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                    if (tex != null) layer.diffuseTexture = tex;
                    AssetDatabase.CreateAsset(layer, $"{layersPath}/{name}.terrainlayer");
                }
                layers.Add(layer);
            }
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            return layers.ToArray();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CreateTerrainLayers error details: {e}");
            return null;
        }
    }

    private float[,,] CreateSplatmapFromRaw(int textureCount)
    {
        try
        {
            var rawData = File.ReadAllBytes(rawFilePath);
            int size = targetTerrain.terrainData.alphamapResolution;
            var splatmapData = new float[size, size, textureCount];
            int rawSize = Mathf.CeilToInt(Mathf.Sqrt(rawData.Length));
            var blendBuffer = new Dictionary<int, float>[size, size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    blendBuffer[x, y] = new Dictionary<int, float>();
                    int kr = Mathf.CeilToInt(blendRadius);
                    for (int ky = -kr; ky <= kr; ky++)
                        for (int kx = -kr; kx <= kr; kx++)
                        {
                            float dist = Mathf.Sqrt(kx * kx + ky * ky);
                            if (dist > blendRadius) continue;
                            float rawX = (x + kx) * rawSize / (float)size;
                            float rawY = (y + ky) * rawSize / (float)size;
                            int sx = flipHorizontal ? rawSize - 1 - Mathf.FloorToInt(rawX) : Mathf.FloorToInt(rawX);
                            int sy = flipVertical ? rawSize - 1 - Mathf.FloorToInt(rawY) : Mathf.FloorToInt(rawY);
                            if (sx < 0 || sx >= rawSize || sy < 0 || sy >= rawSize) continue;
                            int idx = sy * rawSize + sx;
                            if (idx < 0 || idx >= rawData.Length) continue;
                            int tIndex = rawData[idx]; if (tIndex <= 0 || tIndex > textureCount) continue; tIndex--;
                            float weight = Mathf.Max(0, 1f - (dist / blendRadius));
                            weight = Mathf.Pow(weight, blendSharpness);
                            if (tIndex == 0) weight *= 1.2f;
                            if (!blendBuffer[x, y].ContainsKey(tIndex)) blendBuffer[x, y][tIndex] = 0;
                            blendBuffer[x, y][tIndex] += weight;
                        }
                    float total = blendBuffer[x, y].Values.Sum();
                    if (total > 0)
                        foreach (var kvp in blendBuffer[x, y])
                            splatmapData[y, x, kvp.Key] = kvp.Value / total;
                    else splatmapData[y, x, 0] = 1f;
                }
            return splatmapData;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CreateSplatmapFromRaw error details: {e}");
            return null;
        }
    }
}
