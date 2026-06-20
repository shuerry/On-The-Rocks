using UnityEngine;

public class AutoHide : MonoBehaviour
{
    public float visibleTime = 5f;

    private void OnEnable()
    {
        Invoke(nameof(Hide), visibleTime);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}