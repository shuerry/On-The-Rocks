using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BelayUIController : MonoBehaviour
{
    [Header("References")]
    public BelayController belayController;
    public RockClimberController rockClimberController;

    [Header("Root UI")]
    public GameObject root;

    [Header("Timer UI")]
    public Slider timerSlider;
    public Image timerFillImage;
    public TMP_Text timerText;

    [Header("Distance UI")]
    [Tooltip("This slider should represent the player's current rope distance on a real range.")]
    public Slider tensionSlider;

    [Tooltip("Optional fill image if not using a normal slider fill.")]
    public Image tensionFillImage;

    public TMP_Text tensionStatusText;
    public TMP_Text distanceText;

    [Header("Sweet Spot UI")]
    [Tooltip("Marker showing the center target distance.")]
    public RectTransform targetMarker;

    [Tooltip("Band showing the acceptable target zone (target ± tolerance).")]
    public RectTransform sweetSpotBand;

    [Tooltip("Optional parent rect of the slider fill area. If null, uses tensionSlider RectTransform.")]
    public RectTransform sliderVisualArea;

    [Header("Hints")]
    public GameObject hintPanel;

    [Header("Feedback")]
    public GameObject resultPanel;
    public TMP_Text resultText;
    public Image resultBackground;
    public float resultMessageDuration = 1.25f;
    public bool hideUIWhenInactive = true;

    [Header("Audio")]
    public AudioSource source;
    public AudioClip successClip;
    public AudioClip failureClip;
    public AudioClip stretchClip;
    public AudioClip winClip;

    private bool isEventActive;
    private RockClimberController.ClimberState currentState;
    private float resultMessageTimer;
    private bool routeCompleted = false;

    private void Start()
    {
        if (!belayController)
        {
            belayController = FindAnyObjectByType<BelayController>();
        }
        if (!rockClimberController)
        {
            rockClimberController = FindAnyObjectByType<RockClimberController>();
        }

        if (!source)
        {
            // Remove this is there is more than one AudioSource in the scene
            source = FindFirstObjectByType<AudioSource>();
        }
        source.clip = stretchClip;

        hintPanel.SetActive(false);

        ConfigureSliders();
        ResetUIImmediate();
    }

    private void Update()
    {
        if (resultText != null && resultPanel.gameObject.activeSelf)
        {
            resultMessageTimer -= Time.deltaTime;
            if (resultMessageTimer <= 0f)
            {
                resultPanel.SetActive(false);
            }
        }

        if (belayController == null)
            return;

        bool controllerSaysActive = belayController.IsTensionEventActive;
        if (controllerSaysActive != isEventActive)
        {
            isEventActive = controllerSaysActive;
        }

        if (currentState != rockClimberController.CurrentState)
        {
            currentState = rockClimberController.CurrentState;
        }

        if (isEventActive)
        {
            UpdateTimerUIFromController();
            UpdateDistanceUI();
            UpdateLiveStatusText();
            UpdateSweetSpotVisuals();
        }

        if (currentState == RockClimberController.ClimberState.Finished && !routeCompleted)
        {
            routeCompleted = true;
            source.PlayOneShot(winClip);
        }
    }

    public void HandleTensionEventStarted()
    {
        isEventActive = true;

        if (root != null)
            root.SetActive(true);

        ConfigureSliders();
        SetResult(true, false);

        if (targetMarker != null)
            targetMarker.gameObject.SetActive(true);

        if (sweetSpotBand != null)
            sweetSpotBand.gameObject.SetActive(true);

        source.clip = stretchClip;
        source.Play();

        UpdateTimerUIFromController();
        UpdateDistanceUI();
        UpdateSweetSpotVisuals();
        UpdateLiveStatusText();
    }

    public void HandleTensionEventSucceeded()
    {
        isEventActive = false;

        SetTimerValue(0f);
        UpdateDistanceUI();

        if (timerText != null)
            timerText.text = "Secured";

        if (tensionStatusText != null)
            tensionStatusText.text = "Good";

        SetResult(true, true);

        if (targetMarker != null)
            targetMarker.gameObject.SetActive(false);

        if (sweetSpotBand != null)
            sweetSpotBand.gameObject.SetActive(false);

        if (hideUIWhenInactive && root != null)
            root.SetActive(false);
    }

    public void HandleTensionEventFailed()
    {
        isEventActive = false;

        SetTimerValue(0f);

        if (timerText != null)
            timerText.text = "Too late";

        if (tensionStatusText != null)
            tensionStatusText.text = "Rocky fell!";

        SetResult(false, true);

        if (targetMarker != null)
            targetMarker.gameObject.SetActive(false);

        if (sweetSpotBand != null)
            sweetSpotBand.gameObject.SetActive(false);

        if (hideUIWhenInactive && root != null)
            root.SetActive(false);
    }

    private void ConfigureSliders()
    {
        if (timerSlider != null)
        {
            timerSlider.minValue = 0f;
            timerSlider.maxValue = 1f;
            timerSlider.wholeNumbers = false;
        }

        if (tensionSlider != null)
        {
            tensionSlider.minValue = 0f;
            tensionSlider.maxValue = 1f;
            tensionSlider.wholeNumbers = false;
        }
    }

    private void UpdateTimerUIFromController()
    {
        if (belayController == null)
            return;

        float normalized = belayController.CurrentTimerRemainingNormalized;
        float seconds = belayController.CurrentTimerRemainingSeconds;

        SetTimerValue(normalized);

        if (timerText != null)
            timerText.text = $"{seconds:0.0}s";

        if (timerSlider != null)
            timerSlider.value = normalized;
    }

    private void UpdateDistanceUI()
    {
        if (belayController == null)
            return;

        float currentNormalized = belayController.CurrentDistanceNormalized;
        SetTensionValue(currentNormalized);

        if (distanceText != null)
        {
            float current = belayController.CurrentDistanceToRocky;
            float target = belayController.CurrentTargetDistance;
            float min = belayController.CurrentAcceptableMinDistance;
            float max = belayController.CurrentAcceptableMaxDistance;

            distanceText.text =
                $"Distance {current:0.00} | Target {target:0.00} | Win Zone {min:0.00}-{max:0.00}";
        }
    }

    private void UpdateLiveStatusText()
    {
        if (belayController == null || tensionStatusText == null)
            return;

        float currentDistance = belayController.CurrentDistanceToRocky;
        float minAcceptable = belayController.CurrentAcceptableMinDistance;
        float maxAcceptable = belayController.CurrentAcceptableMaxDistance;

        if (currentDistance >= minAcceptable && currentDistance <= maxAcceptable)
        {
            tensionStatusText.text = "Just Right!";
        }
        else if (currentDistance > maxAcceptable)
        {
            tensionStatusText.text = "Too far";
        }
        else
        {
            tensionStatusText.text = "Too close";
        }
    }

    private void UpdateSweetSpotVisuals()
    {
        if (belayController == null)
            return;

        RectTransform area = sliderVisualArea;

        if (area == null && tensionSlider != null)
            area = tensionSlider.GetComponent<RectTransform>();

        if (area == null)
            return;

        float targetNorm = belayController.TargetDistanceNormalized;
        float tolMinNorm = belayController.ToleranceMinNormalized;
        float tolMaxNorm = belayController.ToleranceMaxNormalized;

        if (targetMarker != null)
        {
            SetMarkerNormalized(targetMarker, targetNorm);
        }

        if (sweetSpotBand != null)
        {
            SetBandNormalized(sweetSpotBand, tolMinNorm, tolMaxNorm);
        }
    }

    private void SetMarkerNormalized(RectTransform rect, float normalized)
    {
        normalized = Mathf.Clamp01(normalized);

        bool isVertical = IsTensionSliderVertical();
        float visualPos = RemapNormalizedForSliderDirection(normalized);

        if (isVertical)
        {
            rect.anchorMin = new Vector2(rect.anchorMin.x, visualPos);
            rect.anchorMax = new Vector2(rect.anchorMax.x, visualPos);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0f);
        }
        else
        {
            rect.anchorMin = new Vector2(visualPos, rect.anchorMin.y);
            rect.anchorMax = new Vector2(visualPos, rect.anchorMax.y);
            rect.anchoredPosition = new Vector2(0f, rect.anchoredPosition.y);
        }
    }

    private void SetBandNormalized(RectTransform rect, float minNorm, float maxNorm)
    {
        minNorm = Mathf.Clamp01(minNorm);
        maxNorm = Mathf.Clamp01(maxNorm);

        float visualMin = RemapNormalizedForSliderDirection(minNorm);
        float visualMax = RemapNormalizedForSliderDirection(maxNorm);

        if (visualMax < visualMin)
        {
            float temp = visualMin;
            visualMin = visualMax;
            visualMax = temp;
        }

        bool isVertical = IsTensionSliderVertical();

        if (isVertical)
        {
            rect.anchorMin = new Vector2(rect.anchorMin.x, visualMin);
            rect.anchorMax = new Vector2(rect.anchorMax.x, visualMax);
            rect.offsetMin = new Vector2(rect.offsetMin.x, 0f);
            rect.offsetMax = new Vector2(rect.offsetMax.x, 0f);
        }
        else
        {
            rect.anchorMin = new Vector2(visualMin, rect.anchorMin.y);
            rect.anchorMax = new Vector2(visualMax, rect.anchorMax.y);
            rect.offsetMin = new Vector2(0f, rect.offsetMin.y);
            rect.offsetMax = new Vector2(0f, rect.offsetMax.y);
        }
    }

    private bool IsTensionSliderVertical()
    {
        if (tensionSlider == null)
            return false;

        return tensionSlider.direction == Slider.Direction.BottomToTop ||
               tensionSlider.direction == Slider.Direction.TopToBottom;
    }

    private float RemapNormalizedForSliderDirection(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);

        if (tensionSlider == null)
            return normalized;

        switch (tensionSlider.direction)
        {
            case Slider.Direction.LeftToRight:
                return normalized;

            case Slider.Direction.RightToLeft:
                return 1f - normalized;

            case Slider.Direction.BottomToTop:
                return normalized;

            case Slider.Direction.TopToBottom:
                return 1f - normalized;

            default:
                return normalized;
        }
    }

    private void SetTimerValue(float value)
    {
        value = Mathf.Clamp01(value);

        if (timerSlider != null)
            timerSlider.value = value;

        if (timerFillImage != null)
            timerFillImage.fillAmount = value;
    }

    private void SetTensionValue(float value)
    {
        value = Mathf.Clamp01(value);

        if (tensionSlider != null)
            tensionSlider.value = value;

        if (tensionFillImage != null)
            tensionFillImage.fillAmount = value;
    }

    private void SetResult(bool success, bool show)
    {
        source.Stop();

        if (!resultText || !resultBackground)
            return;

        if (success)
        {
            resultText.text = "Nice!";
            resultBackground.color = Color.green;
            if (show) source.PlayOneShot(successClip);
        }
        else
        {
            resultText.text = "Oh no!";
            resultBackground.color = Color.red;
            if (show) source.PlayOneShot(failureClip);
        }

        resultPanel.SetActive(show);

        if (show)
            resultMessageTimer = resultMessageDuration;
    }

    public void ResetUIImmediate()
    {
        isEventActive = false;

        SetTimerValue(0f);
        SetTensionValue(0f);

        if (timerText != null)
            timerText.text = "";

        if (tensionStatusText != null)
            tensionStatusText.text = "";

        if (distanceText != null)
            distanceText.text = "";

        if (resultText != null)
            resultText.gameObject.SetActive(true);

        if (resultBackground != null)
            resultBackground.gameObject.SetActive(true);

        if (resultPanel != null)
            resultPanel.gameObject.SetActive(false);

        if (targetMarker != null)
            targetMarker.gameObject.SetActive(false);

        if (sweetSpotBand != null)
            sweetSpotBand.gameObject.SetActive(false);

        if (hideUIWhenInactive && root != null)
            root.SetActive(false);
        else if (root != null)
            root.SetActive(true);
    }

    public void ToggleHintPanel(bool show)
    {
        hintPanel.SetActive(show);
    }
}