using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;

public class MapManager : MonoBehaviour, ISaveData
{
    public static MapManager instance;

    [Header("Datos de salas")]
    public List<GameObject> createdRooms = new List<GameObject>();
    public List<GameObject> selectedRooms = new List<GameObject>();
    public List<MapNode> nodes = new List<MapNode>();
    public GameObject currentRoom;
    public MapNode currentNode;

    [Header("Referencias")]
    public MapView mapView;
    public MapGenerator mapGenerator;

    [Header("Debug")]
    public bool loadRooms = false;
    public bool canLoadRooms = false;
    public bool mapCreated;
    public bool mapLoaded = false;
    public bool skipBattles = false;

    public string currentRoomName = "";

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
        ControlsUI.instance.ShowSummaryControls();
        if (createdRooms.Count() == 0 && nodes.Count() == 0 && mapCreated == false)
        {
            nodes.Clear();
            createdRooms.Clear();

            mapGenerator.GenerateMap();
            Debug.Log("Mapa Generado");
        }
        else if (nodes.Count != 0)
        {
            mapView.DrawMap(nodes);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MapGeneration")
        {
            mapLoaded = true;
            if (mapCreated == true)
            {
                Debug.Log("Mapa Generado en escena de mapa");
                mapView.DrawMap(nodes);
                currentRoom = GameObject.Find(currentRoomName);
            }

        } else
        {
            mapLoaded = false;
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
            ControlsUI.instance.HideSummaryControls();
            if (sceneName == "BattleScene") 
            { 
                if(currentNode.roomType == GameData.RoomType.Boss)
                {
                    BattleGenerator.Instance.GenerateTeam(BattleData.Difficulty, true);
                }
                else
                {
                    BattleGenerator.Instance.GenerateTeam(BattleData.Difficulty);
                }     
            }
            SceneManager.LoadSceneAsync(sceneName);
        }
    }

    public void LoadMapScene()
    {
        ControlsUI.instance.ShowSummaryControls();
        createdRooms.Clear();
        loadRooms = false;
        SceneManager.LoadSceneAsync("MapGeneration");
    }
    public void LoadMapSceneFromStart()
    {
        createdRooms.Clear();
        nodes.Clear();
        mapCreated = false;
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
    public void SaveData(ref GameSaveData data)
    {
        data.mapData = new();
        data.mapCreated = mapCreated;
        data.currentRoom = currentRoom.name;
        foreach (MapNode mapNode in nodes)
        {
            data.mapData.Add(mapNode.LoadData());
        }
    }

    public void LoadData(GameSaveData data)
    {
        nodes = new();
        this.mapCreated = data.mapCreated;
        foreach (MapNodeData mapNode in data.mapData)
        {
            nodes.Add(new MapNode().SaveData(mapNode, data));
        }
        currentRoomName = data.currentRoom;
        StartCoroutine(WaitAndFind(currentRoomName));
    }

    private IEnumerator WaitAndFind(string name)
    {
        yield return new WaitForEndOfFrame();
        currentRoom = GameObject.Find(name);
        currentNode = NodeToMapNode(currentRoom.GetComponent<Node>());
    }
}