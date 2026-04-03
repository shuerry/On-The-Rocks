using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fishing minigame UI — pure visual, no text.
/// Features: stick fishing rod with tapered segments, fishing line with hook,
/// water depth layers, rising bubbles, surface ripples, sparkle particles,
/// animated Rocky & Peggy. All built in code at runtime.
/// </summary>
[RequireComponent(typeof(FishingMinigame))]
public class FishingMinigameUI : MonoBehaviour
{
    [Header("Sprites (auto-loaded from Resources if null)")]
    public Sprite peggySprite;
    public Sprite rockySprite;

    [Header("Colours")]
    public Color waterDeep = new Color(0.04f, 0.12f, 0.25f, 0.95f);
    public Color waterMid = new Color(0.08f, 0.22f, 0.40f, 0.90f);
    public Color waterLight = new Color(0.15f, 0.35f, 0.55f, 0.80f);
    public Color surfaceColor = new Color(0.45f, 0.78f, 1f, 0.55f);
    public Color barBorderColor = new Color(0.35f, 0.6f, 0.85f, 0.65f);
    public Color catchZoneColor = new Color(0.2f, 0.7f, 0.35f, 0.35f);
    public Color catchZoneActiveColor = new Color(0.1f, 1f, 0.3f, 0.65f);
    public Color progBgColor = new Color(0.08f, 0.08f, 0.08f, 0.85f);
    public Color progFillColor = new Color(1f, 0.72f, 0.12f, 1f);
    public Color progDangerColor = new Color(0.95f, 0.22f, 0.12f, 1f);
    public Color progFullColor = new Color(0.25f, 1f, 0.35f, 1f);
    public Color stickBase = new Color(0.55f, 0.36f, 0.18f, 1f);
    public Color stickTip = new Color(0.72f, 0.52f, 0.28f, 1f);
    public Color stickDark = new Color(0.38f, 0.24f, 0.1f, 1f);
    public Color lineCol = new Color(0.9f, 0.9f, 0.9f, 0.6f);
    public Color hookCol = new Color(0.7f, 0.7f, 0.7f, 0.9f);
    public Color bubbleCol = new Color(0.65f, 0.88f, 1f, 0.35f);
    public Color sparkleCol = new Color(1f, 1f, 0.8f, 0.6f);
    public Color heartCol = new Color(1f, 0.35f, 0.45f, 0.9f);
    public Color splashCol = new Color(0.7f, 0.92f, 1f, 0.7f);

    // Runtime refs
    private FishingMinigame game;
    private RectTransform barRect, catchZoneRect, fishIconRect;
    private RectTransform progressFillRect;
    private Image progressFillImage, catchZoneImage, barBorderImage;
    private GameObject progBorderGO, progBgGO;
    private GameObject barGlowGO, overlayGO;

    // Stick rod
    private RectTransform stickRect, stickGripRect, knotRect;
    private RectTransform fishingLineRect, hookRect;

    // Characters
    private RectTransform rockyRect;
    private Image peggyResultIcon;
    private RectTransform peggyResultRect;

    // Effects
    private Image winFlash;
    private Image[] bubbles;
    private RectTransform[] bubbleRects;
    private float[] bubbleSpeeds;
    private float[] bubbleStartX;
    private Image[] sparkles;
    private RectTransform[] sparkleRects;
    private float[] sparkleSpeeds;
    private Image[] waterBands;

    // Win effects
    private Sprite heartSprite;
    private Image[] splashDrops;
    private RectTransform[] splashDropRects;
    private float[] splashVelY;
    private Image[] hearts;
    private RectTransform[] heartRects;
    private float[] heartSpeeds;
    private RectTransform canvasRoot;
    private Vector2 shakeOffset;
    private float shakeTimer;

