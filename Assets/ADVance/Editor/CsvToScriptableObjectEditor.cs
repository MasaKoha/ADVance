using UnityEditor;
using UnityEngine;
using ADVance.Utility;

namespace ADVance.Editor
{
    public class CsvToScriptableObjectEditor : EditorWindow
    {
        private TextAsset _csvFile;
        private CsvImportSettings _settings;
        private string _fileName = "NewScenarioData";

        [MenuItem("Tools/ADVance/CSV to ScriptableObject")]
        public static void ShowWindow()
        {
            GetWindow<CsvToScriptableObjectEditor>("CSV to ScriptableObject");
        }

        private void OnGUI()
        {
            GUILayout.Label("CSV to ScriptableObject", EditorStyles.boldLabel);

            _csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", _csvFile, typeof(TextAsset), false);
            _settings = (CsvImportSettings)EditorGUILayout.ObjectField("Import Settings", _settings, typeof(CsvImportSettings), false);

            var outputFolder = _settings != null ? _settings.OutputFolder : "Assets";
            EditorGUILayout.LabelField("Output Folder", outputFolder);

            _fileName = EditorGUILayout.TextField("File Name", _fileName);

            if (GUILayout.Button("Generate ScriptableObject"))
            {
                if (_csvFile != null)
                {
                    var scenarioData = CsvImporter.ImportFromCsv(_csvFile);
                    string path = System.IO.Path.Combine(outputFolder, _fileName + ".asset");

                    AssetDatabase.CreateAsset(scenarioData, path);
                    AssetDatabase.SaveAssets();
                    EditorUtility.DisplayDialog("Success", "ScriptableObject has been created!", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please select a CSV file.", "OK");
                }
            }
        }
    }
}