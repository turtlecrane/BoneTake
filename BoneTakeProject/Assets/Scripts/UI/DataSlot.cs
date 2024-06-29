using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DataSlot : MonoBehaviour
{
    public bool isDataExists;
 
    public TMP_Text playerName;
    //public TMP_Text mapName;
    public TMP_Text playTime;
    public Transform LifePointTransform;
    public Image weaponIcon;
    public GameObject newSlot;
    public GameObject dataContent;
    public List<GameObject> lifePoints;

    private Image slotImage;

    private void Start()
    {
        slotImage = gameObject.GetComponent<Image>();
    }

    void Update()
    {
        dataContent.SetActive(isDataExists);
        newSlot.SetActive(!isDataExists);
        if (isDataExists)
        {
            var color = slotImage.color;
            color.a = 1f;
            slotImage.color = color;
        }
        else
        {
            var color = slotImage.color;
            color.a = 0.2f;
            slotImage.color = color;
        }
    }
}
