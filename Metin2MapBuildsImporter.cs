using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System;

namespace Metin2MapTools
{
    // Bu, seri hale getirilebilir veri yapısı için oluşturduğum interface. Bu sayede verileri kolayca saklamak ve yönetmek için kullanıyorum.
    // This is the interface I created for serializable data structure. This allows me to easily store and manage data.
    public interface IMetin2ObjectData
    {
        string ObjectID { get; set; }
        string PropertyName { get; set; }
        string PropertyType { get; set; }
        string OriginalModelPath { get; set; }
        Vector3 OriginalPosition { get; set; }
        Vector3 OriginalRotation { get; set; }
        float OriginalSize { get; set; }
        float OriginalVariance { get; set; }
        UnityEngine.Object UnityModel { get; set; }
        string AreadataPath { get; set; }
        string PropertyPath { get; set; }
    }

    // Bu sınıfı, seri hale getirilebilir veri yapısı olarak tasarladım. Verileri Unity'nin serialization sistemi ile saklayabiliyorum.
    // I designed this class as a serializable data structure. I can store data using Unity's serialization system.
    [Serializable]
    public class ObjectDataWrapper : IMetin2ObjectData
    {
        [SerializeField] private string m_objectID;
        [SerializeField] private string m_propertyName;
        [SerializeField] private string m_propertyType;
        [SerializeField] private string m_originalModelPath;
        [SerializeField] private Vector3 m_originalPosition;
        [SerializeField] private Vector3 m_originalRotation;
        [SerializeField] private float m_originalSize;
        [SerializeField] private float m_originalVariance;
        [SerializeField] private UnityEngine.Object m_unityModel;
        [SerializeField] private string m_areadataPath;
        [SerializeField] private string m_propertyPath;

        // Public property interface implementations
        public string ObjectID { get { return m_objectID; } set { m_objectID = value; } }
        public string PropertyName { get { return m_propertyName; } set { m_propertyName = value; } }
        public string PropertyType { get { return m_propertyType; } set { m_propertyType = value; } }
        public string OriginalModelPath { get { return m_originalModelPath; } set { m_originalModelPath = value; } }
        public Vector3 OriginalPosition { get { return m_originalPosition; } set { m_originalPosition = value; } }
        public Vector3 OriginalRotation { get { return m_originalRotation; } set { m_originalRotation = value; } }
        public float OriginalSize { get { return m_originalSize; } set { m_originalSize = value; } }
        public float OriginalVariance { get { return m_originalVariance; } set { m_originalVariance = value; } }
        public UnityEngine.Object UnityModel { get { return m_unityModel; } set { m_unityModel = value; } }
        public string AreadataPath { get { return m_areadataPath; } set { m_areadataPath = value; } }
        public string PropertyPath { get { return m_propertyPath; } set { m_propertyPath = value; } }

        // Geri uyumluluk için eski adları da destekle
        public string objectID { get { return m_objectID; } set { m_objectID = value; } }
        public string propertyName { get { return m_propertyName; } set { m_propertyName = value; } }
        public string propertyType { get { return m_propertyType; } set { m_propertyType = value; } }
        public string originalModelPath { get { return m_originalModelPath; } set { m_originalModelPath = value; } }
        public Vector3 originalPosition { get { return m_originalPosition; } set { m_originalPosition = value; } }
        public Vector3 originalRotation { get { return m_originalRotation; } set { m_originalRotation = value; } }
        public float originalSize { get { return m_originalSize; } set { m_originalSize = value; } }
        public float originalVariance { get { return m_originalVariance; } set { m_originalVariance = value; } }
        public UnityEngine.Object unityModel { get { return m_unityModel; } set { m_unityModel = value; } }
        public string areadataPath { get { return m_areadataPath; } set { m_areadataPath = value; } }
        public string propertyPath { get { return m_propertyPath; } set { m_propertyPath = value; } }
    }

    // Yeni bir koleksiyon tipi oluşturdum - her sektör ve obje tipi için ayrı bir koleksiyon oluşturuyorum. Bu sayede verileri düzenli tutabiliyorum.
    // I created a new collection type - I create a separate collection for each sector and object type. This helps me keep the data organized.
    public class Metin2ObjectsCollection : ScriptableObject
    {
        public List<ObjectDataWrapper> objects = new List<ObjectDataWrapper>();
        public string sectorName;
        public string objectType;
    }
    public class Metin2MapBuildsImporter : EditorWindow
    {
        private Terrain targetTerrain;
        private string mapFolderPath = ""; // Ana klasör yolu
        // Main folder path
        private string propertyFolderPath = ""; // Property dosyaları klasör yolu
        // Property folder path
        private string modelsFolderPath = ""; // Unity modelleri klasör yolu
        // Unity models folder path
        private Vector3 scaleFactor = Vector3.one;
        private Vector2 scrollPosition;
        private const float COORDINATE_SCALE = 100f;
        private bool flipX = false;
        private bool flipZ = true;
        private bool fixTreeRotation = true; // Ağaçların X rotasyonunu -90 derece yapmak için kullandığım seçenek
        // Option I use to set tree X rotation to -90 degrees
        private bool createScriptableObjects = true;
        private bool createPrimitiveIfModelNotFound = true;
        private string scriptableObjectsPath = "Assets/Metin2Data/Objects";

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

        // Harita pozisyon düzeltme değerleri. Bu değerlerle haritanın konumlandırmasını ayarlıyorum.
        // Map position correction values. I use these values to adjust the positioning of the map.
        private bool applyMapOffset = true;
        private Vector2 mapOffset = new Vector2(-65.5f, 65.5f); // X ve Z ekseni kaydırma değerleri
        // X and Z offset values

        // Arazi yapışması için kullandığım değişkenler. Nesneleri araziye otomatik olarak yerleştirmemi sağlıyor.
        // Variables I use for terrain snapping. They allow me to automatically place objects on the terrain.
        private bool snapToGround = true;
        private float groundOffset = 0f; // Zeminden ekstra yükseklik değeri
        // Extra offset height from the ground

        // Property dosyalarını önbelleğe almak için oluşturduğum dictionary. Performansı önemli ölçüde artırıyor.
        // Dictionary I created to cache property files. This significantly improves performance.
        private Dictionary<string, string> propertyFileCache = new Dictionary<string, string>();

        // Obje ID'lerini property dosyalarına eşleştirmek için kullandığım dictionary. Arama sürecini hızlandırıyor.
        // Dictionary I use to map object IDs to property files. This speeds up the search process.
        private Dictionary<string, string> objectIdToPropertyMap = new Dictionary<string, string>();

