using System;
using System.Collections;
using System.Collections.Generic;
using CleverCrow.Fluid.Dialogues.Graphs;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Test_DataNPC : InteractableObject
{
    private string nowMapName;
    
    private void Start()
    {
        nowMapName = SceneManager.GetActiveScene().name;
    }
    
    public void NpcInteraction(DialoguePlayback _dialoguePlayback)
    {
        if (talkEnable)
        {
            _dialoguePlayback.gameObject.SetActive(true);
            _dialoguePlayback.PlayDialogue(dialogue[0]);
        }
    }

    public void Save()
    {
        PopupManager popup = GameManager.Instance.GetPopupManager();
        popup.SetPopup("저장하시겠습니까?", false, () => 
            {
                PlayerDataManager.instance.nowPlayer.mapName = nowMapName;
                PlayerDataManager.instance.SaveData();
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
}
