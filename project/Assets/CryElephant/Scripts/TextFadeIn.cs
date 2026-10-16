using UnityEngine;
using TMPro;

public class TextFadeIn : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;

    private TMP_Text text;
    private float timer;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        timer = 0f;
        Color color = text.color;
        color.a = 0f;
        text.color = color;
    }

    private void Update()
    {
        if (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            Color color = text.color;
            color.a = alpha;
            text.color = color;
        }
    }
}
