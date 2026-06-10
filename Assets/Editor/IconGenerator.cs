using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class IconGenerator : EditorWindow
{
    private Camera targetCamera;
    private string folderPath = "Assets/Sprites/Monstruos/";

    [SerializeField]
    private List<GameObject> monsterPrefabs = new List<GameObject>();

    private SerializedObject serializedWindow;
    private SerializedProperty monsterPrefabsProperty;

    [MenuItem("Tools/Generador de Iconos de Monstruos")]
    public static void ShowWindow()
    {
        GetWindow<IconGenerator>("Generador de Iconos");
    }

    private void OnEnable()
    {
        serializedWindow = new SerializedObject(this);
        monsterPrefabsProperty = serializedWindow.FindProperty(nameof(monsterPrefabs));
    }

    void OnGUI()
    {
        serializedWindow.Update();

        GUILayout.Label("Configuración del Generador", EditorStyles.boldLabel);
        GUILayout.Space(5);

        targetCamera = (Camera)EditorGUILayout.ObjectField("Cámara del Estudio", targetCamera, typeof(Camera), true);
        folderPath = EditorGUILayout.TextField("Ruta de Guardado", folderPath);

        GUILayout.Space(15);
        GUILayout.Label("Lista de Monstruos (Prefabs):", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(monsterPrefabsProperty, true);
        serializedWindow.ApplyModifiedProperties();

        GUILayout.Space(20);

        if (GUILayout.Button("¡Generar Todos los Iconos Ahora!", GUILayout.Height(30)))
        {
            GenerateIcons();
        }
    }

    private void GenerateIcons()
    {
        if (targetCamera == null || monsterPrefabs == null || monsterPrefabs.Count == 0)
        {
            Debug.LogError("Por favor, asigna la cámara y al menos un Prefab.");
            return;
        }

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        int resWidth = 256;
        int resHeight = 256;
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        targetCamera.targetTexture = rt;

        CameraClearFlags oldFlags = targetCamera.clearFlags;
        Color oldBg = targetCamera.backgroundColor;

        targetCamera.clearFlags = CameraClearFlags.Color;
        targetCamera.backgroundColor = new Color(0, 0, 0, 0);

        foreach (GameObject prefab in monsterPrefabs)
        {
            if (prefab == null) continue;

            GameObject spawnedMon = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            spawnedMon.transform.position = Vector3.zero;

            targetCamera.Render();
            RenderTexture.active = rt;

            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGBA32, false);
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            screenShot.Apply();

            DestroyImmediate(spawnedMon);

            byte[] bytes = screenShot.EncodeToPNG();
            string fileName = folderPath + prefab.name + "_Icon.png";
            File.WriteAllBytes(fileName, bytes);

            AssetDatabase.ImportAsset(fileName);

            TextureImporter textureImporter = AssetImporter.GetAtPath(fileName) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                textureImporter.alphaIsTransparency = true;
                textureImporter.SaveAndReimport();
            }
        }

        targetCamera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);
        targetCamera.clearFlags = oldFlags;
        targetCamera.backgroundColor = oldBg;

        AssetDatabase.Refresh();
        Debug.Log("¡Proceso terminado! Todos los iconos se han generado con fondo transparente.");
    }
}
