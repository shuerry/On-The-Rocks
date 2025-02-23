using System;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueScript : MonoBehaviour {
    public static event Action<Story> OnCreateStory;

    [SerializeField] private TextAsset inkJSONAsset = null;
    public Story story;

    [SerializeField] private GameObject dialogueBox = null;

    // UI Prefabs
    [SerializeField] private TextMeshProUGUI dialogueText = null;
    [SerializeField] private TextMeshProUGUI nameText = null;
    [SerializeField] private Button buttonPrefab = null;
    
    private bool justClicked = false;

    void Update() {
        // Only process the click if it hasn't been processed already
        if (Input.GetMouseButtonDown(0) && !justClicked) {
            justClicked = true;  // Prevent multiple clicks from advancing
            RefreshView();
        }
    }

    void Awake () {
        StartStory();
    }

    // Creates a new Story object with the compiled story which we can then play!
    void StartStory () {
        justClicked = false;
        Debug.Log("Start Story");
        story = new Story(inkJSONAsset.text);
        if (OnCreateStory != null) OnCreateStory(story);
        RefreshView();
    }
    
    void RefreshView() {
        // Remove all the UI on screen
        RemoveChildren();
        Debug.Log("Refresh View");

        // Read all the content until we can't continue anymore
        if (story.canContinue) {
            // Continue gets the next line of the story
            string text = story.Continue();
            text = text.Trim();  // Clean up whitespace
            CreateContentView(text);
        }

        // Display all the choices if there are any
        if (story.currentChoices.Count > 0) {
            for (int i = 0; i < story.currentChoices.Count; i++) {
                Choice choice = story.currentChoices[i];
                Button button = CreateChoiceView(choice.text.Trim());
                // Tell the button what to do when we press it
                button.onClick.AddListener(delegate {
                    OnClickChoiceButton(choice);
                });
            }
        }
        // If we've read all the content and there are no choices, show the "End of story" button
        else {
            Button choice = CreateChoiceView("End of story.\nRestart?");
            choice.onClick.AddListener(delegate {
                StartStory();
            });
        }
    }

    // When we click the choice button, tell the story to choose that choice!
    void OnClickChoiceButton(Choice choice) {
        story.ChooseChoiceIndex(choice.index);
        RefreshView();
    }

    // Creates a textbox showing the line of text
    void CreateContentView(string text) {
        HandleTags(story.currentTags);
        dialogueText.text = text;
        dialogueText.transform.SetParent(dialogueBox.transform, false);
        justClicked = false;
    }

    // Creates a button showing the choice text
    Button CreateChoiceView(string text) {
        Button choice = Instantiate(buttonPrefab) as Button;
        choice.transform.SetParent(dialogueBox.transform, false);

        // Get the text from the button prefab
        Text choiceText = choice.GetComponentInChildren<Text>();
        if (choiceText != null) {
            choiceText.text = text;
        }

        // Make the button expand to fit the text
        // HorizontalLayoutGroup layoutGroup = choice.GetComponent<HorizontalLayoutGroup>();
        // layoutGroup.childForceExpandHeight = false;

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
        Debug.Log(sceneName);
    }
}
