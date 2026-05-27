using UnityEngine;

public class CameraSeeThrough : MonoBehaviour
{
    public GameObject camera;
    private LayerMask decorationLayerMask;

    private void Start()
    {
        decorationLayerMask = LayerMask.GetMask("Decoration");
    }

    private void FixedUpdate()
    {
        CameraTransparencyRaycast();
    }
    private void CameraTransparencyRaycast()
    {
        Vector3 rayStartPosition = camera.transform.position;
        Vector3 rayEndPosition = MapManager.instance.currentRoom.transform.position;

        Vector3 direction = (rayEndPosition - rayStartPosition).normalized;

        RaycastHit hit;

        if (Physics.Raycast(rayStartPosition, direction, out hit, float.MaxValue, decorationLayerMask))
        {
            Debug.DrawRay(rayStartPosition, rayEndPosition, Color.red, 0.1f);

            FresnelApplier.SetTransparencyToMapDecoration(hit.collider.gameObject, 0.3f);
            Debug.Log("Camera Hit Decoration");
            Debug.Log(rayStartPosition);
            Debug.Log(direction);

        }
    }
}
