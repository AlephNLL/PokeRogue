using Unity.VisualScripting;
using UnityEngine;

public class FresnelApplier : MonoBehaviour
{
    public static void applyFresnel(GameObject unit, Color color)
    {
        MeshRenderer capsule = unit.GetComponentInChildren<MeshRenderer>();
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetFloat("_ApplyFresnel", 1f);
        mpb.SetColor("_FresnelColor", color);
        capsule.SetPropertyBlock(mpb);
    }

}
