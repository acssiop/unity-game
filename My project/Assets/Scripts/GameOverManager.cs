using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }
    public static bool IsGameOver { get; private set; } = false;
    [Tooltip("死亡面板（包含按钮）")]
    public GameObject deathPanel;
    [Tooltip("重新开始按钮")]
    public Button restartButton;
    [Tooltip("返回主菜单按钮")]
    public Button mainMenuButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 绑定按钮事件
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        // 初始隐藏面板
        if (deathPanel != null)
            deathPanel.SetActive(false);
    }

    public void ShowDeathPanel()
    {
        if (deathPanel != null)
            deathPanel.SetActive(true);
    }

    private void RestartGame()
    {
        // 重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void GoToMainMenu()
    {
        // 回到主菜单（请根据你的实际主菜单场景索引或名称修改）
        SceneManager.LoadScene(0);   // 假设主菜单是 Build Settings 中的第 0 个场景
    }
}