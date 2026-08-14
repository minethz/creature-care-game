using System.Collections.Generic;
using UnityEngine;

public class PetVisual : MonoBehaviour
{
    [Header("Sprite sheets (loaded from Resources/Pet)")]
    public string idleSheet = "interogative-sheet";
    public string happySheet = "wink-sheet";
    public string sadSheet = "bored-sheet";
    public string sickSheet = "cold";
    public string hungrySheet = "anger";
    public string sleepSheet = "Sleep-sheet";
    public string playSheet = "ball";
    public string celebrateSheet = "whatsup-sheet";
    public string eatSheet = "eat";

    [Header("Presentation")]
    public float displayScale = 2.6f;
    public float bobHeight = 0.06f;
    public float bobSpeed = 2f;
    public float baseFps = 10f;
    [Tooltip("How long the eating animation plays after Feed.")]
    public float eatDuration = 2.2f;
    [Tooltip("Frames smaller than this (in pixels) are skipped so the pet keeps a consistent size.")]
    public float minFrameWidth = 100f;

    private static readonly Color White = Color.white;
    private static readonly Color Gray = new Color(0.45f, 0.45f, 0.45f, 1f);

    private Pet pet;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool animatorActive;
    private readonly Dictionary<string, List<Sprite>> sheets = new Dictionary<string, List<Sprite>>();
    private string currentSheet;
    private List<Sprite> frames;
    private int frameIndex;
    private float animTime;
    private float popTimer;
    private float popDuration = 0.28f;
    private float eatTimer;
    private Vector3 basePosition;
    private bool fallenOver;
    private float fallTimer;
    private float celebrateTimer;

    private void Awake()
    {
        pet = GetComponent<Pet>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        transform.localScale = Vector3.one * displayScale;
        basePosition = transform.localPosition;

        if (pet != null)
        {
            pet.StateChanged += OnStateChanged;
            pet.ActionPerformed += OnAction;
            pet.GameOver += OnGameOver;
            pet.Victory += OnVictory;
        }

        currentSheet = idleSheet;
        frames = GetFrames(idleSheet);
        frameIndex = 0;
        spriteRenderer.sprite = frames != null && frames.Count > 0 ? frames[0] : spriteRenderer.sprite;
    }

    private void OnDestroy()
    {
        if (pet != null)
        {
            pet.StateChanged -= OnStateChanged;
            pet.ActionPerformed -= OnAction;
            pet.GameOver -= OnGameOver;
            pet.Victory -= OnVictory;
        }
    }

    private void OnVictory()
    {
        celebrateTimer = 3.5f;
    }

    private void OnStateChanged(PetState previous, PetState next)
    {
        if (next == PetState.Dead) return;
        if (next == PetState.Playing)
        {
            SetAnimation(playSheet, baseFps * 1.4f);
            return;
        }
        if (eatTimer > 0f) return;
        SetAnimationForState(next);
    }

    private void OnAction(PetAction action)
    {
        popTimer = popDuration;
        if (action == PetAction.Feed)
        {
            eatTimer = eatDuration;
        }
        if (action == PetAction.Sleep)
        {
            popDuration = 0.4f;
        }
    }

    private void OnGameOver()
    {
        fallenOver = true;
        fallTimer = 0f;
    }

    private void SetAnimationForState(PetState state)
    {
        switch (state)
        {
            case PetState.Happy: SetAnimation(happySheet, baseFps * 1.1f); break;
            case PetState.Hungry: SetAnimation(hungrySheet, baseFps); break;
            case PetState.Sad: SetAnimation(sadSheet, baseFps * 0.8f); break;
            case PetState.Tired: SetAnimation(idleSheet, baseFps * 0.55f); break;
            case PetState.Sick: SetAnimation(sickSheet, baseFps); break;
            case PetState.Sleeping: SetAnimation(sleepSheet, baseFps * 0.45f); break;
            default: SetAnimation(idleSheet, baseFps); break;
        }
    }

    private void SetAnimation(string sheet, float fps)
    {
        if (sheet == currentSheet) return;
        List<Sprite> next = GetFrames(sheet);
        if (next == null || next.Count == 0) return;
        currentSheet = sheet;
        frames = next;
        frameIndex = 0;
        animTime = 0f;
        spriteRenderer.sprite = frames[0];
    }

    private List<Sprite> GetFrames(string sheetName)
    {
        if (string.IsNullOrEmpty(sheetName)) return null;
        if (sheets.TryGetValue(sheetName, out List<Sprite> cached)) return cached;

        Sprite[] loaded = Resources.LoadAll<Sprite>("Pet/" + sheetName);
        List<Sprite> list = new List<Sprite>();
        if (loaded != null)
        {
            for (int i = 0; i < loaded.Length; i++)
            {
                if (loaded[i].rect.width >= minFrameWidth)
                    list.Add(loaded[i]);
            }
        }
        sheets[sheetName] = list;
        Debug.Log("[PetVisual] sheet \"" + sheetName + "\" -> " + list.Count + " usable frames (of " + (loaded != null ? loaded.Length : 0) + " raw)");
        return list;
    }

