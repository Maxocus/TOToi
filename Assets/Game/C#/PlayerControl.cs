using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float rbDrag = 6f; // 用于控制惯性滑动

    private Vector3 movement;
    private Rigidbody rb;
    private Animator _animator;
    private bool isRun;
    private bool isSleep;
    private MakingManager makingManager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // 防止物理旋转干扰
        rb.linearDamping = rbDrag;        // 设置阻力减少滑动
        _animator= GetComponentInChildren<Animator>();
        makingManager = GetComponent<MakingManager>();
    }

    void Update()
    {
        // 输入检测仍在Update处理
        float moveX = Input.GetAxisRaw("Horizontal"); // 使用Raw更灵敏
        float moveZ = Input.GetAxisRaw("Vertical");
        
        movement = new Vector3(moveX, 0, moveZ).normalized;
    }

    void FixedUpdate()
    {
        // 物理操作在FixedUpdate执行
        if(movement != Vector3.zero)
        {
            MovePlayer();
            RotatePlayer();
            isRun = Input.GetKey(KeyCode.LeftShift)&&makingManager.GetCup()==null;
            if (isRun)
            {
                _animator.SetBool("Run", true);
                _animator.SetBool("Walk", false);

            }
            else
            {
                _animator.SetBool("Walk", true);
                _animator.SetBool("Run", false);

            }
        }
        else
        {
            _animator.SetBool("Walk", false);
            _animator.SetBool("Run", false);
        }
    }

    void MovePlayer()
    {
        // 通过力或速度移动（这里使用velocity直接控制）
        Vector3 moveVelocity = movement * moveSpeed;
        rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
    }

    void RotatePlayer()
    {
        // 使用Rigidbody的旋转方法（更平滑）
        Quaternion targetRot = Quaternion.LookRotation(movement);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
    }

    public void PlayerAni(string name,bool isActive)
    {
        _animator.SetBool(name, isActive);
    }

    public void Sleep(Transform point)
    {
        isSleep = true;
        PlayerAni("Sleep", true);
        Vector3 rot = new Vector3(-90, 180, 0);
        transform.eulerAngles = rot;
        transform.position= point.position;
    }

    public void UpSleep(Transform point)
    {
        isSleep = false;
        PlayerAni("Sleep", false);
        Vector3 rot = new Vector3(0, 0, 0);
        transform.eulerAngles = rot;
        transform.position = point.position;



    }
   
}