using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRandomSpawner : MonoBehaviour
{
    public GameObject spawnObjectPrefab;
    public float randomValue;

    void Start()
    {
        
        // 0과 1 사이의 랜덤 값을 생성
        float spawnChance = Random.Range(0f, 1f);

        // 생성된 랜덤 값이 randomValue보다 작거나 같으면 오브젝트 스폰
        if (spawnChance <= randomValue)
        {
            Instantiate(spawnObjectPrefab, transform.position, transform.rotation);
            Debug.Log("랜덤스포너 실행됨 결과 : 성공");
        }
        else
        {
            Debug.Log("랜덤스포너 실행됨 결과 : 실패");
        }
    }
}
