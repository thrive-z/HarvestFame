using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 玩家移动脚本
/// </summary>
public class PlayerMovement : MonoBehaviour
{

    public float moveSpeed = 5f; //移动速度
    public float sprintSpeed = 8f; //疾跑状态速度
    public Rigidbody _rb;
    public bool  canMove = true; //可以移动

    public PersonCamera camScript; //第三人称相机脚本

    public float jumpFore = 30f; //跳跃力度

    public bool isGrounded = true; //玩家是否在地上
    public float rotationSpeed = 10f;//角色转向速度

    public Animator _anim;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        camScript = Camera.main.GetComponent<PersonCamera>();
        capsule = GetComponent<CapsuleCollider>();
        groundLayer = LayerMask.GetMask("Ground");

        _anim = GetComponentInChildren<Animator>();                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    
    }

    // Start is called before the first frame update
    void Start()
    {
        _rb.freezeRotation = true; //冻结刚体旋转
    }

    // Update is called once per frame
    void Update()
    {
        MoveUpdate();
        JumpUpdate();

    }

    private void JumpUpdate()
    {
        //如果在地面上 并且按下了空格
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            _rb.AddForce(Vector3.up * jumpFore,ForceMode.Impulse);
            isGrounded = false; //不在地面上
            //添加跳跃动画
            _anim.SetTrigger("jump");

            //添加音效
        }
    }

    //移动函数
    private void MoveUpdate()
    {
        if (!canMove)
        {
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        //相机的正前方
        Vector3 camForward = camScript.GetCameraForward();
        //相机的正右方
        Vector3 camRight = Quaternion .Euler(x:0, y:90f, z:0)* camForward;

        //朝相机方向移动
        Vector3 moveDir =(camRight * h + camForward * v).normalized;

        


        //获取移动速度
        float speed = Input.GetKey(KeyCode.LeftShift)
            ? sprintSpeed: moveSpeed;

        //刚体移动
        _rb.velocity = moveDir * speed;

        //如果人物有移动
        if (moveDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            //通知动画器 速度
            _anim.SetFloat("speed", 0.5f);
        }
        else
        {
            _anim.SetFloat("speed", 0f);
        }

    }



    private void FixedUpdate()
    {
        CheckGround();
    }

    public float groundCheckDistance = 0.10f; //检测高度
    public CapsuleCollider capsule; //玩家碰撞体
    public LayerMask groundLayer;

    //检查是否在地上
    void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * (groundCheckDistance);

        //画一个球检测是否碰到地面
        isGrounded = Physics.CheckSphere(
             origin, capsule.radius * 0.95f,
            groundLayer);

        //通知动画器
        _anim.SetBool("isGround",isGrounded);
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = isGrounded ? Color.green : Color.yellow;

        Vector3 origin = transform.position + Vector3.up * (groundCheckDistance);
        Gizmos.DrawWireSphere(origin, capsule.radius * 0.95f);

    }
}
