using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 애니메이션에서 호출되는 이벤트 함수 모음
/// </summary>
public class PlayerEventKey : MonoBehaviour
{
    /// <summary>
    /// 기본 공격의 데미지를 주는 함수
    /// </summary>
    public void Player_DoBasicDamege()
    {
        float xOffset = GameManager.Instance.GetCharacterController2D().m_FacingRight ? 2.125f : -2.125f;
        Collider2D[] basicHitBox = Physics2D.OverlapBoxAll(new Vector2(transform.position.x + xOffset, transform.position.y), new Vector2(2.25f, 2f), 0f);
        
        for (int i = 0; i < basicHitBox.Length; i++)
        {
            if (basicHitBox[i].gameObject != null && basicHitBox[i].CompareTag("Enemy"))
            {
                //해당 오브젝트의 상태 스크립트에 접근해서 HP를 깎아야함.
                //HP를 줄이는건 0+데이터ATK로 깎는다.
                //0인이유는 기본공격이라서. 다른 무기들은 도끼) 3+ATK 이런식이다
                basicHitBox[i].gameObject.SendMessage("ApplyDamage", 0+GameManager.Instance.GetCharacterController2D().playerdata.playerATK);
            }
        }
    }
    
    /// <summary>
    /// 플레이어가 이동 가능하도록 함
    /// </summary>
    public void EnablePlayerMovement()
    {
        GameManager.Instance.GetCharacterController2D().canMove = true;
    }
    
    /// <summary>
    /// 플레이어를 조작불가 상태로 만듬
    /// </summary>
    public void DisablePlayerMovement()
    {
        GameManager.Instance.GetCharacterController2D().canMove = false;
        GameManager.Instance.GetCharacterController2D().m_Rigidbody2D.velocity = Vector2.zero;
    }
    
    //착지했을때 착지애니메이션 이 종료되면 착지상태를 false 상태로 만듬
    public void ExitPlayerLanding()
    {
        GameManager.Instance.GetCharacterController2D().isLanding = false;
        GameManager.Instance.GetCharacterController2D().isBigLanding = false;
    }
}
