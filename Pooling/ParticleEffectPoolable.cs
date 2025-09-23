using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleEffectPoolable : MonoBehaviour, IPoolable
{
    ParticleSystem ps;

    private void Awake() { ps = GetComponent<ParticleSystem>(); }

    public void OnSpawnFromPool()
    {
        ps.Clear();
        ps.Play();
        gameObject.SetActive(true);
        StartCoroutine(WaitAndReturn());
    }

    public void OnReturnToPool()
    {
        StopAllCoroutines();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private IEnumerator WaitAndReturn()
    {
        while (ps.IsAlive(true)) yield return null;
        PoolManager.Instance.ReturnToPool(this.gameObject);
    }
}


