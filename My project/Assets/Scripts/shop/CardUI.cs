using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using Text = UnityEngine.UI.Text;
using Image = UnityEngine.UI.Image;

public class CardUI : MonoBehaviour
{
    public Image icon;          // 可选（卡片图标）
    public Text nameText;
    public Text descText;
    public Text costText;
    public Button buyButton;    // 卡片自身的按钮组件

    private UpgradeData data;
    private ShopManager shopManager;

    /// <summary>
    /// 初始化卡片显示
    /// </summary>
    /// <param name="cardData">卡片数据</param>
    /// <param name="manager">商店管理器（若为 null 则表示用于升级面板，免费）</param>
    public void Setup(UpgradeData cardData, ShopManager manager)
    {
        data = cardData;
        shopManager = manager;

        // 填充文字
        nameText.text = cardData.upgradeName;
        descText.text = cardData.description;

        // 价格 / 免费
        if (manager == null)
            costText.text = "免费";
        else
            costText.text = $"{cardData.baseCost} 金币";

        // 图标（如果拖入且卡片有图标）
        if (icon != null && cardData.icon != null)
            icon.sprite = cardData.icon;

        // 按钮事件绑定
        buyButton.onClick.RemoveAllListeners();
        if (manager != null)
        {
            // 商店模式：绑定购买，并根据金币设置交互
            buyButton.onClick.AddListener(() => manager.PurchaseCard(this, data));
            UpdateInteractable();   // 立刻检查金币是否足够
        }
        else
        {
            // 升级模式：按钮始终可点，购买回调由外部（LevelUpManager）绑定
            buyButton.interactable = true;
        }
    }

    /// <summary>
    /// 根据当前金币数量刷新按钮的可点击状态（仅在商店模式有效）
    /// </summary>
    public void UpdateInteractable()
    {
        if (shopManager != null && data != null)
        {
            bool canAfford = PlayerGold.Instance != null && PlayerGold.Instance.currentGold >= data.baseCost;
            buyButton.interactable = canAfford;
        }
    }
}