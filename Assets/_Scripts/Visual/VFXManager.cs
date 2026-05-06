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

    [SerializeField] List<VFXAction> scheduledActions = new List<VFXAction>();
    [SerializeField] List<VFXAction> onGoingActions = new List<VFXAction>();
    bool canSpawnVFX = true;

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

        scheduledActions = new List<VFXAction>();
        onGoingActions = new List<VFXAction>();
    }

    public void SpawnGlobalEffect(VFX vfx, GameObject unit)
    {
        VFXAction action = new()
        {
            vfx = vfx,
            unit = unit,
        };

        if (onGoingActions.Find(action => action.unit == unit) != null)
        {
            scheduledActions.Add(action);
            StartCoroutine(RetryVFXAction());
            return;
        }

        switch (vfx)
        {
            case VFX.BUFF:
                StartCoroutine(SpawnVFX(buffVFXPrefab, unit));
                onGoingActions.Add(action);
                break;
            case VFX.NERF:
                StartCoroutine(SpawnVFX(nerfVFXPrefab, unit));
                onGoingActions.Add(action);
                break;
            case VFX.HIT:
                StartCoroutine(SpawnVFX(hitVFXPrefab, unit));
                onGoingActions.Add(action);
                break;
            case VFX.HEAL:
                StartCoroutine(SpawnVFX(healVFXPrefab, unit));
                onGoingActions.Add(action);
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
                SpawnStatusVFXPrefab(asleepVFXPrefab, unit, 1.85f);
                break;
        }
    }
    IEnumerator RetryVFXAction()
    {
        yield return new WaitForSeconds(.1f);

        if(scheduledActions.Count > 0)
        {
            VFXAction action = scheduledActions[0];
            scheduledActions.RemoveAt(0);
            if(action.unit) SpawnGlobalEffect(action.vfx, action.unit);
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

    IEnumerator SpawnVFX(GameObject vfxPrefab, GameObject unit)
    {
        GameObject vfx = Instantiate(vfxPrefab, unit.transform.position, vfxPrefab.transform.rotation);

        yield return new WaitForSeconds(vfx.GetComponent<ParticleSystem>().main.duration);

        Destroy(vfx);

        if (onGoingActions.Find(action => action.unit == unit) != null)
        {
            onGoingActions.Remove(onGoingActions.Find(action => action.unit == unit));
        }   
    }

    private void SpawnStatusVFXPrefab(GameObject vfxPrefab, GameObject unit, float offset = 0.5f)
    {
        Vector3 spawnpoint = unit.transform.position + Vector3.up * offset;
        GameObject vfx = Instantiate(vfxPrefab, spawnpoint, vfxPrefab.transform.rotation);
        vfx.transform.parent = unit.transform;
    }

    public void ClearStatusVFXPrefab(GameObject unit)
    {
        List<Transform> vfxs = new List<Transform>();

        foreach (Transform child in unit.transform)
        { if (child.CompareTag("Status")) { vfxs.Add(child); } }

        if (vfxs.Count == 0) { return; }
        foreach (Transform vfx in vfxs) { Destroy(vfx.gameObject); }
    }
}
[System.Serializable]
public class VFXAction
{
    public VFX vfx;
    public GameObject unit;
}
