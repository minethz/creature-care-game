using System;
using UnityEngine;

public enum PetState
{
    Neutral,
    Happy,
    Hungry,
    Sad,
    Tired,
    Sick,
    Sleeping,
    Playing,
    Dead
}

public enum PetAction
{
    Feed,
    Play,
    Sleep
}

[Serializable]
public struct PetSaveData
{
    public string petName;
    public float hunger;
    public float happiness;
    public float energy;
    public int day;
    public float dayElapsed;
    public long savedAtUnix;
    public bool started;
}

public class Pet : MonoBehaviour
{
    [Header("Stats")]
    [Tooltip("Max value for every stat (0-100 scale).")]
    public float maxStat = 100f;
    public float hungerDecayPerSecond = 1.4f;
    public float happinessDecayPerSecond = 0.8f;
    public float energyDecayPerSecond = 0.45f;
    [Range(1f, 3f)] public float nightDecayMultiplier = 1.3f;
    [Range(0f, 1f)] public float sleepDecayFactor = 0.2f;

    [Header("State thresholds")]
    public float sickThreshold = 12f;
    public float lowThreshold = 25f;
    public float happyThreshold = 65f;

    [Header("Actions")]
    public float feedAmount = 40f;
    public float feedHappinessBonus = 5f;
    public float feedCooldown = 3f;
    public float playAmount = 35f;
    public float playEnergyCost = 15f;
    public float playCooldown = 4f;
    public float playDuration = 2f;
    public float sleepEnergyPerSecond = 12f;

    [Header("Goal / Days")]
    public int daysToWin = 5;
    public float secondsPerDay = 60f;
    [Range(0f, 1f)] public float nightStartFraction = 0.55f;
    [Range(0f, 1f)] public float nightEndFraction = 0.95f;

