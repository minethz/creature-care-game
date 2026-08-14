using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneProbe
{
    [MenuItem("Tools/Probe Scene")]
    public static void Probe()
    {
        var path = "Assets/Scenes/SampleScene.unity";
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            Debug.Log($"SR: {sr.transform.name} pos={sr.transform.position} scale={sr.transform.localScale} " +
                      $"boundsMin={sr.bounds.min} boundsMax={sr.bounds.max} layer={sr.sortingLayerName} order={sr.sortingOrder} sprite={sr.sprite?.name}");
        }
        var cam = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (var c in cam) Debug.Log($"CAM: {c.name} pos={c.transform.position} ortho={c.orthographicSize}");
        var pet = GameObject.Find("MyPet");
        if (pet != null)
        {
            var sr = pet.GetComponent<SpriteRenderer>();
            Debug.Log($"PET: pos={pet.transform.position} boundsMin={sr.bounds.min} boundsMax={sr.bounds.max} feetY={sr.bounds.min.y}");
        }
    }

    [MenuItem("Tools/Capture Scene")]
    public static void Capture()
    {
        var path = "Assets/Scenes/SampleScene.unity";
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        var cam = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)[0];
        var rt = new RenderTexture(800, 450, 24);
        var oldRT = RenderTexture.active;
        var oldTarget = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(800, 450, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 800, 450), 0, 0);
        tex.Apply();
        cam.targetTexture = oldTarget;
        RenderTexture.active = oldRT;
        var bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes("/tmp/unity_scene.png", bytes);
        Debug.Log("Captured /tmp/unity_scene.png");
    }

    [MenuItem("Tools/Capture Play")]
    public static void CapturePlay()
    {
        EditorApplication.EnterPlaymode();
        _playT = 0f;
        EditorApplication.update += PlayPoll;
    }

    private static float _playT;
    private static void PlayPoll()
    {
        if (!EditorApplication.isPlaying) return;
        _playT += 0.02f;
        if (_playT < 2f) return;
        EditorApplication.update -= PlayPoll;
        var cam = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)[0];
        var rt = new RenderTexture(800, 450, 24);
        var oldRT = RenderTexture.active;
        var oldTarget = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(800, 450, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 800, 450), 0, 0);
        tex.Apply();
        cam.targetTexture = oldTarget;
        RenderTexture.active = oldRT;
        System.IO.File.WriteAllBytes("/tmp/unity_runtime.png", tex.EncodeToPNG());
        var pet = GameObject.Find("MyPet");
        if (pet != null)
        {
            var sr = pet.GetComponent<SpriteRenderer>();
            Debug.Log($"RUNTIME PET: pos={pet.transform.position} scale={pet.transform.localScale} " +
                      $"boundsMin={sr.bounds.min} boundsMax={sr.bounds.max} feetY={sr.bounds.min.y} sprite={sr.sprite?.name}");
        }
        Debug.Log("Captured /tmp/unity_runtime.png");
        EditorApplication.ExitPlaymode();
    }
}
