using UnityEngine;
using UnityEngine.EventSystems;

public class TogglePopup : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject popup;

    private static GameObject currentPopup;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (popup == null) return;

        // 点击的是同一个弹窗 → 切回隐藏
        if (popup.activeSelf)
        {
            popup.SetActive(false);
            currentPopup = null;
            return;
        }

        // 隐藏上一个弹窗
        if (currentPopup != null)
            currentPopup.SetActive(false);

        // 显示当前弹窗
        popup.SetActive(true);
        currentPopup = popup;
    }
}
