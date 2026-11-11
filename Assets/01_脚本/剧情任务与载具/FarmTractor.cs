using System.Linq;
using UnityEngine;

public sealed class FarmTractor : MonoBehaviour, IInteractable
{
    [SerializeField] private string displayName = "拖拉机";
    [SerializeField] private float topSpeed = 8f;
    [SerializeField] private bool hideDriver;
    [SerializeField] private Transform seat;
    [SerializeField] private Transform model;
    [SerializeField] private BoxCollider chassis;
    private FarmCampaign campaign;
    private Transform player;
    private PlayerMovement movement;
    private Rigidbody playerBody;
    private CapsuleCollider playerCollider;
    private Renderer[] driverRenderers;
    private Wheel[] wheels;
    private float wheelAngle;
    private float speed;
    private float throttle;
    private float steering;
    private bool brake;
    private int lastInteractionFrame = -1;

    public bool IsOccupied => player != null;
    public string DisplayName => displayName;
    public float SpeedKph => Mathf.Abs(speed) * 3.6f;

    public void Initialize(FarmCampaign owner)
    {
        campaign = owner;
        wheels = model.GetComponentsInChildren<Transform>()
            .Where(child => child.name.Contains("_Wheel_"))
            .Select(child => new Wheel(child))
            .ToArray();
    }

    public bool CanInteract() => campaign != null && campaign.IsRunning;
    public string GetInteractTip() => campaign.DrivingUnlocked ? $"F · 驾驶{displayName}" : $"{displayName} · 请先找罗德";

    public void Interact(GameObject interactor)
    {
        if (!campaign.DrivingUnlocked)
        {
            campaign.Notify("和罗德交谈后才能开车。");
            return;
        }
        if (IsOccupied || lastInteractionFrame == Time.frameCount)
            return;
        lastInteractionFrame = Time.frameCount;

        player = interactor.transform;
        movement = interactor.GetComponent<PlayerMovement>();
        playerBody = interactor.GetComponent<Rigidbody>();
        playerCollider = interactor.GetComponent<CapsuleCollider>();
        movement.enabled = false;
        playerBody.velocity = Vector3.zero;
        playerBody.isKinematic = true;
        playerCollider.enabled = false;
        if (hideDriver)
        {
            driverRenderers = interactor.GetComponentsInChildren<Renderer>().Where(renderer => renderer.enabled).ToArray();
            foreach (var renderer in driverRenderers)
                renderer.enabled = false;
        }
        campaign.SetDriving(this);
        PlaceDriver();
    }

    private void Update()
    {
        if (!IsOccupied)
            return;

        if (lastInteractionFrame != Time.frameCount && Input.GetKeyDown(KeyCode.F) && AgricultureSystem.WorldInputEnabled)
        {
            if (Mathf.Abs(speed) > 2f)
                campaign.Notify("先刹车，再下车。");
            else
            {
                Dismount();
                return;
            }
        }

        var controls = AgricultureSystem.WorldInputEnabled;
        throttle = controls ? Input.GetAxisRaw("Vertical") : 0f;
        steering = controls ? Input.GetAxisRaw("Horizontal") : 0f;
        brake = !controls || Input.GetKey(KeyCode.Space);
    }

    private void FixedUpdate()
    {
        if (!IsOccupied)
            return;

        Simulate(throttle, steering, brake, Time.fixedDeltaTime);
        PlaceDriver();
        campaign.CheckRoute(transform.position);
    }

    private void Simulate(float gas, float turn, bool braking, float dt)
    {
        var target = braking ? 0f : gas > 0f ? topSpeed : gas < 0f ? -3.5f : 0f;
        var rate = braking ? 17f : Mathf.Abs(gas) < 0.01f ? 4.5f : 6f;
        if (speed * target < 0f)
        {
            target = 0f;
            rate = 10f;
        }

        speed = Mathf.MoveTowards(speed, target, rate * dt);
        var currentRotation = transform.rotation;
        var turnRate = 68f * Mathf.Clamp01(Mathf.Abs(speed) / 2f);
        var nextRotation = currentRotation * Quaternion.Euler(0f, turn * turnRate * Mathf.Sign(speed) * dt, 0f);
        var displacement = nextRotation * Vector3.forward * (speed * dt);
        var next = transform.position + displacement;
        if (displacement.sqrMagnitude > 0.00001f && !CanMove(next, nextRotation))
        {
            speed = 0f;
            return;
        }

        transform.SetPositionAndRotation(next, nextRotation);
        model.localRotation = Quaternion.Euler(0f, 0f, -turn * Mathf.Abs(speed) * 0.3f);
        wheelAngle = Mathf.Repeat(wheelAngle + speed * dt * Mathf.Rad2Deg / 0.38f, 360f);
        foreach (var wheel in wheels)
        {
            var steer = Quaternion.Euler(0f, wheel.front ? turn * 24f : 0f, 0f);
            wheel.transform.localRotation = wheel.rest * steer * Quaternion.Euler(wheelAngle, 0f, 0f);
        }
    }

    private sealed class Wheel
    {
        public readonly Transform transform;
        public readonly Quaternion rest;
        public readonly bool front;

        public Wheel(Transform wheel)
        {
            transform = wheel;
            rest = wheel.localRotation;
            front = wheel.name.EndsWith("_fl") || wheel.name.EndsWith("_fr");
        }
    }

    private bool CanMove(Vector3 next, Quaternion rotation)
    {
        var ground = LayerMask.GetMask("Ground");
        var front = rotation * Vector3.forward * (Mathf.Sign(speed) * chassis.size.z * 0.48f);
        if (!Physics.Raycast(next + front + Vector3.up, Vector3.down, 1.6f, ground))
            return false;

        var center = next + rotation * chassis.center;
        var solid = Physics.DefaultRaycastLayers & ~ground;
        foreach (var obstacle in Physics.OverlapBox(center, chassis.size * 0.48f, rotation, solid, QueryTriggerInteraction.Ignore))
        {
            if (!obstacle.transform.IsChildOf(transform) && !obstacle.transform.IsChildOf(player))
                return false;
        }
        return true;
    }

    private void PlaceDriver() => player.SetPositionAndRotation(seat.position, transform.rotation);

    public void Dismount()
    {
        if (!IsOccupied)
            return;

        var destination = transform.position + transform.right * 1.9f;
        var ground = LayerMask.GetMask("Ground");
        if (!Physics.Raycast(destination + Vector3.up, Vector3.down, out var hit, 2f, ground))
        {
            destination = transform.position - transform.right * 1.9f;
            if (Physics.Raycast(destination + Vector3.up, Vector3.down, out hit, 2f, ground))
                destination = hit.point;
        }
        else
            destination = hit.point;

        player.position = destination + Vector3.up * 0.06f;
        playerCollider.enabled = true;
        playerBody.isKinematic = false;
        movement.enabled = true;
        player = null;
        lastInteractionFrame = Time.frameCount;
        speed = 0f;
        if (driverRenderers != null)
        {
            foreach (var renderer in driverRenderers)
                renderer.enabled = true;
            driverRenderers = null;
        }
        campaign.SetDriving(null);
        Physics.SyncTransforms();
    }
}
