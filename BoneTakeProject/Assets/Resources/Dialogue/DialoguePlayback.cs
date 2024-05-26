using System.Collections;
using System.Collections.Generic;
using CleverCrow.Fluid.Databases;
using CleverCrow.Fluid.Dialogues.Graphs;
using UnityEngine;
using UnityEngine.UI;
using CleverCrow.Fluid.Dialogues.Graphs;
using CleverCrow.Fluid.Dialogues;
using CleverCrow.Fluid.Dialogues.Choices;
using DG.Tweening;
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

    [SerializeField] private float textDuration;

    public void PlayDialogue(DialogueGraph _dialogue) {
        var database = new DatabaseInstance();
        _ctrl = new DialogueController(database);

        // @NOTE 오디오가 필요하지 않은 경우 이것 대신 _ctrl.Events.Speak((actor, text) => {})를 호출하세요.
        _ctrl.Events.SpeakWithAudio.AddListener((actor, text, audioClip) => {
            HandleDialogue(actor, text, audioClip);
            StartCoroutine(NextDialogue());
        });

        _ctrl.Events.Choice.AddListener((actor, text, choices) => {
            HandleDialogue(actor, text, null);
            DisplayChoices(choices);
        });

        _ctrl.Events.End.AddListener(() => {
            speakerContainer.SetActive(false);
        });

        _ctrl.Events.NodeEnter.AddListener((node) => {
            Debug.Log($"Node Enter: {node.GetType()} \n - {node.UniqueId}");
            if (node.GetType().ToString() == "CleverCrow.Fluid.Dialogues.Nodes.NodeRoot")
            {
                leftPortrait.sprite = blank;
                rightPortrait.sprite = blank;
            }
        });

        _ctrl.Play(_dialogue);
    }

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
        
        contentText.text = text;
        TMPDOText(contentText, textDuration); // (빠르게 = 0.1f, 보통 = 0.5f, 느리게 = 1f)
    }

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

    private void DisplayChoices(List<IChoice> choices) {
        choices.ForEach(c => {
            var choice = Instantiate(choicePrefab, choiceList);
            choice.contentText.text = c.Text;
            choice.clickEvent.AddListener(_ctrl.SelectChoice);
        });
    }
    
    private void ClearChoices () {
        foreach (Transform child in choiceList) {
            Destroy(child.gameObject);
        }
    }

    private IEnumerator NextDialogue () {
        yield return null;

        while (!Input.GetMouseButtonDown(0)) {
            yield return null;
        }

        _ctrl.Next();
    }

    private void Update () {
        // Required to run actions that may span multiple frames
        _ctrl.Tick();
    }

    public static void TMPDOText(TextMeshProUGUI textMesh, float duration)
    {
        textMesh.maxVisibleCharacters = 0;
        DOTween.To(x => textMesh.maxVisibleCharacters = (int)x, 0f, textMesh.text.Length, duration).SetUpdate(UpdateType.Normal, true);
    }
}