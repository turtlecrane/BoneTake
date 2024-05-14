using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFollower : MonoBehaviour
{
    public Transform target; // 따라갈 대상
    //public Collider2D confineArea; // 경계를 나타내는 Collider2D
    public float followSpeed = 2f; // 따라가는 속도

    void Update()
    {
        // 오브젝트의 현재 위치를 저장
        //lastValidPosition = transform.position;

        // 타겟을 향해 오브젝트를 이동시킴
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z); // Z축은 변경하지 않음
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}
