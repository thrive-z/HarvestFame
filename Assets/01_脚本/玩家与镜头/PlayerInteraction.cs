using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public IInteractable current;
    public Camera playerCamera;
    public float interactDistance = 8f;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Update()
    {
        current = null;
        if (AgricultureSystem.WorldInputEnabled && FarmCampaign.Instance?.IsDriving != true && TryGetTarget(out var hit))
            current = hit.collider.GetComponentInParent<IInteractable>();

        if (current == null)
        {
            InteractionUI.Instance?.Close();
            return;
        }

        InteractionUI.Instance?.Open(current.GetInteractTip());
        if (Input.GetKeyDown(KeyCode.F) && current.CanInteract())
            current.Interact(gameObject);
    }

    public bool TryGetTarget(out RaycastHit hit, float reach = 0f)
    {
        hit = default;
        if (playerCamera == null)
            return false;

        if (reach <= 0f)
            reach = interactDistance;
        var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        var viewDistance = Vector3.Distance(ray.origin, transform.position) + reach;
        var hits = Physics.RaycastAll(ray, viewDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        var closest = float.PositiveInfinity;
        foreach (var candidate in hits)
        {
            if (candidate.collider.transform.IsChildOf(transform) || candidate.distance >= closest)
                continue;

            hit = candidate;
            closest = candidate.distance;
        }

        return hit.collider != null && (hit.point - transform.position).sqrMagnitude <= reach * reach;
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCamera == null)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactDistance);
    }
}
