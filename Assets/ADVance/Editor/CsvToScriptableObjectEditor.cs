using UnityEditor;
using UnityEngine;
using ADVance.Utility;
using ADVance.Data;

namespace ADVance.Editor
{
    public class CsvToScriptableObjectEditor : EditorWindow
    {
        private TextAsset _csvFile;
        private CsvImportSettings _settings;
        private string _fileName = "NewScenarioData";
        private ScenarioData _existingScenarioData;
        private ScenarioCommandRegistry _commandRegistry;

        [MenuItem("Tools/ADVance/CSV to ScriptableObject")]
        public static void ShowWindow()
        {
            GetWindow<CsvToScriptableObjectEditor>("CSV to ScriptableObject");
        }

        private void CreateCommandRegistry()
        {
            if (_commandRegistry != null)
            {
                return;
            }

            // 一時的なADVanceManagerを作成してコマンドレジストリを初期化
            var tempManager = new GameObject().AddComponent<TempADVanceManager>();
            tempManager.Initialize();
            _commandRegistry = tempManager.CommandRegistry;
            DestroyImmediate(tempManager.gameObject);
        }

        private void OnGUI()
        {
            GUILayout.Label("CSV to ScriptableObject", EditorStyles.boldLabel);

            _csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", _csvFile, typeof(TextAsset), false);
            _settings = (CsvImportSettings)EditorGUILayout.ObjectField("Import Settings", _settings, typeof(CsvImportSettings), false);

            var outputFolder = _settings != null ? _settings.OutputFolder : "Assets";
            EditorGUILayout.LabelField("Output Folder", outputFolder);

            _fileName = EditorGUILayout.TextField("File Name", _fileName);

            EditorGUILayout.Space();
            GUILayout.Label("Overwrite Existing ScriptableObject", EditorStyles.boldLabel);
            _existingScenarioData = (ScenarioData)EditorGUILayout.ObjectField("Existing ScenarioData", _existingScenarioData, typeof(ScenarioData), false);

            if (GUILayout.Button("Generate ScriptableObject"))
            {
                if (_csvFile != null)
                {
                    try
                    {
                        CreateCommandRegistry();
                        var scenarioData = CsvImporter.ImportFromCsv(_csvFile, _commandRegistry);
                        var path = System.IO.Path.Combine(outputFolder, _fileName + ".asset");
                        AssetDatabase.CreateAsset(scenarioData, path);
                        AssetDatabase.SaveAssets();
                        EditorUtility.DisplayDialog("Success", "ScriptableObject has been created!", "OK");
                    }
                    catch (System.ArgumentException ex)
                    {
                        EditorUtility.DisplayDialog("Command Validation Error", ex.Message, "OK");
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please select a CSV file.", "OK");
                }
            }

            if (GUILayout.Button("Overwrite Existing ScriptableObject"))
            {
                if (_csvFile != null && _existingScenarioData != null)
                {
                    try
                    {
                        CreateCommandRegistry();
                        var newData = CsvImporter.ImportFromCsv(_csvFile, _commandRegistry);

                        _existingScenarioData.Lines = newData.Lines;

                        EditorUtility.SetDirty(_existingScenarioData);
                        AssetDatabase.SaveAssets();
                        EditorUtility.DisplayDialog("Success", "ScriptableObject has been overwritten!", "OK");
                    }
                    catch (System.ArgumentException ex)
                    {
                        EditorUtility.DisplayDialog("Command Validation Error", ex.Message, "OK");
                    }
                }
                else
                {
                    if (_csvFile == null)
                        EditorUtility.DisplayDialog("Error", "Please select a CSV file.", "OK");
                    else
                        EditorUtility.DisplayDialog("Error", "Please select an existing ScriptableObject to overwrite.", "OK");
                }
            }
        }
    }
}