        [MenuItem("Tools/Metin2 Map Builds Importer - @Metin2Avi")]
        public static void ShowWindow()
        {
            GetWindow<Metin2MapBuildsImporter>("Metin2 Map Builds Importer - @Metin2Avi");
        }

        private void OnGUI()
        {
            DrawSocialLinks();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Metin2 Map Builds Importer - @Metin2Avi", EditorStyles.boldLabel);

            targetTerrain = EditorGUILayout.ObjectField("Target Terrain", targetTerrain, typeof(Terrain), true) as Terrain;

            EditorGUILayout.BeginHorizontal();
            mapFolderPath = EditorGUILayout.TextField("Map Folder Path", mapFolderPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Map Folder", "", "");
                if (!string.IsNullOrEmpty(path))
                {
                    mapFolderPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            propertyFolderPath = EditorGUILayout.TextField("Property Folder Path", propertyFolderPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Property Folder", "", "");
                if (!string.IsNullOrEmpty(path))
                {
                    propertyFolderPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            modelsFolderPath = EditorGUILayout.TextField("Unity Models Folder Path", modelsFolderPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Unity Models Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    // Convert to relative path if inside Assets folder
                    if (path.Contains("Assets"))
                    {
                        int index = path.IndexOf("Assets");
                        modelsFolderPath = path.Substring(index);
                    }
                    else
                    {
                        modelsFolderPath = path;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            flipX = EditorGUILayout.Toggle("Flip X Coordinates", flipX);
            flipZ = EditorGUILayout.Toggle("Flip Z Coordinates", flipZ);
            EditorGUILayout.EndHorizontal();

            fixTreeRotation = EditorGUILayout.Toggle(new GUIContent("Fix Tree Rotation (X = -90)", "Apply -90 degree X rotation to tree objects"), fixTreeRotation);

            // Map offset option
            EditorGUILayout.BeginHorizontal();
            applyMapOffset = EditorGUILayout.Toggle(new GUIContent("Apply Map Offset", "Apply position offset to the entire map"), applyMapOffset);
            EditorGUILayout.EndHorizontal();

            if (applyMapOffset)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Map Offset (X, Z):");
                EditorGUILayout.BeginHorizontal();
                mapOffset.x = EditorGUILayout.FloatField("X", mapOffset.x);
                mapOffset.y = EditorGUILayout.FloatField("Z", mapOffset.y);
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            // Ground snapping option
            EditorGUILayout.BeginHorizontal();
            snapToGround = EditorGUILayout.Toggle(new GUIContent("Snap to Ground During Import", "Position objects on terrain surface when importing"), snapToGround);
            EditorGUILayout.EndHorizontal();

            if (snapToGround)
            {
                EditorGUI.indentLevel++;
                groundOffset = EditorGUILayout.FloatField(new GUIContent("Ground Offset", "Extra height above ground"), groundOffset);
                EditorGUI.indentLevel--;
            }

            createScriptableObjects = EditorGUILayout.Toggle("Create ScriptableObjects", createScriptableObjects);
            createPrimitiveIfModelNotFound = EditorGUILayout.Toggle("Create Primitive If Model Not Found", createPrimitiveIfModelNotFound);

            if (createScriptableObjects)
            {
                EditorGUILayout.BeginHorizontal();
                scriptableObjectsPath = EditorGUILayout.TextField("ScriptableObjects Path", scriptableObjectsPath);
                if (GUILayout.Button("Browse", GUILayout.Width(60)))
                {
                    string path = EditorUtility.OpenFolderPanel("Select ScriptableObjects Folder", "Assets", "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        // Convert to relative path if inside project
                        if (path.Contains("Assets"))
                        {
                            int index = path.IndexOf("Assets");
                            scriptableObjectsPath = path.Substring(index);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Warning", "Please select a folder inside your Unity project.", "OK");
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            // Pre-scan button for property files
            if (GUILayout.Button("Pre-Scan Property Files"))
            {
                if (string.IsNullOrEmpty(propertyFolderPath) || !Directory.Exists(propertyFolderPath))
                {
                    EditorUtility.DisplayDialog("Error", "Please select a valid property folder!", "OK");
                    return;
                }

                ScanPropertyFiles();
            }

            // Snap to Ground button
            if (GUILayout.Button("Snap Objects To Ground"))
            {
                if (targetTerrain == null)
                {
                    EditorUtility.DisplayDialog("Error", "Please select a target terrain!", "OK");
                    return;
                }

                SnapObjectsToGround();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Import All Objects"))
            {
                if (targetTerrain == null)
                {
                    EditorUtility.DisplayDialog("Error", "Please assign a target terrain!", "OK");
                    return;
                }

                if (string.IsNullOrEmpty(mapFolderPath) || !Directory.Exists(mapFolderPath))
                {
                    EditorUtility.DisplayDialog("Error", "Please select a valid map folder!", "OK");
                    return;
                }

                if (string.IsNullOrEmpty(propertyFolderPath) || !Directory.Exists(propertyFolderPath))
                {
                    EditorUtility.DisplayDialog("Error", "Please select a valid property folder!", "OK");
                    return;
                }

                // If property file cache is empty, scan first
                if (propertyFileCache.Count == 0)
                {
                    ScanPropertyFiles();
                }

                ImportAllObjects();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Instructions:\n" +
                "1. Assign your terrain\n" +
                "2. Select your map folder containing areadata files\n" +
                "3. Select your property folder containing .prt files\n" +
                "4. (Optional) Select Unity models folder for .fbx files\n" +
                "5. Adjust flip settings if needed\n" +
                "6. (Optional) Pre-scan property files for larger maps\n" +
                "7. Click 'Import All Objects'\n" +
                "Note: Objects will be scaled and positioned relative to terrain size",
                MessageType.Info
            );

            EditorGUILayout.EndScrollView();
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
        private void ScanPropertyFiles()
        {
            EditorUtility.DisplayProgressBar("Scanning Property Files", "Finding property files...", 0f);
            propertyFileCache.Clear();
            objectIdToPropertyMap.Clear();

            // Get all property files (.prt and .prb) recursively
            string[] prtFiles = Directory.GetFiles(propertyFolderPath, "*.prt", SearchOption.AllDirectories);
            string[] prbFiles = Directory.GetFiles(propertyFolderPath, "*.prb", SearchOption.AllDirectories);

            List<string> allPropertyFiles = new List<string>();
            allPropertyFiles.AddRange(prtFiles);
            allPropertyFiles.AddRange(prbFiles);

            Debug.Log($"Found {prtFiles.Length} .prt files and {prbFiles.Length} .prb files");

            for (int i = 0; i < allPropertyFiles.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Scanning Property Files", $"Processing {i + 1}/{allPropertyFiles.Count}...", (float)i / allPropertyFiles.Count);

                string filePath = allPropertyFiles[i];
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                try
                {
                    string fileContent = File.ReadAllText(filePath);

                    // Store content with full path as key
                    propertyFileCache[filePath] = fileContent;

                    // Extract property ID from content and add to mapping
                    string objectId = ExtractPropertyIdFromContent(fileContent);
                    if (!string.IsNullOrEmpty(objectId))
                    {
                        objectIdToPropertyMap[objectId] = filePath;
                        Debug.Log($"Mapped ID {objectId} to file {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error reading property file {filePath}: {ex.Message}");
                }
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Scan Complete", $"Scanned {allPropertyFiles.Count} property files successfully!", "OK");
        }

        private string ExtractPropertyIdFromContent(string content)
        {
            if (string.IsNullOrEmpty(content)) return string.Empty;

            // Property ID is usually on the second line
            string[] lines = content.Split('\n');
            if (lines.Length >= 2)
            {
                return lines[1].Trim();
            }

            return string.Empty;
        }

        private void ImportAllObjects()
        {
            GameObject parentObject = new GameObject("Metin2_All_Imported_Objects");

            // Apply map offset if enabled
            if (applyMapOffset)
            {
                parentObject.transform.position = new Vector3(mapOffset.x, 0, mapOffset.y);
                Debug.Log($"Applied map offset: X={mapOffset.x}, Z={mapOffset.y}");
            }

            int totalObjectsImported = 0;
            int totalObjectsWithModels = 0;
            int totalAreasFound = 0;
            int totalDirectoriesScanned = 0;

            // Obje koleksiyonları için oluşturduğum dictionary'ler. Farklı sektör ve tiplerdeki nesneleri gruplamak için kullanıyorum.
            // Dictionaries I created for object collections. I use these to group objects of different sectors and types.
            Dictionary<string, Dictionary<string, Metin2ObjectsCollection>> sectorTypeCollections = new Dictionary<string, Dictionary<string, Metin2ObjectsCollection>>();

            // Hata ayıklama bilgileri. İçe aktarma sürecini takip etmemi ve sorunları tespit etmemi sağlıyor.
            // Debug information. This allows me to track the import process and identify issues.
            Debug.Log($"Starting import. Map folder: {mapFolderPath}, Property folder: {propertyFolderPath}");
            Debug.Log($"Property cache contains {propertyFileCache.Count} files and {objectIdToPropertyMap.Count} ID mappings");

            // ScriptableObjects dizininin var olduğundan emin oluyorum. Yoksa gerekli klasörleri oluşturuyorum.
            // I make sure the ScriptableObjects directory exists. If not, I create the necessary folders.
            if (createScriptableObjects && !AssetDatabase.IsValidFolder(scriptableObjectsPath))
            {
                string parentFolder = Path.GetDirectoryName(scriptableObjectsPath);
                string newFolderName = Path.GetFileName(scriptableObjectsPath);

                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    AssetDatabase.CreateFolder("Assets", Path.GetFileName(parentFolder));
                }

                AssetDatabase.CreateFolder(parentFolder, newFolderName);
                AssetDatabase.SaveAssets();
            }

            // Arazi boyutları. Bu değerleri konumlandırma hesaplamalarında kullanıyorum.
            // Terrain dimensions. I use these values in positioning calculations.
            Vector3 terrainPos = targetTerrain.transform.position;
            Vector3 terrainSize = targetTerrain.terrainData.size;

            // Metin2 haritası 25600x25600 birimdir, bizim arazimiz ise 131x131. Ölçeklendirme için bu oranı kullanıyorum.
            // Metin2 map is 25600x25600 units, our terrain is 131x131. I use this ratio for scaling.
            // Buna göre ölçek faktörünü hesaplıyorum. Doğru boyutlandırma için kritik bir değer.
            // I calculate the scale factor accordingly. This is a critical value for proper sizing.
            float scaleFactor = 131f / 256f; // 131 / 256 ≈ 0.511 scale factor

            List<string> areadataFiles = new List<string>();

            // Search for areadata.txt files directly
            string[] allFiles = Directory.GetFiles(mapFolderPath, "areadata.txt", SearchOption.AllDirectories);
            areadataFiles.AddRange(allFiles);

            if (areadataFiles.Count == 0)
            {
                string[] subDirectories = Directory.GetDirectories(mapFolderPath, "*", SearchOption.AllDirectories);
                totalDirectoriesScanned = subDirectories.Length;
                Debug.Log($"No areadata.txt found directly. Searching in {subDirectories.Length} subdirectories...");

                foreach (string dir in subDirectories)
                {
                    string areadataPath = Path.Combine(dir, "areadata.txt");
                    if (File.Exists(areadataPath))
                    {
                        areadataFiles.Add(areadataPath);
                    }
                }
            }

            Debug.Log($"Found {areadataFiles.Count} areadata.txt files");
            totalAreasFound = areadataFiles.Count;

            // Eğer areadata dosyaları bulunamazsa, klasörleri doğrudan kullanmayı deniyorum. Bu, farklı map formatlarına uyum sağlamamı sağlıyor.
            // If no areadata files are found, I try to use folders directly. This allows me to accommodate different map formats.
            if (areadataFiles.Count == 0)
            {
                EditorUtility.DisplayDialog("Warning", "No areadata.txt files found! Check your map folder path.", "OK");
                EditorUtility.ClearProgressBar();
                return;
            }

            int processedFiles = 0;

            foreach (string areadataPath in areadataFiles)
            {
                string sectorName = Path.GetFileName(Path.GetDirectoryName(areadataPath));
                EditorUtility.DisplayProgressBar("Importing Objects", $"Processing file {processedFiles + 1}/{areadataFiles.Count}...", (float)processedFiles / areadataFiles.Count);
                processedFiles++;

                Debug.Log($"Processing areadata.txt in sector: {sectorName}");

                GameObject sectorContainer = new GameObject($"Sector_{sectorName}");
                sectorContainer.transform.parent = parentObject.transform;

                // Create containers for different property types
                Dictionary<string, GameObject> typeContainers = new Dictionary<string, GameObject>();

                string[] lines = File.ReadAllLines(areadataPath);
                Debug.Log($"Areadata file has {lines.Length} lines");

                int objectsInThisSector = 0;

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();

                    // Check for "Start Object" at the beginning of the line
                    if (line.StartsWith("Start Object"))
                    {
                        string objectName = line.Replace("Start Object", "").Trim();
                        Debug.Log($"Found object: {objectName} at line {i}");

                        // Make sure we have enough lines to read position data
                        if (i + 1 >= lines.Length)
                        {
                            Debug.LogWarning($"Not enough lines to read position for object {objectName}");
                            continue;
                        }

                        string[] positionData = lines[i + 1].Trim().Split(' ');
                        if (positionData.Length >= 3)
                        {
                            try
                            {
                                // Convert coordinates from Metin2 format to Unity
                                float originalX = float.Parse(positionData[0], CultureInfo.InvariantCulture);
                                float originalY = float.Parse(positionData[1], CultureInfo.InvariantCulture);
                                float originalZ = float.Parse(positionData[2], CultureInfo.InvariantCulture);

                                Debug.Log($"Object position: X={originalX}, Y={originalY}, Z={originalZ}");

                                // Apply scaling
                                float scaledX = (originalX / COORDINATE_SCALE) * scaleFactor;
                                float scaledZ = (-originalY / COORDINATE_SCALE) * scaleFactor; // Convert Y to Z axis

                                if (flipX) scaledX = -scaledX;
                                if (flipZ) scaledZ = -scaledZ;

                                // Get object ID from the next line
                                string objectId = string.Empty;
                                if (i + 2 < lines.Length)
                                {
                                    objectId = lines[i + 2].Trim();
                                    Debug.Log($"Object ID: {objectId}");
                                }
                                else
                                {
                                    Debug.LogWarning("Could not read object ID - not enough lines");
                                    continue;
                                }

                                // Get rotation
                                Vector3 rotation = Vector3.zero;
                                if (i + 3 < lines.Length)
                                {
                                    string[] rotationData = lines[i + 3].Trim().Split('#');
                                    if (rotationData.Length >= 3)
                                    {
                                        rotation = new Vector3(
                                            float.Parse(rotationData[0], CultureInfo.InvariantCulture),
                                            float.Parse(rotationData[2], CultureInfo.InvariantCulture),
                                            float.Parse(rotationData[1], CultureInfo.InvariantCulture)
                                        );
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning("Could not read rotation - not enough lines");
                                }

                                if (flipZ)
                                {
                                    rotation.y = 180f - rotation.y;
                                }

                                // Position calculation without terrain offset
                                // First apply scale to original coordinates
                                float worldX = scaledX;
                                float worldY = originalZ / COORDINATE_SCALE;
                                float worldZ = scaledZ;

                                // Terrain size is still used for scaling reference
                                worldX += terrainSize.x / 2;
                                worldZ += terrainSize.z / 2;

                                // Create final position (terrain position is not added)
                                Vector3 relativePosition = new Vector3(worldX, worldY, worldZ);

                                // Apply map offset if enabled
                                if (applyMapOffset)
                                {
                                    relativePosition += new Vector3(mapOffset.x, 0, mapOffset.y);
                                }

                                // Snap to terrain surface if enabled
                                if (snapToGround)
                                {
                                    // Find all terrains in the scene
                                    Terrain[] allTerrains = Terrain.activeTerrains;

                                    if (allTerrains.Length > 0)
                                    {
                                        float highestPoint = float.MinValue;
                                        bool foundTerrain = false;

                                        // Try each terrain to find the one at this position
                                        foreach (Terrain terrain in allTerrains)
                                        {
                                            // Check if position is within this terrain's bounds
                                            Bounds terrainBounds = new Bounds(
                                                terrain.transform.position + new Vector3(terrain.terrainData.size.x / 2, 0, terrain.terrainData.size.z / 2),
                                                terrain.terrainData.size
                                            );

                                            // Expand bounds slightly to catch objects at the edge
                                            terrainBounds.Expand(new Vector3(1, 100, 1));

                                            // Create a position to test (ignoring Y)
                                            Vector3 testPosition = new Vector3(relativePosition.x, 0, relativePosition.z);
                                            Vector3 terrainTestPosition = new Vector3(terrainBounds.center.x, 0, terrainBounds.center.z);
                                            Vector3 terrainTestSize = new Vector3(terrainBounds.size.x, 0, terrainBounds.size.z);

                                            // Check if within this terrain's XZ bounds
                                            if (Mathf.Abs(testPosition.x - terrainTestPosition.x) < terrainTestSize.x / 2 &&
                                                Mathf.Abs(testPosition.z - terrainTestPosition.z) < terrainTestSize.z / 2)
                                            {
                                                // Get terrain-local position
                                                Vector3 terrainLocalPos = relativePosition - terrain.transform.position;

                                                // Convert to terrain space (0-1)
                                                float normalizedX = Mathf.Clamp01(terrainLocalPos.x / terrain.terrainData.size.x);
                                                float normalizedZ = Mathf.Clamp01(terrainLocalPos.z / terrain.terrainData.size.z);

                                                // Sample height at this position
                                                float terrainHeight = terrain.terrainData.GetHeight(
                                                    Mathf.RoundToInt(normalizedX * (terrain.terrainData.heightmapResolution - 1)),
                                                    Mathf.RoundToInt(normalizedZ * (terrain.terrainData.heightmapResolution - 1))
                                                );

                                                // World space height
                                                float worldHeight = terrain.transform.position.y + terrainHeight;

                                                // Keep track of the highest point found
                                                if (worldHeight > highestPoint)
                                                {
                                                    highestPoint = worldHeight;
                                                    foundTerrain = true;
                                                }
                                            }
                                        }

                                        // If a valid terrain was found, set the height
                                        if (foundTerrain)
                                        {
                                            relativePosition.y = highestPoint + groundOffset;
                                            Debug.Log($"Snapped object to ground at height {highestPoint}");
                                        }
                                        else if (targetTerrain != null)
                                        {
                                            // Fall back to target terrain if position is outside all terrain bounds
                                            Debug.LogWarning("Object position outside all terrain bounds, falling back to target terrain");

                                            // Use the target terrain as before
                                            // Get the world position
                                            Vector3 terrainLocalPos = relativePosition - targetTerrain.transform.position;

                                            // Convert to terrain space (0-1)
                                            Vector3 terrainNormalizedPos = new Vector3(
                                                terrainLocalPos.x / targetTerrain.terrainData.size.x,
                                                0,
                                                terrainLocalPos.z / targetTerrain.terrainData.size.z
                                            );

                                            // Clamp to terrain bounds
                                            terrainNormalizedPos.x = Mathf.Clamp01(terrainNormalizedPos.x);
                                            terrainNormalizedPos.z = Mathf.Clamp01(terrainNormalizedPos.z);

                                            // Sample height at this position
                                            float terrainHeight = targetTerrain.terrainData.GetHeight(
                                                Mathf.RoundToInt(terrainNormalizedPos.x * (targetTerrain.terrainData.heightmapResolution - 1)),
                                                Mathf.RoundToInt(terrainNormalizedPos.z * (targetTerrain.terrainData.heightmapResolution - 1))
                                            );

                                            // Set the Y position to terrain height plus offset
                                            relativePosition.y = targetTerrain.transform.position.y + terrainHeight + groundOffset;
                                        }
                                    }
                                    else if (targetTerrain != null)
                                    {
                                        // If no terrains were found, fall back to the target terrain
                                        Debug.LogWarning("No active terrains found in scene, falling back to target terrain");

                                        // Use the target terrain as before
                                        // Get the world position
                                        Vector3 terrainLocalPos = relativePosition - targetTerrain.transform.position;

                                        // Convert to terrain space (0-1)
                                        Vector3 terrainNormalizedPos = new Vector3(
                                            terrainLocalPos.x / targetTerrain.terrainData.size.x,
                                            0,
                                            terrainLocalPos.z / targetTerrain.terrainData.size.z
                                        );

                                        // Clamp to terrain bounds
                                        terrainNormalizedPos.x = Mathf.Clamp01(terrainNormalizedPos.x);
                                        terrainNormalizedPos.z = Mathf.Clamp01(terrainNormalizedPos.z);

                                        // Sample height at this position
                                        float terrainHeight = targetTerrain.terrainData.GetHeight(
                                            Mathf.RoundToInt(terrainNormalizedPos.x * (targetTerrain.terrainData.heightmapResolution - 1)),
                                            Mathf.RoundToInt(terrainNormalizedPos.z * (targetTerrain.terrainData.heightmapResolution - 1))
                                        );

                                        // Set the Y position to terrain height plus offset
                                        relativePosition.y = targetTerrain.transform.position.y + terrainHeight + groundOffset;
                                    }
                                    else
                                    {
                                        Debug.LogWarning("No terrains available for ground snapping");
                                    }
                                }

                                // Try to find property file
                                string propertyFilePath = string.Empty;
                                string propertyContent = string.Empty;

                                if (objectIdToPropertyMap.TryGetValue(objectId, out propertyFilePath))
                                {
                                    Debug.Log($"Found property file in mapping: {propertyFilePath}");
                                    if (File.Exists(propertyFilePath))
                                    {
                                        propertyContent = File.ReadAllText(propertyFilePath);
                                    }
                                    else
                                    {
                                        Debug.LogWarning($"Property file mapped but does not exist: {propertyFilePath}");
                                    }
                                }
                                else
                                {
                                    Debug.Log($"Property file not in mapping for ID: {objectId}, searching manually...");
                                    // Search through all property files if not in cache
                                    bool found = false;
                                    foreach (var kvp in propertyFileCache)
                                    {
                                        if (kvp.Value.Contains(objectId))
                                        {
                                            propertyContent = kvp.Value;
                                            propertyFilePath = kvp.Key; // Use the full filepath directly
                                            found = true;
                                            Debug.Log($"Found property file by content: {propertyFilePath}");
                                            break;
                                        }
                                    }

                                    if (!found)
                                    {
                                        Debug.LogWarning($"No property file found for object ID: {objectId}");
                                    }
                                }

                                // Parse property data
                                string propertyName = string.Empty;
                                string propertyType = string.Empty;
                                string modelPath = string.Empty;
                                float size = 1.0f;
                                float variance = 0.0f;

                                if (!string.IsNullOrEmpty(propertyContent))
                                {
                                    string[] propertyLines = propertyContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                    Debug.Log($"Property file has {propertyLines.Length} lines");

                                    foreach (var propLine in propertyLines)
                                    {
                                        string trimmedLine = propLine.Trim();

                                        if (trimmedLine.StartsWith("propertyname"))
                                        {
                                            propertyName = ExtractValueInQuotes(trimmedLine);
                                            Debug.Log($"Property name: {propertyName}");
                                        }
                                        else if (trimmedLine.StartsWith("propertytype"))
                                        {
                                            propertyType = ExtractValueInQuotes(trimmedLine);
                                            Debug.Log($"Property type: {propertyType}");
                                        }
                                        else if (trimmedLine.StartsWith("treefile") || trimmedLine.StartsWith("modelfile") || trimmedLine.StartsWith("buildingfile"))
                                        {
                                            modelPath = ExtractValueInQuotes(trimmedLine);
                                            Debug.Log($"Model path: {modelPath}");
                                        }
                                        else if (trimmedLine.StartsWith("treesize") || trimmedLine.StartsWith("modelsize") || trimmedLine.StartsWith("buildingsize"))
                                        {
                                            float.TryParse(ExtractValueInQuotes(trimmedLine), NumberStyles.Any, CultureInfo.InvariantCulture, out size);
                                            Debug.Log($"Size: {size}");
                                        }
                                        else if (trimmedLine.StartsWith("treevariance") || trimmedLine.StartsWith("modelvariance") || trimmedLine.StartsWith("buildingvariance"))
                                        {
                                            float.TryParse(ExtractValueInQuotes(trimmedLine), NumberStyles.Any, CultureInfo.InvariantCulture, out variance);
                                            Debug.Log($"Variance: {variance}");
                                        }
                                    }
                                }
                                else
                                {
                                    // When no property data is found, use defaults
                                    // Extract model name from the model path if available
                                    if (!string.IsNullOrEmpty(modelPath))
                                    {
                                        propertyName = Path.GetFileNameWithoutExtension(modelPath);
                                    }
                                    else
                                    {
                                        propertyName = $"Object_{objectId}";
                                    }
                                    propertyType = "Unknown";
                                    Debug.LogWarning($"No property data found, using name from model path: {propertyName}");
                                }

                                // Create object name based on property data
                                string objectDisplayName = !string.IsNullOrEmpty(propertyName) ? propertyName : $"Object_{objectName}";

                                // Get fbx model name from path
                                string fbxModelName = string.Empty;
                                if (!string.IsNullOrEmpty(modelPath))
                                {
                                    fbxModelName = GetFbxNameFromModelPath(modelPath);
                                    Debug.Log($"FBX model name: {fbxModelName}");
                                }

                                // Find model in Unity project
                                GameObject modelPrefab = null;
                                if (!string.IsNullOrEmpty(fbxModelName))
                                {
                                    modelPrefab = FindModelInProject(fbxModelName);
                                    if (modelPrefab != null)
                                    {
                                        Debug.Log($"Found Unity model: {fbxModelName}");
                                    }
                                    else
                                    {
                                        Debug.Log($"Unity model not found: {fbxModelName}");
                                    }
                                }

                                // Create game object
                                GameObject newObject;

                                if (modelPrefab != null)
                                {
                                    newObject = Instantiate(modelPrefab);
                                    totalObjectsWithModels++;
                                }
                                else
                                {
                                    if (createPrimitiveIfModelNotFound)
                                    {
                                        Debug.Log("Creating primitive cube as fallback");
                                        newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                        newObject.transform.localScale = new Vector3(1, 1, 1) * (size / 1000f); // Scale based on property size
                                    }
                                    else
                                    {
                                        Debug.Log("Creating empty GameObject as fallback");
                                        newObject = new GameObject();
                                    }
                                }

                                // Group by property type
                                string groupType = propertyType;
                                if (string.IsNullOrEmpty(groupType))
                                {
                                    groupType = "Unknown";
                                }

                                // Normalize property type for better grouping
                                groupType = NormalizePropertyType(groupType);

                                // Get or create container for this type
                                GameObject typeContainer;
                                if (!typeContainers.TryGetValue(groupType, out typeContainer))
                                {
                                    typeContainer = new GameObject(groupType);
                                    typeContainer.transform.parent = sectorContainer.transform;
                                    typeContainers[groupType] = typeContainer;
                                }

                                newObject.name = objectDisplayName;
                                newObject.transform.parent = typeContainer.transform;
                                newObject.transform.position = relativePosition;

                                // Ağaçlar için özel düzenlemeler uygula
                                if (groupType == "Trees")
                                {
                                    // Rotasyon değerlerini ayarla
                                    newObject.transform.eulerAngles = new Vector3(-90f, 0f, -180f);

                                    // Scale değerlerini ayarla
                                    Vector3 currentScale = newObject.transform.localScale;
                                    // X ve Z scale'i 1, Y scale'i -1 olarak ayarla
                                    newObject.transform.localScale = new Vector3(1f, -1f, 1f);

                                    Debug.Log($"Applied tree rotation X:-90, Y:0, Z:-180 and scale X:1, Y:-1, Z:1 to {objectDisplayName}");
                                }
                                else
                                {
                                    newObject.transform.eulerAngles = rotation;
                                }

                                // Obje verisi oluştur
                                if (createScriptableObjects)
                                {
                                    Debug.Log($"Creating object data for: {objectDisplayName}");
                                    ObjectDataWrapper objectData = CreateObjectData(
                                        objectId,
                                        propertyName,
                                        propertyType,
                                        modelPath,
                                        new Vector3(originalX, originalZ, originalY),
                                        rotation,
                                        size,
                                        variance,
                                        modelPrefab,
                                        areadataPath,
                                        propertyFilePath);

                                    if (objectData != null)
                                    {
                                        // Normalize property type for better grouping
                                        string normalizedType = NormalizePropertyType(propertyType);

                                        // Sektör için koleksiyon al veya oluştur
                                        if (!sectorTypeCollections.TryGetValue(sectorName, out Dictionary<string, Metin2ObjectsCollection> typeCollectionsInSector))
                                        {
                                            typeCollectionsInSector = new Dictionary<string, Metin2ObjectsCollection>();
                                            sectorTypeCollections[sectorName] = typeCollectionsInSector;
                                        }

                                        // Bu tip için koleksiyon al veya oluştur
                                        if (!typeCollectionsInSector.TryGetValue(normalizedType, out Metin2ObjectsCollection collection))
                                        {
                                            collection = ScriptableObject.CreateInstance<Metin2ObjectsCollection>();
                                            collection.objectType = normalizedType;
                                            collection.sectorName = sectorName;
                                            typeCollectionsInSector[normalizedType] = collection;
                                        }

                                        // Obje verisini koleksiyona ekle
                                        collection.objects.Add(objectData);

                                        // Şimdilik GameObject içine sadece referans değerleri koy - daha sonra ScriptableObject assetlerini kaydedince güncellenir
                                        Metin2ObjectReference objRef = newObject.AddComponent<Metin2ObjectReference>();
                                        objRef.collectionName = $"{sectorName}_{normalizedType}";
                                        objRef.objectIndex = collection.objects.Count - 1;
                                        Debug.Log("Added Metin2ObjectReference component to GameObject");
                                    }
                                    else
                                    {
                                        Debug.LogError("Failed to create object data");
                                    }
                                }

                                objectsInThisSector++;
                                totalObjectsImported++;
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError($"Error processing object: {ex.Message}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Invalid position data format at line {i + 1}");
                        }
                    }
                }

                Debug.Log($"Imported {objectsInThisSector} objects from sector {sectorName}");

                // If no objects were found in this sector, remove the empty container
                if (objectsInThisSector == 0)
                {
                    DestroyImmediate(sectorContainer);
                }
            }

            // Oluşturduğum koleksiyonları kaydetme işlemini gerçekleştiriyorum.
            // I'm saving the collections I've created.
            if (createScriptableObjects)
            {
                Debug.Log("Saving all object collections as ScriptableObjects...");
                int collectionsCreated = 0;

                // Her sektör ve obje tipi için koleksiyonları ayrı ayrı kaydediyorum.
                // I'm saving collections separately for each sector and object type.
                foreach (var sectorKvp in sectorTypeCollections)
                {
                    string sectorName = sectorKvp.Key;
                    var typeCollections = sectorKvp.Value;

                    foreach (var typeKvp in typeCollections)
                    {
                        string typeName = typeKvp.Key;
                        Metin2ObjectsCollection collection = typeKvp.Value;

                        string assetName = $"{sectorName}_{typeName}";
                        string assetPath = Path.Combine(scriptableObjectsPath, $"{assetName}.asset");
                        assetPath = assetPath.Replace('\\', '/');

                        Debug.Log($"Creating collection ScriptableObject at: {assetPath} with {collection.objects.Count} objects");
                        AssetDatabase.CreateAsset(collection, assetPath);
                        collectionsCreated++;
                    }
                }

                AssetDatabase.SaveAssets();
                Debug.Log($"Created {collectionsCreated} collection ScriptableObjects");
            }

            // If no objects were imported, remove the parent
            if (totalObjectsImported == 0)
            {
                DestroyImmediate(parentObject);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Import Results",
                $"Import completed:\n" +
                $"Total directories scanned: {totalDirectoriesScanned}\n" +
                $"Total areas found: {totalAreasFound}\n" +
                $"Total objects imported: {totalObjectsImported}\n" +
                $"Objects with models: {totalObjectsWithModels}", "OK");
        }

        private ObjectDataWrapper CreateObjectData(
            string objectId,
            string propertyName,
            string propertyType,
            string modelPath,
            Vector3 originalPosition,
            Vector3 rotation,
            float size,
            float variance,
            GameObject modelPrefab,
            string areadataPath,
            string propertyPath)
        {
            try
            {
                // If propertyName is empty, use a better default name
                if (string.IsNullOrEmpty(propertyName))
                {
                    // Try to extract name from model path first
                    if (!string.IsNullOrEmpty(modelPath))
                    {
                        propertyName = Path.GetFileNameWithoutExtension(modelPath);
                    }
                    else
                    {
                        propertyName = $"Object_{objectId}";
                    }
                }

                // Seri hale getirilebilir veri yapısını burada oluşturuyorum.
                // Here I'm creating a serializable data structure.
                ObjectDataWrapper data = new ObjectDataWrapper();
                data.ObjectID = objectId;
                data.PropertyName = propertyName;
                data.PropertyType = propertyType;
                data.OriginalModelPath = modelPath;
                data.OriginalPosition = originalPosition;
                data.OriginalRotation = rotation;
                data.OriginalSize = size;
                data.OriginalVariance = variance;

                // İlk olarak modelPrefab varsa bunu kullanıyorum.
                // First I use the modelPrefab if it exists.
                if (modelPrefab != null)
                {
                    data.UnityModel = modelPrefab;
                    Debug.Log($"Used existing model prefab for {propertyName}");
                }
                else
                {
                    // ModelPrefab bulunamadıysa, property name kullanarak modeli bulmaya çalışıyorum.
                    // If modelPrefab is not found, I try to find the model using the property name.
                    GameObject foundModel = FindModelByPropertyName(propertyName);
                    if (foundModel != null)
                    {
                        data.UnityModel = foundModel;
                        Debug.Log($"Found and assigned model for {propertyName} using property name search");
                    }
                    else
                    {
                        // Property name ile bulunamazsa model path'ten isim çıkar ve ara
                        string modelFileName = string.Empty;
                        if (!string.IsNullOrEmpty(modelPath))
                        {
                            modelFileName = Path.GetFileNameWithoutExtension(modelPath);
                            GameObject modelFromPath = FindModelByPropertyName(modelFileName);
                            if (modelFromPath != null)
                            {
                                data.UnityModel = modelFromPath;
                                Debug.Log($"Found and assigned model for {propertyName} using model path filename: {modelFileName}");
                            }
                        }
                    }
                }

                data.AreadataPath = areadataPath;
                data.PropertyPath = propertyPath;

                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating object data: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        // Property Name kullanarak model arayan yeni metod oluşturdum. İsimlendirme yapısına göre modelleri bulmayı kolaylaştırıyor.
        // I created a new method that searches for models using Property Name. This makes it easier to find models according to the naming structure.
        private GameObject FindModelByPropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return null;

            Debug.Log($"Searching for model using property name: {propertyName}");

            // Önce tam isim eşleşmesi arayalım
            string[] guids = AssetDatabase.FindAssets($"{propertyName} t:prefab t:model t:gameobject");

            if (guids.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                Debug.Log($"Found model at: {assetPath}");
                return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            }

            // Tam eşleşme bulunamadıysa, tüm adları tarayalım
            guids = AssetDatabase.FindAssets("t:prefab t:model t:gameobject");

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string assetName = Path.GetFileNameWithoutExtension(assetPath).ToLower();
                string searchName = propertyName.ToLower();

                // Tam eşleşme
                if (assetName == searchName)
                {
                    Debug.Log($"Found exact match: {assetPath}");
                    return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                }

                // İçeren eşleşme
                if (assetName.Contains(searchName) || searchName.Contains(assetName))
                {
                    Debug.Log($"Found partial match: {assetPath}");
                    return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                }
            }


            string[] searchPaths = new string[] {
                "Assets/Models",
                "Assets/Prefabs",
                "Assets/Resources",
                modelsFolderPath
            };

            foreach (string path in searchPaths)
            {
                if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) continue;

                string[] files = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);
                files = files.Concat(Directory.GetFiles(path, "*.fbx", SearchOption.AllDirectories)).ToArray();

                foreach (string file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file).ToLower();
                    string searchName = propertyName.ToLower();

                    if (fileName == searchName || fileName.Contains(searchName) || searchName.Contains(fileName))
                    {
                        string assetPath = file.Replace('\\', '/');
                        if (!assetPath.StartsWith("Assets/"))
                        {
                            int index = assetPath.IndexOf("Assets/");
                            if (index >= 0)
                            {
                                assetPath = assetPath.Substring(index);
                            }
                            else
                            {
                                continue;
                            }
                        }

                        Debug.Log($"Found match in folder search: {assetPath}");
                        return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    }
                }
            }

            Debug.Log($"No model found for property name: {propertyName}");
            return null;
        }

        private string GetFbxNameFromModelPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            // Extract base file name without extension
            string fileName = Path.GetFileNameWithoutExtension(path);

            // Clean up file name - remove any special characters or paths
            return fileName;
        }

        private GameObject FindModelInProject(string modelName)
        {
            if (string.IsNullOrEmpty(modelName)) return null;

            // Search paths
            List<string> searchPaths = new List<string>();

            // Add user specified path if provided
            if (!string.IsNullOrEmpty(modelsFolderPath))
            {
                searchPaths.Add(modelsFolderPath);
            }

            // Add default paths
            searchPaths.Add("Assets/Models");
            searchPaths.Add("Assets");

            foreach (var path in searchPaths)
            {
                if (!Directory.Exists(path)) continue;

                // Search for .fbx files
                string[] fbxFiles = Directory.GetFiles(path, "*.fbx", SearchOption.AllDirectories);

                foreach (var fbxFile in fbxFiles)
                {
                    string fbxName = Path.GetFileNameWithoutExtension(fbxFile);

                    // Check for exact match
                    if (fbxName.Equals(modelName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        string assetPath = fbxFile.Replace('\\', '/');
                        if (!assetPath.StartsWith("Assets"))
                        {
                            int index = assetPath.IndexOf("Assets");
                            if (index >= 0)
                            {
                                assetPath = assetPath.Substring(index);
                            }
                            else
                            {
                                continue; // Skip if not in Assets folder
                            }
                        }

                        return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    }
                }
            }

            return null;
        }

        private string ExtractValueInQuotes(string line)
        {
            int startQuote = line.IndexOf('"');
            int endQuote = line.LastIndexOf('"');

            if (startQuote >= 0 && endQuote > startQuote)
            {
                return line.Substring(startQuote + 1, endQuote - startQuote - 1);
            }

            // If quotes not found, try to extract value after tabs or spaces
            string[] parts = line.Split(new char[] { '\t', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                return parts[parts.Length - 1].Trim('"');
            }

            return string.Empty;
        }

        private void SnapObjectsToGround()
        {
            // Find the parent object that contains all imported objects
            GameObject parentObject = GameObject.Find("Metin2_All_Imported_Objects");

            if (parentObject == null)
            {
                EditorUtility.DisplayDialog("Error", "No imported objects found! Import objects first.", "OK");
                return;
            }

            // Find all terrains in the scene
            Terrain[] allTerrains = Terrain.activeTerrains;

            if (allTerrains.Length == 0 && targetTerrain == null)
            {
                EditorUtility.DisplayDialog("Error", "No terrains found in the scene!", "OK");
                return;
            }

            // Count objects to process
            int totalObjects = 0;
            CountChildObjects(parentObject.transform, ref totalObjects);

            // Process all children recursively
            int processedObjects = 0;
            bool cancelled = false;

            // Start snapping objects to ground
            SnapChildObjectsToGround(parentObject.transform, allTerrains, ref processedObjects, totalObjects, ref cancelled);

            EditorUtility.ClearProgressBar();

            if (cancelled)
            {
                EditorUtility.DisplayDialog("Operation Cancelled", "Snapping objects to ground was cancelled.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Success", $"Successfully snapped {processedObjects} objects to ground.", "OK");
            }
        }

        private void CountChildObjects(Transform parent, ref int count)
        {
            // Count all objects with transforms, not just ones with meshes
            foreach (Transform child in parent)
            {
                // Don't count objects that are just containers/sectors
                if (!child.name.StartsWith("Sector_"))
                {
                    count++;
                }

                // Continue counting recursively
                CountChildObjects(child, ref count);
            }
        }

        private void SnapChildObjectsToGround(Transform parent, Terrain[] allTerrains, ref int processedObjects, int totalObjects, ref bool cancelled)
        {
            foreach (Transform child in parent)
            {
                // Process all objects except container/sector objects
                if (!child.name.StartsWith("Sector_"))
                {
                    // Show progress
                    if (EditorUtility.DisplayCancelableProgressBar("Snapping Objects to Ground",
                        $"Processing object {processedObjects + 1}/{totalObjects}...",
                        (float)processedObjects / totalObjects))
                    {
                        cancelled = true;
                        return;
                    }

                    Vector3 position = child.position;
                    bool foundTerrain = false;
                    float highestPoint = float.MinValue;

                    // Check each terrain to find the one at this position
                    foreach (Terrain terrain in allTerrains)
                    {
                        // Check if position is within this terrain's bounds
                        Bounds terrainBounds = new Bounds(
                            terrain.transform.position + new Vector3(terrain.terrainData.size.x / 2, 0, terrain.terrainData.size.z / 2),
                            terrain.terrainData.size
                        );

                        // Expand bounds slightly to catch objects at the edge
                        terrainBounds.Expand(new Vector3(1, 100, 1));

                        // Create a position to test (ignoring Y)
                        Vector3 testPosition = new Vector3(position.x, 0, position.z);
                        Vector3 terrainTestPosition = new Vector3(terrainBounds.center.x, 0, terrainBounds.center.z);
                        Vector3 terrainTestSize = new Vector3(terrainBounds.size.x, 0, terrainBounds.size.z);

                        // Check if within this terrain's XZ bounds
                        if (Mathf.Abs(testPosition.x - terrainTestPosition.x) < terrainTestSize.x / 2 &&
                            Mathf.Abs(testPosition.z - terrainTestPosition.z) < terrainTestSize.z / 2)
                        {
                            // Get terrain-local position
                            Vector3 terrainLocalPos = position - terrain.transform.position;

                            // Convert to terrain space (0-1)
                            float normalizedX = Mathf.Clamp01(terrainLocalPos.x / terrain.terrainData.size.x);
                            float normalizedZ = Mathf.Clamp01(terrainLocalPos.z / terrain.terrainData.size.z);

                            // Sample height at this position
                            float terrainHeight = terrain.terrainData.GetHeight(
                                Mathf.RoundToInt(normalizedX * (terrain.terrainData.heightmapResolution - 1)),
                                Mathf.RoundToInt(normalizedZ * (terrain.terrainData.heightmapResolution - 1))
                            );

                            // World space height
                            float worldHeight = terrain.transform.position.y + terrainHeight;

                            // Keep track of the highest point found
                            if (worldHeight > highestPoint)
                            {
                                highestPoint = worldHeight;
                                foundTerrain = true;
                            }
                        }
                    }

                    // If a valid terrain was found, set the height
                    if (foundTerrain)
                    {
                        position.y = highestPoint + groundOffset;
                        child.position = position;
                    }
                    else if (targetTerrain != null)
                    {
                        // Fall back to target terrain if position is outside all terrain bounds

                        // Get the world position
                        Vector3 terrainLocalPos = position - targetTerrain.transform.position;

                        // Convert to terrain space (0-1)
                        Vector3 terrainNormalizedPos = new Vector3(
                            terrainLocalPos.x / targetTerrain.terrainData.size.x,
                            0,
                            terrainLocalPos.z / targetTerrain.terrainData.size.z
                        );

                        // Clamp to terrain bounds
                        terrainNormalizedPos.x = Mathf.Clamp01(terrainNormalizedPos.x);
                        terrainNormalizedPos.z = Mathf.Clamp01(terrainNormalizedPos.z);

                        // Sample height at this position
                        float terrainHeight = targetTerrain.terrainData.GetHeight(
                            Mathf.RoundToInt(terrainNormalizedPos.x * (targetTerrain.terrainData.heightmapResolution - 1)),
                            Mathf.RoundToInt(terrainNormalizedPos.z * (targetTerrain.terrainData.heightmapResolution - 1))
                        );

                        // Set the Y position to terrain height plus offset
                        position.y = targetTerrain.transform.position.y + terrainHeight + groundOffset;
                        child.position = position;
                    }

                    processedObjects++;
                }

                // Process children recursively
                SnapChildObjectsToGround(child, allTerrains, ref processedObjects, totalObjects, ref cancelled);

                // Check if cancelled
                if (cancelled)
                {
                    return;
                }
            }
        }

        private string NormalizePropertyType(string propertyType)
        {
            if (string.IsNullOrEmpty(propertyType))
                return "Unknown";

            // Convert to lowercase for case-insensitive comparison
            string type = propertyType.ToLower();

            // Group similar types together
            if (type.Contains("tree") || type == "forest" || type == "treegroup")
                return "Trees";
            else if (type.Contains("building") || type == "house" || type == "wall" || type.Contains("house"))
                return "Buildings";
            else if (type.Contains("effect") || type == "particle")
                return "Effects";
            else if (type.Contains("object") || type == "prop" || type == "item")
                return "Objects";
            else if (type.Contains("npc") || type.Contains("character") || type.Contains("monster"))
                return "Characters";
            else
                return "Other_" + propertyType; // Keep original name for unknown types
        }
    }
}