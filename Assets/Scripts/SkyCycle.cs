using UnityEngine;

public class SkyCycle : MonoBehaviour
{
    private static readonly string[] SkyNames =
    {
        "6before start morning",
        "1morning",
        "2clear sky - mid time",
        "3Cloudy- near to evening",
        "4evening",
        "5Night"
    };

    private static readonly float[] PhaseStart =
    {
        0.00f, 0.10f, 0.28f, 0.42f, 0.55f, 0.72f
    };

    private const float CrossfadeSeconds = 1.5f;
    private const float SkyLocalZ = 16f;

    private Pet pet;
    private SpriteRenderer[] layers;
    private int currentPhase = -1;
    private int activeLayer;
    private bool fading;
    private float fadeTime;

    private void Awake()
    {
        pet = GetComponent<Pet>();
    }

    private void Start()
    {
        Camera cam = Camera.main;
        if (cam == null || SkyNames.Length == 0) return;

        Sprite first = Resources.Load<Sprite>("Sky/" + SkyNames[0]);
        if (first == null) return;

        GameObject root = new GameObject("SkyBackground");
        root.transform.SetParent(cam.transform, false);
        root.transform.localPosition = new Vector3(0f, 0f, SkyLocalZ);

        float worldH = cam.orthographic ? cam.orthographicSize * 2f : 20f;
        float worldW = worldH * cam.aspect;
        float scale = Mathf.Max(worldH / first.bounds.size.y, worldW / first.bounds.size.x) * 1.05f;

        layers = new SpriteRenderer[2];
        for (int i = 0; i < 2; i++)
        {
            GameObject go = new GameObject("SkyLayer" + i);
            go.transform.SetParent(root.transform, false);
            go.transform.localScale = Vector3.one * scale;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = first;
            sr.sortingOrder = -1000;
            sr.color = new Color(1f, 1f, 1f, i == 0 ? 1f : 0f);
            layers[i] = sr;
        }

        currentPhase = PhaseIndex(pet != null ? pet.DayProgress : 0f);
    }

    private void Update()
    {
        if (pet == null || layers == null) return;

        int target = PhaseIndex(pet.DayProgress);
        if (target != currentPhase)
        {
            CrossfadeTo(target);
            currentPhase = target;
        }

        if (fading)
        {
            fadeTime += Time.deltaTime;
            float t = Mathf.Clamp01(fadeTime / CrossfadeSeconds);
            int inLayer = 1 - activeLayer;
            layers[inLayer].color = new Color(1f, 1f, 1f, t);
            layers[activeLayer].color = new Color(1f, 1f, 1f, 1f - t);
            if (t >= 1f)
            {
                layers[activeLayer].color = new Color(1f, 1f, 1f, 0f);
                activeLayer = inLayer;
                fading = false;
            }
        }
    }

    private static int PhaseIndex(float progress)
    {
        int idx = 0;
        for (int i = 0; i < PhaseStart.Length; i++)
        {
            if (progress >= PhaseStart[i]) idx = i;
        }
        return idx;
    }

    private void CrossfadeTo(int target)
    {
        Sprite next = Resources.Load<Sprite>("Sky/" + SkyNames[target]);
        if (next == null) return;
        int inLayer = 1 - activeLayer;
        layers[inLayer].sprite = next;
        layers[inLayer].color = new Color(1f, 1f, 1f, 0f);
        fading = true;
        fadeTime = 0f;
    }
}
