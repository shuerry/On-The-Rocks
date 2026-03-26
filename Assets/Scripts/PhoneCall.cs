using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhoneCall : MonoBehaviour
{
    [SerializeField] GameObject acceptCall = null;
    [SerializeField] Button acceptCallButton = null;
    [SerializeField] GameObject endCall = null;
    [SerializeField] Button endCallButton = null;
    [Header("Dialogue Settings")]
    [SerializeField] DialogueScript dialogueScript = null;
    [SerializeField] Image dialogueBox = null;
    [SerializeField] Image nameBox = null;
    
    void Start()
    {
        dialogueBox.enabled = false;
        nameBox.enabled = false;

        acceptCallButton.onClick.AddListener(delegate {
            OnClickAcceptCallButton();
        });

        endCallButton.onClick.AddListener(delegate
        {
            OnClickEndCallButton();
        });
    }

    public void OnClickAcceptCallButton()
    {
        Debug.Log("Click accept.");
        HideButtons();
        dialogueScript.SetStartDialogueButtonClicked(true);
        dialogueBox.enabled = true;
        nameBox.enabled = true;
    }

    public void OnClickEndCallButton()
    {
        Debug.Log("Click end.");
        Application.Quit();
    }

    void HideButtons()
    {
        acceptCall.SetActive(false);
        endCall.SetActive(false);
    }
}