    // Layout
    private const float BAR_W = 80f;
    private const float BAR_H = 450f;
    private const float BAR_X = 140f;
    private const float PROG_W = 18f;
    private const float PROG_GAP = 14f;
    private const float FISH_SZ = 55f;
    private const float BORDER = 5f;
    private const float ROCKY_SZ = 220f;
    private const float LINE_W = 2.5f;
    private const float HOOK_W = 8f;
    private const float HOOK_H = 14f;
    private const int BUBBLE_N = 8;
    private const int SPARKLE_N = 5;
    private const float STICK_LEN = 230f;
    private const float STICK_W = 7f;
    private const int SPLASH_N = 6;
    private const int HEART_N = 6;

    private bool waitingToStart = true;
    private float resultTimer, timer;
    private bool showingResult;

    void Start()
    {
        game = GetComponent<FishingMinigame>();
        LoadSprites();
        BuildUI();
    }

    void LoadSprites()
    {
        if (peggySprite == null)
            foreach (Sprite s in Resources.LoadAll<Sprite>("peggy_sprite_sheet"))
                if (s.name.Contains("PeggyP1") || s.name.Contains("PeggyNeturtal"))
                { peggySprite = s; break; }
        if (rockySprite == null)
            foreach (Sprite s in Resources.LoadAll<Sprite>("rocky_sprite_sheet"))
                if (s.name.Contains("RockyP1") || s.name.Contains("RockyBeige"))
                { rockySprite = s; break; }
    }

