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
        if (ObjectPooling.Instance != null)
        {
            go = ObjectPooling.Instance.GetObject(damagePopupPrefab, screenPos, Quaternion.identity);
            go.transform.SetParent(canvas.transform, false);
            go.transform.position = screenPos;
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
