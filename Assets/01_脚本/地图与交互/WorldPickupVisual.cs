using UnityEngine;

public sealed class WorldPickupVisual : MonoBehaviour
{
    private Transform billboard;
    public void Initialize(Transform icon) => billboard = icon;

    private void LateUpdate()
    {
        if (billboard == null || Camera.main == null)
            return;
        billboard.rotation = Camera.main.transform.rotation;
        billboard.localPosition = Vector3.up * (0.42f + Mathf.Sin(Time.time * 3f) * 0.06f);
    }
}
