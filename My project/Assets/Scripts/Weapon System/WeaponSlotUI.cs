using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Text = UnityEngine.UI.Text;

public class WeaponSlotUI : MonoBehaviour
{
    // 武器信息区域（在 WeaponSlot 子物体下）
    public Image weaponIcon;
    public Text weaponNameText;
    public Text weaponStatsText;

    // 出售按钮与价格（在 SellButton 子物体下）
    public Button sellButton;
    public Text sellButtonText;      // 按钮上的文本（如果不需要改文字可省略）
    public Text sellPriceText;       // 显示售价的文本

    private WeaponData weaponData;
    private ShopManager shopManager;

    public void Setup(WeaponData data, ShopManager manager)
    {
        weaponData = data;
        shopManager = manager;

        weaponNameText.text = data.weaponName;
        weaponStatsText.text = $"品质: {data.quality}  伤害 x{data.damageMultiplier:F1}";
        sellPriceText.text = $"{data.basePrice / 2} 金";

        // 按钮事件绑定
        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(() => shopManager.SellPlayerWeapon(weaponData));

        // 如果只有一把武器，可以选择禁止出售（可选）
        // if (WeaponManager.Instance.weapons.Count <= 1)
        //     sellButton.interactable = false;
    }
}