using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class IconGenerator : EditorWindow
{
    private Camera targetCamera;
    private string folderPath = "Assets/Sprites/Monstruos/";

    // Cambiamos a una lista estándar para que sea más fácil de manejar sin errores
    private List<GameObject> monsterPrefabs = new List<GameObject>();

    [MenuItem("Tools/Generador de Iconos de Monstruos")]
    public static void ShowWindow()
    {
        GetWindow<IconGenerator>("Generador de Iconos");
    }

    void OnGUI()
    {
        GUILayout.Label("Configuración del Generador", EditorStyles.boldLabel);
        GUILayout.Space(5);

        targetCamera = (Camera)EditorGUILayout.ObjectField("Cámara del Estudio", targetCamera, typeof(Camera), true);
        folderPath = EditorGUILayout.TextField("Ruta de Guardado", folderPath);

        GUILayout.Space(15);
        GUILayout.Label("Lista de Monstruos (Prefabs):", EditorStyles.boldLabel);

        // Dibujamos la lista de forma manual y segura
        for (int i = 0; i < monsterPrefabs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            // Campo para cada monstruo
            monsterPrefabs[i] = (GameObject)EditorGUILayout.ObjectField($"Monstruo {i + 1}", monsterPrefabs[i], typeof(GameObject), false);

            // Botón por si quieres eliminar un monstruo de la lista
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                monsterPrefabs.RemoveAt(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(5);

        // Botón para añadir un nuevo espacio a la lista
        if (GUILayout.Button("+ Añadir Espacio para Monstruo"))
        {
            monsterPrefabs.Add(null);
        }

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