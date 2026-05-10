using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneLoader : MonoBehaviour
{
    // 将这个方法绑定到按钮的 OnClick 事件上
    public void LoadScene0()
    {
        SceneManager.LoadScene(1);
    }
}