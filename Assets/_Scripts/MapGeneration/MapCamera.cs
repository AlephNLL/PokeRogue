using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraNodeFollower : MonoBehaviour
{
    public MapView mapView;
    [Header("Referencias")]
    public GameObject targetCameraTarget;

    [Header("Ajustes del Seguimiento")]
    public float followOffsetY = 3.0f;
    public float followOffsetX = 2.0f;
    public float smoothSpeed = 15.0f;
    public LayerMask nodeLayerMask;
    public bool reachedTarget = false;

    [Header("Opciones de Interacción")]
    public bool enableFollowMode = true;

    void Start()
    {
        nodeLayerMask = LayerMask.GetMask("Node");
    }

    void Update()
    {
        if (!enableFollowMode) return;

        if (Input.GetMouseButton(0))
        {
            CheckRaycast();
        }

        if (targetCameraTarget != null && targetCameraTarget.gameObject.activeInHierarchy)
        {
            if (!reachedTarget) { FollowTarget(); }
        }

        if (reachedTarget)
        {
            SceneManager.LoadSceneAsync(targetCameraTarget.gameObject.GetComponent<Node>().sceneName);
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
            MapManager.instance.selectedNodes.Add(obj);
        }
    }

    private void FollowTarget()
    {
        Vector3 currentPos = targetCameraTarget.transform.position;


        Vector3 desiredPosition = new Vector3(
            currentPos.x - followOffsetX,
            currentPos.y + followOffsetY,
            currentPos.z
        );

        if (Vector3.Distance(transform.position, desiredPosition) < 0.01f)
        {
            reachedTarget = true;
            transform.position = desiredPosition;
        } else
        {
            reachedTarget = false;
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
    }

    private void SetSelectedObject(GameObject obj)
    {
        Debug.Log("Objeto actual seleccionado: " + obj.name);
        reachedTarget = false;

        targetCameraTarget = obj;

        if (obj.GetComponent<Node>())
        {
            Node node = obj.GetComponent<Node>();

            foreach (GameObject conection in node.connectedNodes)
            {
                if (conection != null)
                {
                    conection.layer = LayerMask.NameToLayer("Node");
                }
            }
        }
    }
}
