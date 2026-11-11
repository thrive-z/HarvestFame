using UnityEngine;

public sealed class FarmRotor : MonoBehaviour
{
    [SerializeField] private Vector3 axis = Vector3.forward;
    [SerializeField] private float degreesPerSecond = 36f;

    private void Update() => transform.Rotate(axis, degreesPerSecond * Time.deltaTime, Space.Self);
}
