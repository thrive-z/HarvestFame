using System;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public IInteractable current; // 当前的可用交互接口
    public Camera playerCamera; //主相机
    public float interactDistance = 8f; //交互距离 

    private void Awake()
    {
        playerCamera = Camera.main;
    }

    private void Update()
    {
        FindInteractable();
        HandleInput();
    }

    //检测是否按下交互按键
    private void HandleInput()
    {
        if (current == null)
            return;

        // 按 F 交互
        if (Input.GetKeyDown(KeyCode.F))
        {
            current.Interact(gameObject);
        }
    }

    //每帧查找是否有交互接口
    private void FindInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                current = interactable;
                InteractionUI.Instance.Open(current.GetInteractTip());
            }
            else
            {
                InteractionUI.Instance.Close();
                current = null;
            }
        }
        //【重要修复】没射到任何东西时，必须清空
        else
        {
            InteractionUI.Instance.Close();
            current = null;
        }
    }

    private void OnDrawGizmos()
    {
        if (playerCamera == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactDistance);
    }
}