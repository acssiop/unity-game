using UnityEngine;
using UnityEngine.UI;
using Text = UnityEngine.UI.Text;

public class GoldUI : MonoBehaviour
{
    private Text text;
    private void Awake() => text = GetComponent<Text>();

    private void Start()
    {
        if (PlayerGold.Instance != null)
        {
            PlayerGold.Instance.OnGoldChanged += UpdateGold;
            UpdateGold(PlayerGold.Instance.currentGold);
        }
    }

    private void UpdateGold(int gold) => text.text = $"½ð±Ò: {gold}";

    private void OnDestroy()
    {
        if (PlayerGold.Instance != null)
            PlayerGold.Instance.OnGoldChanged -= UpdateGold;
    }
}