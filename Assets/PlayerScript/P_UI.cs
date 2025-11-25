using UnityEngine;
using UnityEngine.UI;

public class P_UI : MonoBehaviour
{
    public Slider hpSlider;

    public void UpdateHealth(int current, int max)
    {
        if (hpSlider != null)
            hpSlider.value = (float)current / max;
    }
}
