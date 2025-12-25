using UnityEditor;
using UnityEngine;
using ADVance.Utility;
using ADVance.Data;

namespace ADVance.Editor
{
    public class CsvToScriptableObjectEditor : EditorWindow
    {
        private const string SaveKeyCsvFile = "CsvToScriptableObject_CsvFile";
        private const string SaveKeyOutputFolder = "CsvToScriptableObject_OutputFolder";
        private const string SaveKeyFileName = "CsvToScriptableObject_FileName";
        private const string SaveKeyExistingData = "CsvToScriptableObject_ExistingData";

        private TextAsset _csvFile;
        private string _outputFolder = "Assets";
        private string _fileName = "NewScenarioData";
        private ScenarioData _existingScenarioData;
        private ScenarioCommandRegistry _commandRegistry;
        private ScenarioCommandRegistry _commandRegistryLineAdv;
        private StatusKind _statusKind = StatusKind.None;
        private string _statusMessage = string.Empty;

        [MenuItem("Tools/ADVance/CSV to ScriptableObject")]
        public static void ShowWindow()
        {
            GetWindow<CsvToScriptableObjectEditor>("CSV to ScriptableObject");
        }

        private void OnEnable()
        {
            LoadEditorPrefs();
        }

        private void OnDisable()
        {
            SaveEditorPrefs();
        }

        private void LoadEditorPrefs()
        {
            var csvFileGuid = EditorPrefs.GetString(SaveKeyCsvFile, "");
            if (!string.IsNullOrEmpty(csvFileGuid))
            {
                var csvFilePath = AssetDatabase.GUIDToAssetPath(csvFileGuid);
                _csvFile = AssetDatabase.LoadAssetAtPath<TextAsset>(csvFilePath);
            }

            _outputFolder = EditorPrefs.GetString(SaveKeyOutputFolder, "Assets");
            _fileName = EditorPrefs.GetString(SaveKeyFileName, "NewScenarioData");

            var existingDataGuid = EditorPrefs.GetString(SaveKeyExistingData, "");
            if (!string.IsNullOrEmpty(existingDataGuid))
            {
                var existingDataPath = AssetDatabase.GUIDToAssetPath(existingDataGuid);
                _existingScenarioData = AssetDatabase.LoadAssetAtPath<ScenarioData>(existingDataPath);
            }
        }

        private void SaveEditorPrefs()
        {
            if (_csvFile != null)
            {
                var csvFileGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_csvFile));
                EditorPrefs.SetString(SaveKeyCsvFile, csvFileGuid);
            }
            else
            {
                EditorPrefs.DeleteKey(SaveKeyCsvFile);
            }

            EditorPrefs.SetString(SaveKeyOutputFolder, _outputFolder);
            EditorPrefs.SetString(SaveKeyFileName, _fileName);

