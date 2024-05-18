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
    public GameObject creat;	// 플레이어 닉네임 입력UI

    public DataSlot[] slots;
    public GameObject lifePoint_Prefab;
    
    public TMP_Text newPlayerName;	// 새로 입력된 플레이어의 닉네임
    bool[] savefile = new bool[3];	// 세이브파일 존재유무 저장
    
    void Start()
    {
        bool dataExists = false; // 데이터 존재 여부를 확인하기 위한 변수

        // 슬롯별로 저장된 데이터가 존재하는지 판단.
        for (int i = 0; i < 3; i++)
        {
            string filePath = PlayerDataManager.instance.path + $"{i}";
            if (File.Exists(filePath))// 데이터가 있는 경우
            {
                savefile[i] = true; // 해당 슬롯 번호의 bool배열 true로 변환
                PlayerDataManager.instance.nowSlot = i; // 선택한 슬롯 번호 저장
                PlayerDataManager.instance.LoadData(); // 해당 슬롯 데이터 불러옴
                slots[i].playerName.text = PlayerDataManager.instance.nowPlayer.playerName;	// 버튼에 닉네임 표시
                dataExists = true;
                
                slots[i].isDataExists = true;
                UpdateSlotInfo(i);// 데이터가 존재함을 표시
            }
            else
            {
                slots[i].isDataExists = false;
            }
        }
        
        // 불러온 데이터를 초기화시킴.(버튼에 닉네임을 표현하기위함이었기 때문)
        PlayerDataManager.instance.DataClear();	

        if (dataExists)
        {
            titleText.text = "이어할 데이터슬롯을 선택해 주세요.";
        }
        else
        {
            titleText.text = "새롭게 시작할 데이터슬롯을 선택해 주세요.";
        }
    }

    private void UpdateSlotInfo(int slotIndex)
    {
        PlayerData player = PlayerDataManager.instance.nowPlayer;
        slots[slotIndex].playerName.text = player.playerName;
        slots[slotIndex].playTime.text = $"플레이 타임 : {player.playTime}";
        slots[slotIndex].weaponIcon.sprite = WeaponData.instance.weaponGFXSource.freshIcon[WeaponData.instance.GetName_WeaponID(PlayerDataManager.instance.nowPlayer.weaponName)];

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
    
    public void Slot(int number)	// 슬롯의 기능 구현
    {
        PlayerDataManager.instance.nowSlot = number;	// 슬롯의 번호를 슬롯번호로 입력함.

        if (savefile[number])	// bool 배열에서 현재 슬롯번호가 true라면 = 데이터 존재한다는 뜻
        {
            PlayerDataManager.instance.LoadData();	// 데이터를 로드하고
            GoGame();	// 게임씬으로 이동
        }
        else	// bool 배열에서 현재 슬롯번호가 false라면 데이터가 없다는 뜻
        {
            NamePopupCreate();	// 플레이어 닉네임 입력 UI 활성화
        }
    }

    public void NamePopupCreate()	// 플레이어 닉네임 입력 UI를 활성화하는 메소드
    {
        creat.gameObject.SetActive(true);
    }

    public void GoGame()	// 게임씬으로 이동
    {
        if (!savefile[PlayerDataManager.instance.nowSlot])	// 현재 슬롯번호의 데이터가 없다면
        {
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