using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance;
    public GameObject damagePopupPrefab;
    public Canvas canvas;

    void Awake()
    {
        Instance = this;
    }

    public void ShowDamage(Vector3 worldPos, float amount, Color color)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        GameObject go = null;
        if (PoolManager.Instance != null && PoolManager.Instance.HasPool("DamagePopup"))
        {
            go = PoolManager.Instance.Spawn("DamagePopup", screenPos, Quaternion.identity, canvas.transform);
        }
        else
        {
            go = Instantiate(damagePopupPrefab, canvas.transform);
            go.transform.position = screenPos;
        }
        var popup = go.GetComponent<DamagePopup>();
        popup.Setup(amount.ToString(), color);
    }
}
