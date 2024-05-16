using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_DataNPC : MonoBehaviour
{
    public bool talkEnable;
    
    public void NpcInteraction()
    {
        if (!talkEnable)
        {
            PopupManager popup = GameManager.Instance.GetPopupManager();
            popup.SetPopup("저장하시겠습니까?", false, 
            () =>
            {
                Save();
                popup.ClosePopup();
                popup.SetPopup("저장되었습니다.", true, () =>
                {
                    popup.gameObject.SetActive(false);
                },null);
            }, 
            () =>
            {
                Debug.Log("취소 버튼을 누름");
            });
        }
        else
        {
            //다이얼로그 로직
            return;
        }
    }

    public void Save()
    {
        PlayerDataManager.instance.SaveData();
    }
}
