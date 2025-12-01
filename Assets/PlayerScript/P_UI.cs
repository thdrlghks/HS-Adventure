using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class P_UI : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider hpSlider;
    public Slider staminaSlider;

    [Header("Death UI")]
    public GameObject deathPanel;

    public void UpdateHealth(int current, int max)
    {
        if (hpSlider != null)
            hpSlider.value = (float)current / max;
    }

    public void UpdateStamina(float current, float max)
    {
        if (staminaSlider != null)
            staminaSlider.value = current / max;
    }

    public void ShowDeathPanel()
    {
        if (deathPanel != null)
            deathPanel.SetActive(true);
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}