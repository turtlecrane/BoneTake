using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boreph : MonoBehaviour
{
    public ImageChangeObject imageChangeObject;

    public void OpenDoor()
    {
        //문열리는 오디오 재생
        
        //이미지 변화시키기
        imageChangeObject.ChangeImage();
    }
}
