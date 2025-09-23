using UnityEngine;

public class AutoReturnOnDisable : MonoBehaviour
{
    private void OnDisable()
    {
        if (PoolManager.Instance != null && gameObject != null)
        {
            if (!Application.isPlaying) return;
            PoolManager.Instance.ReturnToPool(gameObject);
        }
    }
}


