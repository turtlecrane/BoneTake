using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Narration : MonoBehaviour
{
    public TMP_Text text;
    public List<string> narrationText;
    public bool isEnd = false;
    
    void Start()
    {
        var color = text.color;
        color.a = 0f;
        text.color = color;
        
        StartCoroutine(NarrationRoutine());
    }
    
    private IEnumerator NarrationRoutine()
    {
        foreach (var narration in narrationText)
        {
            text.text = narration;
            text.DOFade(1f, 1f);
            yield return new WaitForSeconds(3.5f);//3-4초
            text.DOFade(0f, 1f);
            yield return new WaitForSeconds(1.0f);
        }

        isEnd = true;
    }
}
