using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;

public class DataSlot : MonoBehaviour
{
    public TMP_Text titleText;
    public GameObject creat;	// 플레이어 닉네임 입력UI
    public TMP_Text[] slotText;		// 슬롯버튼 아래에 존재하는 Text들
    public TMP_Text newPlayerName;	// 새로 입력된 플레이어의 닉네임

    bool[] savefile = new bool[3];	// 세이브파일 존재유무 저장

    void Start()
    {
        bool dataExists = false; // 데이터 존재 여부를 확인하기 위한 변수
        
        // 슬롯별로 저장된 데이터가 존재하는지 판단.
        for (int i = 0; i < 3; i++)
        {
            if (File.Exists(PlayerDataManager.instance.path + $"{i}"))	// 데이터가 있는 경우
            {
                savefile[i] = true;			// 해당 슬롯 번호의 bool배열 true로 변환
                PlayerDataManager.instance.nowSlot = i;	// 선택한 슬롯 번호 저장
                PlayerDataManager.instance.LoadData();	// 해당 슬롯 데이터 불러옴
                slotText[i].text = PlayerDataManager.instance.nowPlayer.playerName;	// 버튼에 닉네임 표시
                dataExists = true; // 데이터가 존재함을 표시
            }
            else	// 데이터가 없는 경우
            {
                slotText[i].text = "비어있음";
            }
        }
        // 불러온 데이터를 초기화시킴.(버튼에 닉네임을 표현하기위함이었기 때문)
        PlayerDataManager.instance.DataClear();	
        
        // 데이터 존재 여부에 따라 titleText의 내용을 설정
        if (dataExists) // 1개라도 데이터가 있으면
        {
            titleText.text = "이어할 데이터 슬롯을 선택해주세요.";
        }
        else // 데이터가 전혀 없으면
        {
            titleText.text = "새롭게 시작할 데이터 슬롯을 선택해 주세요.";
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
            Creat();	// 플레이어 닉네임 입력 UI 활성화
        }
    }

    public void Creat()	// 플레이어 닉네임 입력 UI를 활성화하는 메소드
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
        SceneManager.LoadScene(1); // 게임씬으로 이동
    }
}