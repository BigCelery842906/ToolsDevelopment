using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// Found tutorial here: https://medium.com/xrpractices/building-a-custom-editor-window-in-unity-5b8a1378e734
// https://docs.unity3d.com/2022.3/Documentation//Manual/UIE-uxml-element-Button.html
// https://docs.unity3d.com/ScriptReference/UIElements.Button.html
// https://docs.unity3d.com/ScriptReference/EditorWindow.html?source=post_page-----5b8a1378e734---------------------------------------

public class PrefabPlacer : EditorWindow
{
    // Plan
    // Add list of GameObjects that can be placed
    // Toggle for each gameobject that changes whether it is currently able to be placed - Ideally in line with the gameobjects
    // Min and Max Angle for gameobjects to be placed on - Stretch is to have this per GO
    // Toggle for whether the GO will aim to match the normal of the object its being placed on - Might have to look into whether this is possible without loads of complex maths
    // Density of objects
    // Size of Brush


    [SerializeField] private List<GameObject> prefabs =  new List<GameObject>();
    [SerializeField] private List<bool> toggles =  new List<bool>();

    // [Range(0, 100f)] [SerializeField] private float brushSize;
    RangeAttribute brushSize = new RangeAttribute(0.0f, 100.0f);

    [Range(0.0f, 360.0f)] [SerializeField] private float minBrushAngle;
    [Range(0.0f, 360.0f)] [SerializeField] private float maxBrushAngle;

    [Range(0.0f, 100f)] [SerializeField] private float densityOfObjects;
    
    private bool randomRotation = true;
    

    // Values that can change
    private bool drawingPrefabs = false;
    
    
    [MenuItem("Tools/Prefab Placer")]
    public static void ShowWindow()
    {
        PrefabPlacer prefabPlacerWindow = GetWindow<PrefabPlacer>();
        var icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor/icon.jpg");
        prefabPlacerWindow.titleContent = new GUIContent("Prefab Placer Tool", icon);
    }

    // public void CreateGUI()
    // {
    //     // Each editor window contains a root VisualElement object
    //     VisualElement root = rootVisualElement;
    //
    //     // VisualElements objects can contain other VisualElement following a tree hierarchy.
    //     VisualElement label = new Label("Hello World! From C#");
    //     root.Add(label);
    //
    //     // // Instantiate UXML
    //     // VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
    //     // root.Add(labelFromUXML);
    //     
    //     
    //     Button paintButton = new Button();
    //     paintButton.name = "Paint Button";
    //     paintButton.text = "Enable Painting";
    //     paintButton.clicked += OnDrawClick;
    //     root.Add(paintButton);
    //
    //     // SpaceAttribute(5.0f);
    //
    //     Toggle toggleButton = new Toggle();
    //     toggleButton.name = "Toggle Toggle";
    //     toggleButton.text = "Enable Toggle";
    //     root.Add(toggleButton);
    //     
    //     var listView = new ListView(prefabs, 20, () => new Label(), (element, i) =>
    //     {
    //         (element as Label).text = prefabs[i] ? prefabs[i].name : "None";
    //     });
    //     root.Add(listView);
    //     
    //     
    //     int removeIndex = -1;
    //
    //     for (int i = 0; i < prefabs.Count; i++)
    //     {
    //         EditorGUILayout.BeginHorizontal();
    //         if (GUILayout.Button("X", GUILayout.Width(20)))
    //             removeIndex = i;
    //
    //         if (GUILayout.Toggle(true, ""))
    //         {
    //             Debug.Log("Enabled");
    //         }
    //         prefabs[i] = (GameObject)EditorGUILayout.ObjectField(prefabs[i], typeof(GameObject), false);
    //
    //         
    //
    //         EditorGUILayout.EndHorizontal();
    //     }
    //
    //     if (removeIndex >= 0)
    //         prefabs.RemoveAt(removeIndex);
    //
    //     if (GUILayout.Button("Add Prefab"))
    //         prefabs.Add(null);
    //
    //     GUILayout.Space(10);
    //     
    // }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Placer", EditorStyles.boldLabel);
        
        #region Number of Prefabs
        int numOfPrefabs = prefabs.Count;
        numOfPrefabs = EditorGUILayout.IntField("Number of Prefabs", numOfPrefabs);
        if (prefabs.Count != numOfPrefabs)
        {
            for (int i = prefabs.Count; i < numOfPrefabs; i++)
            {
                AddNewObject();
            }
            //Where there are more prefabs in the prefabs list than in NumOfPrefabs, remove down to that amount.
            //Remove the last one in the list until the specified amount has been reached

            while (prefabs.Count > numOfPrefabs)
            {
                RemoveObject(prefabs.Count - 1);
            }
        }
        #endregion

        if (GUILayout.Button("Add Prefab"))
        {
            AddNewObject();
        }
        
        if (prefabs.Count > 0)
        {
            for (int i = 0; i < prefabs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                string buttonText = "";
                
                if (!toggles[i])
                {
                    buttonText = "Enable";
                }
                else
                {
                    buttonText = "Disable";
                }
                if (GUILayout.Button(buttonText, GUILayout.Width(70)))
                {
                    //Do enable tag on this prefab
                    toggles[i] = !toggles[i];
                    Debug.Log("Toggle Button: " + i + " set to: " + toggles[i] );
                }

                prefabs[i] = (GameObject)EditorGUILayout.ObjectField(prefabs[i], typeof(GameObject), false, GUILayout.Width(200), GUILayout.ExpandWidth(true));

                
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    Debug.Log("Removing item:" + i);
                    RemoveObject(i);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        
        Color trueColour = new Color32(70, 115, 105, 255);
        Color falseColour = Color.white;
        
        GUI.backgroundColor = drawingPrefabs ? trueColour : falseColour;
        bool shouldPaint = GUILayout.Toggle(drawingPrefabs, "Enable Painting", "Button"); //Need to do the background colour change thing
        if (shouldPaint != drawingPrefabs)
        {
            OnDrawClick();
        }
        GUI.backgroundColor = Color.white; // Reset to default colour, otherwise it will draw with the true colour.
        
        // This is used just to check that the gui stops drawing with the true colour
        // if (GUILayout.Button("X", GUILayout.Width(20)))
        // {
        //     Debug.Log("Removing item:" + 1);
        //     RemoveObject(1);
        // }
    }

    void OnDrawClick()
    {
        drawingPrefabs = !drawingPrefabs;
        //
        // var button = rootVisualElement.Q<Button>("Button");
        //
        // Color trueColour = new Color32(70, 115, 105, 255);
        // Color falseColour = new Color32(88, 88, 88, 255);
        // button.style.backgroundColor = drawingPrefabs ? trueColour : falseColour;
        
        Debug.Log("DRAW BUTTON CLICKED: Set to " + drawingPrefabs);
    }

    void AddNewObject()
    {
        prefabs.Add(null);
        toggles.Add(true);
    }

    void RemoveObject(int position)
    {
        prefabs.RemoveAt(position);
        toggles.RemoveAt(position);
    }
}