using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;
using UnityEngine.Serialization;

public class DataSlotManager : MonoBehaviour
{
    public TMP_Text titleText;
    public GameObject nameInputPopup;	// 플레이어 닉네임 입력UI
    public PopupManager popupManager;

    public DataSlot[] slots;
    public GameObject lifePoint_Prefab;
    
    public TMP_Text newPlayerName;	// 새로 입력된 플레이어의 닉네임
    public bool[] savefile = new bool[3];	// 세이브파일 존재유무 저장
    
    void Start()
    {
        bool dataExists = false; // 데이터 존재 여부를 확인하기 위한 변수

        // 슬롯별로 저장된 데이터가 존재하는지 판단.
        for (int i = 0; i < 3; i++)
        {
            string filePath = PlayerDataManager.instance.path + $"{i}";
            if (File.Exists(filePath))// 데이터가 있는 경우
            {
                dataExists = true;
                savefile[i] = true; // 해당 슬롯 번호의 bool배열 true로 변환
                PlayerDataManager.instance.nowSlot = i; // 선택한 슬롯 번호 저장
                PlayerDataManager.instance.LoadData(); // 해당 슬롯 데이터 불러옴
                slots[i].isDataExists = true; // 데이터가 존재함을 표시
                UpdateSlotInfo(i);
            }
            else
            {
                slots[i].isDataExists = false;
            }
        }
        
        // 불러온 데이터를 초기화시킴.(버튼에 닉네임을 표현하기위함이었기 때문)
        PlayerDataManager.instance.DataClear();	
        
        int entryType = PlayerPrefs.GetInt("DataSlotEntryType", -1);
        if (entryType == 0)
        {
            titleText.text = "새롭게 시작할 데이터슬롯을 선택해 주세요.";
        }
        else if(entryType == 1)
        {
            titleText.text = "이어할 데이터슬롯을 선택해 주세요.";
        }
        else
        {
            titleText.text = "오류 발생";
        }
    }

    /// <summary>
    /// 데이터슬롯에 정보 넣기
    /// </summary>
    /// <param name="slotIndex">데이터 슬롯 번호</param>
    private void UpdateSlotInfo(int slotIndex)
    {
        PlayerData player = PlayerDataManager.instance.nowPlayer;
        slots[slotIndex].playerName.text = player.playerName;
        slots[slotIndex].weaponIcon.sprite = WeaponData.instance.weaponGFXSource.freshIcon[WeaponData.instance.GetName_WeaponID(PlayerDataManager.instance.nowPlayer.weaponName)];

        PlaytimeTextConvertor(player.playTime, slotIndex);
        
        slots[slotIndex].lifePoints.Clear();
        for (int i = 0; i < player.playerMaxHP; i++)
        {
            GameObject lifePoint = Instantiate(lifePoint_Prefab, slots[slotIndex].LifePointTransform);
            slots[slotIndex].lifePoints.Add(lifePoint);

            LifePointState hpScript = lifePoint.GetComponent<LifePointState>();
            hpScript.isDisable = i >= player.playerHP;
            lifePoint.GetComponent<Image>().color = hpScript.isDisable ? Color.grey : Color.black;
        }
    }

    /// <summary>
    /// 플레이타임을 식별용이하게 변환
    /// </summary>
    private void PlaytimeTextConvertor(float _playTime, int _index)
    {
        // playTime을 시간, 분, 초로 변환
        int hours = (int)_playTime / 3600;
        int minutes = ((int)_playTime % 3600) / 60;
        int seconds = (int)_playTime % 60;
        
        string playTimeText = "";

        if (hours > 0) {
            playTimeText = $"{hours}시간";
            if (minutes > 0) playTimeText += $" {minutes}분";
        } else if (minutes > 0) {
            playTimeText = $"{minutes}분 {seconds}초";
        } else {
            playTimeText = $"{seconds}초";
        }

        slots[_index].playTime.text = $"플레이 타임 : {playTimeText}";
    }
    
    /// <summary>
    /// 슬롯의 기능 구현
    /// </summary>
    /// <param name="number"></param>
    public void Slot(int number)
    {
        PlayerDataManager.instance.nowSlot = number;	// 슬롯의 번호를 슬롯번호로 입력함.
        int entryType = PlayerPrefs.GetInt("DataSlotEntryType", -1);
        
        if (savefile[number])	// bool 배열에서 현재 슬롯번호가 true라면 = 데이터 존재한다는 뜻
        {
            if (entryType == 0)
            {
                popupManager.SetPopup("이미 데이터가 존재하는 데이터 슬롯입니다.\n정말 새롭게 시작하시겠습니까?\n<size=70%>이미 존재하는 데이터는 삭제됩니다.</size>",false,
                    () =>
                    {
                        popupManager.ClosePopup(); //팝업 닫기
                        PlayerDataManager.instance.DeleteData(); //데이터파일 삭제
                        savefile[number] = false;
                        NamePopupCreate();
                    }, () => { });
            }
            else if(entryType == 1)
            {
                PlayerDataManager.instance.LoadData();	// 데이터를 로드하고
                GoGame();	//게임씬으로 이동
            }
            else
            {
                popupManager.SetPopup("오류 발생",true,
                    () =>
                    {
                        Debug.Log("오류 발생 확인버튼");
                        popupManager.ClosePopup();
                    },()=>{});
            }
        }
        else	// bool 배열에서 현재 슬롯번호가 false라면 데이터가 없다는 뜻
        {
            if (entryType == 0)
            {
                NamePopupCreate();	// 플레이어 닉네임 입력 UI 활성화
            }
            else if (entryType == 1)
            {
                popupManager.SetPopup("새로운 게임을 시작하겠습니까?",false,
                    () =>
                    {
                        Debug.Log("이어하기로 데이터가 없는 슬롯을 선택함");
                        popupManager.ClosePopup(); //팝업 닫기
                        NamePopupCreate();	// 플레이어 닉네임 입력 UI 활성화
                    },()=>{});
            }
            else
            {
                popupManager.SetPopup("오류 발생",true,
                    () =>
                    {
                        Debug.Log("오류 발생 확인버튼");
                        popupManager.ClosePopup();
                    },()=>{});
            }
        }
    }

    /// <summary>
    /// 플레이어 닉네임 입력 UI를 활성화하는 메소드
    /// </summary>
    public void NamePopupCreate()
    {
        nameInputPopup.gameObject.SetActive(true);
    }

    /// <summary>
    /// 게임씬으로 이동
    /// </summary>
    public void GoGame()
    {
        if (!savefile[PlayerDataManager.instance.nowSlot])	// 현재 슬롯번호의 데이터가 없다면
        {
            Debug.Log("현재 슬롯번호의 데이터가 없다");
            PlayerDataManager.instance.nowPlayer.playerName = newPlayerName.text; // 입력한 이름을 복사해옴
            PlayerDataManager.instance.SaveData(); // 현재 정보를 저장함.
        }
        
        SceneChange("Interaction"); // 게임씬으로 이동
    }
    
    public void SceneChange(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}