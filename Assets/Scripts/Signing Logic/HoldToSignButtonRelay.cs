using UnityEngine;
using UnityEngine.EventSystems;

public class HoldToSignButtonRelay : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Place Singer here!")]
    public SignerOrTyper signer;  // drag your SignerOrTyper here in the Inspector

    public void OnPointerDown(PointerEventData eventData) { signer?.BeginMobileSign(); }
    public void OnPointerUp(PointerEventData eventData)   { signer?.EndMobileSign();   }
}
