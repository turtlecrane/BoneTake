using System;
using System.Collections;
using KoreanTyper;
using System.Collections.Generic;
using CleverCrow.Fluid.Databases;
using CleverCrow.Fluid.Dialogues.Graphs;
using UnityEngine;
using UnityEngine.UI;
using CleverCrow.Fluid.Dialogues;
using CleverCrow.Fluid.Dialogues.Choices;
using TMPro;

public class DialoguePlayback : MonoBehaviour {
    private DialogueController _ctrl;

    [Header("Graphics")]
    public GameObject speakerContainer; //다이얼로그 최상단 오브젝트
    public Image leftPortrait; //초상화
    public Image rightPortrait; //초상화
    public TMP_Text name; //이름
    public TextMeshProUGUI contentText; //대화내용

    public RectTransform choiceList; //선택버튼이 생성될 위치
    public ChoiceButton choicePrefab; //선택버튼 프리팹
    public Sprite blank;

    private float textDuration;
    private bool isAllTyped;
    private bool isSkiped;
    private bool isDialogueChanged;

    public void PlayDialogue(DialogueGraph _dialogue) {
        var database = new DatabaseInstance();
        _ctrl = new DialogueController(database);

        // @NOTE 오디오가 필요하지 않은 경우 이것 대신 _ctrl.Events.Speak((actor, text) => {})를 호출하세요.
        _ctrl.Events.SpeakWithAudio.AddListener((actor, text, audioClip) => {
            HandleDialogue(actor, text, audioClip);
            StartCoroutine(NextDialogue(text, () =>
            {
                isDialogueChanged = true;
                _ctrl.Next();
            }));
        });

        _ctrl.Events.Choice.AddListener((actor, text, choices) => {
            HandleDialogue(actor, text, null);
            StartCoroutine(NextDialogue(text, () =>
            {
                isDialogueChanged = true;
                DisplayChoices(choices);
            }));
        });

        _ctrl.Events.End.AddListener(() => {
            speakerContainer.SetActive(false);
        });

        _ctrl.Events.NodeEnter.AddListener((node) => {
            if (node.GetType().ToString() == "CleverCrow.Fluid.Dialogues.Nodes.NodeRoot")
            {
                leftPortrait.sprite = blank;
                rightPortrait.sprite = blank;
            }
        });

        _ctrl.Play(_dialogue);
    }

    /// <summary>
    /// 다이얼로그 표시
    /// </summary>
    /// <param name="actor">화자</param>
    /// <param name="text">대화 내용</param>
    private void HandleDialogue(IActor actor, string text, AudioClip audioClip) {
        // 설정된 텍스트 속도 가져오기
        float textDuration = PlayerPrefs.GetFloat("textSpeedSelected", 0.5f);

        if (audioClip) Debug.Log($"Audio Clip Detected ${audioClip.name}");

        ClearChoices();
        
        if (actor.DisplayName == "Player") 
        {
            SetPortraits(leftPortrait, rightPortrait, actor.Portrait, PlayerDataManager.instance.nowPlayer.playerName);
        }
        else
        {
            SetPortraits(rightPortrait, leftPortrait, actor.Portrait, actor.DisplayName);
        }
        
        StartCoroutine(TypingCoroutine(contentText, text, textDuration));
    }

    /// <summary>
    /// 초상화 표시
    /// </summary>
    /// <param name="activePortrait">활성상태의 초상화</param>
    /// <param name="inactivePortrait">비활성상태의 초상화</param>
    /// <param name="portrait">초상화 스프라이트</param>
    /// <param name="actorName">화자의 이름</param>
    private void SetPortraits(Image activePortrait, Image inactivePortrait, Sprite portrait, string actorName) {
        activePortrait.sprite = portrait;
        name.text = actorName;

        Color inactiveColor = inactivePortrait.color;
        inactiveColor.a = 0.5f;
        inactivePortrait.color = inactiveColor;

        Color activeColor = activePortrait.color;
        activeColor.a = 1f;
        activePortrait.color = activeColor;
    }

    /// <summary>
    /// 선택(분기)버튼 생성,표시
    /// </summary>
    /// <param name="choices"></param>
    private void DisplayChoices(List<IChoice> choices) {
        choices.ForEach(c => {
            var choice = Instantiate(choicePrefab, choiceList);
            choice.contentText.text = c.Text;
            choice.clickEvent.AddListener(_ctrl.SelectChoice); //다이얼로그 전환
        });
    }
    
    /// <summary>
    /// 선택(분기)버튼 초기화
    /// </summary>
    private void ClearChoices () {
        foreach (Transform child in choiceList) {
            Destroy(child.gameObject);
        }
    }
    
    /// <summary>
    /// 기본 다이얼로그에서 다음 다이얼로그로 전환
    /// </summary>
    private IEnumerator NextDialogue(string _text, Action _transitionAction)
    {
        while (true) // 무한 루프로 계속 클릭 감지
        {
            if (Input.GetMouseButtonDown(0)) // 클릭 감지
            {
                if (!isAllTyped) // 텍스트가 아직 모두 출력되지 않았다면
                {
                    if (!isDialogueChanged)
                    {
                        Debug.Log("대화 스킵");
                        isSkiped = true;
                        contentText.text = _text;
                        yield return new WaitForSecondsRealtime(0.1f);
                        isSkiped = false;
                        isAllTyped = true;
                        yield return null;
                    }
                        
                    yield return null;
                }
                else // 텍스트가 모두 출력되었다면
                {
                    _transitionAction?.Invoke();
                    break; // 코루틴에서 탈출
                }
            }
            else // 클릭이 감지되지 않았다면
            {
                // 다음 프레임까지 대기
                yield return null;
            }
        }
    }


    private void Update () 
    {
        // 여러 프레임에 걸쳐 있을 수 있는 작업을 실행하는 데 필요
        _ctrl.Tick();
    }

    /// <summary>
    /// 다이얼로그 텍스트 타이핑효과
    /// </summary>
    public IEnumerator TypingCoroutine(TextMeshProUGUI textMesh, string str, float duration) 
    {
        textMesh.text = "";                                   // 초기화
        isAllTyped = false;
        yield return new WaitForSecondsRealtime(0.1f);     // 0.1초 대기

        int strTypingLength = str.GetTypingLength();          // 최대 타이핑 수 구함
        for(int  i = 0 ; i <= strTypingLength ; i ++ ) {      // 반복문
            //AudioManager.instance.PlaySFX("TextTyping");
            isDialogueChanged = false;
            if (isSkiped) break;
            
            string typingText = str.Typing(i);                // 타이핑
            textMesh.text = typingText;                       // TextMesh에 타이핑된 텍스트 설정

            // 마지막 문자가 공백인지 확인
            if (typingText.EndsWith(" ")) {
                AudioManager.instance.PlaySFX("TextTyping", 1); // 공백일 때 사운드 효과
            } else {
                AudioManager.instance.PlaySFX("TextTyping", 0); // 공백이 아닐 때 사운드 효과
            }
            
            yield return new WaitForSecondsRealtime(duration);           //타이핑속도
            
            if (i == strTypingLength) isAllTyped = true;
        }
    }
}