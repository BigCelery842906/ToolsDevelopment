using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PrefabPlacers))]
public class PrefabEditor : Editor
{
    private SerializedProperty val1Property;
    private SerializedProperty val2Property;

    private void OnEnable()
    {
        val1Property = serializedObject.FindProperty("Val1");
        val2Property = serializedObject.FindProperty("Val2");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


        serializedObject.Update();

        //EditorGUILayout.PropertyField(val1Property);
        //EditorGUILayout.PropertyField(val2Property);

        EditorGUILayout.Slider(val1Property.floatValue, 0f, 1f);
        EditorGUILayout.Slider(val2Property.floatValue, 0f, val1Property.floatValue);

        if (GUILayout.Button("Click me"))
        {
            //Call block when button clicked
            Debug.Log("Button Clicked");
            val1Property.floatValue++;
            Debug.Log(val1Property.floatValue + " " + val2Property.floatValue);
        }

        serializedObject.ApplyModifiedProperties();
    }
}