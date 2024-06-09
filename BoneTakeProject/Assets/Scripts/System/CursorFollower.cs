using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorFollower : MonoBehaviour
{
    void Update()
    {
        if (Camera.main != null)
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            // 스크린 좌표를 월드 좌표로 변환 (2D)
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            mouseWorldPosition.z = 0;

            // GameObject의 위치를 마우스 위치로 업데이트
            transform.position = mouseWorldPosition;
        }
    }
}
