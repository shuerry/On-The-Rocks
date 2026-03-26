using System;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueScript : MonoBehaviour {
    public static event Action<Story> OnCreateStory;

    [SerializeField] private TextAsset inkJSONAsset = null;
    public Story story;

    [Header("Dialogue")]
    [SerializeField] private GameObject dialogueBox = null;
    [SerializeField] private Image dialogueBoxImage = null;
    // UI Prefabs
    [SerializeField] private TextMeshProUGUI dialogueText = null;
    [SerializeField] private TextMeshProUGUI nameText = null;
    [SerializeField] private GameObject nameBox = null;
    [SerializeField] private Image nameBoxImage = null;
    private Color rockyColor = new Color(0.5f, 0.273f, 0.074f); // Russet
    private Color peggyColor = new Color(0.434f, 0.559f, 0.684f); // Denim
    private Color therapistColor = new Color(0.996f, 0.977f, 0.625f);// Pastel Yellow
    private Color defaultColor = new Color(0.047f, 0.254f, 0.336f); // Millenial Pink
    
    [Header("Internal Dialogue")]
    [SerializeField] private GameObject internalBox = null;
    [SerializeField] private TextMeshProUGUI internalText = null;

    [Header("Clipboard Choices")]
    [SerializeField] private Button buttonPrefab = null;
    [SerializeField] private GameObject choicesBackground = null;
    
    private bool justClicked = false;
    [SerializeField] private LevelManager levelManager;

    private static bool carnivalEnding = true; // assume good
    private Dictionary<string, Sprite> pigeonSpriteMap;
    private Dictionary<string, Sprite> ratSpriteMap;
    [SerializeField] GameObject pigeon = null;
    [SerializeField] GameObject rat = null;
    private AudioSource pigeon_audioSource = null;
    private AudioSource rat_audioSource = null;

    [Header("Peggy Movement")]
    [SerializeField] private PeggyCarnivalMovements peggyMover = null;
    [SerializeField] private CarnivalWaypointMap waypointMap = null;
    bool innerVoice = false;

    [Header("Camera")]
    [SerializeField] private TherapyCameraController cameraController = null;

    private bool pausedForDistance = false;
    private bool holdUntilPeggyArrived = false;

    void Update() {
        if (holdUntilPeggyArrived)
        {
            dialogueBox.SetActive(false);
            nameBox.SetActive(false);
            return;
        } 

        if (pausedForDistance) return;

        // Only process the click if it hasn't been processed already
        if (Input.GetMouseButtonDown(0) && !justClicked && (!MainMenu.useVoiceActing || (!pigeon_audioSource.isPlaying && !rat_audioSource.isPlaying))) {
            justClicked = true;  // Prevent multiple clicks from advancing
            if (innerVoice)
            {
                Debug.Log("inner voice? " + innerVoice);
            }
            RefreshView();
        }
    }

    void Awake () {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (peggyMover != null)
        {
            peggyMover.Arrived += OnPeggyArrived;
        }

        if (cameraController == null)
            cameraController = FindFirstObjectByType<TherapyCameraController>();
        if (pigeon != null) {
            pigeon_audioSource = pigeon.GetComponent<AudioSource>();
            Sprite[] pigeonSprites = Resources.LoadAll<Sprite>("peggy_sprite_sheet");

            pigeonSpriteMap = new Dictionary<string, Sprite>();
            foreach (Sprite s in pigeonSprites) {
                pigeonSpriteMap[s.name] = s;
            }
        }
        if (rat != null) {
            rat_audioSource = rat.GetComponent<AudioSource>();
            Sprite[] ratSprites = Resources.LoadAll<Sprite>("rocky_sprite_sheet");

            ratSpriteMap = new Dictionary<string, Sprite>();
            foreach (Sprite s in ratSprites) {
                ratSpriteMap[s.name] = s;
            }
        }
        if (SceneManager.GetActiveScene().name != "Subway Scene") {
            StartStory();
        } else {
            dialogueBox.SetActive(false);
            nameBox.SetActive(false);
            internalBox.SetActive(false);
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
        if (innerVoice)
        {
            internalBox.SetActive(true);
            dialogueBoxImage.enabled = false;
            dialogueText.enabled = false;
        } else
        {
            nameBox.SetActive(true);
            dialogueBoxImage.enabled = true;
            dialogueText.enabled = true;
        }
        
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
                SceneManager.LoadScene("EndScene");
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
        internalBox.SetActive(false);
        choicesBackground.SetActive(false);
   } 

    void HandleChoices() {
        dialogueBox.SetActive(false);
        nameBox.SetActive(false);
        internalBox.SetActive(false);
        choicesBackground.SetActive(true);
        
        // Enable mouse cursor
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor from the center
        Cursor.visible = true; // Make the cursor visible

        cameraController.LockAndCenter();

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
            //cameraController.UnlockCamera();
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
        if (innerVoice)
        {
            internalText.text = text;
        } else
        {
            dialogueText.text = text;
        }
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
                        switch (tagValue)
                        {
                            case "Peggy":
                                nameBoxImage.color = peggyColor;
                                break;
                            case "Rocky":
                                nameBoxImage.color = rockyColor;
                                break;
                            case "Therapist":
                                nameBoxImage.color = therapistColor;
                                break;
                            default:
                                nameBoxImage.color = defaultColor;
                                break;
                        }
                        break;
                    case "pigeon":
                        if (pigeon) {
                            Debug.Log("pigeoning " + tagValue);
                            if (pigeonSpriteMap.TryGetValue(tagValue, out Sprite pigeon_sprite)) {
                                if (pigeon.GetComponent<Animator>() != null)
                                    pigeon.GetComponent<Animator>().enabled = false;
                                pigeon.GetComponent<SpriteRenderer>().sprite = pigeon_sprite;
                            } else {
                                Debug.LogWarning("Sprite not found in sheet: " + tagValue);
                            }
                        }
                        break;
                    case "rat":
                        if (rat) {
                            if (ratSpriteMap.TryGetValue(tagValue, out Sprite rat_sprite)) {
                                rat.GetComponent<SpriteRenderer>().sprite = rat_sprite;
                            } else {
                                Debug.LogWarning("Sprite not found in sheet: " + tagValue);
                            }
                        }
                        break;
                    case "va":
                        if (MainMenu.useVoiceActing) {
                            // Debug.Log("playing audio file " + tagValue);
                            AudioClip voice_acting = Resources.Load<AudioClip>("VoiceOver/" + tagValue);
                            if (tagValue.Contains("Peggy")) {
                                if (pigeon_audioSource != null) {
                                    pigeon_audioSource.clip = voice_acting;
                                    pigeon_audioSource.Play();
                                }
                            } else if (tagValue.Contains("Rocky")) {
                                if (rat_audioSource != null) {
                                    rat_audioSource.clip = voice_acting;
                                    rat_audioSource.Play();
                                }
                            }
                        }
                        break;
                    case "npc":
                        if (tagValue.StartsWith("peggy_to="))
                        {
                            if (peggyMover == null || waypointMap == null)
                            {
                                Debug.LogWarning("Missing peggyMover or waypointMap reference.");
                                break;
                            }

                            string wpId = tagValue.Substring("peggy_to=".Length).Trim();
                            Transform wp = waypointMap.Get(wpId);

                            if (wp == null)
                            {
                                Debug.LogWarning($"Waypoint '{wpId}' not found.");
                                break;
                            }

                            peggyMover.SetWaypoint(wp);
                        }
                        break;
                    case "hold":
                        if (tagValue == "peggy_arrived")
                        {
                            pigeon.GetComponent<Animator>().enabled = true;
                            holdUntilPeggyArrived = true;
                        }
                        break;
                    default:
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
                Destroy(child.gameObject);
            }
        }
    }

    public void SetInkScene(string sceneName) {
        levelManager.SetScene(sceneName);
    }

    public static void SetCarnivalEnding(bool goodEnding) {
        carnivalEnding = goodEnding;
    }

    public void PauseForDistance(bool pause)
    {
        pausedForDistance = pause;

        if (pause)
        {
            dialogueBox.SetActive(false);
            nameBox.SetActive(false);
        }
        else
        {
            dialogueBox.SetActive(true);
            nameBox.SetActive(true);
        }
    }

    private void OnPeggyArrived()
    {
        Debug.Log("Peggy has arrived at her destination.");
        holdUntilPeggyArrived = false;

        if (!pausedForDistance)
        {
            dialogueBox.SetActive(true);
            nameBox.SetActive(true);
            justClicked = false;
        }
    }


    public void SetInternalVoice(bool internalVoice) {
        innerVoice = internalVoice;
    }
}
