using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPBarUI : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text hpText;

    public void SetHP(int currentHP, int maxHP)
    {
        slider.maxValue = maxHP;
        slider.value = currentHP;

        hpText.text = "HP " + currentHP + " / " + maxHP;
    }
}