    void BuildUI()
    {
        // ── Canvas ──
        var go = new GameObject("FishingMinigameCanvas");
        go.transform.SetParent(transform);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        go.AddComponent<GraphicRaycaster>();
        Transform root = go.transform;
        canvasRoot = go.GetComponent<RectTransform>();

        // ── Overlay ──
        var overlay = Img(root, "Overlay", Vector2.zero, new Vector2(1920, 1080),
            new Color(0f, 0.04f, 0.1f, 0.45f));
        overlayGO = overlay.gameObject;

        // ── Rocky (left side) ──
        if (rockySprite != null)
        {
            var ri = Img(root, "Rocky",
                new Vector2(-BAR_X - 50f, -10f),
                new Vector2(ROCKY_SZ, ROCKY_SZ), Color.white);
            ri.sprite = rockySprite;
            ri.preserveAspect = true;
            rockyRect = ri.rectTransform;
        }

        // ── Stick Fishing Rod (single piece, pivot at base) ──
        float rodAngle = -30f;
        float startX = -BAR_X - 50f + ROCKY_SZ * 0.38f; // near Rocky's right hand
        float startY = -10f - 10f; // slightly below Rocky's center

        // Main stick — pivot at the bottom so it rotates from Rocky's hand
        var stick = Img(root, "Stick", new Vector2(startX, startY),
            new Vector2(STICK_W, STICK_LEN), stickBase);
        stickRect = stick.rectTransform;
        stickRect.pivot = new Vector2(0.5f, 0f); // pivot at base
        stickRect.localRotation = Quaternion.Euler(0, 0, rodAngle + 90f);

        // Wood grain line
        Img(stickRect, "Grain", new Vector2(1.5f, STICK_LEN * 0.5f),
            new Vector2(1.5f, STICK_LEN - 14f), stickDark);

        // Lighter tip (overlay on the upper portion)
        Img(stickRect, "Tip", new Vector2(0f, STICK_LEN * 0.75f),
            new Vector2(STICK_W - 1f, STICK_LEN * 0.45f),
            new Color(stickTip.r, stickTip.g, stickTip.b, 0.6f));

        // Grip wrap at the base (slightly wider, darker)
        var grip = Img(stickRect, "Grip", new Vector2(0f, 18f),
            new Vector2(STICK_W + 4f, 36f), stickDark);
        stickGripRect = grip.rectTransform;

        // Knot at tip
        var knot = Img(stickRect, "Knot", new Vector2(0f, STICK_LEN - 2f),
            new Vector2(6f, 6f), stickDark);
        knotRect = knot.rectTransform;

        // ── Fishing Line (from knot to catch zone) ──
        var line = Img(root, "Line", new Vector2(BAR_X * 0.45f, 30f),
            new Vector2(LINE_W, 200f), lineCol);
        fishingLineRect = line.rectTransform;

        // ── Hook (small L-shape at end of line) ──
        var hook = Img(root, "Hook", Vector2.zero,
            new Vector2(HOOK_W, HOOK_H), hookCol);
        hookRect = hook.rectTransform;

        // ── Bar border (outer glow) ──
        barBorderImage = Img(root, "BarBorder",
            new Vector2(BAR_X, 0f),
            new Vector2(BAR_W + BORDER * 2, BAR_H + BORDER * 2),
            barBorderColor);

        // Inner subtle glow ring
        var glow = Img(root, "BarGlow", new Vector2(BAR_X, 0f),
            new Vector2(BAR_W + BORDER * 4, BAR_H + BORDER * 4),
            new Color(barBorderColor.r, barBorderColor.g, barBorderColor.b, 0.08f));
        barGlowGO = glow.gameObject;

        // ── Bar background (layered water depth) ──
        var barBg = Img(root, "BarBg", new Vector2(BAR_X, 0f),
            new Vector2(BAR_W, BAR_H), waterDeep);
        barRect = barBg.rectTransform;

        // Water depth gradient bands
        waterBands = new Image[5];
        for (int i = 0; i < 5; i++)
        {
            float normY = i / 4f; // 0 = bottom, 1 = top
            float bandY = normY * BAR_H - BAR_H * 0.5f;
            Color bandC = Color.Lerp(waterDeep, waterLight, normY);
            bandC.a = 0.15f + normY * 0.1f;
            waterBands[i] = Img(barRect, "WBand" + i,
                new Vector2(0f, bandY),
                new Vector2(BAR_W - 2f, BAR_H * 0.12f), bandC);
            waterBands[i].rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            waterBands[i].rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        }

        // ── Bubbles ──
        bubbles = new Image[BUBBLE_N];
        bubbleRects = new RectTransform[BUBBLE_N];
        bubbleSpeeds = new float[BUBBLE_N];
        bubbleStartX = new float[BUBBLE_N];
        for (int i = 0; i < BUBBLE_N; i++)
        {
            float sz = Random.Range(5f, 16f);
            float bx = Random.Range(-BAR_W * 0.35f, BAR_W * 0.35f);
            var b = Img(barRect, "Bub" + i,
                new Vector2(bx, Random.Range(-BAR_H * 0.45f, BAR_H * 0.45f)),
                new Vector2(sz, sz), bubbleCol);
            b.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            b.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            bubbles[i] = b;
            bubbleRects[i] = b.rectTransform;
            bubbleSpeeds[i] = Random.Range(12f, 35f);
            bubbleStartX[i] = bx;
        }

        // ── Catch Zone ──
        float czH = BAR_H * game.catchZoneSize;
        catchZoneImage = Img(barRect, "CatchZone", Vector2.zero,
            new Vector2(BAR_W - 6f, czH), catchZoneColor);
        catchZoneRect = catchZoneImage.rectTransform;
        catchZoneRect.anchorMin = new Vector2(0.5f, 0f);
        catchZoneRect.anchorMax = new Vector2(0.5f, 0f);
        catchZoneRect.pivot = new Vector2(0.5f, 0.5f);

        // Edge highlights (top + bottom glow lines)
        Img(catchZoneRect, "CZTop",
            new Vector2(0, czH * 0.5f - 1.5f),
            new Vector2(BAR_W - 10f, 3f),
            new Color(1f, 1f, 1f, 0.35f));
        Img(catchZoneRect, "CZBot",
            new Vector2(0, -czH * 0.5f + 1.5f),
            new Vector2(BAR_W - 10f, 3f),
            new Color(1f, 1f, 1f, 0.35f));
        // Inner soft glow
        Img(catchZoneRect, "CZGlow", Vector2.zero,
            new Vector2(BAR_W - 14f, czH - 8f),
            new Color(0.3f, 1f, 0.5f, 0.08f));

        // ── Fish (Peggy) icon ──
        var fish = Img(barRect, "Fish", Vector2.zero,
            new Vector2(FISH_SZ, FISH_SZ), Color.white);
        fishIconRect = fish.rectTransform;
        fishIconRect.anchorMin = new Vector2(0.5f, 0f);
        fishIconRect.anchorMax = new Vector2(0.5f, 0f);
        fishIconRect.pivot = new Vector2(0.5f, 0.5f);
        if (peggySprite != null)
        {
            fish.sprite = peggySprite;
            fish.preserveAspect = true;
        }

        // ── Progress Bar ──
        float px = BAR_X - BAR_W * 0.5f - PROG_GAP - PROG_W * 0.5f;
        var pBorder = Img(root, "PBorder", new Vector2(px, 0f),
            new Vector2(PROG_W + 4f, BAR_H + 4f),
            new Color(0.35f, 0.35f, 0.35f, 0.45f));
        progBorderGO = pBorder.gameObject;
        var pbg = Img(root, "PBg", new Vector2(px, 0f),
            new Vector2(PROG_W, BAR_H), progBgColor);
        progBgGO = pbg.gameObject;
        var pf = Img(pbg.rectTransform, "PFill", Vector2.zero,
            new Vector2(PROG_W - 4f, 0f), progFillColor);
        progressFillRect = pf.rectTransform;
        progressFillRect.anchorMin = new Vector2(0.5f, 0f);
        progressFillRect.anchorMax = new Vector2(0.5f, 0f);
        progressFillRect.pivot = new Vector2(0.5f, 0f);
        progressFillImage = pf;

        // Progress shimmer line
        Img(pf.rectTransform, "PShimmer",
            new Vector2(PROG_W * 0.15f, 0f),
            new Vector2(2f, 9999f),
            new Color(1f, 1f, 1f, 0.12f));



        // ── Sparkle particles (around progress bar when near win) ──
        sparkles = new Image[SPARKLE_N];
        sparkleRects = new RectTransform[SPARKLE_N];
        sparkleSpeeds = new float[SPARKLE_N];
        for (int i = 0; i < SPARKLE_N; i++)
        {
            var sp = Img(root, "Sparkle" + i,
                new Vector2(px + Random.Range(-20f, 20f), Random.Range(-BAR_H * 0.4f, BAR_H * 0.4f)),
                new Vector2(6f, 6f), new Color(sparkleCol.r, sparkleCol.g, sparkleCol.b, 0f));
            sparkles[i] = sp;
            sparkleRects[i] = sp.rectTransform;
            sparkleSpeeds[i] = Random.Range(30f, 60f);
        }

        // ── Win flash (hidden) ──
        winFlash = Img(root, "WinFlash", Vector2.zero,
            new Vector2(1920, 1080), new Color(1f, 1f, 0.8f, 0f));
        winFlash.raycastTarget = false;

        // ── Splash drops (burst upward on win, hidden) ──
        splashDrops = new Image[SPLASH_N];
        splashDropRects = new RectTransform[SPLASH_N];
        splashVelY = new float[SPLASH_N];
        for (int i = 0; i < SPLASH_N; i++)
        {
            var sd = Img(root, "Splash" + i, Vector2.zero,
                new Vector2(8f, 12f), new Color(splashCol.r, splashCol.g, splashCol.b, 0f));
            splashDrops[i] = sd;
            splashDropRects[i] = sd.rectTransform;
        }

        // ── Floating hearts (win celebration, hidden) ──
        heartSprite = MakeHeartSprite(32);
        hearts = new Image[HEART_N];
        heartRects = new RectTransform[HEART_N];
        heartSpeeds = new float[HEART_N];
        for (int i = 0; i < HEART_N; i++)
        {
            var h = Img(root, "Heart" + i, Vector2.zero,
                new Vector2(22f, 22f), new Color(heartCol.r, heartCol.g, heartCol.b, 0f));
            h.sprite = heartSprite;
            h.preserveAspect = true;
            hearts[i] = h;
            heartRects[i] = h.rectTransform;
            heartSpeeds[i] = Random.Range(40f, 75f);
        }

        // ── Peggy result icon (hidden) ──
        if (peggySprite != null)
        {
            peggyResultIcon = Img(root, "PeggyWin",
                new Vector2(0f, 70f), new Vector2(280f, 280f), Color.white);
            peggyResultIcon.sprite = peggySprite;
            peggyResultIcon.preserveAspect = true;
            peggyResultRect = peggyResultIcon.rectTransform;
            peggyResultIcon.gameObject.SetActive(false);
        }
    }

