using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;
    //public Animator animator;

    public float runSpeed = 40f;

    float horizontalMove = 0f;
    bool jump = false;
    bool dash = false;
    
    void Update () {

        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        //animator.SetFloat("Speed", Mathf.Abs(horizontalMove)); //이동 속도에 따른 애니메이션 속도 조절

        /*if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
        {
            jump = true;
        }*/
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jump = true;
            Debug.Log($"{GetType().Name} >> Space 키가 눌렸습니다. \n-----------------------");
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            jump = true;
            Debug.Log($"{GetType().Name} >> W 키가 눌렸습니다. \n-----------------------");
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
