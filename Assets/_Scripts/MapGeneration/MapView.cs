using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GameData;
using System.Collections;
using System.IO;

public class MapView : MonoBehaviour
{
    public static MapView instance;

    [Header("Prefabs Tipos de sala")]
    [SerializeField] private GameObject notAssignedPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject treasurePrefab;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private GameObject healPrefab;
    [SerializeField] private GameObject shopPrefab;
    [SerializeField] private GameObject startPrefab;
    [SerializeField] private GameObject randomPrefab;
    [SerializeField] private GameObject[] decorationPrefabs;

    [Header("Configuracion del path")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineThickness = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugRays = false;
    [SerializeField] private float density = 10f;
    [SerializeField] private float rayDuration = 10f;

    [Header("Referencias")]
    [SerializeField] private TeamManager teamManager;

    [SerializeField] public List<GameObject> team;
    public float moveSpeed = 1f;
    public float moveDelay = 0.2f;

    private GameObject map;
    private GameObject connections;
    private GameObject nodes;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DrawMap(List<MapNode> path)
    {
        ClearMap();
        CreateEmptyMap();
        DrawNodes(path);
        DrawConnections(path);
        PassConnectedRooms(path);
        DrawTeam();

        StartCoroutine(DrawDecorations());

        if (!MapManager.instance.mapCreated)
        {
            MapManager.instance.UnlockStartingPaths();
            MapManager.instance.mapCreated = true;
        }
    }

    public void CreateEmptyMap()
    {
        map = new("Map");
        connections = new("Conections");
        connections.transform.parent = map.transform;
        nodes = new("Nodes");
        nodes.transform.parent = map.transform;
    }

    public void DrawNodes(List<MapNode> path)
    {
        foreach (MapNode node in path)
        {
            switch (node.roomType)
            {
                case RoomType.NOT_ASSIGNED:
                    GameObject mapNode = Instantiate(notAssignedPrefab, node.position, Quaternion.identity);
                    mapNode.name = node.roomType + "-" + node.id;
                    PassNodeData(mapNode, node);
                    MapManager.instance.createdRooms.Add(mapNode);
                    break;
                case RoomType.Spawn:
                    GameObject startNode = Instantiate(startPrefab, node.position, Quaternion.identity);
                    startNode.name = node.roomType + "-" + node.id;
                    PassNodeData(startNode, node);
                    MapManager.instance.createdRooms.Add(startNode);
                    break;
                case RoomType.Boss:
                    GameObject bossNode = Instantiate(bossPrefab, node.position, Quaternion.identity);
                    bossNode.name = node.roomType + "-" + node.id;
                    PassNodeData(bossNode, node);
                    MapManager.instance.createdRooms.Add(bossNode);
                    break;
                case RoomType.Heal:
                    GameObject healNode = Instantiate(healPrefab, node.position, Quaternion.identity);
                    healNode.name = node.roomType + "-" + node.id;
                    PassNodeData(healNode, node);
                    MapManager.instance.createdRooms.Add(healNode);
                    break;
                case RoomType.Enemy:
                    GameObject enemyNode = Instantiate(enemyPrefab, node.position, Quaternion.identity);
                    enemyNode.name = node.roomType + "-" + node.id;
                    enemyNode.GetComponent<Node>().sceneName = "BattleScene";
                    PassNodeData(enemyNode, node);
                    MapManager.instance.createdRooms.Add(enemyNode);
                    break;
                case RoomType.Shop:
                    GameObject shopNode = Instantiate(shopPrefab, node.position, Quaternion.identity);
                    shopNode.name = node.roomType + "-" + node.id;
                    PassNodeData(shopNode, node);
                    MapManager.instance.createdRooms.Add(shopNode);
                    break;
                case RoomType.Treasure:
                    GameObject treasureNode = Instantiate(treasurePrefab, node.position, Quaternion.identity);
                    treasureNode.name = node.roomType + "-" + node.id;
                    PassNodeData(treasureNode, node);
                    MapManager.instance.createdRooms.Add(treasureNode);
                    break;
                case RoomType.Random:
                    GameObject randomNode = Instantiate(randomPrefab, node.position, Quaternion.identity);
                    randomNode.name = node.roomType + "-" + node.id;
                    PassNodeData(randomNode, node);
                    MapManager.instance.createdRooms.Add(randomNode);
                    break;
            }
        }
    }

    public void DrawConnections(List<MapNode> path)
    {
        foreach (MapNode node in path)
        {
            foreach (MapNode connection in node.connectedNodes)
            {
                //GameObject childGO = new("Collider");
                GameObject connectionGO = new("Connection", typeof(LineRenderer));
                //childGO.transform.parent = connectionGO.transform;

                LineRenderer lr = connectionGO.GetComponent<LineRenderer>();
                lr.material = lineMaterial;
                lr.startWidth = lineThickness;
                lr.endWidth = lineThickness;
                lr.positionCount = 2;
                Vector3 offset = new Vector3(0, 0.01f, 0);
                lr.SetPosition(0, node.position - offset);
                lr.SetPosition(1, connection.position - offset);
                lr.alignment = LineAlignment.TransformZ;
                lr.textureMode = LineTextureMode.Tile;
                connectionGO.transform.parent = connections.transform;
                connectionGO.transform.Rotate(new Vector3(90, 0, 0));
                lr.generateLightingData = true;

                AddColliderToLine(lr, node.position - offset, connection.position - offset);
                //Mesh lineMesh = new();
                //lr.BakeMesh(lineMesh, true);

                //childGO.AddComponent<MeshFilter>();
                //childGO.AddComponent<MeshCollider>();
                //MeshFilter filter = childGO.GetComponent<MeshFilter>();
                //MeshCollider collider = childGO.GetComponent<MeshCollider>();

                //filter.mesh = lineMesh;
                //collider.sharedMesh = lineMesh;
                //childGO.transform.Rotate(new Vector3(-90, 0, 0));

                //childGO.layer = LayerMask.NameToLayer("Path");
            }
        }
    }

    private void AddColliderToLine(LineRenderer line, Vector3 startPoint, Vector3 endPoint)
    {
        //create the collider for the line
        BoxCollider lineCollider = new GameObject("Collider").AddComponent<BoxCollider>();
        //set the collider as a child of your line
        lineCollider.transform.parent = line.transform;
        // get width of collider from line 
        float lineWidth = line.endWidth;
        // get the length of the line using the Distance method
        float lineLength = Vector3.Distance(startPoint, endPoint);
        // size of collider is set where X is length of line, Y is width of line
        //z will be how far the collider reaches to the sky
        lineCollider.size = new Vector3(lineLength + 1, lineWidth, 1.5f);
        // get the midPoint
        Vector3 midPoint = (startPoint + endPoint) / 2;
        // move the created collider to the midPoint
        lineCollider.transform.position = midPoint;


        //heres the beef of the function, Mathf.Atan2 wants the slope, be careful however because it wants it in a weird form
        //it will divide for you so just plug in your (y2-y1),(x2,x1)
        float angle = Mathf.Atan2((endPoint.z - startPoint.z), (endPoint.x - startPoint.x));

        // angle now holds our answer but it's in radians, we want degrees
        // Mathf.Rad2Deg is just a constant equal to 57.2958 that we multiply by to change radians to degrees
        angle *= Mathf.Rad2Deg;

        //were interested in the inverse so multiply by -1
        angle *= -1;
        // now apply the rotation to the collider's transform, carful where you put the angle variable
        // in 3d space you don't wan't to rotate on your y axis
        lineCollider.transform.Rotate(0, angle, 0);
        lineCollider.gameObject.layer = LayerMask.NameToLayer("Path");
    }

    private IEnumerator DrawDecorations()
    {

        yield return new WaitForSeconds(0.2f);
        // Get map size
        Vector2 mapSize = new(MapManager.instance.mapGenerator.gridHeight * 3, MapManager.instance.mapGenerator.gridWidth * 3);

        // Cast points across to find outside area
        float stepX = mapSize.x / density;
        float stepZ = mapSize.y / density;

        LayerMask mask = LayerMask.GetMask("Path", "Node");

        List<Vector3> decorationPositions = new List<Vector3>();

        for (float height = -6; height < mapSize.x + 8; height = height + stepX)
        {
            // Cast

            for (float width = -3; width < mapSize.y + 2; width = width + stepZ)
            {
                Vector3 castPosition = new Vector3(height, 2, width);

                // Randomize Position
                float offsetX = stepX / 4;
                float offsetZ = stepZ / 4;

                float randomX = UnityEngine.Random.Range(castPosition.x - offsetX, castPosition.x + offsetX);
                float randomZ = UnityEngine.Random.Range(castPosition.z - offsetZ, castPosition.z + offsetZ);

                Vector3 randomizedPosition = new(randomX, 2, randomZ);

                RaycastHit hit;

                if (Physics.Raycast(randomizedPosition, transform.TransformDirection(Vector3.down), out hit, 3, mask))
                {
                    //Debug.Log("Did Hit");
                    //Debug.Log(hit.collider);
                    //Debug.Log(hit.point);

                    if (hit.collider.gameObject.name == "Collider" && enableDebugRays)
                    {
                        Debug.DrawRay(randomizedPosition, transform.TransformDirection(Vector3.down) * hit.distance, Color.red, rayDuration);
                    }
                    else if (enableDebugRays)
                    {
                        Debug.DrawRay(randomizedPosition, transform.TransformDirection(Vector3.down) * hit.distance, Color.purple, rayDuration);
                    }
                    continue;
                }
                else
                {
                    if (enableDebugRays)
                    {
                        Debug.DrawRay(randomizedPosition, transform.TransformDirection(Vector3.down) * 3, Color.green, rayDuration);
                    }

                    //Debug.Log("Did Not Hit");

                    // Save positions if not hit with path

                    decorationPositions.Add(new Vector3(randomizedPosition.x, 0, randomizedPosition.z));
                }
            }
        }
        InstantiateDecorations(decorationPositions, stepX, stepZ);
    }

    private void InstantiateDecorations(List<Vector3> decorationPositions, float stepX, float stepZ)
    {
        GameObject decorationsGO = GameObject.Find("Decorations");

        if (decorationsGO != null)
        {
            Destroy(decorationsGO);
        }

        GameObject decorations = new("Decorations");
        decorations.transform.parent = map.transform;

        foreach (Vector3 position in decorationPositions)
        {
            int randomPrefab = UnityEngine.Random.Range(0, decorationPrefabs.Length);

            GameObject decorationGO = Instantiate(decorationPrefabs[randomPrefab], position, Quaternion.identity, decorations.transform);

            FresnelApplier.SetMapDecorationShader(decorationGO);

            float randomScale = UnityEngine.Random.Range(-3, 3);
            decorationGO.transform.localScale = new Vector3 (
                decorationGO.transform.localScale.x + randomScale, 
                decorationGO.transform.localScale.y + randomScale, 
                decorationGO.transform.localScale.z + randomScale
                );
            decorationGO.transform.Rotate(new Vector3(-90, 0, 0));
            decorationGO.layer = LayerMask.NameToLayer("Decoration");

            decorationGO.AddComponent<CapsuleCollider>();
        }
    }

    public void ClearMap()
    {
        if (map != null && MapManager.instance.nodes.Count() != 0) { Destroy(map); MapManager.instance.createdRooms.Clear(); MapManager.instance.nodes.Clear(); }
    }

    public void PassNodeData(GameObject mapNode, MapNode node)
    {
        mapNode.transform.parent = nodes.transform;
        Node nodeData = mapNode.GetComponent<Node>();
        nodeData.position = node.position;
        nodeData.gridPosition = node.gridPosition;
        nodeData.roomType = node.roomType;
        //nodeData.sceneName = "TestScene"; // Escena de testeo de momento
        nodeData.id = node.id;
        nodeData.floorLevel = node.floorLevel;

        if (!MapManager.instance.nodes.Contains(node))
        {
            MapManager.instance.nodes.Add(node);
        }
    }

    public void PassConnectedRooms(List<MapNode> path)
    {
        foreach (MapNode node in path)
        {
            GameObject instance = MapManager.instance.createdRooms.FirstOrDefault(g => g.name == $"{node.roomType}-{node.id}");
            Node mapNode = instance.GetComponent<Node>();

            if (mapNode != null)
            {
                foreach (MapNode connectionTarget in node.connectedNodes)
                {
                    GameObject targetInstance = MapManager.instance.createdRooms.FirstOrDefault(g => g.name == $"{connectionTarget.roomType}-{connectionTarget.id}");

                    if (targetInstance != null)
                    {
                        mapNode.connectedNodes.Add(targetInstance);
                        //Debug.Log($"Nodo conectado con {targetInstance.name}");
                    }
                }
            }
        }
    }
    public void DrawTeam()
    {
        if (GameObject.Find("Team"))
        {
            Destroy(GameObject.Find("Team"));
        }

        Vector3 position = Vector3.zero;
        GameObject teamGO = new("Team");
        teamGO.transform.parent = map.transform;

        team = new List<GameObject>();

        Debug.LogWarning($"Hay {PlayerData.Instance.GetTeamPrefabs().Length} unidades en el equipo");
        // Get mesh from prefab and instantiate
        foreach (GameObject unit in PlayerData.Instance.GetTeamPrefabs())
        {
            GameObject unitName = new GameObject(unit.name);
            unitName.transform.parent = teamGO.transform;

            GameObject unitBase = new GameObject("Base");
            unitBase.AddComponent<MeshFilter>(); unitBase.AddComponent<MeshRenderer>();

            MeshFilter baseMeshFilter = unit.transform.Find("Capsule").Find("Base").GetComponentInChildren<MeshFilter>();
            MeshRenderer baseMeshRenderer = unit.transform.Find("Capsule").Find("Base").GetComponentInChildren<MeshRenderer>();

            unitBase.GetComponent<MeshFilter>().mesh = baseMeshFilter.sharedMesh;
            unitBase.GetComponent<MeshRenderer>().material = baseMeshRenderer.sharedMaterial;

            unitBase.transform.parent = unitName.transform;

            GameObject unitGO = new GameObject(unit.name);
            unitGO.AddComponent<MeshFilter>();
            unitGO.AddComponent<MeshRenderer>();

            //MeshFilter meshFilter = unit.GetComponentInChildren<MeshFilter>();
            //MeshRenderer meshRenderer = unit.GetComponentInChildren<MeshRenderer>();
            MeshFilter meshFilter = unit.transform.Find("Capsule").Find("Mons").GetComponentInChildren<MeshFilter>();
            MeshRenderer meshRenderer = unit.transform.Find("Capsule").Find("Mons").GetComponentInChildren<MeshRenderer>();

            unitGO.GetComponent<MeshFilter>().mesh = meshFilter.sharedMesh;
            unitGO.GetComponent<MeshRenderer>().material = meshRenderer.sharedMaterial;

            unitGO.transform.parent = unitName.transform;
            unitName.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            unitGO.transform.rotation = Quaternion.Euler(0, 90, 0);

            team.Add(unitName);
        }
        if (MapManager.instance.currentNode != null)
        {
            position = MapManager.instance.currentNode.position;
        }
        else
        {
            position = GameObject.Find("Spawn-0").transform.position;
        }

        UpdateTeamPositions(team, position);
    }

    private void UpdateTeamPositions(List<GameObject> team, Vector3 position)
    {
        int count = team.Count;
        Vector3 offset = Vector3.zero;
        Vector3 offsetY = new(0, 0.0275f, 0);

        switch (count)
        {
            case 0:
                break;
            case 1:
                team[0].transform.position = position + offsetY;
                break;
            case 2:
                offset = new(0, 0, 0.25f);
                team[0].transform.position = position + offset + offsetY;
                team[1].transform.position = position - offset + offsetY;
                break;
            case 3:
                offset = new(0, 0, 0.45f);
                team[0].transform.position = position + offset + offsetY;
                team[1].transform.position = position + offsetY;
                team[2].transform.position = position - offset + offsetY;
                break;
            case 4:
                offset = new(0, 0, 0.25f);
                team[0].transform.position = position + (offset * 3) + offsetY;
                team[1].transform.position = position + offset + offsetY;
                team[2].transform.position = position - offset + offsetY;
                team[3].transform.position = position - (offset * 3) + offsetY;
                break;
        }
    }

    public void MoveTeam(Vector3 targetPosition)
    {
        int count = team.Count;
        Vector3 offset = Vector3.zero;
        Vector3 offsetY = new(0, 0, 0);
        Vector3 offsetTarget = targetPosition + offsetY;


        switch (count)
        {
            case 1:
                StartCoroutine(MoveTo(team[0], offsetTarget, 0, true));
                break; 
            case 2:
                offset = new(0, 0, 0.25f);
                offsetTarget = targetPosition + offset + offsetY;
                StartCoroutine(MoveTo(team[0], offsetTarget, 0));

                offsetTarget = targetPosition - offset + offsetY;
                StartCoroutine(MoveTo(team[1], offsetTarget, moveDelay, true));

                break;
            case 3:
                offset = new(0, 0, 0.45f);
                offsetTarget = targetPosition + offset + offsetY;
                StartCoroutine(MoveTo(team[0], offsetTarget, 0));

                offsetTarget = targetPosition + offsetY;
                StartCoroutine(MoveTo(team[1], offsetTarget, moveDelay));

                offsetTarget = targetPosition - offset + offsetY;
                StartCoroutine(MoveTo(team[2], offsetTarget, moveDelay * 2, true));

                break;
            case 4:
                offset = new(0, 0, 0.25f);
                offsetTarget = targetPosition + (offset * 3) + offsetY;
                StartCoroutine(MoveTo(team[0], offsetTarget, 0));

                offsetTarget = targetPosition + offset + offsetY;
                StartCoroutine(MoveTo(team[1], offsetTarget, moveDelay));

                offsetTarget = targetPosition - offset + offsetY;
                StartCoroutine(MoveTo(team[2], offsetTarget, moveDelay * 2));

                offsetTarget = targetPosition - (offset * 3) + offsetY;
                StartCoroutine(MoveTo(team[3], offsetTarget, moveDelay * 3, true));
                
                break;
        }
    }

    private IEnumerator MoveTo(GameObject obj, Vector3 targetPosition, float delay, bool last = false)
    {
        Debug.Log("moving");

        yield return new WaitForSeconds(delay);


        while (Vector3.Distance(obj.transform.position, targetPosition) > 0.01f)
        {
            obj.transform.position = Vector3.Lerp(obj.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        if (last)
        {
            Debug.Log("Reached Target");
            MapCamera.instance.ReachedTarget = true;
            MapCamera.UpdateLayers(MapManager.instance.currentRoom);
        }

        yield return null;
    }

    public void DrawNextMap(List<MapNode> path)
    {
        DrawNodes(path);
        DrawConnections(path);
        PassConnectedRooms(path);
        DrawTeam();
    }
}