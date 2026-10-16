using DG.Tweening;
using UnityEngine;

/// <summary>
/// 泪滴动画：从发射点持续生成泪滴并向下掉落（Page 3）。
/// </summary>
public class TearDrops : MonoBehaviour
{
    [SerializeField] private RectTransform template;   // 泪滴模板（默认隐藏）
    [SerializeField] private Vector2 spawnOffset = new Vector2(0f, -40f);
    [SerializeField] private float interval = 0.7f;
    [SerializeField] private float fallDistance = 260f;
    [SerializeField] private float fallDuration = 1.2f;
    [SerializeField] private int maxAlive = 6;

    private RectTransform self;
    private float timer;
    private int alive;

    private void Awake()
    {
        self = GetComponent<RectTransform>();
        if (template != null) template.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        timer = interval;
        alive = 0;
    }

    private void Update()
    {
        if (template == null) return;
        if (alive >= maxAlive) return;

        timer += Time.deltaTime;
        if (timer < interval) return;

        timer = 0f;
        Spawn();
    }

    private void Spawn()
    {
        RectTransform tear = Instantiate(template, self);
        tear.gameObject.SetActive(true);
        tear.localScale = Vector3.one;
        tear.anchoredPosition = self.anchoredPosition + spawnOffset;
        tear.SetAsFirstSibling();

        alive++;
        tear.DOAnchorPos(tear.anchoredPosition + new Vector2(0f, -fallDistance), fallDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                alive--;
                if (tear != null) Destroy(tear.gameObject);
            });
    }
}
