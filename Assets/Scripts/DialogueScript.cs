using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Ink.Runtime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueScript : MonoBehaviour {
    public static event Action<Story> OnCreateStory;

    [SerializeField] private TextAsset inkJSONAsset = null;
    public Story story;

    [SerializeField] private GameObject dialogueBox = null;

    // UI Prefabs
    [SerializeField] private TextMeshProUGUI dialogueText = null;
    [SerializeField] private TextMeshProUGUI nameText = null;
    [SerializeField] private GameObject nameBox = null;
    [SerializeField] private Button buttonPrefab = null;
    [SerializeField] private GameObject choicesBackground = null;
    
    private bool justClicked = false;
    [SerializeField] private LevelManager levelManager;

    private static bool carnivalEnding = true; // assume good
    [SerializeField] GameObject pigeon = null;
    [SerializeField] GameObject rat = null;
    private bool playingVoicedDialogue = false;

    void Update() {
        // Only process the click if it hasn't been processed already
        if (Input.GetMouseButtonDown(0) && !justClicked && !playingVoicedDialogue) {
            justClicked = true;  // Prevent multiple clicks from advancing
            RefreshView();
        }
    }

    void Awake () {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (SceneManager.GetActiveScene().name != "Subway Scene") {
            StartStory();
        } else {
            dialogueBox.SetActive(false);
            nameBox.SetActive(false);
        }
    }

    // Creates a new Story object with the compiled story which we can then play!
    void StartStory () {
        justClicked = false;
        Debug.Log("Start Story");
        story = new Story(inkJSONAsset.text);
        if (OnCreateStory != null) OnCreateStory(story);

        if (story.variablesState.GlobalVariableExistsWithName("good_ending")) {
            story.variablesState["good_ending"] = carnivalEnding;
            Debug.Log("Updated Ink Variable. + " + story.variablesState["good_ending"]);
        } 

        dialogueBox.SetActive(true);
        nameBox.SetActive(true);
        // choicesBackground.SetActive(true);

        RefreshView();
    }
    
    void RefreshView() {
        // Remove all the UI on screen
        RemoveChildren();

        // Read all the content until we can't continue anymore
        if (story.canContinue) {
            // Continue gets the next line of the story
            string text = story.Continue();
            text = text.Trim();  // Clean up whitespace

            if (text.Contains("carnival_minigame")) {
                Debug.Log("Minigame detected! Starting minigame...");
                CarnivalLevelManager.gameStart = true;
                EndOfDialogue();
                return;
            } else if (text.Contains("Therapy Scene Subway Ending")) {
                Debug.Log("Ending subway scene.");
                EndOfDialogue();
                SceneManager.LoadScene("Therapy Scene Subway Ending");
            } else if (text.Contains("the end?")) {
                EndOfDialogue();
                SceneManager.LoadScene("StartScene");
            }

            CreateContentView(text);
        } else {
            Debug.Log("Story over.");
            EndOfDialogue();
            return;
        }

        // Display all the choices if there are any
        if (story.currentChoices.Count > 0) {
            HandleChoices();
        }
    }

    public void SetInkStory(TextAsset newStory) {
        inkJSONAsset = newStory;

        StartStory();
    }

    void EndOfDialogue()
    {
        dialogueBox.SetActive(false);
        nameBox.SetActive(false);
        choicesBackground.SetActive(false);
   } 

    void HandleChoices() {
        dialogueBox.SetActive(false);
        nameBox.SetActive(false);
        choicesBackground.SetActive(true);
        
        // Enable mouse cursor
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor from the center
        Cursor.visible = true; // Make the cursor visible

        for (int i = 0; i < story.currentChoices.Count; i++) {
            Choice choice = story.currentChoices[i];
            Button button = CreateChoiceView(choice.text.Trim(), i);
            // Tell the button what to do when we press it
            button.onClick.AddListener(delegate {
                OnClickChoiceButton(choice);
            });
        }
    }

    // When we click the choice button, tell the story to choose that choice!
    void OnClickChoiceButton(Choice choice) {
        story.ChooseChoiceIndex(choice.index);
        if (story.canContinue) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            string text = story.Continue();
            text = text.Trim();
            SetInkScene(text);
        }
    }

    // Creates a textbox showing the line of text
    void CreateContentView(string text) {
        HandleTags(story.currentTags);
        dialogueText.text = text;
        justClicked = false;
    }

    // Creates a button showing the choice text
    Button CreateChoiceView(string text, int index) {
        Button choice = Instantiate(buttonPrefab) as Button;
        choice.transform.SetParent(choicesBackground.transform, false);

        // Get the text from the button prefab
        TextMeshProUGUI choiceText = choice.GetComponentInChildren<TextMeshProUGUI>();
        if (choiceText != null) {
            choiceText.text = text;
        }

        // Make the button expand to fit the text
        RectTransform buttonRectangleTransform = choice.GetComponent<RectTransform>();
        buttonRectangleTransform.localPosition = new Vector3(buttonRectangleTransform.localPosition.x, buttonRectangleTransform.localPosition.y - (80 * index), buttonRectangleTransform.localPosition.z);
    
        return choice;
    }

    void HandleTags(List<string> tags) {
        if (tags != null) {
            foreach (string tag in tags) {
                string[] splitTag = tag.Split(':');
                if (splitTag.Length != 2) 
                {
                    Debug.LogError("Tag could not be appropriately parsed: " + tag);
                }
                string tagKey = splitTag[0].Trim();
                string tagValue = splitTag[1].Trim();

                switch (tagKey) {
                    case "speaker":
                        nameText.text = tagValue;
                        break;
                    case "pigeon":
                        if (pigeon) {
                            Sprite pigeon_sprite = Resources.Load<Sprite>("Animal Animation/" + tagValue);
                            if (pigeon_sprite != null) {
                                pigeon.GetComponent<SpriteRenderer>().sprite = pigeon_sprite;
                            }
                        }
                        break;
                    case "rat":
                        if (rat) {
                            Sprite rat_sprite = Resources.Load<Sprite>("Animal Animation/" + tagValue);
                            if (rat_sprite != null) {
                                rat.GetComponent<SpriteRenderer>().sprite = rat_sprite;
                            }
                        }
                        break;
                    case "audio":
                        Debug.Log("playing audio file " + tagValue);
                        /* AudioClip voice_acting = Resources.Load<AudioClip>("Audio/" + tagValue);
                        
                        voice_acting.Play();
                        playingVoicedDialogue = true;
                        when voice_acting.finished, stop()
                        playingVoicedDialogue = false; */
                        break;
                    default:
                        nameText.text = " ";
                        break;
                }
            }
        } else {
            nameText.text = " ";
        }
    }

    // Destroys all the children of this gameobject (all the UI)
    void RemoveChildren() {
        int childCount = dialogueBox.transform.childCount;
        for (int i = childCount - 1; i >= 0; --i) {
            Transform child = dialogueBox.transform.GetChild(i);
            if (child.GetComponent<TextMeshProUGUI>() == null) {
                Destroy(dialogueBox.transform.GetChild(i).gameObject);
            }
        }
    }

    public void SetInkScene(string sceneName) {
        levelManager.SetScene(sceneName);
    }

    public static void SetCarnivalEnding(bool goodEnding) {
        carnivalEnding = goodEnding;
    }
}
