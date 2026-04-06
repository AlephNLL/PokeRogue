using UnityEngine;

public class Billboard : MonoBehaviour
{
    GameObject cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GameObject.Find("Camera Brain");
    }

    // Update is called once per frame
    void Update()
    {
        LookAtCam();
    }

    void LookAtCam()
    {
        Vector3 forward = cam.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(-forward.normalized, Vector3.up);
    }
}