    public string PetName { get; private set; } = "Mochi";
    public bool Started { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool HasWon { get; private set; }

    public float Hunger { get; private set; } = 80f;
    public float Happiness { get; private set; } = 80f;
    public float Energy { get; private set; } = 80f;
    public int Day { get; private set; } = 1;
    public float DayProgress { get; private set; }
    public bool IsNight { get; private set; }
    public bool IsSleeping { get; private set; }
    public bool IsPlaying { get; private set; }
    public PetState State { get; private set; } = PetState.Neutral;
    public string DeathReason { get; private set; }

    public float FeedCooldownRemaining { get; private set; }
    public float PlayCooldownRemaining { get; private set; }
    public float SleepCooldownRemaining { get; private set; }

    public event Action<PetState, PetState> StateChanged;
    public event Action<string> Message;
    public event Action<int> DayChanged;
    public event Action<bool> NightChanged;
    public event Action GameOver;
    public event Action Victory;
    public event Action<PetAction> ActionPerformed;
    public event Action<PetAction> ActionBlocked;

    private float dayElapsed;
    private float sleepCooldownRemaining;
    private float playTimer;

    private void Update()
    {
        if (IsGameOver || !Started) return;

        float dt = Time.deltaTime;
        float nightMul = IsNight ? nightDecayMultiplier : 1f;
        float sleepFactor = IsSleeping ? sleepDecayFactor : 1f;

        Hunger = Mathf.Max(0f, Hunger - hungerDecayPerSecond * dt * nightMul * sleepFactor);
        Happiness = Mathf.Max(0f, Happiness - happinessDecayPerSecond * dt * nightMul * sleepFactor);
        Energy = Mathf.Max(0f, Energy - energyDecayPerSecond * dt * nightMul * sleepFactor);

        if (IsSleeping)
        {
            Energy = Mathf.Min(maxStat, Energy + sleepEnergyPerSecond * dt);
            if (Energy >= maxStat - 0.01f)
            {
                IsSleeping = false;
                ShowMessage(PetName + " woke up fully rested!");
            }
        }

        if (IsPlaying)
        {
            playTimer -= dt;
            if (playTimer <= 0f) IsPlaying = false;
        }

        FeedCooldownRemaining = Mathf.Max(0f, FeedCooldownRemaining - dt);
        PlayCooldownRemaining = Mathf.Max(0f, PlayCooldownRemaining - dt);
        sleepCooldownRemaining = Mathf.Max(0f, sleepCooldownRemaining - dt);
        SleepCooldownRemaining = sleepCooldownRemaining;

        TickDay(dt);

        if (Hunger <= 0f) { Die("starvation"); return; }
        if (Happiness <= 0f) { Die("a broken heart"); return; }
        if (Energy <= 0f) { Die("total exhaustion"); return; }

        PetState newState = ComputeState();
        if (newState != State)
        {
            PetState previous = State;
            State = newState;
            StateChanged?.Invoke(previous, newState);
            EmitStateMessage(newState);
        }
    }

    private void TickDay(float dt)
    {
        float previousProgress = DayProgress;
        dayElapsed += dt;

        if (dayElapsed >= secondsPerDay)
        {
            dayElapsed -= secondsPerDay;
            Day++;
            DayChanged?.Invoke(Day);
            ShowMessage("Day " + Day + " begins.");

            if (!HasWon && Day >= daysToWin)
            {
                HasWon = true;
                Victory?.Invoke();
                ShowMessage("Goal reached! " + PetName + " is thriving!");
            }
        }

        DayProgress = dayElapsed / secondsPerDay;

        bool night = DayProgress >= nightStartFraction && DayProgress <= nightEndFraction;
        if (night != IsNight)
        {
            IsNight = night;
            NightChanged?.Invoke(IsNight);
            ShowMessage(IsNight ? "Night falls. Everything decays a bit faster..." : "Morning arrives.");
        }
    }

    private PetState ComputeState()
    {
        if (IsSleeping) return PetState.Sleeping;
        if (IsPlaying) return PetState.Playing;

        float minStat = Mathf.Min(Hunger, Mathf.Min(Happiness, Energy));
        if (minStat <= sickThreshold) return PetState.Sick;
        if (Hunger <= lowThreshold) return PetState.Hungry;
        if (Happiness <= lowThreshold) return PetState.Sad;
        if (Energy <= lowThreshold) return PetState.Tired;
        if (Hunger >= happyThreshold && Happiness >= happyThreshold && Energy >= happyThreshold)
            return PetState.Happy;
        return PetState.Neutral;
    }

    private void EmitStateMessage(PetState state)
    {
        switch (state)
        {
            case PetState.Hungry: ShowMessage(PetName + " is hungry!"); break;
            case PetState.Sad: ShowMessage(PetName + " looks sad..."); break;
            case PetState.Tired: ShowMessage(PetName + " is getting sleepy."); break;
            case PetState.Sick: ShowMessage(PetName + " is feeling unwell!"); break;
            case PetState.Happy: ShowMessage(PetName + " is very happy!"); break;
        }
    }

    private void Die(string reason)
    {
        IsGameOver = true;
        IsSleeping = false;
        IsPlaying = false;
        State = PetState.Dead;
        DeathReason = reason;
        StateChanged?.Invoke(State, PetState.Dead);
        ShowMessage(PetName + " couldn't make it...");
        GameOver?.Invoke();
    }

    // ---------------------------------------------------------------- actions

    public void TryFeed()
    {
        if (IsGameOver || !Started) return;
        if (IsSleeping) { Block(PetAction.Feed, PetName + " is fast asleep."); return; }
        if (IsPlaying) { Block(PetAction.Feed, PetName + " is busy playing."); return; }
        if (FeedCooldownRemaining > 0f) return;
        if (Hunger >= maxStat - 0.01f) { Block(PetAction.Feed, PetName + " isn't hungry right now."); return; }

        Hunger = Mathf.Min(maxStat, Hunger + feedAmount);
        Happiness = Mathf.Min(maxStat, Happiness + feedHappinessBonus);
        FeedCooldownRemaining = feedCooldown;
        ActionPerformed?.Invoke(PetAction.Feed);
        ShowMessage("Yummy! +" + feedAmount + " hunger");
    }

    public void TryPlay()
    {
        if (IsGameOver || !Started) return;
        if (IsSleeping) { Block(PetAction.Play, PetName + " is fast asleep."); return; }
        if (PlayCooldownRemaining > 0f) return;
        if (Energy <= 0f) { Block(PetAction.Play, PetName + " is too exhausted to play."); return; }

        Energy = Mathf.Max(0f, Energy - playEnergyCost);
        Happiness = Mathf.Min(maxStat, Happiness + playAmount);
        PlayCooldownRemaining = playCooldown;
        IsPlaying = true;
        playTimer = playDuration;
        ActionPerformed?.Invoke(PetAction.Play);
        ShowMessage("Wheee! +" + playAmount + " fun");
    }

    public void TrySleepToggle()
    {
        if (IsGameOver || !Started) return;
        if (sleepCooldownRemaining > 0f) return;
        if (IsPlaying) { Block(PetAction.Sleep, PetName + " is busy playing."); return; }

        IsSleeping = !IsSleeping;
        sleepCooldownRemaining = 0.8f;
        ActionPerformed?.Invoke(PetAction.Sleep);
        ShowMessage(IsSleeping ? "Sweet dreams..." : PetName + " woke up.");
    }

    private void Block(PetAction action, string reason)
    {
        ShowMessage(reason);
        ActionBlocked?.Invoke(action);
    }

    private void ShowMessage(string msg)
    {
        Message?.Invoke(msg);
    }

    // ---------------------------------------------------------------- lifecycle

    public void BeginGame(string name)
    {
        PetName = string.IsNullOrWhiteSpace(name) ? "Mochi" : name.Trim();
        Started = true;
        ShowMessage("Welcome, " + PetName + "!");
    }

    public void ResetState()
    {
        Hunger = 80f;
        Happiness = 80f;
        Energy = 80f;
        Day = 1;
        dayElapsed = 0f;
        DayProgress = 0f;
        IsNight = false;
        IsSleeping = false;
        IsPlaying = false;
        IsGameOver = false;
        HasWon = false;
        Started = false;
        DeathReason = null;
        State = PetState.Neutral;
    }

    public PetSaveData CaptureSave()
    {
        return new PetSaveData
        {
            petName = PetName,
            hunger = Hunger,
            happiness = Happiness,
            energy = Energy,
            day = Day,
            dayElapsed = dayElapsed,
            started = Started
        };
    }

    public void ApplySave(PetSaveData d, float elapsedAwaySeconds)
    {
        PetName = string.IsNullOrEmpty(d.petName) ? "Mochi" : d.petName;
        Hunger = d.hunger;
        Happiness = d.happiness;
        Energy = d.energy;
        Day = Mathf.Max(1, d.day);
        dayElapsed = d.dayElapsed;
        DayProgress = Mathf.Clamp01(dayElapsed / secondsPerDay);
        Started = d.started;

        if (Started && elapsedAwaySeconds > 0f)
        {
            float away = Mathf.Min(elapsedAwaySeconds, 8f * 3600f);
            Hunger = Mathf.Max(1f, Hunger - away * hungerDecayPerSecond * 0.5f);
            Happiness = Mathf.Max(1f, Happiness - away * happinessDecayPerSecond * 0.5f);
            Energy = Mathf.Max(1f, Energy - away * energyDecayPerSecond * 0.5f);
        }

        State = ComputeState();
    }
}
