using UnityEngine;

namespace ADVance.Editor
{
    [CreateAssetMenu(menuName = "ADVance/CSV Import Settings")]
    public class CsvImportSettings : ScriptableObject
    {
        public string OutputFolder = "Assets";
    }
}