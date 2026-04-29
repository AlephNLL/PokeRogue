using GameData;
using System.Collections;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager instance;
    [Header("Global Effects")]
    [SerializeField] GameObject buffVFX;
    [SerializeField] GameObject nerfVFX;
    [SerializeField] GameObject hitVFX;
    [SerializeField] GameObject healVFX;
    [SerializeField] static GameObject buffVFXPrefab;
    [SerializeField] static GameObject nerfVFXPrefab;
    [SerializeField] static GameObject hitVFXPrefab;
    [SerializeField] static GameObject healVFXPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        buffVFXPrefab = buffVFX;
        nerfVFXPrefab = nerfVFX;
        hitVFXPrefab = hitVFX;
        healVFXPrefab = healVFX;
    }

    public void SpawnGlobalEffect(VFX vfx, GameObject unit)
    {
        switch (vfx)
        {
            case VFX.BUFF:
                StartCoroutine(SpawnVFX(buffVFXPrefab, unit.transform.position));
                break;
            case VFX.NERF:
                StartCoroutine(SpawnVFX(nerfVFXPrefab, unit.transform.position));
                break;
            case VFX.HIT:
                StartCoroutine(SpawnVFX(hitVFXPrefab, unit.transform.position));
                break;
            case VFX.HEAL:
                StartCoroutine(SpawnVFX(healVFXPrefab, unit.transform.position));
                break;
        }
    }

    public void SpawnStatusVFX(Status statusToApply, GameObject unit)
    {
        switch (statusToApply)
        {
            case Status.NONE:
                break;
            case Status.BURNED:
                FresnelApplier.applyFresnel(unit, Color.orange);
                break;
            case Status.POISONED:
                FresnelApplier.applyFresnel(unit, Color.purple);
                break;
            case Status.PARALYZED:
                FresnelApplier.applyFresnel(unit, Color.yellow);
                break;
            case Status.FROZEN:
                FresnelApplier.applyFresnel(unit, Color.lightBlue);
                break;
            case Status.ASLEEP:
                break;
            default:
                break;
        }
    }

    IEnumerator SpawnVFX(GameObject vfxPrefab, Vector3 pos)
    {
        GameObject vfx = Instantiate(vfxPrefab, pos, vfxPrefab.transform.rotation);

        yield return new WaitForSeconds(vfx.GetComponent<ParticleSystem>().main.duration);

        Destroy(vfx);
    }
}
