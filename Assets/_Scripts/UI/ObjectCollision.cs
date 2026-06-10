using UnityEngine;

public class ObjectCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Path")) Destroy(this.gameObject);

    }
}
