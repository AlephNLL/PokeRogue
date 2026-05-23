using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameData;
using Cinemachine;
using Unity.Burst.CompilerServices;
using System.Collections;
public class MapCamera : MonoBehaviour
{
    public static MapCamera instance;

    [Header("Ajustes del Seguimiento")]
    public float followOffsetY = 3.0f;
    public float followOffsetX = 2.0f;
    public float smoothSpeed = 15.0f;
    public LayerMask nodeLayerMask;
    public LayerMask decorationLayerMask;
    private static bool reachedTarget = false;

    [Header("Opciones de Interacción")]
    public bool enableFollowMode = true;

    public CinemachineVirtualCamera mapCamera;
    public CinemachineVirtualCamera statsCamera;
    public CinemachineVirtualCamera topViewCamera;

    private int lookAtIndex = 0;

    public bool ReachedTarget { get => reachedTarget; set => reachedTarget = value; }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        nodeLayerMask = LayerMask.GetMask("Node");
        decorationLayerMask = LayerMask.GetMask("Decoration");

        if (MapManager.instance.mapCreated)
        {
            UpdateLayers(MapManager.instance.currentRoom);

            //    Vector3 currentPos = MapManager.instance.currentRoom.transform.position;
            //    Vector3 desiredPosition = new Vector3(
            //    currentPos.x - followOffsetX,
            //    currentPos.y + followOffsetY,
            //    currentPos.z
            //    );

            //    transform.position = desiredPosition;
        }

        if (MapManager.instance.mapLoaded == true)
        {
            mapCamera = GameObject.Find("MapCam").GetComponent<CinemachineVirtualCamera>();
            statsCamera = GameObject.Find("StatsCam").GetComponent <CinemachineVirtualCamera>();
            statsCamera.gameObject.SetActive(false);
            statsCamera.Priority = 2;

            topViewCamera.gameObject.SetActive(false);
            topViewCamera.Priority = 2;
        }
    }

    void Update()
    {
        if(MapView.instance.team == null) return;
        if (MapView.instance.team.Count == 0)
        {
            Debug.Log("El equipo esta vacío");
            return;
        }
        if (mapCamera.Follow == null)
        {
            
            mapCamera.Follow = MapView.instance.team[0].transform;
        }

        if (statsCamera.Follow == null || statsCamera.LookAt == null)
        {
            statsCamera.Follow = MapView.instance.team[0].transform;
            statsCamera.LookAt = MapView.instance.team[0].transform;

        }

        if (topViewCamera.Follow == null)
        {
            topViewCamera.Follow = MapView.instance.team [0].transform;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            HandleStatsCam();
        }

        if (statsCamera.gameObject.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                lookAtIndex++;
                UIManager.Instance.lookAtIndex = lookAtIndex;

                if (lookAtIndex >= MapView.instance.team.Count)
                {
                    lookAtIndex = 0;
                    UIManager.Instance.lookAtIndex = lookAtIndex;
                }

                statsCamera.Follow = MapView.instance.team[lookAtIndex].transform;
                statsCamera.LookAt = MapView.instance.team[lookAtIndex].transform;

                UIManager.Instance.UpdateStats(null, lookAtIndex);
                if (UIManager.Instance.abilities.activeInHierarchy)
                {
                    UIManager.Instance.UpdateAbilities(null, lookAtIndex);
                }
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                lookAtIndex--;
                UIManager.Instance.lookAtIndex = lookAtIndex;

                if (lookAtIndex < 0)
                {
                    lookAtIndex = MapView.instance.team.Count - 1;
                    UIManager.Instance.lookAtIndex = lookAtIndex;
                }

                statsCamera.Follow = MapView.instance.team[lookAtIndex].transform;
                statsCamera.LookAt = MapView.instance.team[lookAtIndex].transform;

                if (UIManager.Instance.abilities.activeInHierarchy)
                {
                    UIManager.Instance.UpdateAbilities(null, lookAtIndex);
                }
                UIManager.Instance.UpdateStats(null, lookAtIndex);
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            HandleTopViewCamera();
        }

        if (!enableFollowMode) return;

        if (Input.GetMouseButton(0))
        {
            Debug.Log("Click");
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
                    UpdateLayers(MapManager.instance.currentRoom);
                    break;
                case GameData.NodeEvents.TRANSITION:
                    if (MapManager.instance.currentRoom.GetComponent<Node>().sceneName == "BattleScene" && MapManager.instance.skipBattles)
                    {
                        UpdateLayers(MapManager.instance.currentRoom);
                    }
                    else
                    {
                        MapManager.instance.LoadScene(MapManager.instance.currentRoom.GetComponent<Node>().sceneName);
                    }
                    break;
                case GameData.NodeEvents.RANDOM:
                    RandomEventManager.instance.CreateRandomEvent();
                    MapManager.instance.currentRoom.GetComponent<Node>().nodeEvent = NodeEvents.NONE;
                    UpdateLayers(MapManager.instance.currentRoom);
                    break;
                default:
                    break;
            }

            reachedTarget = false;
        }
    }

    private void FixedUpdate()
    {
        CameraTransparencyRaycast();
    }

    public void HandleTopViewCamera()
    {
        if (topViewCamera == null) { return; }

        if (topViewCamera.gameObject.activeInHierarchy == false)
        {
            statsCamera.gameObject.SetActive(false);
            UIManager.Instance.ShowCanvas(false);

            topViewCamera.gameObject.SetActive(true);
        }
        else
        {
            topViewCamera.gameObject.SetActive(false);
        }
    }

    public void HandleStatsCam()
    {
        if (statsCamera == null) { return; }

        if (statsCamera.gameObject.activeInHierarchy == false)
        {
            topViewCamera.gameObject.SetActive(false);

            statsCamera.gameObject.SetActive(true);
            UIManager.Instance.ShowCanvas(true);
            UIManager.Instance.UpdateStats(null, 0);
            if (UIManager.Instance.abilities.activeInHierarchy)
            {
                UIManager.Instance.UpdateAbilities(null, lookAtIndex);
            }
        }
        else
        {
            statsCamera.gameObject.SetActive(false);
            UIManager.Instance.ShowCanvas(false);
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

    private void CameraTransparencyRaycast()
    {
        Vector3 rayStartPosition = mapCamera.transform.position;
        Vector3 rayEndPosition = MapManager.instance.currentRoom.transform.position;

        Vector3 direction = (rayEndPosition - rayStartPosition).normalized;

        RaycastHit hit;

        if (Physics.Raycast(rayStartPosition, direction, out hit, float.MaxValue,decorationLayerMask))
        {
            Debug.DrawRay(rayStartPosition, rayEndPosition, Color.red, 0.1f);

            FresnelApplier.SetTransparencyToMapDecoration(hit.collider.gameObject, 0.5f);
            Debug.Log("Camera Hit Decoration");
            Debug.Log(rayStartPosition);
            Debug.Log(direction);

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

    public static void UpdateLayers(GameObject obj)
    {
        foreach (GameObject conection in obj.GetComponent<Node>().connectedNodes)
        {
            if (conection != null)
            {
                conection.layer = LayerMask.NameToLayer("Node");
                Debug.Log("updated layer");
            }
        }
    }
}
