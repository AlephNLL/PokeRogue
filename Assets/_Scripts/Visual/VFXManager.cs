using GameData;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager instance;
    [Header("Global Effects")]
    [SerializeField] GameObject buffVFX;
    [SerializeField] GameObject nerfVFX;
    [SerializeField] GameObject hitVFX;
    [SerializeField] GameObject healVFX;
    [SerializeField] GameObject freezeVFX;
    [SerializeField] GameObject burnedVFX;
    [SerializeField] GameObject poisonedVFX;
    [SerializeField] GameObject paralyzedVFX;
    [SerializeField] GameObject asleepVFX;
    [SerializeField] static GameObject buffVFXPrefab;
    [SerializeField] static GameObject nerfVFXPrefab;
    [SerializeField] static GameObject hitVFXPrefab;
    [SerializeField] static GameObject healVFXPrefab;
    [SerializeField] static GameObject freezeVFXPrefab;
    [SerializeField] static GameObject burnedVFXPrefab;
    [SerializeField] static GameObject poisonedVFXPrefab;
    [SerializeField] static GameObject paralyzedVFXPrefab;
    [SerializeField] static GameObject asleepVFXPrefab;

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
        freezeVFXPrefab = freezeVFX;
        burnedVFXPrefab = burnedVFX;
        poisonedVFXPrefab = poisonedVFX;
        paralyzedVFXPrefab = paralyzedVFX;
        asleepVFXPrefab = asleepVFX;
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
            case VFX.FREEZE:
                SpawnStatusVFXPrefab(freezeVFXPrefab, unit);
                break;
            case VFX.BURN:
                SpawnStatusVFXPrefab(burnedVFXPrefab, unit);
                break;
            case VFX.POISON:
                SpawnStatusVFXPrefab(poisonedVFXPrefab, unit);
                break;
            case VFX.PARALYZE:
                SpawnStatusVFXPrefab(paralyzedVFXPrefab, unit);
                break;
            case VFX.SLEEP:
                SpawnStatusVFXPrefab(asleepVFXPrefab, unit);
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
                SpawnGlobalEffect(VFX.BURN, unit);
                break;
            case Status.POISONED:
                FresnelApplier.applyFresnel(unit, Color.purple);
                SpawnGlobalEffect(VFX.POISON, unit);
                break;
            case Status.PARALYZED:
                FresnelApplier.applyFresnel(unit, Color.yellow);
                SpawnGlobalEffect(VFX.PARALYZE, unit);
                break;
            case Status.FROZEN:
                FresnelApplier.applyFresnel(unit, Color.lightBlue);
                SpawnGlobalEffect(VFX.FREEZE, unit);
                break;
            case Status.ASLEEP:
                SpawnGlobalEffect(VFX.SLEEP, unit);
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

    private void SpawnStatusVFXPrefab(GameObject vfxPrefab, GameObject unit)
    {
        Vector3 spawnpoint = unit.transform.position + Vector3.up * 0.5f;
        GameObject vfx = Instantiate(vfxPrefab, spawnpoint, vfxPrefab.transform.rotation);
        vfx.transform.parent = unit.transform;
    }

    public void ClearStatusVFXPrefab(GameObject unit)
    {
        List<GameObject> vfxs = new List<GameObject>();

        foreach (GameObject child in unit.GetComponentsInChildren<GameObject>())
        { if (child.CompareTag("Status")) { vfxs.Add(child); } }

        if (vfxs.Count == 0) { return; }
        foreach (GameObject vfx in vfxs) { Destroy(vfx); }
    }
}
