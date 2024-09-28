using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NextFloorDoor : MonoBehaviour
{
    public Image fadePanel;
    public CinemachineVirtualCamera CMvcam;
    public GameObject bossObjectName;
    private string nowMapName;
    
    private void Start()
    {
        nowMapName = SceneManager.GetActiveScene().name;
        foreach (string bossName in PlayerDataManager.instance.nowPlayer.killedTypeOfBosses)
        {
            if (bossName == bossObjectName.name)
            {
                gameObject.tag = "NPC";
                gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
            }
        }
    }
    
    public void NpcInteraction()
    {
        PopupManager popupManager = GameManager.Instance.GetPopupManager();
        popupManager.SetPopup("Do you want to get out of the first floor and go to the second floor? \n <size=70%>(Note: Select Yes to save the game automatically.)</size>",false, () =>
        {
            popupManager.ClosePopup();
            
            Save();
            StartCoroutine(FadeOut());
        }, ()=>{});
    }
    
    public void Save()
    {
        PlayerDataManager.instance.nowPlayer.mapName = nowMapName;
        PlayerDataManager.instance.SaveData();
    }
    
    public IEnumerator FadeOut()
    {
        CMvcam.gameObject.SetActive(true);
        yield return new WaitForSeconds(2.1f);
        fadePanel.gameObject.SetActive(true);
        fadePanel.DOFade(1f, 1f);
        yield return new WaitForSeconds(1f);
        GameObject playerSystem = GameObject.FindWithTag("PlayerSystem");
        Destroy(playerSystem);
        SceneManager.LoadScene("EndingCredit");
    }
}
