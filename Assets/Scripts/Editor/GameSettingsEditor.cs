using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(GameSettings))]
public class GameSettingsEditor : Editor
{
    // Serialized properties
    SerializedProperty targetScoreProp;
    SerializedProperty breakInScoreProp;
    SerializedProperty pointsMultiplierOfAKindProp;
    SerializedProperty pointsPerOneProp;
    SerializedProperty pointsPerFiveProp;
    SerializedProperty combosProp;
    
    ReorderableList combosList;
    
    private void OnEnable()
    {
        targetScoreProp = serializedObject.FindProperty("targetScore");
        breakInScoreProp = serializedObject.FindProperty("breakInScore");
        pointsMultiplierOfAKindProp = serializedObject.FindProperty("pointsMultiplierOfAKind");
        pointsPerOneProp = serializedObject.FindProperty("pointsPerOne");
        pointsPerFiveProp = serializedObject.FindProperty("pointsPerFive");
        combosProp = serializedObject.FindProperty("combos");
        
        combosList = new ReorderableList(serializedObject, combosProp, true, true, true, true);
        combosList.drawHeaderCallback = (Rect rect) => {EditorGUI.LabelField(rect, "Combo Rules");};
        combosList.elementHeight = EditorGUIUtility.singleLineHeight * 6.2f; 
        
        combosList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = combosProp.GetArrayElementAtIndex(index);
            var idProp = element.FindPropertyRelative("id");
            var nameProp = element.FindPropertyRelative("displayName");
            var descProp = element.FindPropertyRelative("description");
            var pointsProp = element.FindPropertyRelative("points");
            var enabledProp = element.FindPropertyRelative("enabled");

            float line = EditorGUIUtility.singleLineHeight + 2f;
            var r1 = new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight);
            var r2 = new Rect(rect.x, rect.y + 2 + line, rect.width, EditorGUIUtility.singleLineHeight);
            var r3 = new Rect(rect.x, rect.y + 2 + line * 2, rect.width, EditorGUIUtility.singleLineHeight * 2.2f);
            var r4 = new Rect(rect.x, rect.y + 2 + line * 4.4f, rect.width * 0.5f, EditorGUIUtility.singleLineHeight);
            var r5 = new Rect(rect.x + rect.width * 0.52f, rect.y + 2 + line * 4.4f, rect.width * 0.46f, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(r1, idProp, new GUIContent("ID"));
            EditorGUI.PropertyField(r2, nameProp, new GUIContent("Name"));
            EditorGUI.PropertyField(r3, descProp, new GUIContent("Description"));
            EditorGUI.PropertyField(r4, pointsProp, new GUIContent("Points"));
            EditorGUI.PropertyField(r5, enabledProp, new GUIContent("Enabled"));
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //
        // bool rootWasEnabled = GUI.enabled;
        // GUI.enabled = true;
        
        var settings = (GameSettings)target;
        
        EditorGUILayout.LabelField("Game Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);
        
        EditorGUILayout.IntSlider(targetScoreProp, 1000, 50000, new GUIContent("Target Score", "Score required to win the game."));
        EditorGUILayout.IntSlider(breakInScoreProp, 100, 5000, new GUIContent("Break-In Score", "Score the player must reach in a single turn to break in."));
        
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Scoring Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);
        
        combosList.DoLayoutList();
        
        EditorGUILayout.PropertyField(pointsMultiplierOfAKindProp, new GUIContent("Points Multiplier (Of-a-Kind)", "Multiplier used for scoring 3, 4, 5, or 6 of a kind."));
        EditorGUILayout.IntSlider(pointsPerOneProp, 50, 200, new GUIContent("Points per 1", "Points per 1 rolled (not part of another combo)."));
        EditorGUILayout.IntSlider(pointsPerFiveProp, 25, 100, new GUIContent("Points per 5", "Points per 5 rolled (not part of another combo)."));

        EditorGUILayout.Space(8);
        DrawUtilitiesRow(settings);
        
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawUtilitiesRow(GameSettings settings)
    {
        EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Reset to Defaults"))
            {
                Undo.RecordObject(settings, "Reset Game Settings to Defaults");
                settings.ResetToDefaults();
                EditorUtility.SetDirty(settings);
            }
            
            if (GUILayout.Button("Export to JSON"))
            {
                string json = JsonUtility.ToJson(settings, true);
                string path = EditorUtility.SaveFilePanel("Save Game Settings as JSON", Application.dataPath, "game_settings.json", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    System.IO.File.WriteAllText(path, json);
                    EditorUtility.RevealInFinder(path);
                    FarkleLogger.Log($"Game settings exported to {path}");
                }
            }
            
            if (GUILayout.Button("Load JSON"))
            {
                string path = EditorUtility.OpenFilePanel("Import Game Settings from JSON", Application.dataPath, "json");
                if (!string.IsNullOrEmpty(path))
                {
                    try
                    {
                        Undo.RecordObject(settings, "Import Game Settings from JSON");
                        
                        string json = System.IO.File.ReadAllText(path);
                        JsonUtility.FromJsonOverwrite(json, settings);
                        EditorUtility.SetDirty(settings);
                        FarkleLogger.Log($"Game settings imported from {path}");
                    }
                    catch (Exception e)
                    {
                        FarkleLogger.LogError($"Failed to import game settings: {e.Message}");
                    }
                }
            }
        }
    }

    
    string BuildSummary(GameSettings settings)
    {
        string summary = $"<b>Target Score:</b> {settings.targetScore}\n" +
                         $"<b>Break-In Score:</b> {settings.breakInScore}\n\n" +
                         $"<b>Scoring Combos:</b>\n";

        foreach (var combo in settings.combos)
        {
            if (!combo.enabled) continue;
            summary += $"- <b>{combo.displayName}</b> ({combo.id}): {combo.points} points\n  <i>{combo.description}</i>\n";
        }

        return summary;
    }
}
