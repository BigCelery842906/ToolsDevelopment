using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.Composites;
using UnityEngine.UIElements;

// Found tutorial here: https://medium.com/xrpractices/building-a-custom-editor-window-in-unity-5b8a1378e734
// https://docs.unity3d.com/2022.3/Documentation//Manual/UIE-uxml-element-Button.html
// https://docs.unity3d.com/ScriptReference/UIElements.Button.html
// https://docs.unity3d.com/ScriptReference/EditorWindow.html?source=post_page-----5b8a1378e734---------------------------------------

public class PrefabPlacer : EditorWindow
{
    [SerializeField]
    // private VisualTreeAsset m_VisualTreeAsset = default;

    // Values that can change
    private bool drawPrefabs = false;
    
    
    
    
    
    [MenuItem("CS Tool/Prefab Placer")]
    public static void ShowWindow()
    {
        PrefabPlacer prefabPlacerWindow = GetWindow<PrefabPlacer>();
        var icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor/icon.jpg");
        prefabPlacerWindow.titleContent = new GUIContent("Prefab Placer Tool", icon);
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);

        // // Instantiate UXML
        // VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        // root.Add(labelFromUXML);
        
        
        Button draw = new Button();
        draw.name = "DrawButton";
        draw.text = "Draw";
        draw.clicked += OnDrawClick;
        root.Add(draw);

        


    }

    private void OnGUI()
    {
        GUILayout.Space(100);
        
        bool isPainting = false;
        if (GUILayout.Toggle(isPainting, "Enable Painting", "Button") != isPainting)
        {
            isPainting = !isPainting;
        }
    }

    void OnDrawClick()
    {
        drawPrefabs = !drawPrefabs;

        var button = rootVisualElement.Q<Button>("DrawButton");
        
        // button.style.backgroundColor = drawPrefabs ? Color.green : Color.red;

        if (drawPrefabs)
        {
            Color color = new Color(70, 115, 105);
            button.style.backgroundColor = color;
            // Color color = new Color(0.5, 0.6, 0.2, 255);
            // color = new Color()
        }
        else
        {
            //Default Unity Grey
            button.style.backgroundColor = Color.gray;
            // button.style.backgroundColor = new Color(88,88,88, 255);
        }
        Debug.Log("DRAW BUTTON CLICKED: Set to " + drawPrefabs);
    }
}