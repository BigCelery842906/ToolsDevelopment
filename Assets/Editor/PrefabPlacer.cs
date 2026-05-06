using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

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
    
    
    //TODO: Density of objects
    //TODO: Random Placement within brush size
    //TODO: Angle of objects
    //TODO: Make both button of the prefab row equal
    //TODO: Maybe try get the normal without a collider
    //TODO: Delete last placed objects - DONE
    //TODO: Layers that a prefab can be placed on
    //TODO: Clear all placed objects
    //TODO: Gizmos for angle on which a prefab can be placed?
    //TODO: Stop the drag issue - DONE
    //TODO: Stop origin place if not hit anything

    private List<GameObject> prefabs = new List<GameObject>();
    private List<bool> toggles = new List<bool>();
    
    private List<GameObject> lastPlaced = new List<GameObject>();
    private int activeObjects = 0;

    private int maxObjects = 50;
    private float brushSize = 5f;
    
    private float minBrushAngle; 
    private float maxBrushAngle;
    
    private float densityOfObjects;

    private bool randomRotation = true;
    
    // Values that can change
    private bool drawingPrefabs = false;
    
    private GameObject parent;
    private string name = "Placed Prefabs";


    #region valuesForHorizontalSpacing

    private static float assetNumSpacing = 70f;
    private static float enableButtonSpacing = 70f;
    private static float gameObjectSpacing = 200f;
    private static float deleteSpacing = 20f;

    private static float minWidth = assetNumSpacing + enableButtonSpacing + deleteSpacing + gameObjectSpacing + 40;

    #endregion

    Vector2 scrollPos = new Vector2();

    [MenuItem("Tools/Prefab Placer")]
    public static void ShowWindow()
    {
        PrefabPlacer prefabPlacerWindow = GetWindow<PrefabPlacer>();
        prefabPlacerWindow.minSize = new Vector2(minWidth, 100f);
        
        Texture icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor/icon.jpg");
        prefabPlacerWindow.titleContent = new GUIContent("Prefab Placer Tool", icon);
        
    }

    #region OnGUI
    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    #endregion

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GUILayout.Label("Prefab Placer", EditorStyles.boldLabel);
        
        #region The Changeable Values
        
         // min and max brush angle
         // density of objects
         // brush size
         // random rotation
         
         brushSize = EditorGUILayout.FloatField("Brush Size", brushSize);
         brushSize = Mathf.Clamp(brushSize, 0.0f, 100.0f);
         
         minBrushAngle = EditorGUILayout.FloatField("Minimum Brush Angle", minBrushAngle);
         minBrushAngle = (minBrushAngle + 360.0f) % 360.0f;
         minBrushAngle = Mathf.Clamp(minBrushAngle, 0.0f, 360.0f);
         
         maxBrushAngle = EditorGUILayout.FloatField("Maximum Brush Angle", maxBrushAngle);
         maxBrushAngle = (maxBrushAngle + 360.0f) % 360.0f;
         maxBrushAngle = Mathf.Clamp(maxBrushAngle, 0.0f, 360.0f);
         
        EditorGUILayout.MinMaxSlider("Brush Angle", ref minBrushAngle, ref maxBrushAngle, 0.0f, 360.0f);
        
        // TODO: Figure out how I can do this such that it can go from somewhere like 270 to 30, where I want from 271 to 29 filled in, rather than the other way round.
        
        randomRotation = EditorGUILayout.ToggleLeft("Random Rotation", randomRotation);
        #endregion
        
        
        #region Number of Prefabs
        int numOfPrefabs = prefabs.Count;
        numOfPrefabs = EditorGUILayout.IntField("Number of Prefabs", numOfPrefabs);
        numOfPrefabs = Mathf.Clamp(numOfPrefabs, 0, maxObjects);
        if (prefabs.Count != numOfPrefabs)
        {
            for (int i = prefabs.Count; i < numOfPrefabs; i++)
            {
                AddNewObject();
            }
            //Where there are more prefabs in the prefabs list than in NumOfPrefabs, remove down to that amount.
            //Remove the last one in the list until the specified amount has been reached

            for (int i = 0; i < (prefabs.Count - numOfPrefabs); i++)
            {
                RemoveNextObject();
            }
        }
        #endregion

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Prefab"))
        {
            AddNewObject();
        }

        if (GUILayout.Button("Prune Null Prefabs"))
        {
            PruneObjects();
        }
        EditorGUILayout.EndHorizontal();
        
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Set all Enabled"))
        {
            SetAllToValue(true);
        }
        if (GUILayout.Button("Set all Disabled"))
        {
            SetAllToValue(false);
        }
        EditorGUILayout.EndHorizontal();

        if (lastPlaced.Count != 0)
        {
            if (GUILayout.Button("Delete Last Placed"))
            {

                for (int i = lastPlaced.Count-1; i >= 0; i--)
                {
                    DestroyImmediate(lastPlaced[i]);
                    lastPlaced.RemoveAt(i);
                }
            }
        }
        
        if (prefabs.Count > 0)
        {
            GUILayout.Label("Active Assets: " + (activeObjects), GUILayout.Width(assetNumSpacing * 5));
            for (int i = 0; i < prefabs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                
                //TODO: Does this want to be a toggle value
                string buttonText = "";
                
                if (!toggles[i])
                {
                    buttonText = "Enable";
                }
                else
                {
                    buttonText = "Disable";
                }
                
                GUILayout.Label("Asset: " + (i + 1), GUILayout.Width(assetNumSpacing));
                if (GUILayout.Button(buttonText, GUILayout.Width(enableButtonSpacing)))
                {
                    //Do enable tag on this prefab
                    toggles[i] = !toggles[i];
                    Debug.Log("Toggle Button: " + i + " set to: " + toggles[i] );
                    CountActiveObjects();
                }

                prefabs[i] = (GameObject)EditorGUILayout.ObjectField(prefabs[i], typeof(GameObject), false, GUILayout.Width(gameObjectSpacing), GUILayout.ExpandWidth(true));

                
                if (GUILayout.Button("X", GUILayout.Width(deleteSpacing)))
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
        bool shouldPaint = GUILayout.Toggle(drawingPrefabs, "Enable Painting", "Button");
        if (shouldPaint != drawingPrefabs)
        {
            OnDrawClick();
        }
        GUI.backgroundColor = Color.white; // Reset to default colour, otherwise it will draw with the true colour.
        
        EditorGUILayout.EndScrollView();
    }

    void OnDrawClick()
    {
        drawingPrefabs = !drawingPrefabs;
        
        Debug.Log("DRAW BUTTON CLICKED: Set to " + drawingPrefabs);

        CountActiveObjects();
    }

    void AddNewObject()
    {
        if (prefabs.Count >= maxObjects) //Prevent stupid numbers from being reached
        {
            Debug.LogWarning("Max Objects Reached");
            return;
        }
        
        prefabs.Add(null);
        toggles.Add(true);
        
        CountActiveObjects();
    }

    void RemoveObject(int position)
    {
        if (position >= prefabs.Count || position < 0)
        {
            return;
        }
        prefabs.RemoveAt(position);
        toggles.RemoveAt(position);
        
        if (prefabs.Count != toggles.Count)
        {
            Debug.LogError("List mismatch!");
        }
        
        CountActiveObjects();
    }

    void RemoveNextObject()
    {
        if (!PruneNullObject())
        
        //If there are no null objects
        RemoveObject(prefabs.Count - 1);
    }

    void PruneObjects()
    {
        while (PruneNullObject()) ;
    }

    bool PruneNullObject()
    {
        for (int i = 0; i < prefabs.Count; i++)
        {
            if (prefabs[i] == null)
            {
                RemoveObject(i);
                return true;
            }
        }

        return false;
    }

    void SetAllToValue(bool value)
    {
        for (int i = 0; i < prefabs.Count; i++)
        {
            toggles[i] = value;
        }
        CountActiveObjects();
    }

    void CountActiveObjects()
    {
        activeObjects = 0;

        for (int i = 0; i < prefabs.Count; i++)
        {
            if (prefabs[i] == null) continue;

            if (toggles[i] == true)
            {
                activeObjects++;
            }
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (!drawingPrefabs) return;

        Ray ray = HandleUtility.GUIPointToWorldRay(new Vector2((sceneView.camera.pixelWidth / 2) - 50, sceneView.camera.pixelHeight / 2)); // -50 is to center the text a little bit
        
        if (prefabs.Count == 0)
        {
            Handles.Label(ray.origin, "No Prefabs Selected");
            return;
        }

        if (activeObjects == 0)
        {
            Handles.Label(ray.origin, "No Active Prefabs");
            return;
        }
        
        Paint();
        
    }
    
    private void Paint()
    {
        // TODO: NEEDS COLLIDERS OTHERWISE IT DONT WORK 
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        Event e = Event.current;
        Vector3 mousePosition = e.mousePosition;
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            
        Physics.Raycast(ray, out RaycastHit hit);
        
        Handles.color = Color.green;
            
        // Debug.Log(ray + ": " + hit.point);
        
        Handles.DrawWireDisc(hit.point, hit.normal, brushSize);
        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            GUIUtility.hotControl = controlID;
            lastPlaced.Clear();
            e.Use();
        }

        if (e.type == EventType.MouseDrag && GUIUtility.hotControl == controlID)
        {
            PlaceGameObject(ray, hit);
            e.Use();
        }

        if (e.type == EventType.MouseUp && e.button == 0 && GUIUtility.hotControl == controlID)
        {
            GUIUtility.hotControl = 0;
            e.Use();
            
            Debug.Log("Placed " + lastPlaced.Count + " objects in this stroke");
        }
        
    }

    void PlaceGameObject(Ray ray, RaycastHit hit)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(GetRandomPrefab());
        instance.transform.SetParent(GetParent().transform);
        instance.transform.position = hit.point; //TODO: This spawns half the object in the floor, look into making this not the case.
        instance.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

        if (randomRotation)
        {
            instance.transform.Rotate(0, Random.Range(0.0f, 360.0f), 0, Space.Self);
        }
        
        lastPlaced.Add(instance);
    }

    private GameObject GetRandomPrefab()
    {
        List<GameObject> activePrefabs = new List<GameObject>();

        for (int i = 0; i < prefabs.Count; i++)
        {
            if (prefabs[i] == null) continue;

            if (toggles[i])
            {
                activePrefabs.Add(prefabs[i]);
            }
        }
        
        int randomObject = Random.Range(0, activePrefabs.Count);

        return activePrefabs[randomObject];

        return null;
    }

    
    GameObject GetParent()
    {
        if (parent == null || parent.name != name)
        {
            parent = null;
            parent = GameObject.Find(name);

            if (parent == null)
            {
                parent = new GameObject(name);
            }
        }
        
        return parent;
    }
    
}