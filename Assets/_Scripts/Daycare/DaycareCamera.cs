using Cinemachine;
using System.Collections;
using UnityEngine;

public class DaycareCamera : MonoBehaviour
{
    public static DaycareCamera instance;

    public CinemachineVirtualCamera daycareCamera;
    public CinemachineVirtualCamera fusionCamera;

    private Transform lastTarget;
    private void Awake()
    {
        instance = this;
        RenderSettings.skybox.SetFloat("Rotation", 136);
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
        if (lastTarget != null) Destroy(lastTarget.gameObject);

        Transform transform = new GameObject().transform;
        transform.position = target.position - new Vector3(0, -1f, 3f);
        transform.rotation = Quaternion.identity;

        lastTarget = transform;

        fusionCamera.LookAt = transform;
        fusionCamera.Follow = transform;
    }

    public IEnumerator LerpCameraOffset(float x, float duration)
    {
        var transposer = fusionCamera.GetCinemachineComponent<CinemachineTransposer>();
        var composer = fusionCamera.GetCinemachineComponent<CinemachineComposer>();

        Vector3 startingPos = transposer.m_FollowOffset;
        Vector3 endingPos = new Vector3(x, startingPos.y, startingPos.z);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float percent = Mathf.Clamp01(elapsedTime / duration);
            transposer.m_FollowOffset = Vector3.Lerp(startingPos, endingPos, percent);
            composer.m_TrackedObjectOffset = Vector3.Lerp(startingPos, new Vector3(x, 0, 0), percent);
            yield return null;
        }
    }

}
