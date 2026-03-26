using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BelayUIController : MonoBehaviour
{
    [Header("References")]
    public BelayController belayController;

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

    [Header("Feedback")]
    public TMP_Text resultText;
    public float resultMessageDuration = 1.25f;
    public bool hideUIWhenInactive = true;

    private bool isEventActive;
    private float resultMessageTimer;

    private void Start()
    {
        if (belayController == null)
        {
            belayController = FindObjectOfType<BelayController>();
        }

        ResetUIImmediate();
    }

    private void Update()
    {
        if (resultText != null && resultText.gameObject.activeSelf)
        {
            resultMessageTimer -= Time.deltaTime;
            if (resultMessageTimer <= 0f)
            {
                resultText.gameObject.SetActive(false);
            }
        }

        if (isEventActive && belayController != null)
        {
            UpdateDistanceUI();
            UpdateLiveStatusText();
            UpdateSweetSpotVisuals();
        }
    }

    public void HandleTensionEventStarted()
    {
        isEventActive = true;

        if (root != null)
            root.SetActive(true);

        SetResultText("", false);
        SetTimerValue(1f);

        if (timerText != null)
            timerText.text = "";

        if (targetMarker != null)
            targetMarker.gameObject.SetActive(true);

        if (sweetSpotBand != null)
            sweetSpotBand.gameObject.SetActive(true);

        UpdateDistanceUI();
        UpdateSweetSpotVisuals();
        UpdateLiveStatusText();
    }

    public void HandleTensionEventSucceeded()
    {
        isEventActive = false;

        SetTimerValue(1f);
        UpdateDistanceUI();

        if (timerText != null)
            timerText.text = "Secured";

        if (tensionStatusText != null)
            tensionStatusText.text = "Good";

        SetResultText("Success", true);

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
            tensionStatusText.text = "Rocky fell";

        SetResultText("Failed", true);

        if (targetMarker != null)
            targetMarker.gameObject.SetActive(false);

        if (sweetSpotBand != null)
            sweetSpotBand.gameObject.SetActive(false);

        if (hideUIWhenInactive && root != null)
            root.SetActive(false);
    }

    public void HandleTimerUpdated(float normalizedRemaining)
    {
        SetTimerValue(normalizedRemaining);

        if (timerText != null && isEventActive && belayController != null)
        {
            float seconds = normalizedRemaining; // normalized display fallback
            timerText.text = $"Time: {(seconds * 100f):0}%";
        }
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
            distanceText.text = $"Distance {current:0.00} / Target {target:0.00}";
        }
    }

    private void UpdateLiveStatusText()
    {
        if (belayController == null || tensionStatusText == null)
            return;

        float currentDistance = belayController.CurrentDistanceToRocky;
        float targetDistance = belayController.CurrentTargetDistance;
        float delta = currentDistance - targetDistance;

        if (Mathf.Abs(delta) <= belayController.tensionTolerance)
        {
            tensionStatusText.text = "In range";
        }
        else if (delta > 0f)
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
        RectTransform area = sliderVisualArea;

        if (area == null && tensionSlider != null)
            area = tensionSlider.GetComponent<RectTransform>();

        if (area == null)
            return;

        float width = area.rect.width;

        float targetNorm = belayController.TargetDistanceNormalized;
        float tolMinNorm = belayController.ToleranceMinNormalized;
        float tolMaxNorm = belayController.ToleranceMaxNormalized;

        if (targetMarker != null)
        {
            SetAnchoredNormalizedX(targetMarker, width, targetNorm);
        }

        if (sweetSpotBand != null)
        {
            float bandCenter = (tolMinNorm + tolMaxNorm) * 0.5f;
            float bandWidthNorm = Mathf.Max(0.01f, tolMaxNorm - tolMinNorm);

            Vector2 size = sweetSpotBand.sizeDelta;
            size.x = width * bandWidthNorm;
            sweetSpotBand.sizeDelta = size;

            SetAnchoredNormalizedX(sweetSpotBand, width, bandCenter);
        }
    }

    private void SetAnchoredNormalizedX(RectTransform rect, float width, float normalized)
    {
        normalized = Mathf.Clamp01(normalized);

        float x = Mathf.Lerp(-width * 0.5f, width * 0.5f, normalized);

        Vector2 pos = rect.anchoredPosition;
        pos.x = x;
        rect.anchoredPosition = pos;
    }

    private void SetTimerValue(float value)
    {
        value = Mathf.Clamp01(value);

        if (timerSlider != null)
            timerSlider.value = value;

        //if (timerFillImage != null)
        //    timerFillImage.fillAmount = value;
    }

    private void SetTensionValue(float value)
    {
        value = Mathf.Clamp01(value);

        if (tensionSlider != null)
            tensionSlider.value = value;

        if (tensionFillImage != null)
            tensionFillImage.fillAmount = value;
    }

    private void SetResultText(string message, bool show)
    {
        if (resultText == null)
            return;

        resultText.text = message;
        resultText.gameObject.SetActive(show);

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
            resultText.gameObject.SetActive(false);

        if (targetMarker != null)
            targetMarker.gameObject.SetActive(false);

        if (sweetSpotBand != null)
            sweetSpotBand.gameObject.SetActive(false);

        if (hideUIWhenInactive && root != null)
            root.SetActive(false);
        else if (root != null)
            root.SetActive(true);
    }
}