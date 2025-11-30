using UnityEngine;
using UnityEngine.UI;

public class P_UI : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider hpSlider;
    public Slider staminaSlider; // 🔥 [추가] 스테미너 슬라이더

    // HP 업데이트 (int형)
    public void UpdateHealth(int current, int max)
    {
        if (hpSlider != null)
            hpSlider.value = (float)current / max;
    }

    // 🔥 [추가] 스테미너 업데이트 (float형)
    public void UpdateStamina(float current, float max)
    {
        if (staminaSlider != null)
        {
            // 현재 / 최대값 비율로 슬라이더 조절 (0.0 ~ 1.0)
            staminaSlider.value = current / max;
        }
    }
}