    private void Update()
    {
        if (spriteRenderer == null) return;

        bool useAnimator = ShouldUseAnimator();
        SetAnimatorActive(useAnimator);

        if (fallenOver)
        {
            fallTimer += Time.deltaTime;
            float t = Mathf.Clamp01(fallTimer / 0.6f);
            transform.localRotation = Quaternion.Euler(0f, 0f, -90f * t);
            spriteRenderer.color = Color.Lerp(White, Gray, t);
            transform.localPosition = basePosition + Vector3.down * 0.1f * t;
            return;
        }

        if (pet != null && pet.IsGameOver)
        {
            spriteRenderer.color = Gray;
            return;
        }

        bool sleeping = pet != null && pet.IsSleeping;
        bool playing = pet != null && pet.IsPlaying;

        // Slow movement / bob
        float bobPhase = Time.time * bobSpeed;
        float bob = sleeping ? bobHeight * 0.35f * (0.5f + 0.5f * Mathf.Sin(bobPhase * 0.5f))
                             : bobHeight * Mathf.Sin(bobPhase);
        transform.localPosition = basePosition + new Vector3(0f, bob, 0f);
        transform.localRotation = Quaternion.identity;

        // Action pop (scale bounce)
        if (popTimer > 0f)
        {
            popTimer -= Time.deltaTime;
            float p = 1f - Mathf.Clamp01(popTimer / popDuration);
            float scaleMul = 1f + 0.22f * Mathf.Sin(p * Mathf.PI);
            transform.localScale = Vector3.one * displayScale * scaleMul;
        }
        else
        {
            transform.localScale = Vector3.one * displayScale;
        }

        // Frame animation
        if (!useAnimator && frames != null && frames.Count > 0)
        {
            if (eatTimer > 0f)
            {
                eatTimer -= Time.deltaTime;
                SetAnimation(eatSheet, baseFps);
                if (eatTimer <= 0f)
                {
                    SetAnimationForState(pet != null ? pet.State : PetState.Neutral);
                }
            }

            if (celebrateTimer > 0f)
            {
                celebrateTimer -= Time.deltaTime;
                if (celebrateTimer <= 0f)
                {
                    SetAnimationForState(pet != null ? pet.State : PetState.Neutral);
                }
                else
                {
                    SetAnimation(celebrateSheet, baseFps);
                }
            }

            float fps = GetCurrentFps();
            if (fps > 0f)
            {
                animTime += Time.deltaTime;
                int target = Mathf.FloorToInt(animTime * fps);
                if (target < 0) target = 0;
                if (target >= frames.Count) target = frames.Count - 1;
                if (target != frameIndex)
                {
                    frameIndex = target;
                    spriteRenderer.sprite = frames[frameIndex];
                }
            }
            else
            {
                spriteRenderer.sprite = frames[0];
            }
        }

        ApplyTint(sleeping, playing);
    }

    private bool ShouldUseAnimator()
    {
        if (animator == null) return false;
        if (fallenOver) return false;
        if (pet != null && pet.IsGameOver) return false;
        if (celebrateTimer > 0f) return false;
        if (eatTimer > 0f) return false;
        if (pet == null) return true;
        return pet.State == PetState.Neutral;
    }

    private void SetAnimatorActive(bool active)
    {
        if (animator == null || animatorActive == active) return;
        animatorActive = active;
        if (active)
        {
            animator.enabled = true;
            animator.Play("petty");
        }
        else
        {
            animator.enabled = false;
            if (frames != null && frames.Count > 0)
            {
                int i = frameIndex >= 0 && frameIndex < frames.Count ? frameIndex : 0;
                spriteRenderer.sprite = frames[i];
            }
        }
    }

    private float GetCurrentFps()
    {
        if (pet == null) return baseFps;
        switch (pet.State)
        {
            case PetState.Happy: return baseFps * 1.1f;
            case PetState.Sad: return baseFps * 0.8f;
            case PetState.Tired: return baseFps * 0.55f;
            case PetState.Sleeping: return baseFps * 0.45f;
            case PetState.Playing: return baseFps * 1.4f;
            case PetState.Sick: return baseFps;
            default: return baseFps;
        }
    }

    private void ApplyTint(bool sleeping, bool playing)
    {
        Color target;
        if (sleeping)
        {
            target = new Color(0.55f, 0.55f, 0.78f, 1f);
        }
        else
        {
            switch (pet == null ? PetState.Neutral : pet.State)
            {
                case PetState.Sad: target = new Color(0.8f, 0.75f, 0.8f, 1f); break;
                case PetState.Tired: target = new Color(0.8f, 0.8f, 0.9f, 1f); break;
                case PetState.Sick: target = new Color(0.72f, 0.9f, 1f, 1f); break;
                default: target = White; break;
            }
        }
        spriteRenderer.color = Color.Lerp(spriteRenderer.color, target, Time.deltaTime * 8f);
    }
}
