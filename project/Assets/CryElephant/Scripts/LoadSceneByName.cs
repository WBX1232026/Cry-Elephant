using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Page navigation: plays the click sound and loads the target scene. Attached to the Next / Start / Back to Home buttons.
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
