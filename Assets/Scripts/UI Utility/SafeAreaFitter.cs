using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour {
    Rect last;
    void OnEnable() => Apply();
    void Update()
    {
        if (Screen.safeArea != last) Apply();
    }
    void Apply()
    {
        var rt=(RectTransform)transform; var sa=Screen.safeArea; last=sa;
        Vector2 min=sa.position, max=sa.position+sa.size;
        min.x/=Screen.width; min.y/=Screen.height;
        max.x/=Screen.width; max.y/=Screen.height;
        rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=rt.offsetMax=Vector2.zero;
    }
}

