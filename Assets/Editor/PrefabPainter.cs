using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

public class PrefabPainter : EditorWindow
{
    private List<GameObject> prefabs = new List<GameObject>();

    private float brushSize = 5f;
    private float density = 1f;
    private LayerMask placementMask = ~0;
    private float maxSlopeAngle = 45f;
    private bool randomRotation = true;

    private bool isPainting = false;

    [MenuItem("Tools/Ball/Prefab Painter")]
    public static void ShowWindow()
    {
        GetWindow<PrefabPainter>("Prefab Painter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Painter Settings", EditorStyles.boldLabel);

        int removeIndex = -1;

        for (int i = 0; i < prefabs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            prefabs[i] = (GameObject)EditorGUILayout.ObjectField(prefabs[i], typeof(GameObject), false);

            if (GUILayout.Button("X", GUILayout.Width(20)))
                removeIndex = i;

            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex >= 0)
            prefabs.RemoveAt(removeIndex);

        if (GUILayout.Button("Add Prefab"))
            prefabs.Add(null);

        brushSize = EditorGUILayout.Slider("Brush Size", brushSize, 0.1f, 50f);
        density = EditorGUILayout.Slider("Density", density, 0.1f, 10f);

        placementMask = LayerMaskField("Placement Mask", placementMask);

        maxSlopeAngle = EditorGUILayout.Slider("Max Slope Angle", maxSlopeAngle, 0f, 90f);
        randomRotation = EditorGUILayout.Toggle("Random Y Rotation", randomRotation);

        GUILayout.Space(10);

        bool newPainting = GUILayout.Toggle(isPainting, "Enable Painting", "Button");

        if (newPainting != isPainting)
        {
            isPainting = newPainting;

            SceneView.duringSceneGui -= OnSceneGUI;

            if (isPainting)
                SceneView.duringSceneGui += OnSceneGUI;
        }
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!isPainting)
            return;

        if (prefabs.Count == 0)
        {
            Handles.Label(Vector3.zero, "No prefabs assigned.");
            return;
        }

        Event e = Event.current;

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, placementMask))
            return;

        Handles.color = new Color(0, 1, 0, 0.3f);
        Handles.DrawSolidDisc(hit.point, hit.normal, brushSize);

        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0 && !e.alt)
        {
            Paint(hit);
            e.Use();
        }
    }

    private void Paint(RaycastHit hit)
    {
        float angle = Vector3.Angle(hit.normal, Vector3.up);
        if (angle > maxSlopeAngle)
            return;

        int spawnCount = Mathf.Max(1, Mathf.RoundToInt(density));

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * brushSize;
            Vector3 offset = new Vector3(randomCircle.x, 0, randomCircle.y);

            Vector3 spawnPos = hit.point + offset;

            if (!Physics.Raycast(spawnPos + Vector3.up * 10f, Vector3.down, out RaycastHit newHit, 20f, placementMask))
                continue;

            float newAngle = Vector3.Angle(newHit.normal, Vector3.up);
            if (newAngle > maxSlopeAngle)
                continue;

            GameObject prefab = GetRandomPrefab();
            if (prefab == null)
            {
                Debug.LogWarning("Prefab is null or missing!");
                continue;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            if (instance == null)
            {
                Debug.LogError("Failed to instantiate prefab.");
                continue;
            }

            Undo.RegisterCreatedObjectUndo(instance, "Paint Prefab");

            instance.transform.position = newHit.point;

            if (randomRotation)
                instance.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
        }
    }

    private GameObject GetRandomPrefab()
    {
        List<GameObject> validPrefabs = prefabs.FindAll(p => p != null);

        if (validPrefabs.Count == 0)
            return null;

        return validPrefabs[Random.Range(0, validPrefabs.Count)];
    }

    private LayerMask LayerMaskField(string label, LayerMask layerMask)
    {
        var layers = InternalEditorUtility.layers;
        int maskWithoutEmpty = 0;

        for (int i = 0; i < layers.Length; i++)
        {
            int layer = LayerMask.NameToLayer(layers[i]);
            if (((1 << layer) & layerMask.value) > 0)
                maskWithoutEmpty |= (1 << i);
        }

        maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers);

        int newMask = 0;
        for (int i = 0; i < layers.Length; i++)
        {
            if ((maskWithoutEmpty & (1 << i)) > 0)
                newMask |= (1 << LayerMask.NameToLayer(layers[i]));
        }

        layerMask.value = newMask;
        return layerMask;
    }
}