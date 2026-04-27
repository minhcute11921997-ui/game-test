#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MoveEffect), true)]
public class MoveEffectDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUI.GetPropertyHeight(property, true);
}

[CustomEditor(typeof(MoveData))]
public class MoveDataEditor : Editor
{
    // Cache danh sách subtype — chỉ quét 1 lần
    private static List<Type> _effectTypes;
    private static string[] _effectNames;

    static void BuildTypeCache()
    {
        if (_effectTypes != null) return;
        _effectTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(MoveEffect)))
            .OrderBy(t => t.Name)
            .ToList();
        _effectNames = _effectTypes.Select(t => PrettyName(t)).ToArray();
    }

    static string PrettyName(Type t)
    {
        // DamageEffect → Damage, KnockbackEffect → Knockback, v.v.
        var name = t.Name.Replace("Effect", "");
        return name;
    }

    public override void OnInspectorGUI()
    {
        BuildTypeCache();
        serializedObject.Update();

        // Vẽ các field không phải effects bình thường
        DrawPropertiesExcluding(serializedObject, "effects");

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("⚔ Effects", EditorStyles.boldLabel);

        var effectsProp = serializedObject.FindProperty("effects");

        for (int i = 0; i < effectsProp.arraySize; i++)
        {
            var elem = effectsProp.GetArrayElementAtIndex(i);
            var managedRef = elem.managedReferenceValue;
            string typeName = managedRef != null ? PrettyName(managedRef.GetType()) : "null";

            EditorGUILayout.BeginVertical(GUI.skin.box);

            // Header: tên type + nút xóa + nút lên/xuống
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"[{i}] {typeName}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (i > 0 && GUILayout.Button("▲", GUILayout.Width(24)))
            {
                effectsProp.MoveArrayElement(i, i - 1);
                break;
            }
            if (i < effectsProp.arraySize - 1 && GUILayout.Button("▼", GUILayout.Width(24)))
            {
                effectsProp.MoveArrayElement(i, i + 1);
                break;
            }
            if (GUILayout.Button("✕", GUILayout.Width(24)))
            {
                effectsProp.DeleteArrayElementAtIndex(i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            // Vẽ các field của effect đó
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(elem, GUIContent.none, true);
            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(4);

        // Dropdown thêm effect mới
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("➕ Thêm Effect:", GUILayout.Width(100));
        int chosen = EditorGUILayout.Popup(-1, _effectNames);
        if (chosen >= 0)
        {
            var newEffect = (MoveEffect)Activator.CreateInstance(_effectTypes[chosen]);
            effectsProp.arraySize++;
            effectsProp.GetArrayElementAtIndex(effectsProp.arraySize - 1)
                       .managedReferenceValue = newEffect;
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }
}
#endif