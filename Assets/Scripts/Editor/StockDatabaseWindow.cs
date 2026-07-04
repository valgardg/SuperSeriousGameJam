using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class StockDatabaseWindow : EditorWindow
{
    private enum DatabaseView
    {
        Stocks,
        Effects
    }

    private enum StockSortColumn
    {
        Asset,
        StockName,
        Type,
        Rarity,
        BaseValue,
        Icon,
        EffectCount
    }

    private enum EffectSortColumn
    {
        Asset,
        EffectName,
        EffectType,
        Description
    }

    private struct StockSortCriterion
    {
        public StockSortColumn column;
        public bool ascending;

        public StockSortCriterion(StockSortColumn column, bool ascending)
        {
            this.column = column;
            this.ascending = ascending;
        }
    }

    private struct EffectSortCriterion
    {
        public EffectSortColumn column;
        public bool ascending;

        public EffectSortCriterion(EffectSortColumn column, bool ascending)
        {
            this.column = column;
            this.ascending = ascending;
        }
    }

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

    private readonly List<StockDefinition> stocks = new List<StockDefinition>();
    private readonly List<StockEffect> effects = new List<StockEffect>();
    private readonly List<StockSortCriterion> stockSortCriteria = new List<StockSortCriterion>
    {
        new StockSortCriterion(StockSortColumn.Asset, true)
    };
    private readonly List<EffectSortCriterion> effectSortCriteria = new List<EffectSortCriterion>
    {
        new EffectSortCriterion(EffectSortColumn.Asset, true)
    };
    private readonly HashSet<int> collapsedEffectIds = new HashSet<int>();

    private static readonly IComparer<object> SortValueComparer = Comparer<object>.Create(
        CompareSortValues
    );

    private DatabaseView currentView;
    private string searchText = string.Empty;
    private Vector2 tableScroll;
    private Vector2 detailScroll;
    private UnityEngine.Object selectedAsset;
    private UnityEditor.Editor selectedAssetEditor;

    [MenuItem("Tools/Stocks/Open Stock Database")]
    public static void Open()
    {
        StockDatabaseWindow window = GetWindow<StockDatabaseWindow>("Stock Database");
        window.minSize = new Vector2(900f, 450f);
        window.Show();
    }

    private void OnEnable()
    {
        RefreshAssets();
    }

    private void OnDisable()
    {
        if (selectedAssetEditor != null)
            DestroyImmediate(selectedAssetEditor);
    }

    private void OnFocus()
    {
        RefreshAssets();
    }

    private void OnGUI()
    {
        DrawToolbar();

        EditorGUILayout.BeginHorizontal();
        DrawTablePanel();
        DrawDetailPanel();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        DatabaseView nextView = (DatabaseView)GUILayout.Toolbar(
            (int)currentView,
            new[] { "Stocks", "Effects" },
            EditorStyles.toolbarButton,
            GUILayout.Width(150f)
        );

        if (nextView != currentView)
        {
            currentView = nextView;
            tableScroll = Vector2.zero;
            selectedAsset = null;
        }

        GUILayout.Space(8f);
        searchText = GUILayout.TextField(
            searchText,
            GUI.skin.FindStyle("ToolbarSearchTextField"),
            GUILayout.MinWidth(180f)
        );

        if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(45f)))
            searchText = string.Empty;

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60f)))
            RefreshAssets();

        if (GUILayout.Button("Export All", EditorStyles.toolbarButton, GUILayout.Width(75f)))
            StockDataJsonTool.ExportAll();

        if (GUILayout.Button("Import All", EditorStyles.toolbarButton, GUILayout.Width(75f)))
        {
            StockDataJsonTool.ImportAll();
            RefreshAssets();
        }

        if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(45f)))
            AssetDatabase.SaveAssets();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawTablePanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.MinWidth(590f), GUILayout.ExpandWidth(true));

        if (currentView == DatabaseView.Stocks)
            DrawStockHeader();
        else
            DrawEffectHeader();

        tableScroll = EditorGUILayout.BeginScrollView(tableScroll);

        if (currentView == DatabaseView.Stocks)
        {
            foreach (StockDefinition stock in FilterStocks())
                DrawStockRow(stock);
        }
        else
        {
            foreach (StockEffect effect in FilterEffects())
                DrawEffectRow(effect);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawStockHeader()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        DrawStockSortHeader("Asset", StockSortColumn.Asset, 120f);
        DrawStockSortHeader("Ticker / Name", StockSortColumn.StockName, 130f);
        DrawStockSortHeader("Type", StockSortColumn.Type, 85f);
        DrawStockSortHeader("Rarity", StockSortColumn.Rarity, 85f);
        DrawStockSortHeader("Base", StockSortColumn.BaseValue, 45f);
        DrawStockSortHeader("Icon", StockSortColumn.Icon, 95f);
        DrawStockSortHeader("Effects", StockSortColumn.EffectCount, 50f);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawEffectHeader()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        DrawEffectSortHeader("Asset", EffectSortColumn.Asset, 130f);
        DrawEffectSortHeader("Effect Name", EffectSortColumn.EffectName, 150f);
        DrawEffectSortHeader("Effect Type", EffectSortColumn.EffectType, 180f);
        DrawEffectSortHeader("Description", EffectSortColumn.Description, 0f);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawStockRow(StockDefinition stock)
    {
        if (stock == null)
            return;

        Rect rowRect = EditorGUILayout.BeginHorizontal(
            selectedAsset == stock ? "SelectionRect" : GUIStyle.none,
            GUILayout.Height(22f)
        );

        if (Event.current.type == EventType.MouseDown
            && rowRect.Contains(Event.current.mousePosition))
        {
            SelectAsset(stock);
        }

        if (GUILayout.Button(stock.name, EditorStyles.linkLabel, GUILayout.Width(120f)))
            SelectAndPing(stock);

        string stockName = EditorGUILayout.TextField(stock.stockName, GUILayout.Width(130f));
        StockType stockType = (StockType)EditorGUILayout.EnumPopup(stock.stockType, GUILayout.Width(85f));
        Rarity rarity = (Rarity)EditorGUILayout.EnumPopup(stock.rarity, GUILayout.Width(85f));
        int baseValue = EditorGUILayout.IntField(stock.baseValue, GUILayout.Width(45f));
        Sprite icon = (Sprite)EditorGUILayout.ObjectField(
            stock.icon,
            typeof(Sprite),
            false,
            GUILayout.Width(95f)
        );
        int effectCount = stock.effects == null ? 0 : stock.effects.Count(effect => effect != null);
        GUILayout.Label(effectCount.ToString(), GUILayout.Width(50f));

        if (stockName != stock.stockName
            || stockType != stock.stockType
            || rarity != stock.rarity
            || baseValue != stock.baseValue
            || icon != stock.icon)
        {
            Undo.RecordObject(stock, "Edit Stock Definition");
            stock.stockName = stockName;
            stock.stockType = stockType;
            stock.rarity = rarity;
            stock.baseValue = baseValue;
            stock.icon = icon;
            EditorUtility.SetDirty(stock);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawEffectRow(StockEffect effect)
    {
        if (effect == null)
            return;

        Rect rowRect = EditorGUILayout.BeginHorizontal(
            selectedAsset == effect ? "SelectionRect" : GUIStyle.none,
            GUILayout.Height(22f)
        );

        if (Event.current.type == EventType.MouseDown
            && rowRect.Contains(Event.current.mousePosition))
        {
            SelectAsset(effect);
        }

        if (GUILayout.Button(effect.name, EditorStyles.linkLabel, GUILayout.Width(130f)))
            SelectAndPing(effect);

        string effectName = EditorGUILayout.TextField(effect.effectName, GUILayout.Width(150f));
        GUILayout.Label(effect.GetType().Name, GUILayout.Width(180f));
        string description = EditorGUILayout.TextField(
            effect.description,
            GUILayout.ExpandWidth(true)
        );

        if (effectName != effect.effectName || description != effect.description)
        {
            Undo.RecordObject(effect, "Edit Stock Effect");
            effect.effectName = effectName;
            effect.description = description;
            EditorUtility.SetDirty(effect);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawDetailPanel()
    {
        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox,
            GUILayout.Width(Mathf.Max(280f, position.width * 0.34f)),
            GUILayout.ExpandHeight(true)
        );

        if (selectedAsset == null)
        {
            GUILayout.Label("Select a stock or effect to edit all of its fields here.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(selectedAsset.name, EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Ping", GUILayout.Width(45f)))
            EditorGUIUtility.PingObject(selectedAsset);

        EditorGUILayout.EndHorizontal();

        string assetPath = AssetDatabase.GetAssetPath(selectedAsset);
        EditorGUILayout.LabelField(assetPath, EditorStyles.miniLabel);
        EditorGUILayout.Space(4f);

        UnityEditor.Editor.CreateCachedEditor(
            selectedAsset,
            null,
            ref selectedAssetEditor
        );

        detailScroll = EditorGUILayout.BeginScrollView(detailScroll);
        selectedAssetEditor?.OnInspectorGUI();

        if (selectedAsset is StockDefinition selectedStock)
            DrawStockEffectDetails(selectedStock);

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawStockEffectDetails(StockDefinition stock)
    {
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Configured Stock Effects", EditorStyles.boldLabel);

        if (stock.effects == null || stock.effects.Length == 0)
        {
            EditorGUILayout.HelpBox(
                "This stock has no configured effects.",
                MessageType.Info
            );
            return;
        }

        for (int index = 0; index < stock.effects.Length; index++)
        {
            StockEffect effect = stock.effects[index];
            if (effect == null)
            {
                EditorGUILayout.HelpBox(
                    $"Effect slot {index + 1} is empty.",
                    MessageType.Warning
                );
                continue;
            }

            int effectId = effect.GetInstanceID();
            bool expanded = !collapsedEffectIds.Contains(effectId);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            bool nextExpanded = EditorGUILayout.Foldout(
                expanded,
                $"{index + 1}. {effect.effectName} ({effect.GetType().Name})",
                true,
                EditorStyles.foldoutHeader
            );

            if (GUILayout.Button("Ping", GUILayout.Width(45f)))
                EditorGUIUtility.PingObject(effect);

            EditorGUILayout.EndHorizontal();

            if (nextExpanded)
                collapsedEffectIds.Remove(effectId);
            else
                collapsedEffectIds.Add(effectId);

            if (nextExpanded)
                DrawInlineEffectInspector(effect);

            EditorGUILayout.EndVertical();
        }
    }

    private static void DrawInlineEffectInspector(StockEffect effect)
    {
        SerializedObject serializedEffect = new SerializedObject(effect);
        serializedEffect.Update();

        SerializedProperty property = serializedEffect.GetIterator();
        bool enterChildren = true;

        EditorGUI.BeginChangeCheck();

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (property.name == "m_Script")
                continue;

            EditorGUILayout.PropertyField(property, true);
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedEffect.ApplyModifiedProperties();
            EditorUtility.SetDirty(effect);
        }
    }

    private IEnumerable<StockDefinition> FilterStocks()
    {
        IEnumerable<StockDefinition> filteredStocks = stocks;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            string search = searchText.Trim();
            filteredStocks = filteredStocks.Where(stock =>
                stock.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                || (stock.stockName ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                || stock.stockType.ToString().IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                || stock.rarity.ToString().IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
            );
        }

        IOrderedEnumerable<StockDefinition> orderedStocks = null;

        foreach (StockSortCriterion criterion in stockSortCriteria)
        {
            Func<StockDefinition, object> selector = GetStockSortSelector(criterion.column);

            if (orderedStocks == null)
            {
                orderedStocks = criterion.ascending
                    ? filteredStocks.OrderBy(selector, SortValueComparer)
                    : filteredStocks.OrderByDescending(selector, SortValueComparer);
            }
            else
            {
                orderedStocks = criterion.ascending
                    ? orderedStocks.ThenBy(selector, SortValueComparer)
                    : orderedStocks.ThenByDescending(selector, SortValueComparer);
            }
        }

        return (orderedStocks ?? filteredStocks.OrderBy(stock => stock.name))
            .ThenBy(stock => stock.name, StringComparer.OrdinalIgnoreCase);
    }

    private IEnumerable<StockEffect> FilterEffects()
    {
        IEnumerable<StockEffect> filteredEffects = effects;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            string search = searchText.Trim();
            filteredEffects = filteredEffects.Where(effect =>
                effect.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                || (effect.effectName ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                || (effect.description ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                || effect.GetType().Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
            );
        }

        IOrderedEnumerable<StockEffect> orderedEffects = null;

        foreach (EffectSortCriterion criterion in effectSortCriteria)
        {
            Func<StockEffect, object> selector = GetEffectSortSelector(criterion.column);

            if (orderedEffects == null)
            {
                orderedEffects = criterion.ascending
                    ? filteredEffects.OrderBy(selector, SortValueComparer)
                    : filteredEffects.OrderByDescending(selector, SortValueComparer);
            }
            else
            {
                orderedEffects = criterion.ascending
                    ? orderedEffects.ThenBy(selector, SortValueComparer)
                    : orderedEffects.ThenByDescending(selector, SortValueComparer);
            }
        }

        return (orderedEffects ?? filteredEffects.OrderBy(effect => effect.name))
            .ThenBy(effect => effect.name, StringComparer.OrdinalIgnoreCase);
    }

    private void RefreshAssets()
    {
        stocks.Clear();
        effects.Clear();

        stocks.AddRange(LoadAssets<StockDefinition>(StockFolders));
        effects.AddRange(LoadAssets<StockEffect>(EffectFolders));
        Repaint();
    }

    private static IEnumerable<T> LoadAssets<T>(string[] folders)
        where T : UnityEngine.Object
    {
        return AssetDatabase.FindAssets($"t:{typeof(T).Name}", folders)
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(path => path)
            .Select(AssetDatabase.LoadAssetAtPath<T>)
            .Where(asset => asset != null);
    }

    private void SelectAsset(UnityEngine.Object asset)
    {
        if (selectedAsset == asset)
            return;

        selectedAsset = asset;
        detailScroll = Vector2.zero;
        GUI.FocusControl(null);
    }

    private void SelectAndPing(UnityEngine.Object asset)
    {
        SelectAsset(asset);
        EditorGUIUtility.PingObject(asset);
    }

    private void DrawStockSortHeader(
        string label,
        StockSortColumn column,
        float width
    )
    {
        int criterionIndex = stockSortCriteria.FindIndex(
            criterion => criterion.column == column
        );
        string displayLabel = criterionIndex >= 0
            ? $"{label} {criterionIndex + 1}{(stockSortCriteria[criterionIndex].ascending ? "▲" : "▼")}"
            : label;

        if (GUILayout.Button(displayLabel, EditorStyles.miniButton, GUILayout.Width(width)))
            ApplyStockSort(column, Event.current.shift);
    }

    private void DrawEffectSortHeader(
        string label,
        EffectSortColumn column,
        float width
    )
    {
        int criterionIndex = effectSortCriteria.FindIndex(
            criterion => criterion.column == column
        );
        string displayLabel = criterionIndex >= 0
            ? $"{label} {criterionIndex + 1}{(effectSortCriteria[criterionIndex].ascending ? "▲" : "▼")}"
            : label;

        bool clicked = width > 0f
            ? GUILayout.Button(displayLabel, EditorStyles.miniButton, GUILayout.Width(width))
            : GUILayout.Button(displayLabel, EditorStyles.miniButton, GUILayout.ExpandWidth(true));

        if (clicked)
            ApplyEffectSort(column, Event.current.shift);
    }

    private void ApplyStockSort(StockSortColumn column, bool append)
    {
        int existingIndex = stockSortCriteria.FindIndex(
            criterion => criterion.column == column
        );

        if (!append)
        {
            bool ascending = existingIndex == 0 && stockSortCriteria.Count == 1
                ? !stockSortCriteria[0].ascending
                : true;

            stockSortCriteria.Clear();
            stockSortCriteria.Add(new StockSortCriterion(column, ascending));
            return;
        }

        if (existingIndex >= 0)
        {
            StockSortCriterion criterion = stockSortCriteria[existingIndex];
            criterion.ascending = !criterion.ascending;
            stockSortCriteria[existingIndex] = criterion;
        }
        else
        {
            stockSortCriteria.Add(new StockSortCriterion(column, true));
        }
    }

    private void ApplyEffectSort(EffectSortColumn column, bool append)
    {
        int existingIndex = effectSortCriteria.FindIndex(
            criterion => criterion.column == column
        );

        if (!append)
        {
            bool ascending = existingIndex == 0 && effectSortCriteria.Count == 1
                ? !effectSortCriteria[0].ascending
                : true;

            effectSortCriteria.Clear();
            effectSortCriteria.Add(new EffectSortCriterion(column, ascending));
            return;
        }

        if (existingIndex >= 0)
        {
            EffectSortCriterion criterion = effectSortCriteria[existingIndex];
            criterion.ascending = !criterion.ascending;
            effectSortCriteria[existingIndex] = criterion;
        }
        else
        {
            effectSortCriteria.Add(new EffectSortCriterion(column, true));
        }
    }

    private static Func<StockDefinition, object> GetStockSortSelector(
        StockSortColumn column
    )
    {
        switch (column)
        {
            case StockSortColumn.StockName:
                return stock => stock.stockName ?? string.Empty;
            case StockSortColumn.Type:
                return stock => (int)stock.stockType;
            case StockSortColumn.Rarity:
                return stock => (int)stock.rarity;
            case StockSortColumn.BaseValue:
                return stock => stock.baseValue;
            case StockSortColumn.Icon:
                return stock => stock.icon == null ? string.Empty : stock.icon.name;
            case StockSortColumn.EffectCount:
                return stock => stock.effects == null
                    ? 0
                    : stock.effects.Count(effect => effect != null);
            default:
                return stock => stock.name;
        }
    }

    private static Func<StockEffect, object> GetEffectSortSelector(
        EffectSortColumn column
    )
    {
        switch (column)
        {
            case EffectSortColumn.EffectName:
                return effect => effect.effectName ?? string.Empty;
            case EffectSortColumn.EffectType:
                return effect => effect.GetType().Name;
            case EffectSortColumn.Description:
                return effect => effect.description ?? string.Empty;
            default:
                return effect => effect.name;
        }
    }

    private static int CompareSortValues(object left, object right)
    {
        if (ReferenceEquals(left, right))
            return 0;
        if (left == null)
            return -1;
        if (right == null)
            return 1;

        if (left is string leftString && right is string rightString)
            return StringComparer.OrdinalIgnoreCase.Compare(leftString, rightString);

        if (left is IComparable comparable)
            return comparable.CompareTo(right);

        return string.Compare(
            left.ToString(),
            right.ToString(),
            StringComparison.OrdinalIgnoreCase
        );
    }
}
