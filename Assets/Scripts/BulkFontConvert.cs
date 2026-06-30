using UnityEngine;
using UnityEditor;
using TMPro;

public class BulkFontChanger : EditorWindow
{
    private TMP_FontAsset newFont;

    [MenuItem("Tools/Bulk Font Changer")]
    public static void ShowWindow() => GetWindow<BulkFontChanger>("Font Changer");

    void OnGUI()
    {
        GUILayout.Label("Change All Fonts in Scene", EditorStyles.boldLabel);
        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("New Font Asset", newFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("Update All Text Components") && newFont != null)
        {
            // Finds both active and inactive TMP components in the scene
            TMP_Text[] textComponents = Resources.FindObjectsOfTypeAll<TMP_Text>();
            int count = 0;

            foreach (TMP_Text text in textComponents)
            {
                // Ensure it belongs to a scene object, not a project prefab
                if (text.gameObject.scene.name != null) 
                {
                    Undo.RecordObject(text, "Bulk Font Change");
                    text.font = newFont;
                    EditorUtility.SetDirty(text);
                    count++;
                }
            }
            Debug.Log($"Successfully updated {count} text components!");
        }
    }
}
