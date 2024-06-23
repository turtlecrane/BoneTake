using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss00_EventKeyController : MonoBehaviour
{
    public Boss00_Attack bossAttack;
    public BossHitHandler bossHitHandler;
    public Vector2 hitBoxSize; //공격이 맞는 히트박스의 크기 조절
    public float hitBoxOffset_X;//공격 X축 반경을 조절
    public float hitBoxOffset_Y; //공격 Y축 반경을 조절

    public void Boss00_CloseAttack()
    {
        StartCoroutine(bossAttack.Boss00_Attack01());
    }
    
    public void Boss00_DashAttack()
    {
        StartCoroutine(bossAttack.DashAttack());
    }

    public void Boss00_FarAttack()
    {
        bossAttack.FarAttack_Shoot();
    }

    public void Boss00_ActiveDamage()
    {
        bossAttack.Boss00_DoDamage();
    }

    public void Boss00_PlayBloodParticle()
    {
        bossHitHandler.PlayBloodParticle();
    }
    
    public void Boss00_PlayDustParticle()
    {
        bossAttack.PlayDustParticle();
    }

    public void Boss00_PlayBombAudio()
    {
        AudioManager.instance.PlaySFX("Bomb", Random.Range(0, 3));
    }
    
    private void OnDrawGizmosSelected()
    {
        //히트박스 에디터 상에서 표시
        float xOffset = bossAttack.facingRight ? -1 : 1;
        Vector2 hitboxCenter = new Vector2(transform.position.x + (xOffset * hitBoxOffset_X), transform.position.y + hitBoxOffset_Y);
        
        Gizmos.color = Color.cyan;
        
        Gizmos.DrawWireCube(hitboxCenter, hitBoxSize);
    }

}
