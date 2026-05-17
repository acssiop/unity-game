using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using Text = UnityEngine.UI.Text;
using Debug = UnityEngine.Debug;

public class ExpBarUI : MonoBehaviour
{
    private Slider slider;
    public Text expText;   // 拖入刚才创建的文本（没有可留空）

    private void Awake()
    {
        slider = GetComponent<Slider>();
        if (slider == null)
        {
            Debug.LogError("ExpBarUI 必须挂在 Slider 上！");
            return;
        }
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.wholeNumbers = false;
    }

    private void Start()
    {
        if (PlayerExperience.Instance != null)
        {
            PlayerExperience.Instance.OnXPChanged += UpdateBar;
            UpdateBar(PlayerExperience.Instance.currentXP);
        }
        else
        {
            Debug.LogError("ExpBarUI 找不到 PlayerExperience.Instance！");
        }
    }

    private void UpdateBar(int currentXP)
    {
        if (PlayerExperience.Instance == null) return;

        float targetXP = PlayerExperience.Instance.xpToNextLevel;
        float percent = targetXP > 0 ? currentXP / targetXP : 0;
        slider.value = percent;

        if (expText != null)
        {
            expText.text = $"{currentXP} / {targetXP}";
        }
    }

    private void OnDestroy()
    {
        if (PlayerExperience.Instance != null)
            PlayerExperience.Instance.OnXPChanged -= UpdateBar;
    }
}