using GameData;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.CanvasScaler;

public class FresnelApplier : MonoBehaviour
{
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

    public static void changeStance(GameObject unit, Stance stance)
    {
        switch (stance)
        {
            case Stance.AGRESSIVE:
                changeBase(unit, Color.darkRed);
                break;
            case Stance.DEFENSIVE:
                changeBase(unit, Color.blue);
                break;
            case Stance.AGILE:
                changeBase(unit, Color.green);
                break;
            case Stance.CAUTIOUS:
                changeBase(unit, Color.cyan);
                break;
            case Stance.TRICKY:
                changeBase(unit, Color.purple);
                break;
            default:
                break;
        }

    }

    public static void changeBase(GameObject unit, Color color)
    {
        MeshRenderer baseMesh = unit.transform.Find("Capsule").Find("Base").GetComponent<MeshRenderer>();
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetColor("_MaskColor", color);
        baseMesh.SetPropertyBlock(mpb);
        print("hola");
    }
}
