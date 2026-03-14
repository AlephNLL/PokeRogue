using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;

    public List<GameObject> createdRooms = new List<GameObject>();
    public List<GameObject> selectedRooms = new List<GameObject>();
    public List<Node> nodes = new List<Node>();

    public MapView mapView;
    public MapGenerator mapGenerator;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance.gameObject);
        } else
        {
            Destroy(gameObject);
        }

        if (createdRooms.Count() == 0)
        {
            mapGenerator.GenerateMap();
        }
    }

    void OnEnable()
    {
        Debug.Log("OnEnable called");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
    }

    public void UnlockStartingPaths()
    {
        foreach (GameObject node in createdRooms)
        {
            if (node != null)
            {
                Node mapNode = node.GetComponent<Node>();
                if ( mapNode.floorLevel == 0)
                {
                    node.layer = LayerMask.NameToLayer("Node");
                }
            }
        }
    }
    public void BlockOtherPaths(GameObject obj)
    {
        foreach (GameObject node in createdRooms)
        {
            if (node.layer == LayerMask.NameToLayer("Node") && node != obj)
            {
                node.layer = 0;
            }
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }

    public void LoadMapScene()
    {
        SceneManager.LoadSceneAsync("MapScene");
    }

    public void PassNodeData()
    {
        nodes.Clear();
        foreach (GameObject obj in createdRooms)
        {
            Node node = obj.GetComponent<Node>();
            nodes.Add(node);
        }
    }
}