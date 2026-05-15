using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    public Health playerHealth;
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        if (playerHealth != null)
        {
            slider.maxValue = playerHealth.maxHealth;
            slider.value = playerHealth.GetCurrentHealth();
            // ¶©ÔÄÊÂ¼þ
            playerHealth.OnHealthChanged += UpdateBar;
        }
    }

    private void UpdateBar(float currentHealth)
    {
        slider.value = currentHealth;
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateBar;
    }
}