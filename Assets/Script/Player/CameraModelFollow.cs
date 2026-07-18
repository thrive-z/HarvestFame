using System;
using UnityEngine;

/// <summary>
/// 相机模型跟随真实 Camera 的位置和旋转（用于调试可视化）
/// </summary>
public class CameraModelFollow : MonoBehaviour
{
    [Header("真实相机（通常是 MainCamera）")]
    public Transform cameraTarget;

    [Header("位置偏移（用于对齐模型）")]
    public Vector3 positionOffset;

    [Header("旋转偏移（用于修正模型朝向）")]
    public Vector3 rotationOffset;

    private void Awake()
    {
        cameraTarget = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (!cameraTarget) return;

        // 位置：完全跟随相机
        transform.position =
            cameraTarget.position
            + cameraTarget.rotation * positionOffset;

        // 旋转：完全跟随相机
        transform.rotation =
            cameraTarget.rotation * Quaternion.Euler(rotationOffset);
    }
}