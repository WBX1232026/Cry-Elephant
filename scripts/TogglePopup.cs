using UnityEngine;
using UnityEngine.EventSystems;

public class TogglePopup : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject popup;

    private static GameObject currentPopup;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (popup == null) return;

        // Clicking the popup that is already open toggles it back to hidden
        if (popup.activeSelf)
        {
            popup.SetActive(false);
            currentPopup = null;
            return;
        }

        // Hide the previous popup
        if (currentPopup != null)
            currentPopup.SetActive(false);

        // Show the current popup
        popup.SetActive(true);
        currentPopup = popup;
    }
}
