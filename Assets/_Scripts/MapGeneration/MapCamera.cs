using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraNodeFollower : MonoBehaviour
{
    public MapView mapView;
    [Header("Referencias")]
    public Transform targetCameraTarget;

    [Header("Ajustes del Seguimiento")]
    public float followOffsetY = 3.0f;
    public float followOffsetX = 2.0f;
    public float smoothSpeed = 15.0f;
    public LayerMask nodeLayerMask;

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
            FollowTarget();
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

            BlockOtherPaths(mapView.createdNodes, hit.collider.gameObject);
            SetSelectedObject(hit.collider.gameObject);
        }
    }

    private void FollowTarget()
    {
        Vector3 currentPos = targetCameraTarget.position;

        Vector3 desiredPosition = new Vector3(
            currentPos.x - followOffsetX,
            currentPos.y + followOffsetY,
            currentPos.z
        );
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
    }

    private void SetSelectedObject(GameObject obj)
    {
        Debug.Log("Objeto actual seleccionado: " + obj.name);

        targetCameraTarget = obj.transform;

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

    private void BlockOtherPaths(List<GameObject> createdNodes, GameObject obj)
    {
        foreach (GameObject node in createdNodes)
        {
            if (node.layer == LayerMask.NameToLayer("Node") && node != obj)
            {
                node.layer = 0;
            }
        }
    }
}
