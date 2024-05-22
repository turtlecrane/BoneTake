using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public Transform SpawnPoint;
    public GameObject playerSystemPrefab;
    private GameObject player;

    private void Awake()
    {
        if (!GameObject.FindWithTag("Player"))
        {
            Debug.Log("플레이어가 존재하지않습니다. 플레이어를 생성합니다.");
            player = Instantiate(playerSystemPrefab, SpawnPoint.position, SpawnPoint.rotation);
        }
        else
        {
            Debug.Log("플레이어가 이미 존재합니다.");
            return;
        }
    }
}
