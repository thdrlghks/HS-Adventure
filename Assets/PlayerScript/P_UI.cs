using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class P_UI : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider hpSlider;
    public Slider staminaSlider; // 🔥 [추가] 스테미너 슬라이더
    [Header("Death UI")]
    public GameObject deathPanel;
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

    public void ShowDeathPanel()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }
    }
    public void RestartGame()
    {
        // 시간 정지 해제 (만약 멈췄었다면)
        Time.timeScale = 1f;

        // 현재 씬을 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}