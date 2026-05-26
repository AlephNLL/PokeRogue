using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BulkReplaceSelected : EditorWindow
{
    private enum ReplacementKind
    {
        Prefab,
        MeshAndMaterials
    }

    private ReplacementKind replacementKind = ReplacementKind.Prefab;
    private List<GameObject> replacementPrefabs = new List<GameObject>();
    private List<Mesh> replacementMeshes = new List<Mesh>();
    private List<Material> replacementMaterials = new List<Material>();
    private Vector3 prefabRotationEuler;
    private float prefabScaleMin;
    private float prefabScaleMax;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Bulk Replace Selected")]

    public static void Open()
    {
        GetWindow<BulkReplaceSelected>("Bulk Replace");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Selection", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Selected objects: {Selection.gameObjects.Length}");

        EditorGUILayout.Space();
        replacementKind = (ReplacementKind)EditorGUILayout.EnumPopup("Replacement Type", replacementKind);

        EditorGUILayout.Space();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        if (replacementKind == ReplacementKind.Prefab)
        {
            DrawPrefabList();

            prefabRotationEuler = EditorGUILayout.Vector3Field("Prefab Rotation", prefabRotationEuler);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Prefab Scale Range", EditorStyles.boldLabel);
            prefabScaleMin = EditorGUILayout.FloatField("Min Scale", prefabScaleMin);
            prefabScaleMax = EditorGUILayout.FloatField("Max Scale", prefabScaleMax);

            EditorGUILayout.HelpBox("One prefab is chosen at random from the list for each selected object. It keeps position, but uses the rotation and scale range you set here.", MessageType.Info);
        }
        else
        {
            DrawMeshList();

            EditorGUILayout.HelpBox("One mesh is chosen at random from the list for each selected object. A new GameObject will be created with MeshFilter and MeshRenderer, then deleted originals will be removed.", MessageType.Info);
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(!CanReplace()))
        {
            if (GUILayout.Button("Replace Selected", GUILayout.Height(32f)))
            {
                ReplaceSelected();
            }
        }
    }

    private bool CanReplace()
    {
        if (Selection.gameObjects.Length == 0)
        {
            return false;
        }

        return replacementKind == ReplacementKind.Prefab
            ? HasAnyPrefab()
            : HasAnyMesh();
    }

    private bool HasAnyPrefab()
    {
        for (int i = 0; i < replacementPrefabs.Count; i++)
        {
            if (replacementPrefabs[i] != null)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasAnyMesh()
    {
        for (int i = 0; i < replacementMeshes.Count; i++)
        {
            if (replacementMeshes[i] != null)
            {
                return true;
            }
        }

        return false;
    }

    private void EnsureMaterialListSize(int size)
    {
        while (replacementMaterials.Count < size)
        {
            replacementMaterials.Add(null);
        }

        while (replacementMaterials.Count > size)
        {
            replacementMaterials.RemoveAt(replacementMaterials.Count - 1);
        }
    }

    private void ReplaceSelected()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();

        try
        {
            foreach (GameObject source in selectedObjects)
            {
                if (source == null)
                {
                    continue;
                }

                if (EditorUtility.IsPersistent(source))
                {
                    continue;
                }

                Transform sourceTransform = source.transform;
                Transform parent = sourceTransform.parent;
                int siblingIndex = sourceTransform.GetSiblingIndex();

                GameObject replacement = CreateReplacement(source, parent);
                if (replacement == null)
                {
                    continue;
                }

                if (replacement.scene != source.scene)
                {
                    SceneManager.MoveGameObjectToScene(replacement, source.scene);
                }

                Undo.RegisterCreatedObjectUndo(replacement, "Bulk Replace Selected");
                replacement.transform.SetSiblingIndex(siblingIndex);
                if (replacementKind == ReplacementKind.Prefab)
                {
                    ApplyPrefabTransform(sourceTransform, replacement.transform);
                }
                else
                {
                    CopyTransform(sourceTransform, replacement.transform);
                }

                Undo.DestroyObjectImmediate(source);
            }
        }
        finally
        {
            Undo.CollapseUndoOperations(undoGroup);
        }
    }

    private GameObject CreateReplacement(GameObject source, Transform parent)
    {
        if (replacementKind == ReplacementKind.Prefab)
        {
            GameObject prefab = GetRandomPrefab();
            if (prefab == null)
            {
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (instance != null)
            {
                instance.name = source.name;
                instance.layer = source.layer;
                instance.tag = source.tag;
                instance.isStatic = source.isStatic;
                instance.transform.SetParent(parent, false);
            }

            return instance;
        }

        Mesh mesh = GetRandomMesh();
        if (mesh == null)
        {
            return null;
        }

        GameObject replacement = new GameObject(source.name);
        replacement.transform.SetParent(parent, false);
        replacement.layer = source.layer;
        replacement.tag = source.tag;
        replacement.isStatic = source.isStatic;
        
        MeshFilter meshFilter = replacement.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        MeshRenderer meshRenderer = replacement.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterials = GetMaterialsArray();

        return replacement;
    }

    private GameObject GetRandomPrefab()
    {
        List<GameObject> validPrefabs = new List<GameObject>();
        for (int i = 0; i < replacementPrefabs.Count; i++)
        {
            if (replacementPrefabs[i] != null)
            {
                validPrefabs.Add(replacementPrefabs[i]);
            }
        }

        if (validPrefabs.Count == 0)
        {
            return null;
        }

        return validPrefabs[Random.Range(0, validPrefabs.Count)];
    }

    private void DrawPrefabList()
    {
        EditorGUILayout.LabelField("Replacement Prefabs", EditorStyles.boldLabel);
        DrawDropArea("Drop prefabs here", obj => obj is GameObject, AddPrefabsFromDrop);

        for (int i = 0; i < replacementPrefabs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            replacementPrefabs[i] = (GameObject)EditorGUILayout.ObjectField(replacementPrefabs[i], typeof(GameObject), false);
            if (GUILayout.Button("-", GUILayout.Width(22f)))
            {
                replacementPrefabs.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawMeshList()
    {
        EditorGUILayout.LabelField("Replacement Meshes", EditorStyles.boldLabel);
        DrawDropArea("Drop meshes here", obj => obj is Mesh, AddMeshesFromDrop);

        for (int i = 0; i < replacementMeshes.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            replacementMeshes[i] = (Mesh)EditorGUILayout.ObjectField(replacementMeshes[i], typeof(Mesh), false);
            if (GUILayout.Button("-", GUILayout.Width(22f)))
            {
                replacementMeshes.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Materials", EditorStyles.boldLabel);

        int materialCount = Mathf.Max(1, EditorGUILayout.IntField("Material Slots", Mathf.Max(1, replacementMaterials.Count)));
        EnsureMaterialListSize(materialCount);

        for (int i = 0; i < replacementMaterials.Count; i++)
        {
            replacementMaterials[i] = (Material)EditorGUILayout.ObjectField($"Material {i}", replacementMaterials[i], typeof(Material), false);
        }
    }

    private void DrawDropArea(string label, System.Func<Object, bool> accepts, System.Action<Object[]> addObjects)
    {
        Rect dropArea = GUILayoutUtility.GetRect(0f, 42f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, label, EditorStyles.helpBox);

        Event evt = Event.current;
        if (!dropArea.Contains(evt.mousePosition))
        {
            return;
        }

        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            Object[] draggedObjects = DragAndDrop.objectReferences;
            bool accepted = false;

            for (int i = 0; i < draggedObjects.Length; i++)
            {
                if (accepts(draggedObjects[i]))
                {
                    accepted = true;
                    break;
                }
            }

            if (!accepted)
            {
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                addObjects(draggedObjects);
            }

            evt.Use();
        }
    }

    private void AddPrefabsFromDrop(Object[] draggedObjects)
    {
        for (int i = 0; i < draggedObjects.Length; i++)
        {
            if (draggedObjects[i] is GameObject prefab
                && PrefabUtility.GetPrefabAssetType(prefab) != PrefabAssetType.NotAPrefab
                && !replacementPrefabs.Contains(prefab))
            {
                replacementPrefabs.Add(prefab);
            }
        }
    }

    private void AddMeshesFromDrop(Object[] draggedObjects)
    {
        for (int i = 0; i < draggedObjects.Length; i++)
        {
            if (draggedObjects[i] is Mesh mesh && !replacementMeshes.Contains(mesh))
            {
                replacementMeshes.Add(mesh);
            }
        }
    }

    private Mesh GetRandomMesh()
    {
        List<Mesh> validMeshes = new List<Mesh>();
        for (int i = 0; i < replacementMeshes.Count; i++)
        {
            if (replacementMeshes[i] != null)
            {
                validMeshes.Add(replacementMeshes[i]);
            }
        }

        if (validMeshes.Count == 0)
        {
            return null;
        }

        return validMeshes[Random.Range(0, validMeshes.Count)];
    }

    private Material[] GetMaterialsArray()
    {
        if (replacementMaterials.Count == 0)
        {
            return new Material[] { null };
        }

        return replacementMaterials.ToArray();
    }

    private static void CopyTransform(Transform source, Transform target)
    {
        target.SetParent(source.parent, true);
        target.SetPositionAndRotation(source.position, source.rotation);
        target.localScale = source.localScale;
    }

    private void ApplyPrefabTransform(Transform source, Transform target)
    {
        float scale = (Random.Range(prefabScaleMin, prefabScaleMax));
        target.SetParent(source.parent, true);
        target.position = source.position;
        target.rotation = Quaternion.Euler(prefabRotationEuler);
        target.localScale = new Vector3(scale, scale, scale);

    }
}
