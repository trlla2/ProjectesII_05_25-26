using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;

[CustomPropertyDrawer(typeof(IItemEffect), true)]
public class ItemEffectDrawer : PropertyDrawer
{
    // THIS SCRIPT WAS MADE TO SEE IItemEffect IN THE INSPECTOR

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect buttonRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

        EditorGUI.LabelField(labelRect, label);

        string typeName = "null";
        if (!string.IsNullOrEmpty(property.managedReferenceFullTypename))
        {
            string[] parts = property.managedReferenceFullTypename.Split(' ');
            if (parts.Length > 1) typeName = parts[1].Split('.').Last();
        }

        if (GUI.Button(buttonRect, typeName, EditorStyles.popup))
        {
            ShowTypeSelector(property);
        }

        if (!string.IsNullOrEmpty(property.managedReferenceFullTypename))
        {
            EditorGUI.indentLevel++;
            SerializedProperty child = property.Copy();
            SerializedProperty endProperty = child.GetEndProperty();

            if (child.NextVisible(true))
            {
                while (!SerializedProperty.EqualContents(child, endProperty))
                {
                    float height = EditorGUI.GetPropertyHeight(child);
                    Rect childRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, height);

                    EditorGUI.PropertyField(childRect, child, true);

                    if (!child.NextVisible(false)) break;
                }
            }
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (!string.IsNullOrEmpty(property.managedReferenceFullTypename))
        {
            SerializedProperty child = property.Copy();
            SerializedProperty endProperty = child.GetEndProperty();

            if (child.NextVisible(true))
            {
                while (!SerializedProperty.EqualContents(child, endProperty))
                {
                    height += EditorGUI.GetPropertyHeight(child) + 2;
                    if (!child.NextVisible(false)) break;
                }
            }
        }
        return height;
    }

    private void ShowTypeSelector(SerializedProperty property)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("None"), false, () =>
        {
            property.managedReferenceValue = null;
            property.serializedObject.ApplyModifiedProperties();
        });

        Type type = typeof(IItemEffect);
        IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

        foreach (Type t in types)
        {
            menu.AddItem(new GUIContent(t.Name), false, () =>
            {
                property.managedReferenceValue = Activator.CreateInstance(t);
                property.serializedObject.ApplyModifiedProperties();
            });
        }

        menu.ShowAsContext();
    }
}