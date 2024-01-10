using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;
    public Animator animator;

    public float runSpeed = 40f;

    float horizontalMove = 0f;
    bool jump = false;
    bool dash = false;
    
    void Update () {
        
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        
        //이동 속도에 따른 뛰는(Player_Running)애니메이션 조절
        animator.SetFloat("Speed", Mathf.Abs(horizontalMove)); 

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
        {
            jump = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            dash = true;
        }
    }
    
    void FixedUpdate ()
    {
        //움직이기
        controller.Move(horizontalMove * Time.fixedDeltaTime, jump, dash);
        jump = false;
        dash = false;
    }

    public void OnFall()
    {
        Debug.Log($"{GetType().Name} >> OnFall()\n애니메이션 >> IsJumping - true\n-----------------------");
        //animator.SetBool("IsJumping", true);
    }

    /// <summary>
    /// 점프 하고 착지할때 CharacterController2D에서 호출됨
    /// </summary>
    public void OnLanding()
    {
        Debug.Log($"{GetType().Name} >> OnFall()\n애니메이션 >> IsJumping - fasle\n-----------------------");
        //animator.SetBool("IsJumping", false);
    }
}
