using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkableObject : InteractableObject
{
    public void NpcInteraction(DialoguePlayback _dialoguePlayback)
    {
        if (talkEnable)
        {
            _dialoguePlayback.gameObject.SetActive(true);
            _dialoguePlayback.PlayDialogue(dialogue[0]);
        }
    }
}
