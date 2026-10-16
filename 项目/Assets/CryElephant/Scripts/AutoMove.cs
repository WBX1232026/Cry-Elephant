using UnityEngine;

public class AutoMove : MonoBehaviour
{
    [SerializeField] private Vector2 direction = Vector2.right;
    [SerializeField] private float distance = 200f;
    [SerializeField] private float duration = 1f;

    private RectTransform rect;
    private Vector2 startPos;
    private float timer;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        startPos = rect.anchoredPosition;
        timer = 0f;
    }

    private void Update()
    {
        if (timer >= duration) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);
        rect.anchoredPosition = startPos + direction.normalized * distance * t;
    }
}
