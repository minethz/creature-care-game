using System;
using UnityEngine;

public class PetSave : MonoBehaviour
{
    public float saveInterval = 5f;

    private const string SaveKey = "CreatureCare.Save.v1";
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private Pet pet;
    private float timer;

    private void Awake()
    {
        pet = GetComponent<Pet>();
    }

    private void Start()
    {
        Load();
        if (pet != null) pet.GameOver += ClearSave;
    }

    private void OnDestroy()
    {
        if (pet != null) pet.GameOver -= ClearSave;
    }

    private void Update()
    {
        if (pet == null || !pet.Started || pet.IsGameOver) return;
        timer += Time.deltaTime;
        if (timer >= saveInterval)
        {
            timer = 0f;
            Save();
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) Save();
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    public void Save()
    {
        if (pet == null || !pet.Started || pet.IsGameOver) return;

        PetSaveData data = pet.CaptureSave();
        data.savedAtUnix = NowUnix();
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (pet == null || !PlayerPrefs.HasKey(SaveKey)) return;

        try
        {
            PetSaveData data = JsonUtility.FromJson<PetSaveData>(PlayerPrefs.GetString(SaveKey));
            float elapsedAway = Mathf.Max(0f, NowUnix() - data.savedAtUnix);
            pet.ApplySave(data, elapsedAway);
        }
        catch (Exception)
        {
            PlayerPrefs.DeleteKey(SaveKey);
        }
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }

    private static long NowUnix()
    {
        return (long)(DateTime.UtcNow - UnixEpoch).TotalSeconds;
    }
}
