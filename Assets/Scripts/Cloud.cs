using UnityEngine;

public class Cloud : MonoBehaviour
{
    [Tooltip("Speed in world units per second, moving right to left.")]
    public float speed = 1.5f;

    [Tooltip("Spawn again on the right edge once it fully passes the left edge.")]
    public bool loop = true;

    [Tooltip("Starting position: 0 = already off-screen on the right, 1 = fully off-screen on the left.")]
    [Range(0f, 1f)] public float startProgress = 0.5f;

    private Camera cam;
    private SpriteRenderer sr;
    private float halfWidth;
    private float baseY;
    private float baseZ;
    private bool initialized;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        cam = Camera.main;
        if (cam == null || sr == null) return;

        float worldH = cam.orthographic ? cam.orthographicSize * 2f : 20f;
        halfWidth = worldH * cam.aspect * 0.5f;

        baseY = transform.position.y;
        baseZ = transform.position.z;

        if (loop)
        {
            float half = SpriteHalfWidth();
            float startX = Mathf.Lerp(halfWidth + half, -halfWidth - half, startProgress);
            MoveTo(startX);
        }
        initialized = true;
    }

    private void Update()
    {
        if (!initialized || cam == null || sr == null) return;

        float x = transform.position.x - speed * Time.deltaTime;
        MoveTo(x);

        if (loop && x < -halfWidth - SpriteHalfWidth())
        {
            float overflow = x - (-halfWidth - SpriteHalfWidth());
            MoveTo(halfWidth + SpriteHalfWidth() + overflow);
        }
    }

    private float SpriteHalfWidth()
    {
        return sr.bounds.size.x * 0.5f;
    }

    private void MoveTo(float x)
    {
        transform.position = new Vector3(x, baseY, baseZ);
    }
}
