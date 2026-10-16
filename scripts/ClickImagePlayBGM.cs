using UnityEngine;
using UnityEngine.EventSystems;

public class ClickImagePlayBGM : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private AudioClip bgmClip;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (BgmManager.Instance != null)
            BgmManager.Instance.Play(bgmClip);

        gameObject.SetActive(false);
    }
}
