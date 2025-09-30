using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float moveUpSpeed = 50f;
    public float duration = 1f;
    public Vector3 moveOffset = new Vector3(0, 50, 0);

    private float timer;
    private Vector3 startPos;

    public void Setup(string value, Color color)
    {
        text.text = value;
        text.color = color;
        timer = 0f;
        startPos = transform.position;
    }

    void Update()
    {
        timer += Time.deltaTime;
        // Move up
        float t = timer / duration;
        transform.position = startPos + moveOffset * t;
        // Fade out
        var c = text.color;
        c.a = Mathf.Lerp(1, 0, t);
        text.color = c;

        if (timer >= duration)
        {
            if (ObjectPooling.Instance != null)
                ObjectPooling.Instance.ReturnToPool(gameObject);
            else
                Destroy(gameObject);
        }
    }
}