            if (_existingScenarioData != null)
            {
                var existingDataGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_existingScenarioData));
                EditorPrefs.SetString(SaveKeyExistingData, existingDataGuid);
            }
            else
            {
                EditorPrefs.DeleteKey(SaveKeyExistingData);
            }
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

        private enum StatusKind
        {
            None,
            Info,
            Warning,
            Error
        }

        private void SetStatus(string titleEn, string messageEn, string titleJa, string messageJa, StatusKind kind)
        {
            _statusKind = kind;
            _statusMessage = BuildBilingualMessage(titleEn, messageEn, titleJa, messageJa);
        }

        private static string BuildBilingualMessage(string titleEn, string messageEn, string titleJa, string messageJa)
        {
            var en = string.IsNullOrEmpty(titleEn) ? messageEn : $"{titleEn}: {messageEn}";
            var ja = string.IsNullOrEmpty(titleJa) ? messageJa : $"{titleJa}: {messageJa}";
            if (string.IsNullOrEmpty(en))
            {
                return ja;
            }

            if (string.IsNullOrEmpty(ja))
            {
                return en;
            }

            return $"{en}\n{ja}";
        }

        private void DrawStatus()
        {
            if (_statusKind == StatusKind.None || string.IsNullOrEmpty(_statusMessage))
            {
                return;
            }

            var icon = GetStatusIcon(_statusKind);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                if (icon?.image != null)
                {
                    GUILayout.Label(icon, GUILayout.Width(18), GUILayout.Height(18));
                }

                EditorGUILayout.LabelField(_statusMessage, EditorStyles.wordWrappedLabel);
                if (GUILayout.Button("Clear", GUILayout.Width(60)))
                {
                    _statusKind = StatusKind.None;
                    _statusMessage = string.Empty;
                }
            }

            EditorGUILayout.Space();
        }

        private static GUIContent GetStatusIcon(StatusKind kind)
        {
            return kind switch
            {
                StatusKind.Info => EditorGUIUtility.IconContent("console.infoicon"),
                StatusKind.Warning => EditorGUIUtility.IconContent("console.warnicon"),
                StatusKind.Error => EditorGUIUtility.IconContent("console.erroricon"),
                _ => null
            };
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();

            GUILayout.Label("CSV to ScriptableObject", EditorStyles.boldLabel);
            DrawStatus();

            _csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", _csvFile, typeof(TextAsset), false);

            _outputFolder = EditorGUILayout.TextField("Output Folder", _outputFolder);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                var selectedPath = EditorUtility.OpenFolderPanel("Select Output Folder", _outputFolder, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // プロジェクトの相対パスに変換
                    var relativePath = FileUtil.GetProjectRelativePath(selectedPath);
                    if (!string.IsNullOrEmpty(relativePath))
                    {
                        _outputFolder = relativePath;
                    }
                }
            }

            GUILayout.EndHorizontal();

            _fileName = EditorGUILayout.TextField("File Name", _fileName);

            EditorGUILayout.Space();
            GUILayout.Label("Overwrite Existing ScriptableObject", EditorStyles.boldLabel);
            _existingScenarioData = (ScenarioData)EditorGUILayout.ObjectField("Existing ScenarioData", _existingScenarioData, typeof(ScenarioData), false);

            if (EditorGUI.EndChangeCheck())
            {
                SaveEditorPrefs();

                // インスペクターでアセットが変更された場合、自動保存
                if (_csvFile != null)
                {
                    EditorUtility.SetDirty(_csvFile);
                }

                if (_existingScenarioData != null)
                {
                    EditorUtility.SetDirty(_existingScenarioData);
                }

                AssetDatabase.SaveAssets();
            }

            if (GUILayout.Button("Generate ScriptableObject"))
            {
                if (_csvFile != null)
                {
                    try
                    {
                        CreateCommandRegistry();
                        var scenarioData = CsvImporter.ImportFromCsv(_csvFile, _commandRegistry);
                        var path = System.IO.Path.Combine(_outputFolder, _fileName + ".asset");
                        AssetDatabase.CreateAsset(scenarioData, path);
                        AssetDatabase.SaveAssets();
                        SetStatus(
                            "Success",
                            "ScriptableObject has been created!",
                            "成功",
                            "ScriptableObject を作成しました。",
                            StatusKind.Info);
                    }
                    catch (System.ArgumentException ex)
                    {
                        SetStatus(
                            "Command Validation Error",
                            ex.Message,
                            "コマンド検証エラー",
                            ex.Message,
                            StatusKind.Error);
                    }
                }
                else
                {
                    SetStatus(
                        "Error",
                        "Please select a CSV file.",
                        "エラー",
                        "CSV ファイルを選択してください。",
                        StatusKind.Warning);
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
                        SetStatus(
                            "Success",
                            "ScriptableObject has been overwritten!",
                            "成功",
                            "ScriptableObject を上書きしました。",
                            StatusKind.Info);
                    }
                    catch (System.ArgumentException ex)
                    {
                        SetStatus(
                            "Command Validation Error",
                            ex.Message,
                            "コマンド検証エラー",
                            ex.Message,
                            StatusKind.Error);
                    }
                }
                else
                {
                    if (_csvFile == null)
                    {
                        SetStatus(
                            "Error",
                            "Please select a CSV file.",
                            "エラー",
                            "CSV ファイルを選択してください。",
                            StatusKind.Warning);
                    }
                    else
                    {
                        SetStatus(
                            "Error",
                            "Please select an existing ScriptableObject to overwrite.",
                            "エラー",
                            "上書き対象の ScriptableObject を選択してください。",
                            StatusKind.Warning);
                    }
                }
            }

            EditorGUILayout.Space();
        }

    }
}
