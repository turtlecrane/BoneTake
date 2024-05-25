using System.Collections;
using CleverCrow.Fluid.Databases;
using CleverCrow.Fluid.Dialogues.Graphs;
using UnityEngine;
using UnityEngine.UI;
using CleverCrow.Fluid.Dialogues.Graphs;
using CleverCrow.Fluid.Dialogues;
using DG.Tweening;
using TMPro;

public class DialoguePlayback : MonoBehaviour {
    private DialogueController _ctrl;

    public DialogueGraph dialogue; //재생할 다이얼로그 그래프

    [Header("Graphics")]
    public GameObject speakerContainer; //다이얼로그 최상단 오브젝트
    public Image leftPortrait; //초상화
    public Image rightPortrait; //초상화
    public TMP_Text name; //이름
    public TMP_Text contentText; //대화내용

    public RectTransform choiceList; //선택버튼이 생성될 위치
    public ChoiceButton choicePrefab; //선택버튼 프리팹

    private void Awake () {
        var database = new DatabaseInstance();
       _ctrl = new DialogueController(database);

       // @NOTE 오디오가 필요하지 않은 경우 이것 대신 _ctrl.Events.Speak((actor, text) => {})를 호출하세요.
       _ctrl.Events.SpeakWithAudio.AddListener((actor, text, audioClip) => {
           if (audioClip) Debug.Log($"Audio Clip Detected ${audioClip.name}");

           ClearChoices();
           rightPortrait.sprite = actor.Portrait;
           contentText.text = text;

           StartCoroutine(NextDialogue());
       });

       _ctrl.Events.Choice.AddListener((actor, text, choices) => {
           ClearChoices();
           rightPortrait.sprite = actor.Portrait;
           contentText.text = text;

           choices.ForEach(c => {
               var choice = Instantiate(choicePrefab, choiceList);
               choice.contentText.text = c.Text;
               choice.clickEvent.AddListener(_ctrl.SelectChoice);
           });
       });

       _ctrl.Events.End.AddListener(() => {
           speakerContainer.SetActive(false);
       });

       _ctrl.Events.NodeEnter.AddListener((node) => {
           Debug.Log($"Node Enter: {node.GetType()} - {node.UniqueId}");
       });

       _ctrl.Play(dialogue);
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

    public static void TMPDOText(TMP_Text text, float duration)
    {
        text.maxVisibleCharacters = 0;
        DOTween.To(x => text.maxVisibleCharacters = (int)x, 0f, text.text.Length, duration);
    }
}