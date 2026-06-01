using UnityEngine;

public class PathCollision : MonoBehaviour
{
    BoxCollider boxCollider;
    RaycastHit hit;
    LayerMask mask;
    private bool hitDetected;
    private float maxDistance;

    private void Start()
    {
        mask = LayerMask.GetMask("Decoration");
        boxCollider = GetComponent<BoxCollider>();
        maxDistance = 0;

        hitDetected = Physics.BoxCast(transform.position, boxCollider.bounds.extents, transform.forward, out hit, transform.rotation, maxDistance, mask);

        if (hitDetected)
        {
            Destroy(hit.collider.gameObject);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (hitDetected)
        {
            //Draw a Ray forward from GameObject toward the hit
            Gizmos.DrawRay(transform.position, transform.forward * hit.distance);
            //Draw a cube that extends to where the hit exists
            Gizmos.DrawWireCube(transform.position + transform.forward * hit.distance, transform.localScale);
        }
        else
        {
            //Draw a Ray forward from GameObject toward the maximum distance
            Gizmos.DrawRay(transform.position, transform.forward * maxDistance);
            //Draw a cube at the maximum distance
            Gizmos.DrawWireCube(transform.position + transform.forward * maxDistance, transform.localScale);
        }
    }
}
