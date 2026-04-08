using Unity.VisualScripting;
using UnityEngine;

public class FresnelApplier : MonoBehaviour
{
    static Color lastColor;
    public static void applyFresnel(GameObject unit, Color color)
    {
        MeshRenderer capsule = unit.GetComponentInChildren<MeshRenderer>();
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetFloat("_ApplyFresnel", 1f);
        mpb.SetColor("_FresnelColor", color);
        capsule.SetPropertyBlock(mpb);
        lastColor = mpb.GetColor("_FresnelColor");
        //print(fresnelColor+"hola");
    }
    public static void clearFresnel(GameObject unit)
    {
        MeshRenderer capsule = unit.GetComponentInChildren<MeshRenderer>();
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetFloat("_ApplyFresnel", 0f);
        capsule.SetPropertyBlock(mpb);
    }

    public static Color getFresnelColor(GameObject unit)
    {
        //MeshRenderer capsule = unit.GetComponentInChildren<MeshRenderer>();
        //Color fresnelColor = capsule.material.GetColor("_FresnelColor");
        //print(fresnelColor+"ho1la1");
        //GetColor
        return lastColor;
    }

}
