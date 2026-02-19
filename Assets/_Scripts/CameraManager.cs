using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    public CinemachineVirtualCamera mainCamera;
    public CinemachineVirtualCamera attackCamera;

    private void Awake()
    {
        instance = this;
    }
    public void ActivateAttackCamera()
    {
        attackCamera.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);
    }

    public void ActivateMainCamera()
    {
        mainCamera.gameObject.SetActive(true);
        attackCamera.gameObject.SetActive(false);
    }
    
}
