using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorFollower : MonoBehaviour
{
    /*public Camera mainCamera; // 메인 카메라를 에디터에서 할당해야 합니다.

    private void Start()
    {
        mainCamera = Camera.main;
    }*/

    void Update()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        // 스크린 좌표를 월드 좌표로 변환 (2D)
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        // Z 좌표는 무시합니다 (2D 게임이므로)
        mouseWorldPosition.z = 0;

        // GameObject의 위치를 마우스 위치로 업데이트합니다.
        transform.position = mouseWorldPosition;
    }
}
