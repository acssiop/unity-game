using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Text = UnityEngine.UI.Text;

public class CardUI : MonoBehaviour
{
    public Image icon;          // 可选
    public Text nameText;
    public Text descText;
    public Text costText;
    public Button buyButton;    // 自身 Button 拖到这里

    private UpgradeData data;
    private ShopManager shopManager;

    public void Setup(UpgradeData cardData, ShopManager manager)
    {
        data = cardData;
        shopManager = manager;

        nameText.text = cardData.upgradeName;
        descText.text = cardData.description;
        costText.text = $"{cardData.baseCost} 金币";

        if (icon != null && cardData.icon != null)
            icon.sprite = cardData.icon;

        buyButton.onClick.AddListener(() => shopManager.PurchaseCard(this, data));

        UpdateInteractable();
    }

    public void UpdateInteractable()
    {
        bool canAfford = PlayerGold.Instance != null && PlayerGold.Instance.currentGold >= data.baseCost;
        buyButton.interactable = canAfford;
    }
}