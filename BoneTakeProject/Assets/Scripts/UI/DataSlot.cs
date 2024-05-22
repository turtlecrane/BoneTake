using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DataSlot : MonoBehaviour
{
    public bool isDataExists;
 
    public TMP_Text playerName;
    public TMP_Text mapName;
    public TMP_Text playTime;
    public Transform LifePointTransform;
    public Image weaponIcon;
    
    public GameObject newSlot;
    public GameObject dataContent;

    public List<GameObject> lifePoints;
    
    void Update()
    {
        newSlot.SetActive(!isDataExists);
        dataContent.SetActive(isDataExists);
    }
}
