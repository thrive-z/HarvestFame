using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonCamera : MonoBehaviour
{

    public Transform playerTarget; //玩家对象

    public float verticalOffset = 2f; //抬高相机高度

    public float yaw; //鼠标水平方向移动
    public float pitch; //鼠标垂直方向移动

    public float xSensitivity = 180f; //鼠标水平灵敏度
    public float ySensitivity = 120f; //鼠标垂直灵敏度

    public float minY = -20f; //俯角限制
    public float maxY = 60f; //仰角限制

    public float distance = 5.0f;//默认距离
    public float minDistance = 2.0f;//最近距离
    public float maxDistance = 20.0f; //最远距离
    public float scrollSpeed = 2.0f; //鼠标滚轮缩放速度

    public float smoothTime = 0.1f; //平滑移动
    public Vector3  currentVelocity;

    private void Awake()
    {
        playerTarget = GameObject.Find ("Player"). transform;
    }
    // Start is called before the first frame update
    void Start()
    {
        
        //鼠标锁定和隐藏
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {

    }



    // 先写个安全判断，没有这个轴就返回0
public static float SafeGetAxis(string axisName)
    {
        try
        {
            return Input.GetAxis(axisName);
        }
        catch
        {
            return 0;
        }
    }



    //相机移动必须写在late里
    private readonly RaycastHit[] cameraHits = new RaycastHit[24];

    private void LateUpdate()
    {

        //水平方向鼠标偏移量
        if (AgricultureSystem.WorldInputEnabled)
        {
            yaw += Input.GetAxis("Mouse X") * xSensitivity * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * ySensitivity * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minY, maxY);
            distance = Mathf.Clamp(distance - SafeGetAxis("Mouse ScrollWheel") * scrollSpeed, minDistance, maxDistance);
        }




        Quaternion rotation = Quaternion.Euler(x:pitch, y: yaw, z:0);


        //玩家位置加上垂直方向偏移量
        Vector3 targetPos = playerTarget.position + Vector3.up * verticalOffset;

        //加上鼠标旋转 和 滚轮缩放的结果
        var backwards = rotation * Vector3.back;
        var wantedDistance = distance;
        var mask = Physics.DefaultRaycastLayers & ~LayerMask.GetMask("UI");
        var count = Physics.SphereCastNonAlloc(targetPos, 0.18f, backwards, cameraHits,
            distance, mask, QueryTriggerInteraction.Ignore);
        for (var i = 0; i < count; i++)
        {
            var hit = cameraHits[i];
            if (hit.collider.transform.IsChildOf(playerTarget)
                || hit.collider.GetComponentInParent<FarmTractor>()?.IsOccupied == true)
                continue;
            wantedDistance = Mathf.Min(wantedDistance, Mathf.Max(0.55f, hit.distance - 0.12f));
        }
        Vector3 desiredPostion = targetPos + backwards * wantedDistance;

        //实现平滑移动
        if (wantedDistance < distance)
        {
            transform.position = desiredPostion;
            currentVelocity = Vector3.zero;
        }
        else
            transform.position = Vector3.SmoothDamp(
            current: transform.position,
            target: desiredPostion,
            ref currentVelocity,
            smoothTime
            );
            
          
        transform.LookAt(targetPos);

    }

    public Vector3 GetCameraForward()
    {
       Vector3 forward =  transform.forward;
        forward .y = 0;//归零y轴偏移量 不然人会飞起来
        return forward.normalized ;
    }
}
