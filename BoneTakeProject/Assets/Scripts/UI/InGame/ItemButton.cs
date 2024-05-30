using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    public Image bg;
    public Image icon;
    public Weapon_Name weaponName;
    
    private void Start()
    {
        bg.DOFade(1f, 0.5f).SetUpdate(UpdateType.Normal, true).OnComplete(() =>
        {
            icon.sprite = WeaponData.instance.weaponGFXSource.freshIcon[WeaponData.instance.GetName_WeaponID(weaponName)];
            icon.DOFade(1f, 0.5f).SetUpdate(UpdateType.Normal, true);
        });
    }
}
