using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSelectUI : MonoBehaviour
{
    public HorizontalLayoutGroup ButtonGroup;
    public ItemButton itemButton;
    public List<GameObject> buttons;
    
    public void CreateItemSelection(Weapon_Name _weaponName, Action buttonAction = null)
    {
        Debug.Log(_weaponName.ToString());
        
        // ItemButton의 복사본을 생성하고 ButtonGroup의 자식으로 설정
        ItemButton buttonInstance = Instantiate(itemButton, ButtonGroup.transform);
        buttons.Add(buttonInstance.gameObject);
        
        // 생성된 버튼의 localScale을 1로 설정하여 부모의 스케일에 영향을 받지 않도록 함
        buttonInstance.gameObject.transform.localScale = Vector3.one;

        buttonInstance.weaponName = _weaponName;
        
        buttonInstance.GetComponent<Button>().onClick.AddListener(() =>
        {
            AudioManager.instance.PlayButtonSound("ButtonClick");
            
            CharacterController2D.instance.playerAttack.weapon_name = buttonInstance.weaponName;
            CharacterController2D.instance.playerAttack.weapon_type = WeaponData.instance.GetName_WeaponType(buttonInstance.weaponName);
            CharacterController2D.instance.playerAttack.weaponManager.weaponLife = WeaponData.instance.GetName_WeaponLifeCount(buttonInstance.weaponName);
            
            gameObject.GetComponent<Image>().DOFade(0f, 1f)
                .SetUpdate(UpdateType.Normal, true)
                .OnComplete(() =>
                {
                    foreach (var obj in buttons)
                    {
                        Destroy(obj);
                    }

                    gameObject.SetActive(false);
                    buttonAction?.Invoke();
                });
        });
        
        EventTrigger trigger = buttonInstance.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;

        entry.callback.AddListener((data) => {
            AudioManager.instance.PlayButtonSound("ButtonHover");
        });

        trigger.triggers.Add(entry);
    }

    public void SortSelectedItems()
    {
        if (ButtonGroup.transform.childCount == 2)
        {
            DOTween.To(()=> ButtonGroup.spacing, x=> ButtonGroup.spacing = x, 300f, 0.5f).SetUpdate(UpdateType.Normal, true);
        }
        else if (ButtonGroup.transform.childCount == 3)
        {
            DOTween.To(()=> ButtonGroup.spacing, x=> ButtonGroup.spacing = x, 200f, 0.5f).SetUpdate(UpdateType.Normal, true);
        }
        else if (ButtonGroup.transform.childCount == 4)
        {
            DOTween.To(()=> ButtonGroup.spacing, x=> ButtonGroup.spacing = x, 100f, 0.5f).SetUpdate(UpdateType.Normal, true);
        }
        else if (ButtonGroup.transform.childCount == 5)
        {
            DOTween.To(()=> ButtonGroup.spacing, x=> ButtonGroup.spacing = x, 50f, 0.5f).SetUpdate(UpdateType.Normal, true);
        }
    }
    
}
