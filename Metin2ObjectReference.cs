using UnityEngine;
using UnityEditor;
using System;

namespace Metin2MapTools
{
    // Component to attach to GameObjects to reference their Metin2ObjectData
    public class Metin2ObjectReference : MonoBehaviour
    {
        // Doðrudan obje referansý
        public Metin2ObjectData objectData;

        // Koleksiyon tabanlý referans için yeni deðiþkenler
        public string collectionName;
        public int objectIndex = -1;

        // Wrapper sýnýfýný tutacak referans
        [HideInInspector]
        public ObjectDataWrapper objectDataWrapper;

        /// <summary>
        /// Koleksiyondan obje verisini getirir (eðer varsa)
        /// </summary>
        public Metin2ObjectData GetObjectDataFromCollection()
        {
            // Eðer doðrudan referans varsa onu kullan
            if (objectData != null) return objectData;

            // Eðer koleksiyon bilgileri var ise
            if (!string.IsNullOrEmpty(collectionName) && objectIndex >= 0)
            {
                // Koleksiyon asset'ini bul
                string assetPath = $"Assets/Metin2Data/Objects/{collectionName}.asset";
                Metin2ObjectsCollection collection = AssetDatabase.LoadAssetAtPath<Metin2ObjectsCollection>(assetPath);

                if (collection != null && collection.objects != null && objectIndex < collection.objects.Count)
                {
                    // ObjectDataWrapper'ý al
                    objectDataWrapper = collection.objects[objectIndex];

                    // Wrapper'dan bilgileri kullanarak Metin2ObjectData oluþtur
                    if (objectDataWrapper != null)
                    {
                        Metin2ObjectData data = ScriptableObject.CreateInstance<Metin2ObjectData>();
                        data.objectID = objectDataWrapper.ObjectID;
                        data.propertyName = objectDataWrapper.PropertyName;
                        data.propertyType = objectDataWrapper.PropertyType;
                        data.originalModelPath = objectDataWrapper.OriginalModelPath;
                        data.originalPosition = objectDataWrapper.OriginalPosition;
                        data.originalRotation = objectDataWrapper.OriginalRotation;
                        data.originalSize = objectDataWrapper.OriginalSize;
                        data.originalVariance = objectDataWrapper.OriginalVariance;
                        data.unityModel = objectDataWrapper.UnityModel;
                        data.areadataPath = objectDataWrapper.AreadataPath;
                        data.propertyPath = objectDataWrapper.PropertyPath;
                        return data;
                    }
                }
            }
            return null;
        }

        // Koleksiyonda bulunan obje verisinin Wrapper'ýna doðrudan eriþim saðlar
        public ObjectDataWrapper GetObjectDataWrapper()
        {
            if (objectDataWrapper != null) return objectDataWrapper;

            if (!string.IsNullOrEmpty(collectionName) && objectIndex >= 0)
            {
                string assetPath = $"Assets/Metin2Data/Objects/{collectionName}.asset";
                Metin2ObjectsCollection collection = AssetDatabase.LoadAssetAtPath<Metin2ObjectsCollection>(assetPath);

                if (collection != null && collection.objects != null && objectIndex < collection.objects.Count)
                {
                    objectDataWrapper = collection.objects[objectIndex];
                    return objectDataWrapper;
                }
            }
            return null;
        }

        // Editor için özellik gösterimini geliþtiren metodlar
#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(Metin2ObjectReference))]
        public class Metin2ObjectReferenceEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                Metin2ObjectReference reference = (Metin2ObjectReference)target;
                if (reference.objectData != null)
                {
                    UnityEditor.EditorGUILayout.LabelField("Object Data", UnityEditor.EditorStyles.boldLabel);
                    UnityEditor.EditorGUILayout.LabelField("Property Name", reference.objectData.propertyName);
                    UnityEditor.EditorGUILayout.LabelField("Property Type", reference.objectData.propertyType);
                    UnityEditor.EditorGUILayout.ObjectField("Model", reference.objectData.unityModel, typeof(UnityEngine.Object), false);
                }
                else if (!string.IsNullOrEmpty(reference.collectionName) && reference.objectIndex >= 0)
                {
                    string assetPath = $"Assets/Metin2Data/Objects/{reference.collectionName}.asset";
                    Metin2ObjectsCollection collection = AssetDatabase.LoadAssetAtPath<Metin2ObjectsCollection>(assetPath);

                    if (collection != null && collection.objects != null && reference.objectIndex < collection.objects.Count)
                    {
                        ObjectDataWrapper wrapper = collection.objects[reference.objectIndex];
                        if (wrapper != null)
                        {
                            UnityEditor.EditorGUILayout.LabelField("Collection Data", UnityEditor.EditorStyles.boldLabel);
                            UnityEditor.EditorGUILayout.LabelField("Collection", reference.collectionName);
                            UnityEditor.EditorGUILayout.LabelField("Index", reference.objectIndex.ToString());
                            UnityEditor.EditorGUILayout.LabelField("Property Name", wrapper.PropertyName);
                            UnityEditor.EditorGUILayout.LabelField("Property Type", wrapper.PropertyType);
                            UnityEditor.EditorGUILayout.ObjectField("Model", wrapper.UnityModel, typeof(UnityEngine.Object), false);
                        }
                    }
                }
            }
        }
#endif
    }
}
