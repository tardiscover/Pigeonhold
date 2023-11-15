using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FoesLeftBar : MonoBehaviour
{
    public Slider foesLeftSlider;
    public TMP_Text foesLeftBarText;

    private float CalculateSliderPercentage(float currentFoesLeft, float maxFoesLeft)
    {
        if (maxFoesLeft == 0)
        {
            return 0;
        }

        return currentFoesLeft / maxFoesLeft;
    }

    public void SetFoesLeft(int newFoesLeft, int maxFoesLeft)
    {
        foesLeftSlider.value = CalculateSliderPercentage(newFoesLeft, maxFoesLeft);
        foesLeftBarText.text = $"Foes Left {newFoesLeft} / {maxFoesLeft}";
    }
}
