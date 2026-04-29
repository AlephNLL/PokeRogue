using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    public CinemachineBrain brain;
    public CinemachineVirtualCamera mainCamera;
    public CinemachineVirtualCamera attackCamera;
    public CinemachineVirtualCamera selectCamera;

    private void Awake()
    {
        instance = this;
    }
    public void ActivateAttackCamera()
    {
        attackCamera.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);
        selectCamera.gameObject.SetActive(false);
    }

    public void ActivateMainCamera()
    {
        mainCamera.gameObject.SetActive(true);
        attackCamera.gameObject.SetActive(false);
        selectCamera.gameObject.SetActive(false);
    }

    public void ActivateSelectionCamera()
    {
        mainCamera.gameObject.SetActive(false);
        attackCamera.gameObject.SetActive(false);
        selectCamera.gameObject.SetActive(true);
    }

    public void SetBlendTime(float blendTime)
    {
        if (brain != null)
        {
            brain.m_DefaultBlend.m_Time = blendTime;
        }
    }
}
