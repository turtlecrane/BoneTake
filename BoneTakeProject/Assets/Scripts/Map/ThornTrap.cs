using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ThornTrap : MonoBehaviour
{
    public bool isWide = false;
    [DrawIf("isWide", true)] 
    public Image fadePanel;
    [DrawIf("isWide", true)] 
    public Vector3 switchPosition;
    
    private Transform player;
    
    private void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerHitHandler hitHandler = other.gameObject.GetComponent<PlayerHitHandler>();
            hitHandler.Player_ApplyDamage(1, false, (player.position - transform.position).x >= 0);
            if (isWide)
            {
                GameManager.Instance.GetDevSetting().Dev_WorldTime = 0f;
                fadePanel.gameObject.SetActive(true);
                fadePanel.DOFade(1, 0.5f).SetUpdate(UpdateType.Normal, true).OnComplete(() =>
                {
                    StartCoroutine(RePosition(player));
                });
            }
        }
    }

    private IEnumerator RePosition(Transform target)
    {
        target.position = switchPosition;
        yield return new WaitForSecondsRealtime(0.5f);
        GameManager.Instance.GetDevSetting().Dev_WorldTime = 1f;
        fadePanel.DOFade(0, 0.5f).SetUpdate(UpdateType.Normal, true).OnComplete(() =>
        {
            fadePanel.gameObject.SetActive(false);
        });
    }
}
