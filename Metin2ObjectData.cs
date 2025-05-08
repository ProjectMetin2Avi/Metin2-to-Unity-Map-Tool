using UnityEngine;

namespace Metin2MapTools
{
    [CreateAssetMenu(fileName = "Metin2ObjectData", menuName = "Metin2/Object Data")]
    public class Metin2ObjectData : ScriptableObject
    {
        public string objectID;
        public string propertyName;
        public string propertyType;
        public string originalModelPath;
        public Vector3 originalPosition;
        public Vector3 originalRotation;
        public float originalSize;
        public float originalVariance;

        public Object unityModel;

        public string areadataPath;
        public string propertyPath;
    }
}
