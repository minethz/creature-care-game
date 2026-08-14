using UnityEngine;

public class Bird : MonoBehaviour
{
    [Tooltip("World units per second horizontal speed.")]
    public float speed = 2f;

    [Tooltip("How far the bird bobs up and down while flying.")]
    public float bobAmplitude = 0.4f;

    [Tooltip("How fast the bobbing cycles (cycles per second).")]
    public float bobFrequency = 0.8f;

    [Tooltip("Maximum bank tilt in degrees while climbing/descending.")]
    public float bankTilt = 8f;

    [Tooltip("Minimum seconds between bird groups.")]
    public float cooldownMin = 20f;

    [Tooltip("Maximum seconds between bird groups.")]
    public float cooldownMax = 45f;

    [Tooltip("Chance a group flies right-to-left instead of left-to-right.")]
    [Range(0f, 1f)] public float rightToLeftChance = 0.5f;

    [Tooltip("Chance only one bird appears.")]
    [Range(0f, 1f)] public float chanceOne = 0.5f;

    [Tooltip("Chance two birds appear together.")]
    [Range(0f, 1f)] public float chanceTwo = 0.35f;

    private static int instanceCount;
    private static int groupActiveCount;
    private static int slotTaken;
    private static int slotCount;
    private static int groupDir = 1;
    private static float nextGroupTime;

    private const string BirdSpritePath = "Birds/bird";
    private const int FlockSize = 4;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instanceCount = 0;
        groupActiveCount = 0;
        slotTaken = 0;
        slotCount = 0;
        groupDir = 1;
        nextGroupTime = 0f;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (Object.FindFirstObjectByType<Bird>() != null) return;

        Sprite[] sprites = Resources.LoadAll<Sprite>(BirdSpritePath);
        for (int i = 0; i < FlockSize; i++)
        {
            GameObject go = new GameObject("Bird_" + i);
            go.transform.position = new Vector3(0f, Random.Range(3.5f, 7f), 0f);

            SpriteRenderer birdSr = go.AddComponent<SpriteRenderer>();
            if (sprites != null && sprites.Length > 0)
                birdSr.sprite = sprites[i % sprites.Length];
            birdSr.sortingLayerName = "2Cloud";
            birdSr.sortingOrder = 1;

            go.AddComponent<Bird>();
        }
    }

    private Camera cam;
    private SpriteRenderer sr;
    private float halfWidth;
    private float spriteHalfWidth;
    private float dir;
    private float bobPhase;
    private float speedMult;
    private bool flying;
    private Vector3 basePos;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        instanceCount++;
    }

    private void OnDestroy()
    {
        instanceCount--;
    }

    private void Start()
    {
        cam = Camera.main;
        if (cam == null || sr == null)
        {
            enabled = false;
            return;
        }

        float worldH = cam.orthographic ? cam.orthographicSize * 2f : 20f;
        halfWidth = worldH * cam.aspect * 0.5f;
        spriteHalfWidth = sr.bounds.size.x * 0.5f;
        basePos = transform.position;

        sr.enabled = false;
    }

    private void Update()
    {
        if (flying)
        {
            Fly();
            return;
        }

        bool canStartGroup = groupActiveCount == 0 && slotTaken == 0
                             && Time.time >= nextGroupTime && instanceCount > 0;
        bool canJoinGroup = groupActiveCount > 0 && slotTaken < slotCount;

        if (canStartGroup)
        {
            slotCount = Mathf.Min(RollGroupSize(), instanceCount);
            groupDir = Random.value < rightToLeftChance ? -1 : 1;
            slotTaken++;
            groupActiveCount++;
            StartFlight();
        }
        else if (canJoinGroup)
        {
            slotTaken++;
            groupActiveCount++;
            StartFlight();
        }
    }

    private void Fly()
    {
        bobPhase += bobFrequency * Mathf.PI * 2f * Time.deltaTime;
        float bob = Mathf.Sin(bobPhase) * bobAmplitude;

        float x = transform.position.x + dir * speed * speedMult * Time.deltaTime;
        transform.position = new Vector3(x, basePos.y + bob, basePos.z);

        float verticalSpeed = Mathf.Cos(bobPhase) * bobAmplitude * bobFrequency * Mathf.PI * 2f;
        float tilt = dir * Mathf.Clamp(verticalSpeed * 4f, -bankTilt, bankTilt);
        transform.rotation = Quaternion.Euler(0f, 0f, tilt);

        if ((dir > 0f && x > halfWidth + spriteHalfWidth) ||
            (dir < 0f && x < -halfWidth - spriteHalfWidth))
        {
            EndFlight();
        }
    }

    private void StartFlight()
    {
        dir = groupDir;
        sr.flipX = dir < 0f;

        bobPhase = Random.value * Mathf.PI * 2f;
        speedMult = Random.Range(0.85f, 1.15f);
        transform.rotation = Quaternion.identity;

        float side = dir > 0f ? -halfWidth - spriteHalfWidth : halfWidth + spriteHalfWidth;
        float spread = Random.Range(-2f, 2f);
        transform.position = new Vector3(side + spread, basePos.y, basePos.z);

        sr.enabled = true;
        flying = true;
    }

    private void EndFlight()
    {
        sr.enabled = false;
        flying = false;

        groupActiveCount--;
        if (groupActiveCount <= 0)
        {
            groupActiveCount = 0;
            slotTaken = 0;
            slotCount = 0;
            nextGroupTime = Time.time + Random.Range(cooldownMin, cooldownMax);
        }
    }

    private int RollGroupSize()
    {
        float r = Random.value;
        if (r < chanceOne) return 1;
        if (r < chanceOne + chanceTwo) return 2;
        return 3;
    }
}
