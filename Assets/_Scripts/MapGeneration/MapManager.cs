using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;

    public List<GameObject> createdRooms = new List<GameObject>();
    public List<GameObject> selectedRooms = new List<GameObject>();
    public List<MapNode> nodes = new List<MapNode>();
    public GameObject currentRoom;
    public MapNode currentNode;

    public MapView mapView;
    public MapGenerator mapGenerator;

    public bool loadRooms = false;
    public bool canLoadRooms = false;
    public bool mapCreated = false;

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
    }

   private void Start()
    {
        if (createdRooms.Count() == 0 && nodes.Count() == 0)
        {
            nodes.Clear();
            createdRooms.Clear();

            mapGenerator.GenerateMap();
            Debug.Log("Mapa Generado");
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

        if (scene.name == "MapGeneration")
        {
            Debug.Log("Mapa Generado en escena de mapa");
            mapView.DrawMap(nodes);
            
            if (mapCreated)
            {
                FindCurrentRoom(currentNode);
            }
        }
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
        if (loadRooms && canLoadRooms && sceneName != "TestScene")
        {
            BattleGenerator.Instance.GenerateTeam(BattleData.Difficulty);
            SceneManager.LoadSceneAsync(sceneName);
        }
    }

    public void LoadMapScene()
    {
        createdRooms.Clear();
        loadRooms = false;
        SceneManager.LoadSceneAsync("MapGeneration");
    }

    public MapNode NodeToMapNode(Node node)
    {
        MapNode mapNode = new();

        mapNode.position = node.position;
        mapNode.gridPosition = node.gridPosition;
        mapNode.roomType = node.roomType;
        mapNode.sceneName = "TestScene"; // Escena de testeo de momento
        mapNode.id = node.id;
        mapNode.floorLevel = node.floorLevel;

        return mapNode;
    }

    public void FindCurrentRoom(MapNode mapNode)
    {
        foreach (GameObject nodeprefab in createdRooms)
        {
            Node node = nodeprefab.GetComponent<Node>();

            if (node.position == mapNode.position && node != null)
            {
                currentRoom = nodeprefab;
            }
        }
    }
}