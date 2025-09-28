using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Health Bar")]
    public Image hpLineImage; // Kéo HpLine vào đây trong Inspector

    // Gọi hàm này khi máu thay đổi
    public void SetHealth(float current, float max)
    {
        if (hpLineImage != null && max > 0)
            hpLineImage.fillAmount = Mathf.Clamp01(current / max);
    }
}