    // ══════════════════════════════════════════════════
    //  UPDATE
    // ══════════════════════════════════════════════════

    void Update()
    {
        timer += Time.deltaTime;
        AnimateBubbles();

        // Screen shake offset
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            float mag = shakeTimer * 12f;
            shakeOffset = new Vector2(
                Mathf.Sin(timer * 55f) * mag,
                Mathf.Cos(timer * 43f) * mag);
            if (canvasRoot != null)
                canvasRoot.anchoredPosition = shakeOffset;
        }
        else if (shakeOffset != Vector2.zero)
        {
            shakeOffset = Vector2.zero;
            if (canvasRoot != null)
                canvasRoot.anchoredPosition = Vector2.zero;
        }


        if (waitingToStart)
        {
            // Pulse border invitingly
            float p = 0.35f + 0.65f * Mathf.Sin(timer * 2.5f);
            barBorderImage.color = Color.Lerp(barBorderColor * 0.3f, barBorderColor, p);

            // Gently bob the stick
            AnimateStickIdle();

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                waitingToStart = false;
                game.StartFishing();
            }
            return;
        }

        if (game.isActive)
        {
            UpdateGameplay();
            AnimateSparkles();
        }
        else if (game.isComplete)
            HandleResult();
    }

    // ══════════════════════════════════════════════════
    //  ANIMATIONS
    // ══════════════════════════════════════════════════

    void AnimateStickIdle()
    {
        if (stickRect == null) return;
        float bob = Mathf.Sin(timer * 1.5f) * 4f;
        float angle = -30f + Mathf.Sin(timer * 1.2f) * 2f;
        float handX = -BAR_X - 50f + ROCKY_SZ * 0.38f;
        float handY = -10f - 10f;
        stickRect.anchoredPosition = new Vector2(handX, handY + bob);
        stickRect.localRotation = Quaternion.Euler(0, 0, angle + 90f);
    }

    void AnimateStickGameplay(bool holding)
    {
        if (stickRect == null) return;
        float targetAngle = holding ? -22f : -35f;
        float targetBob = holding ? 4f : -4f;
        float handX = -BAR_X - 50f + ROCKY_SZ * 0.38f;
        float handY = -10f - 10f;
        float dt = Time.deltaTime;

        float curAngle = stickRect.localRotation.eulerAngles.z - 90f;
        if (curAngle > 180f) curAngle -= 360f;
        float rodAngle = Mathf.Lerp(curAngle, targetAngle, dt * 10f);

        stickRect.anchoredPosition = Vector2.Lerp(
            stickRect.anchoredPosition,
            new Vector2(handX, handY + targetBob), dt * 10f);
        stickRect.localRotation = Quaternion.Euler(0, 0, rodAngle + 90f);
    }

    void AnimateBubbles()
    {
        if (bubbleRects == null) return;
        for (int i = 0; i < BUBBLE_N; i++)
        {
            Vector2 pos = bubbleRects[i].anchoredPosition;
            pos.y += bubbleSpeeds[i] * Time.deltaTime;
            pos.x = bubbleStartX[i] + Mathf.Sin(timer * 1.8f + i * 1.1f) * 4f;
            if (pos.y > BAR_H * 0.5f)
            {
                pos.y = -BAR_H * 0.48f;
                bubbleStartX[i] = Random.Range(-BAR_W * 0.35f, BAR_W * 0.35f);
                pos.x = bubbleStartX[i];
                float sz = Random.Range(5f, 16f);
                bubbleRects[i].sizeDelta = new Vector2(sz, sz);
            }
            bubbleRects[i].anchoredPosition = pos;
            float normY = (pos.y + BAR_H * 0.5f) / BAR_H;
            float alpha = bubbleCol.a * (1f - normY * 0.8f);
            bubbles[i].color = new Color(bubbleCol.r, bubbleCol.g, bubbleCol.b, alpha);
        }
    }



    void AnimateSparkles()
    {
        if (sparkleRects == null) return;
        bool nearWin = game.progress >= 0.7f;
        float px = BAR_X - BAR_W * 0.5f - PROG_GAP - PROG_W * 0.5f;

        for (int i = 0; i < SPARKLE_N; i++)
        {
            if (!nearWin)
            {
                sparkles[i].color = new Color(sparkleCol.r, sparkleCol.g, sparkleCol.b, 0f);
                continue;
            }
            Vector2 pos = sparkleRects[i].anchoredPosition;
            pos.y += sparkleSpeeds[i] * Time.deltaTime;
            pos.x = px + Mathf.Sin(timer * 3f + i * 2f) * 18f;
            if (pos.y > BAR_H * 0.5f + 20f)
                pos.y = -BAR_H * 0.4f;
            sparkleRects[i].anchoredPosition = pos;
            float twinkle = 0.3f + 0.5f * Mathf.Sin(timer * 8f + i * 1.5f);
            sparkles[i].color = new Color(sparkleCol.r, sparkleCol.g, sparkleCol.b, twinkle);
            sparkleRects[i].localRotation = Quaternion.Euler(0, 0, timer * 90f + i * 45f);
        }
    }

    // ══════════════════════════════════════════════════
    //  GAMEPLAY VISUALS
    // ══════════════════════════════════════════════════

    void UpdateGameplay()
    {
        float dt = Time.deltaTime;
        bool holding = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);

        // ── Catch zone ──
        float czY = game.catchZonePosition * BAR_H;
        catchZoneRect.anchoredPosition = new Vector2(0f, czY);
        catchZoneRect.sizeDelta = new Vector2(BAR_W - 6f, BAR_H * game.catchZoneSize);

        float halfZone = game.catchZoneSize * 0.5f;
        bool fishInZone = game.fishPosition >= game.catchZonePosition - halfZone
                       && game.fishPosition <= game.catchZonePosition + halfZone;
        Color targetCZ = fishInZone ? catchZoneActiveColor : catchZoneColor;
        catchZoneImage.color = Color.Lerp(catchZoneImage.color, targetCZ, dt * 12f);

        // ── Bar border ──
        if (fishInZone)
        {
            float glow = 0.55f + 0.45f * Mathf.Sin(timer * 7f);
            barBorderImage.color = Color.Lerp(barBorderColor, catchZoneActiveColor, glow);
        }
        else
            barBorderImage.color = Color.Lerp(barBorderImage.color, barBorderColor, dt * 5f);

        // ── Peggy ──
        float fishY = game.fishPosition * BAR_H;
        fishIconRect.anchoredPosition = new Vector2(0f, fishY);
        float wobble = Mathf.Sin(timer * 3.5f) * 6f;
        fishIconRect.localRotation = Quaternion.Euler(0, 0, wobble);
        float ts = fishInZone ? 1.2f : 1f;
        float cs = fishIconRect.localScale.x;
        fishIconRect.localScale = Vector3.one * Mathf.Lerp(cs, ts, dt * 8f);

        // ── Progress ──
        float fillH = game.progress * (BAR_H - 4f);
        progressFillRect.sizeDelta = new Vector2(PROG_W - 4f, fillH);
        Color progTarget = game.progress >= 0.85f ? progFullColor :
                           game.progress < 0.25f ? progDangerColor : progFillColor;
        progressFillImage.color = Color.Lerp(progressFillImage.color, progTarget, dt * 5f);

        // ── Stick rod reacts ──
        AnimateStickGameplay(holding);

        // ── Fishing line + hook ──
        if (fishingLineRect != null)
        {
            // Line from rod tip to hook near catch zone
            // Get world-space knot position from the stick transform
            Vector2 knotLocal = new Vector2(0f, STICK_LEN - 2f);
            Vector3 knotWorld = stickRect.TransformPoint(knotLocal);
            Vector3 knotInRoot = stickRect.parent.InverseTransformPoint(knotWorld);
            float knotX = knotInRoot.x;
            float knotY = knotInRoot.y;
            float hookX = BAR_X;
            float hookY = -BAR_H * 0.5f + czY;

            float dx = hookX - knotX;
            float dy = hookY - knotY;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

            fishingLineRect.anchoredPosition = new Vector2(
                (knotX + hookX) * 0.5f,
                (knotY + hookY) * 0.5f);
            fishingLineRect.sizeDelta = new Vector2(LINE_W, dist);
            fishingLineRect.localRotation = Quaternion.Euler(0, 0, angle + 90f);

            // Hook at the end
            hookRect.anchoredPosition = new Vector2(hookX, hookY);
            float hookWobble = Mathf.Sin(timer * 4f) * 8f;
            hookRect.localRotation = Quaternion.Euler(0, 0, hookWobble);
        }

        // ── Rocky reacts ──
        if (rockyRect != null)
        {
            float bounce = fishInZone ? Mathf.Sin(timer * 5f) * 7f : 0f;
            rockyRect.anchoredPosition = Vector2.Lerp(
                rockyRect.anchoredPosition,
                new Vector2(-BAR_X - 50f, -10f + bounce), dt * 8f);
        }
    }

    // ══════════════════════════════════════════════════
    //  WIN STATE
    // ══════════════════════════════════════════════════

    void HandleResult()
    {
        if (!showingResult)
        {
            showingResult = true;
            barRect.gameObject.SetActive(false);
            barBorderImage.gameObject.SetActive(false);
            if (barGlowGO != null) barGlowGO.SetActive(false);
            if (overlayGO != null) overlayGO.SetActive(false);
            if (progBorderGO != null) progBorderGO.SetActive(false);
            if (progBgGO != null) progBgGO.SetActive(false);
            if (fishingLineRect != null) fishingLineRect.gameObject.SetActive(false);
            if (hookRect != null) hookRect.gameObject.SetActive(false);
            if (stickRect != null) stickRect.gameObject.SetActive(false);
            for (int i = 0; i < SPARKLE_N; i++)
                if (sparkles[i] != null) sparkles[i].gameObject.SetActive(false);

            if (peggyResultIcon != null)
                peggyResultIcon.gameObject.SetActive(true);

            winFlash.color = new Color(1f, 1f, 0.8f, 0.75f);

            // Trigger screen shake
            shakeTimer = 0.4f;

            // Launch splash drops upward
            for (int i = 0; i < SPLASH_N; i++)
            {
                splashDropRects[i].anchoredPosition = new Vector2(
                    Random.Range(-60f, 60f), Random.Range(-20f, 20f));
                splashVelY[i] = Random.Range(180f, 350f);
                splashDrops[i].color = splashCol;
            }

            // Init hearts
            for (int i = 0; i < HEART_N; i++)
            {
                heartRects[i].anchoredPosition = new Vector2(
                    Random.Range(-180f, 180f), Random.Range(-80f, -20f));
                heartSpeeds[i] = Random.Range(40f, 75f);
                hearts[i].color = new Color(heartCol.r, heartCol.g, heartCol.b, 0.85f);
            }
        }

        resultTimer += Time.deltaTime;

        // Fade flash
        if (winFlash.color.a > 0)
        {
            float a = Mathf.Max(0, winFlash.color.a - Time.deltaTime * 0.5f);
            winFlash.color = new Color(1f, 1f, 0.8f, a);
        }

        // Splash drops (gravity)
        for (int i = 0; i < SPLASH_N; i++)
        {
            splashVelY[i] -= 600f * Time.deltaTime; // gravity
            Vector2 pos = splashDropRects[i].anchoredPosition;
            pos.y += splashVelY[i] * Time.deltaTime;
            pos.x += Mathf.Sin(timer * 3f + i) * 0.5f;
            splashDropRects[i].anchoredPosition = pos;
            if (pos.y < -400f)
                splashDrops[i].color = new Color(splashCol.r, splashCol.g, splashCol.b, 0f);
        }

        // Floating hearts rise
        for (int i = 0; i < HEART_N; i++)
        {
            Vector2 pos = heartRects[i].anchoredPosition;
            pos.y += heartSpeeds[i] * Time.deltaTime;
            pos.x += Mathf.Sin(timer * 2f + i * 1.3f) * 0.8f;
            heartRects[i].anchoredPosition = pos;
            float fade = Mathf.Clamp01(1f - (pos.y - 100f) / 300f);
            hearts[i].color = new Color(heartCol.r, heartCol.g, heartCol.b, fade * 0.85f);
            float hs = 0.8f + 0.3f * Mathf.Sin(timer * 4f + i);
            heartRects[i].localScale = Vector3.one * hs;
            heartRects[i].localRotation = Quaternion.Euler(0, 0, Mathf.Sin(timer * 2f + i) * 15f);
        }

        // Peggy celebration
        if (peggyResultRect != null)
        {
            float bounce = Mathf.Abs(Mathf.Sin(resultTimer * 2.5f)) * 30f;
            peggyResultRect.anchoredPosition = new Vector2(0f, 70f + bounce);
            float spin = Mathf.Sin(resultTimer * 1.8f) * 12f;
            peggyResultRect.localRotation = Quaternion.Euler(0, 0, spin);
            float s = 1f + 0.06f * Mathf.Sin(resultTimer * 5f);
            peggyResultRect.localScale = Vector3.one * s;
        }

        // Rocky happy
        if (rockyRect != null)
        {
            float happy = Mathf.Abs(Mathf.Sin(resultTimer * 3.5f)) * 14f;
            rockyRect.anchoredPosition = new Vector2(-BAR_X - 50f, -10f + happy);
        }

        if (resultTimer > 2.5f)
        {
            var mgr = FindFirstObjectByType<FishingMinigameLevelManager>();
            if (mgr != null) mgr.OnFishingComplete(game.isWon);
        }
    }

    // ══════════════════════════════════════════════════
    //  FACTORY
    // ══════════════════════════════════════════════════

    Image Img(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return img;
    }

    /// <summary>Creates a heart-shaped sprite at runtime.</summary>
    static Sprite MakeHeartSprite(int sz)
    {
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float half = sz * 0.5f;
        float r = sz * 0.25f; // radius of each lobe
        // Two circle centers (upper lobes)
        Vector2 cL = new Vector2(half - r, sz - r - 1);
        Vector2 cR = new Vector2(half + r, sz - r - 1);
        // Bottom tip
        Vector2 tip = new Vector2(half, sz * 0.12f);
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < sz; y++)
        {
            for (int x = 0; x < sz; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                bool inside = false;
                // Upper lobes: two circles
                if ((p - cL).sqrMagnitude <= r * r || (p - cR).sqrMagnitude <= r * r)
                    inside = true;
                // Lower body: triangle from outer edges of lobes to tip
                if (!inside)
                {
                    float bottom = cL.y; // horizontal dividing line
                    if (p.y <= bottom && p.y >= tip.y)
                    {
                        float t = (bottom - p.y) / (bottom - tip.y);
                        float left = Mathf.Lerp(cL.x - r, tip.x, t);
                        float right = Mathf.Lerp(cR.x + r, tip.x, t);
                        if (p.x >= left && p.x <= right)
                            inside = true;
                    }
                }
                tex.SetPixel(x, y, inside ? Color.white : clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), sz);
    }
}
