using GameData;
using System.Collections;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager instance;
    [Header("Global Effects")]
    [SerializeField] GameObject buffVFXPrefab;
    [SerializeField] GameObject nerfVFXPrefab;

    private void Awake()
    {
        instance = this;
    }

    public void SpawnGlobalEffect(VFX vfx, Vector3 pos)
    {
        switch (vfx)
        {
            case VFX.BUFF:
                StartCoroutine(SpawnVFX(buffVFXPrefab, pos));
                break;
            case VFX.NERF:
                StartCoroutine(SpawnVFX(nerfVFXPrefab, pos));
                break;
            case VFX.HIT:
                break;
            default:
                break;
        }
    }

    IEnumerator SpawnVFX(GameObject vfxPrefab, Vector3 pos)
    {
        GameObject vfx = Instantiate(vfxPrefab, pos, buffVFXPrefab.transform.rotation);

        yield return new WaitForSeconds(vfx.GetComponent<ParticleSystem>().main.duration);

        Destroy(vfx);
    }
}
