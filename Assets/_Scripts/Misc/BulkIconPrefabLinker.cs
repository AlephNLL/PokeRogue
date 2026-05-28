#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BulkIconPrefabLinker : EditorWindow
{
    private const string OutputFolder = "Assets/Prefabs/UI/Items";

    private readonly List<Texture2D> imageAssets = new List<Texture2D>();
    private readonly List<Sprite> importedSprites = new List<Sprite>();
    private readonly List<GameObject> generatedPrefabs = new List<GameObject>();
    private readonly List<ScriptableObjectLink> scriptableObjects = new List<ScriptableObjectLink>();

    private GameObject examplePrefab;
    private Vector2 scrollPosition;
    private string statusMessage = "Drop images, choose an example prefab, then generate and link item prefabs.";

    [MenuItem("Tools/Bulk Icon Prefab Linker")]
    public static void Open()
    {
        GetWindow<BulkIconPrefabLinker>("Icon Prefab Linker");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawImagesSection();
        EditorGUILayout.Space(12f);
        DrawPrefabSection();
        EditorGUILayout.Space(12f);
        DrawScriptableObjectSection();
        EditorGUILayout.Space(12f);

        EditorGUILayout.HelpBox(statusMessage, MessageType.Info);

        EditorGUILayout.EndScrollView();
    }

    private void DrawImagesSection()
    {
        EditorGUILayout.LabelField("1. Images", EditorStyles.boldLabel);
        DrawDropArea("Drop image assets here", obj => obj is Texture2D, AddImagesFromDrop);

        for (int i = 0; i < imageAssets.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            imageAssets[i] = (Texture2D)EditorGUILayout.ObjectField(imageAssets[i], typeof(Texture2D), false);
            if (GUILayout.Button("-", GUILayout.Width(22f)))
            {
                imageAssets.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        using (new EditorGUI.DisabledScope(!HasAnyImage()))
        {
            if (GUILayout.Button("Import Images As Sprites", GUILayout.Height(28f)))
            {
                ImportImagesAsSprites();
            }
        }

        if (importedSprites.Count > 0)
        {
            EditorGUILayout.LabelField($"Imported sprites: {importedSprites.Count}");
        }
    }

    private void DrawPrefabSection()
    {
        EditorGUILayout.LabelField("2. Prefabs", EditorStyles.boldLabel);
        examplePrefab = (GameObject)EditorGUILayout.ObjectField("Example Prefab", examplePrefab, typeof(GameObject), false);
        EditorGUILayout.LabelField("Output Folder", OutputFolder);

        using (new EditorGUI.DisabledScope(examplePrefab == null || importedSprites.Count == 0))
        {
            if (GUILayout.Button("Generate / Overwrite Prefabs", GUILayout.Height(28f)))
            {
                GeneratePrefabs();
            }
        }

        for (int i = 0; i < generatedPrefabs.Count; i++)
        {
            EditorGUILayout.ObjectField(generatedPrefabs[i], typeof(GameObject), false);
        }
    }

    private void DrawScriptableObjectSection()
    {
        EditorGUILayout.LabelField("3. ScriptableObjects", EditorStyles.boldLabel);
        DrawDropArea("Drop ScriptableObject assets here", obj => obj is ScriptableObject, AddScriptableObjectsFromDrop);

        for (int i = 0; i < scriptableObjects.Count; i++)
        {
            ScriptableObjectLink link = scriptableObjects[i];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            link.asset = (ScriptableObject)EditorGUILayout.ObjectField(link.asset, typeof(ScriptableObject), false);
            if (GUILayout.Button("-", GUILayout.Width(22f)))
            {
                scriptableObjects.RemoveAt(i);
                i--;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                continue;
            }
            EditorGUILayout.EndHorizontal();

            RefreshAutoMatch(link);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Auto Match", link.autoMatchedPrefab, typeof(GameObject), false);
            }

            link.manualPrefab = (GameObject)EditorGUILayout.ObjectField("Manual Override", link.manualPrefab, typeof(GameObject), false);
            EditorGUILayout.EndVertical();
        }

        using (new EditorGUI.DisabledScope(scriptableObjects.Count == 0))
        {
            if (GUILayout.Button("Apply Icon Links", GUILayout.Height(28f)))
            {
                ApplyIconLinks();
            }
        }
    }

    private void ImportImagesAsSprites()
    {
        importedSprites.Clear();
        int importedCount = 0;
        int skippedCount = 0;

        for (int i = 0; i < imageAssets.Count; i++)
        {
            Texture2D texture = imageAssets[i];
            if (texture == null)
            {
                skippedCount++;
                continue;
            }

            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                skippedCount++;
                continue;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                skippedCount++;
                continue;
            }

            if (!importedSprites.Contains(sprite))
            {
                importedSprites.Add(sprite);
                importedCount++;
            }
        }

        statusMessage = $"Imported {importedCount} sprite(s). Skipped {skippedCount} image(s).";
    }

    private void GeneratePrefabs()
    {
        if (examplePrefab == null)
        {
            statusMessage = "Choose an example prefab first.";
            return;
        }

        string sourcePath = AssetDatabase.GetAssetPath(examplePrefab);
        if (string.IsNullOrEmpty(sourcePath) || PrefabUtility.GetPrefabAssetType(examplePrefab) == PrefabAssetType.NotAPrefab)
        {
            statusMessage = "The example object must be a prefab asset.";
            return;
        }

        EnsureOutputFolderExists();
        generatedPrefabs.Clear();

        int createdCount = 0;
        int skippedCount = 0;

        for (int i = 0; i < importedSprites.Count; i++)
        {
            Sprite sprite = importedSprites[i];
            if (sprite == null)
            {
                skippedCount++;
                continue;
            }

            string targetPath = $"{OutputFolder}/{sprite.name}.prefab";
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(sourcePath);

            try
            {
                Image image = prefabContents.GetComponentInChildren<Image>(true);
                if (image == null)
                {
                    skippedCount++;
                    continue;
                }

                prefabContents.name = sprite.name;
                image.sprite = sprite;

                PrefabUtility.SaveAsPrefabAsset(prefabContents, targetPath);
                GameObject generatedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(targetPath);
                if (generatedPrefab != null)
                {
                    generatedPrefabs.Add(generatedPrefab);
                    createdCount++;
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        RefreshAllAutoMatches();

        statusMessage = $"Generated/overwrote {createdCount} prefab(s) in {OutputFolder}. Skipped {skippedCount} sprite(s).";
    }

    private void ApplyIconLinks()
    {
        int linkedCount = 0;
        int missingIconCount = 0;
        int noPrefabCount = 0;

        for (int i = 0; i < scriptableObjects.Count; i++)
        {
            ScriptableObjectLink link = scriptableObjects[i];
            if (link.asset == null)
            {
                continue;
            }

            RefreshAutoMatch(link);
            GameObject prefab = link.manualPrefab != null ? link.manualPrefab : link.autoMatchedPrefab;
            if (prefab == null)
            {
                noPrefabCount++;
                continue;
            }

            SerializedObject serializedObject = new SerializedObject(link.asset);
            SerializedProperty iconProperty = serializedObject.FindProperty("icon");
            if (iconProperty == null || iconProperty.propertyType != SerializedPropertyType.ObjectReference)
            {
                missingIconCount++;
                continue;
            }

            iconProperty.objectReferenceValue = prefab;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(link.asset);
            linkedCount++;
        }

        AssetDatabase.SaveAssets();
        statusMessage = $"Linked {linkedCount} ScriptableObject(s). Missing Icon field: {missingIconCount}. No prefab match: {noPrefabCount}.";
    }

    private void AddImagesFromDrop(Object[] draggedObjects)
    {
        for (int i = 0; i < draggedObjects.Length; i++)
        {
            if (draggedObjects[i] is Texture2D texture && !imageAssets.Contains(texture))
            {
                imageAssets.Add(texture);
            }
        }
    }

    private void AddScriptableObjectsFromDrop(Object[] draggedObjects)
    {
        for (int i = 0; i < draggedObjects.Length; i++)
        {
            if (!(draggedObjects[i] is ScriptableObject scriptableObject) || HasScriptableObject(scriptableObject))
            {
                continue;
            }

            ScriptableObjectLink link = new ScriptableObjectLink { asset = scriptableObject };
            RefreshAutoMatch(link);
            scriptableObjects.Add(link);
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

        if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
        {
            return;
        }

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

    private void RefreshAllAutoMatches()
    {
        for (int i = 0; i < scriptableObjects.Count; i++)
        {
            RefreshAutoMatch(scriptableObjects[i]);
        }
    }

    private void RefreshAutoMatch(ScriptableObjectLink link)
    {
        if (link.asset == null)
        {
            link.autoMatchedPrefab = null;
            return;
        }

        string assetName = link.asset.name;
        link.autoMatchedPrefab = FindGeneratedPrefab(assetName);
    }

    private GameObject FindGeneratedPrefab(string assetName)
    {
        for (int i = 0; i < generatedPrefabs.Count; i++)
        {
            if (generatedPrefabs[i] != null && generatedPrefabs[i].name == assetName)
            {
                return generatedPrefabs[i];
            }
        }

        return AssetDatabase.LoadAssetAtPath<GameObject>($"{OutputFolder}/{assetName}.prefab");
    }

    private bool HasAnyImage()
    {
        for (int i = 0; i < imageAssets.Count; i++)
        {
            if (imageAssets[i] != null)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasScriptableObject(ScriptableObject asset)
    {
        for (int i = 0; i < scriptableObjects.Count; i++)
        {
            if (scriptableObjects[i].asset == asset)
            {
                return true;
            }
        }

        return false;
    }

    private static void EnsureOutputFolderExists()
    {
        if (AssetDatabase.IsValidFolder(OutputFolder))
        {
            return;
        }

        string[] parts = OutputFolder.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }

        if (!Directory.Exists(OutputFolder))
        {
            Directory.CreateDirectory(OutputFolder);
        }
    }

    private class ScriptableObjectLink
    {
        public ScriptableObject asset;
        public GameObject autoMatchedPrefab;
        public GameObject manualPrefab;
    }
}

#endif
