using Cinemachine;
using UnityEngine;

public class DaycareCamera : MonoBehaviour
{
    public static DaycareCamera instance;

    public CinemachineVirtualCamera daycareCamera;
    public CinemachineVirtualCamera fusionCamera;
    private void Awake()
    {
        instance = this;
    }
    public void EnableFusionCamera()
    {
        fusionCamera.gameObject.SetActive(true);
        daycareCamera.gameObject.SetActive(false);
    }

    public void DisableFusionCamera()
    {
        fusionCamera.gameObject.SetActive(false);
        daycareCamera.gameObject.SetActive(true);
    }
    public void SetCameraTarget(Transform target)
    {
        Transform transform = new GameObject().transform;
        transform.position = target.position;
        transform.rotation = Quaternion.identity;

        fusionCamera.LookAt = transform;
        fusionCamera.Follow = transform;
    }

    
}
