using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 页面跳转：播放点击音效并加载目标场景。挂在 Next / Start / Back to Home 按钮上。
/// </summary>
public class LoadSceneByName : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private AudioClip clickSound;

    public void LoadScene()
    {
        if (Sfx.Instance != null) Sfx.Instance.Play(clickSound);

        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name is empty! Please set the scene name in the Inspector.");
        }
    }
}
