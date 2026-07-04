using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class StockDataJsonTool
{
    private const int SchemaVersion = 1;
    private const string StockExportPath = "Assets/StockData/stock_definitions.json";
    private const string EffectExportPath = "Assets/StockData/stock_effects.json";

    private static readonly string[] StockFolders =
    {
        "Assets/Resources/Stocks",
        "Assets/Resources/InitialStock",
        "Assets/Resources/EmptyStock"
    };

    private static readonly string[] EffectFolders =
    {
        "Assets/Resources/StockEffects"
    };

    [MenuItem("Tools/Stocks/JSON/Export All")]
    public static void ExportAll()
    {
        int effectCount = ExportEffectsInternal();
        int stockCount = ExportStocksInternal();
        AssetDatabase.Refresh();
        Debug.Log(
            $"Exported {stockCount} stock definitions and {effectCount} stock effects to Assets/StockData."
        );
    }

    [MenuItem("Tools/Stocks/JSON/Import All")]
    public static void ImportAll()
    {
        int effectCount = ImportEffectsInternal();
        int stockCount = ImportStocksInternal();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(
            $"Imported {stockCount} stock definitions and {effectCount} stock effects from Assets/StockData."
        );
    }

    [MenuItem("Tools/Stocks/JSON/Export Stock Definitions")]
    public static void ExportStocks()
    {
        int count = ExportStocksInternal();
        AssetDatabase.Refresh();
        Debug.Log($"Exported {count} stock definitions to {StockExportPath}.");
    }

    [MenuItem("Tools/Stocks/JSON/Import Stock Definitions")]
    public static void ImportStocks()
    {
        int count = ImportStocksInternal();
        AssetDatabase.SaveAssets();
        Debug.Log($"Imported {count} stock definitions from {StockExportPath}.");
    }

    [MenuItem("Tools/Stocks/JSON/Export Stock Effects")]
    public static void ExportEffects()
    {
        int count = ExportEffectsInternal();
        AssetDatabase.Refresh();
        Debug.Log($"Exported {count} stock effects to {EffectExportPath}.");
    }

    [MenuItem("Tools/Stocks/JSON/Import Stock Effects")]
    public static void ImportEffects()
    {
        int count = ImportEffectsInternal();
        AssetDatabase.SaveAssets();
        Debug.Log($"Imported {count} stock effects from {EffectExportPath}.");
    }

    private static int ExportStocksInternal()
    {
        StockExportFile exportFile = new StockExportFile
        {
            schemaVersion = SchemaVersion
        };

        foreach (string guid in FindAssetGuids<StockDefinition>(StockFolders))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            StockDefinition stock = AssetDatabase.LoadAssetAtPath<StockDefinition>(path);
            if (stock == null)
                continue;

            exportFile.stocks.Add(new StockRecord
            {
                id = guid,
                assetPath = path,
                stockName = stock.stockName,
                iconId = GetGlobalObjectId(stock.icon),
                stockType = stock.stockType.ToString(),
                baseValue = stock.baseValue,
                rarity = stock.rarity.ToString(),
                effectIds = stock.effects == null
                    ? new List<string>()
                    : stock.effects
                        .Where(effect => effect != null)
                        .Select(GetAssetGuid)
                        .Where(effectGuid => !string.IsNullOrEmpty(effectGuid))
                        .ToList()
            });
        }

        WriteJson(StockExportPath, exportFile);
        return exportFile.stocks.Count;
    }

    private static int ExportEffectsInternal()
    {
        EffectExportFile exportFile = new EffectExportFile
        {
            schemaVersion = SchemaVersion
        };

        foreach (string guid in FindAssetGuids<StockEffect>(EffectFolders))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            StockEffect effect = AssetDatabase.LoadAssetAtPath<StockEffect>(path);
            if (effect == null)
                continue;

            exportFile.effects.Add(new EffectRecord
            {
                id = guid,
                assetPath = path,
                effectType = effect.GetType().FullName,
                effectName = effect.effectName,
                description = effect.description,
                settings = ExportEffectSettings(effect)
            });
        }

        WriteJson(EffectExportPath, exportFile);
        return exportFile.effects.Count;
    }

    private static int ImportStocksInternal()
    {
        StockExportFile exportFile = ReadJson<StockExportFile>(StockExportPath);
        if (exportFile == null)
            return 0;

        ValidateSchema(exportFile.schemaVersion, StockExportPath);
        int importedCount = 0;

        foreach (StockRecord record in exportFile.stocks)
        {
            StockDefinition stock = LoadAssetByRecord<StockDefinition>(
                record.id,
                record.assetPath
            );

            if (stock == null)
            {
                Debug.LogWarning(
                    $"Skipped stock '{record.stockName}': no existing asset matches ID '{record.id}'."
                );
                continue;
            }

            if (!Enum.TryParse(record.stockType, true, out StockType stockType))
            {
                Debug.LogWarning(
                    $"Skipped stock '{record.stockName}': unknown StockType '{record.stockType}'."
                );
                continue;
            }

            if (!Enum.TryParse(record.rarity, true, out Rarity rarity))
            {
                Debug.LogWarning(
                    $"Skipped stock '{record.stockName}': unknown Rarity '{record.rarity}'."
                );
                continue;
            }

            List<StockEffect> effects = new List<StockEffect>();
            foreach (string effectId in record.effectIds ?? new List<string>())
            {
                StockEffect effect = LoadAssetByGuid<StockEffect>(effectId);
                if (effect != null)
                {
                    effects.Add(effect);
                }
                else
                {
                    Debug.LogWarning(
                        $"Stock '{record.stockName}' references missing effect ID '{effectId}'."
                    );
                }
            }

            Undo.RecordObject(stock, "Import Stock Definition JSON");
            stock.stockName = record.stockName;
            stock.icon = LoadGlobalObject<Sprite>(record.iconId);
            stock.stockType = stockType;
            stock.baseValue = record.baseValue;
            stock.rarity = rarity;
            stock.effects = effects.ToArray();
            EditorUtility.SetDirty(stock);
            importedCount++;
        }

        return importedCount;
    }

    private static int ImportEffectsInternal()
    {
        EffectExportFile exportFile = ReadJson<EffectExportFile>(EffectExportPath);
        if (exportFile == null)
            return 0;

        ValidateSchema(exportFile.schemaVersion, EffectExportPath);
        int importedCount = 0;

        foreach (EffectRecord record in exportFile.effects)
        {
            StockEffect effect = LoadAssetByRecord<StockEffect>(record.id, record.assetPath);
            if (effect == null)
            {
                Debug.LogWarning(
                    $"Skipped effect '{record.effectName}': no existing asset matches ID '{record.id}'."
                );
                continue;
            }

            if (!string.Equals(effect.GetType().FullName, record.effectType, StringComparison.Ordinal))
            {
                Debug.LogWarning(
                    $"Skipped effect '{record.effectName}': JSON type '{record.effectType}' does not match asset type '{effect.GetType().FullName}'."
                );
                continue;
            }

            Undo.RecordObject(effect, "Import Stock Effect JSON");
            effect.effectName = record.effectName;
            effect.description = record.description;

            SerializedObject serializedEffect = new SerializedObject(effect);
            foreach (SerializedFieldRecord setting in record.settings ?? new List<SerializedFieldRecord>())
                ImportSerializedSetting(serializedEffect, setting, effect);

            serializedEffect.ApplyModifiedProperties();
            EditorUtility.SetDirty(effect);
            importedCount++;
        }

        return importedCount;
    }

    private static List<SerializedFieldRecord> ExportEffectSettings(StockEffect effect)
    {
        List<SerializedFieldRecord> settings = new List<SerializedFieldRecord>();
        SerializedObject serializedEffect = new SerializedObject(effect);
        SerializedProperty property = serializedEffect.GetIterator();
        bool enterChildren = true;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (property.name == "m_Script"
                || property.name == nameof(StockEffect.effectName)
                || property.name == nameof(StockEffect.description))
                continue;

            if (TryExportSerializedSetting(property, out SerializedFieldRecord setting))
            {
                settings.Add(setting);
            }
            else
            {
                Debug.LogWarning(
                    $"Effect '{effect.name}' field '{property.name}' uses unsupported serialized type {property.propertyType} and was not exported.",
                    effect
                );
            }
        }

        return settings;
    }

    private static bool TryExportSerializedSetting(
        SerializedProperty property,
        out SerializedFieldRecord setting
    )
    {
        setting = new SerializedFieldRecord
        {
            name = property.name,
            type = property.propertyType.ToString()
        };

        switch (property.propertyType)
        {
            case SerializedPropertyType.Integer:
                setting.value = property.longValue.ToString(CultureInfo.InvariantCulture);
                return true;

            case SerializedPropertyType.Boolean:
                setting.value = property.boolValue.ToString();
                return true;

            case SerializedPropertyType.Float:
                setting.value = property.doubleValue.ToString("R", CultureInfo.InvariantCulture);
                return true;

            case SerializedPropertyType.String:
                setting.value = property.stringValue;
                return true;

            case SerializedPropertyType.Enum:
                setting.value = property.enumValueIndex >= 0
                    && property.enumValueIndex < property.enumNames.Length
                        ? property.enumNames[property.enumValueIndex]
                        : property.enumValueIndex.ToString(CultureInfo.InvariantCulture);
                return true;

            case SerializedPropertyType.ObjectReference:
                setting.value = GetGlobalObjectId(property.objectReferenceValue);
                return true;

            default:
                return false;
        }
    }

    private static void ImportSerializedSetting(
        SerializedObject serializedObject,
        SerializedFieldRecord setting,
        UnityEngine.Object owner
    )
    {
        SerializedProperty property = serializedObject.FindProperty(setting.name);
        if (property == null)
        {
            Debug.LogWarning(
                $"Asset '{owner.name}' no longer has serialized field '{setting.name}'.",
                owner
            );
            return;
        }

        try
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.longValue = long.Parse(setting.value, CultureInfo.InvariantCulture);
                    break;

                case SerializedPropertyType.Boolean:
                    property.boolValue = bool.Parse(setting.value);
                    break;

                case SerializedPropertyType.Float:
                    property.doubleValue = double.Parse(setting.value, CultureInfo.InvariantCulture);
                    break;

                case SerializedPropertyType.String:
                    property.stringValue = setting.value;
                    break;

                case SerializedPropertyType.Enum:
                    int enumIndex = Array.FindIndex(
                        property.enumNames,
                        enumName => string.Equals(enumName, setting.value, StringComparison.OrdinalIgnoreCase)
                    );

                    if (enumIndex < 0)
                        throw new FormatException($"Unknown enum value '{setting.value}'.");

                    property.enumValueIndex = enumIndex;
                    break;

                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = LoadGlobalObject<UnityEngine.Object>(setting.value);
                    break;

                default:
                    Debug.LogWarning(
                        $"Asset '{owner.name}' field '{setting.name}' uses unsupported serialized type {property.propertyType}.",
                        owner
                    );
                    break;
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"Could not import '{setting.name}' on '{owner.name}': {exception.Message}",
                owner
            );
        }
    }

    private static IEnumerable<string> FindAssetGuids<T>(string[] folders)
        where T : UnityEngine.Object
    {
        return AssetDatabase.FindAssets($"t:{typeof(T).Name}", folders)
            .OrderBy(AssetDatabase.GUIDToAssetPath);
    }

    private static string GetAssetGuid(UnityEngine.Object asset)
    {
        return asset == null
            ? string.Empty
            : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
    }

    private static string GetGlobalObjectId(UnityEngine.Object asset)
    {
        return asset == null
            ? string.Empty
            : GlobalObjectId.GetGlobalObjectIdSlow(asset).ToString();
    }

    private static T LoadGlobalObject<T>(string globalId) where T : UnityEngine.Object
    {
        if (string.IsNullOrWhiteSpace(globalId)
            || !GlobalObjectId.TryParse(globalId, out GlobalObjectId parsedId))
            return null;

        return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(parsedId) as T;
    }

    private static T LoadAssetByGuid<T>(string guid) where T : UnityEngine.Object
    {
        if (string.IsNullOrWhiteSpace(guid))
            return null;

        string path = AssetDatabase.GUIDToAssetPath(guid);
        return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<T>(path);
    }

    private static T LoadAssetByRecord<T>(string guid, string fallbackPath)
        where T : UnityEngine.Object
    {
        T asset = LoadAssetByGuid<T>(guid);
        if (asset != null)
            return asset;

        return string.IsNullOrWhiteSpace(fallbackPath)
            ? null
            : AssetDatabase.LoadAssetAtPath<T>(fallbackPath);
    }

    private static void WriteJson<T>(string path, T data)
    {
        string directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(path, JsonUtility.ToJson(data, true));
    }

    private static T ReadJson<T>(string path) where T : class
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"JSON import file does not exist: {path}");
            return null;
        }

        try
        {
            return JsonUtility.FromJson<T>(File.ReadAllText(path));
        }
        catch (Exception exception)
        {
            Debug.LogError($"Could not parse {path}: {exception.Message}");
            return null;
        }
    }

    private static void ValidateSchema(int schemaVersion, string path)
    {
        if (schemaVersion != SchemaVersion)
        {
            Debug.LogWarning(
                $"{path} uses schema version {schemaVersion}; this editor expects version {SchemaVersion}."
            );
        }
    }

    [Serializable]
    private class StockExportFile
    {
        public int schemaVersion;
        public List<StockRecord> stocks = new List<StockRecord>();
    }

    [Serializable]
    private class StockRecord
    {
        public string id;
        public string assetPath;
        public string stockName;
        public string iconId;
        public string stockType;
        public int baseValue;
        public string rarity;
        public List<string> effectIds = new List<string>();
    }

    [Serializable]
    private class EffectExportFile
    {
        public int schemaVersion;
        public List<EffectRecord> effects = new List<EffectRecord>();
    }

    [Serializable]
    private class EffectRecord
    {
        public string id;
        public string assetPath;
        public string effectType;
        public string effectName;
        public string description;
        public List<SerializedFieldRecord> settings = new List<SerializedFieldRecord>();
    }

    [Serializable]
    private class SerializedFieldRecord
    {
        public string name;
        public string type;
        public string value;
    }
}
