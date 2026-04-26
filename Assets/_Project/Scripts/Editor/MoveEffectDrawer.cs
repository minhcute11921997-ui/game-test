#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(MoveData))]
public class MoveDataEditor : Editor
{
    static readonly List<Type> effectTypes = new()
    {
        typeof(DamageEffect),
        typeof(HealEffect),
        typeof(StatStageEffect),
        typeof(WeatherEffect),
        typeof(TerrainEffect),
    };

    public override void OnInspectorGUI()
    {
        // Vẽ các field mặc định (moveName, elementType, category...)
        DrawPropertiesExcluding(serializedObject, "effects");
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);

        var data = (MoveData)target;

        for (int i = 0; i < data.effects.Count; i++)
        {
            var effect = data.effects[i];
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(effect?.GetType().Name ?? "(null)", EditorStyles.boldLabel);
            if (GUILayout.Button("✕", GUILayout.Width(24)))
            {
                Undo.RecordObject(data, "Remove Effect");
                data.effects.RemoveAt(i);
                EditorUtility.SetDirty(data);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }
            EditorGUILayout.EndHorizontal();

            if (effect != null)
            {
                var so = new SerializedObject(data);
                var listProp = so.FindProperty("effects");
                var elemProp = listProp.GetArrayElementAtIndex(i);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(elemProp, true);
                EditorGUI.indentLevel--;
                so.ApplyModifiedProperties();
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(4);

        // Dropdown thêm effect mới
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Thêm Effect:");
        foreach (var t in effectTypes)
        {
            if (GUILayout.Button(t.Name.Replace("Effect", "")))
            {
                Undo.RecordObject(data, $"Add {t.Name}");
                data.effects.Add((MoveEffect)Activator.CreateInstance(t));
                EditorUtility.SetDirty(data);
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif