using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class FresnelApplier : MonoBehaviour
{
    static Color lastColor;
    public static void applyFresnel(GameObject unit, Color color)
    {
        MeshRenderer capsule = unit.transform.Find("Capsule").Find("Mons").GetComponentInChildren<MeshRenderer>();
        print(capsule.name);
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetFloat("_ApplyFresnel", 1);
        mpb.SetColor("_FresnelColor", color * 5f);
        capsule.SetPropertyBlock(mpb);
    }
    public static void clearFresnel(GameObject unit)
    {
        MeshRenderer capsule = unit.transform.Find("Capsule").Find("Mons").GetComponentInChildren<MeshRenderer>();
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetFloat("_ApplyFresnel", 0);
        capsule.SetPropertyBlock(mpb);
    }
}
