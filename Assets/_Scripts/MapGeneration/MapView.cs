using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.MemoryProfiler;
using System.Linq;
using GameData;
using Unity.VisualScripting;

public class MapView : MonoBehaviour
{
    [Header("Prefabs Tipos de sala")]
    [SerializeField] private GameObject notAssignedPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject treasurePrefab;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private GameObject healPrefab;
    [SerializeField] private GameObject shopPrefab;
    [SerializeField] private GameObject startPrefab;
    [SerializeField] private GameObject decorationPrefab;

    [SerializeField] private Material lineMaterial;

    [SerializeField] private float lineThickness = 0.1f;

    private GameObject map;
    private GameObject connections;
    private GameObject nodes;

    public void DrawMap(List<MapNode> path)
    {
        ClearMap();
        CreateEmptyMap();
        DrawNodes(path);
        DrawConnections(path);
        PassConnectedRooms(path);
        DrawDecorations();
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
        lineCollider.size = new Vector3(lineLength, lineWidth, 1f);
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

    public void DrawDecorations()
    {
        // Get map size
        Vector2 mapSize = new(MapManager.instance.mapGenerator.gridHeight * 3, MapManager.instance.mapGenerator.gridWidth * 3);

        // Cast points across to find outside area
        float density = 10f;
        float stepX = mapSize.x / density;
        float stepY = mapSize.y / density;

        LayerMask mask = LayerMask.GetMask("Path", "Node");

        for (float height = 0; height < mapSize.x; height = height + stepX)
        {
            // Cast

            for (float width = 0; width < mapSize.y; width = width + stepY)
            {
                //Vector3 castPosition = new Vector3(height, 5, width);

                //RaycastHit hit;

                //if (Physics.Raycast(castPosition, transform.TransformDirection(Vector3.down), out hit, 10, mask))
                //{
                //    Debug.Log("Did Hit");
                //    Debug.Log(hit.collider);
                //    Debug.Log(hit.point);

                //    if (hit.collider.gameObject.name == "Collider")
                //    {
                //        Debug.DrawRay(castPosition, transform.TransformDirection(Vector3.down) * hit.distance, Color.red, 10f);
                //    }
                //    else
                //    {
                //        Debug.DrawRay(castPosition, transform.TransformDirection(Vector3.down) * hit.distance, Color.purple, 10f);
                //    }
                //}
                //else
                //{
                //    Debug.DrawRay(castPosition, transform.TransformDirection(Vector3.down) * 20, Color.green, 10f);
                //    Debug.Log("Did Not Hit");
                //}
            }
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
}