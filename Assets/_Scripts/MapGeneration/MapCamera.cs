using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameData;
using Cinemachine;
public class MapCamera : MonoBehaviour
{
    public static MapCamera instance;

    [Header("Ajustes del Seguimiento")]
    public float followOffsetY = 3.0f;
    public float followOffsetX = 2.0f;
    public float smoothSpeed = 15.0f;
    public LayerMask nodeLayerMask;
    private static bool reachedTarget = false;

    [Header("Opciones de Interacción")]
    public bool enableFollowMode = true;

    public CinemachineVirtualCamera virtualCamera;

    public bool ReachedTarget { get => reachedTarget; set => reachedTarget = value; }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        nodeLayerMask = LayerMask.GetMask("Node");
        if (MapManager.instance.mapCreated)
        {
            UpdateLayers(MapManager.instance.currentRoom);

            Vector3 currentPos = MapManager.instance.currentRoom.transform.position;
            Vector3 desiredPosition = new Vector3(
            currentPos.x - followOffsetX,
            currentPos.y + followOffsetY,
            currentPos.z
            );

            transform.position = desiredPosition;
        }

    }

    void Update()
    {
        if (virtualCamera.Follow == null)
        {
            virtualCamera.Follow = MapView.instance.team[0].transform;
        }

        if (!enableFollowMode) return;

        if (Input.GetMouseButton(0))
        {
            CheckRaycast();
        }

        //if (MapManager.instance.currentRoom != null && MapManager.instance.currentRoom.gameObject.activeInHierarchy)
        //{
        //    if (!reachedTarget) { FollowTarget(); }
        //}

        if (reachedTarget)
        {
            switch (MapManager.instance.currentRoom.GetComponent<Node>().nodeEvent)
            {
                case GameData.NodeEvents.NONE:
                    break;
                case GameData.NodeEvents.GOLD:
                    break;
                case GameData.NodeEvents.HEAL:
                    TeamManager.instance.HealTeam(.5f);
                    VFXManager.instance.SpawnGlobalEffect(VFX.BUFF, MapManager.instance.currentRoom);
                    break;
                case GameData.NodeEvents.TRANSITION:
                    MapManager.instance.LoadScene(MapManager.instance.currentRoom.GetComponent<Node>().sceneName);
                    break;
                case GameData.NodeEvents.SPECIAL:
                    break;
                default:
                    break;
            }

            reachedTarget = false;
        }
    }

    private void CheckRaycast()
    {
        Vector2 mousePosition = Input.mousePosition;

        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.MaxValue, nodeLayerMask))
        {
            Debug.Log($"Selección: {hit.collider.gameObject.name}");
            GameObject obj = hit.collider.gameObject;

            MapManager.instance.BlockOtherPaths(obj);
            SetSelectedObject(obj);
            MapManager.instance.selectedRooms.Add(obj);
            MapManager.instance.mapView.MoveTeam(obj.transform.position);

        }
    }

    private void FollowTarget()
    {
        Vector3 currentPos = MapManager.instance.currentRoom.transform.position;


        Vector3 desiredPosition = new Vector3(
            currentPos.x - followOffsetX,
            currentPos.y + followOffsetY,
            currentPos.z
        );

        if (Vector3.Distance(transform.position, desiredPosition) < 0.01f)
        {
            reachedTarget = true;
            transform.position = desiredPosition;
            UpdateLayers(MapManager.instance.currentRoom);
        } else
        {
            reachedTarget = false;
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
    }

    public static void SetSelectedObject(GameObject obj)
    {
        Debug.Log("Objeto actual seleccionado: " + obj.name);
        reachedTarget = false;

        MapManager.instance.currentRoom = obj;
        MapManager.instance.loadRooms = true;

        if (obj.GetComponent<Node>())
        {
            Node node = obj.GetComponent<Node>();
            MapNode mapNode = MapManager.instance.NodeToMapNode(node);
            MapManager.instance.currentNode = mapNode;
        }

    }

    private static void UpdateLayers(GameObject obj)
    {
        foreach (GameObject conection in obj.GetComponent<Node>().connectedNodes)
        {
            if (conection != null)
            {
                conection.layer = LayerMask.NameToLayer("Node");
            }
        }
    }
}
