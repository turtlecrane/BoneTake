using System;
using System.Collections;
using System.Collections.Generic;
using CleverCrow.Fluid.Dialogues.Graphs;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveNPC : InteractableObject
{
    public NamePopup namePopup;
    
    private string nowMapName;
    //private DialoguePlayback playback;
    
    private void Start()
    {
        nowMapName = SceneManager.GetActiveScene().name;
    }
    
    public void NpcInteraction(DialoguePlayback _dialoguePlayback)
    {
        if (talkEnable)
        {
            //playback = _dialoguePlayback;
            _dialoguePlayback.gameObject.SetActive(true);
            //플레이어의 이름이 지어지지않은 상태라면(처음 말 건 상태)
            if (PlayerDataManager.instance.nowPlayer.playerName == "")
            {
                _dialoguePlayback.PlayDialogue(dialogue[1]);
            }
            else //이름이 지어진 상태라면
            {
                _dialoguePlayback.PlayDialogue(dialogue[0]);
            }
        }
    }

    public void Save()
    {
        PopupManager popup = PopupManager.instance;
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

    public void MakeName()
    {
        namePopup.gameObject.SetActive(true);
        namePopup.confirm.onClick.AddListener(() =>
        {
            //인풋필드가 공백인지 확인하고 공백이 아니면 수행
            if (namePopup.NameMakeCheck())
            {
                //플레이어 데이터의 이름을 인풋박스 이름으로 변경
                PlayerDataManager.instance.nowPlayer.playerName = namePopup.inputField.text;
                
                //이름팝업 종료
                namePopup.gameObject.SetActive(false);
                
                //이름 완료 다이얼로그 재생
                //playback.PlayDialogue(dialogue[2]);
            }
        });
    }
}
