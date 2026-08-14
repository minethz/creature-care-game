using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class PlayerAnimationSetup
{
    private const string SpriteFolder = "Assets/png";
    private const string OutputFolder = "Assets/Animations";

    [MenuItem("Tools/Player Setup/Generate Ninja Animations")]
    public static void Generate()
    {
        if (!Directory.Exists(SpriteFolder))
        {
            Debug.LogError("Sprite folder not found: " + SpriteFolder);
            return;
        }

        Directory.CreateDirectory(OutputFolder);
        AssetDatabase.Refresh();

        Dictionary<string, List<Sprite>> groups = GroupByAction(LoadSprites());
        if (groups.Count == 0)
        {
            Debug.LogError("No sprites found in " + SpriteFolder);
            return;
        }

        AnimationClip idleClip = CreateClip("Idle", Get(groups, "Idle"), 8f, true);
        AnimationClip runClip = CreateClip("Run", Get(groups, "Run"), 12f, true);
        AnimationClip attackClip = CreateClip("Attack", Get(groups, "Attack"), 12f, false);
        AnimationClip jumpClip = CreateClip("Jump", Get(groups, "Jump"), 12f, false);
        AnimationClip slideClip = CreateClip("Slide", Get(groups, "Slide"), 12f, false);
        AnimationClip throwClip = CreateClip("Throw", Get(groups, "Throw"), 12f, false);
        AnimationClip jumpAttackClip = CreateClip("JumpAttack", Get(groups, "Jump_Attack"), 12f, false);
        AnimationClip jumpThrowClip = CreateClip("JumpThrow", Get(groups, "Jump_Throw"), 12f, false);
        AnimationClip climbClip = CreateClip("Climb", Get(groups, "Climb"), 10f, true);
        AnimationClip glideClip = CreateClip("Glide", Get(groups, "Glide"), 10f, true);
        AnimationClip deadClip = CreateClip("Dead", Get(groups, "Dead"), 8f, false);

        AnimatorController controller = CreateController(
            idleClip, runClip, attackClip, jumpClip, slideClip, throwClip, deadClip);

        SetupSelectedGameObject(controller);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Player animations and Animator Controller generated in " + OutputFolder);
    }

    private static List<Sprite> Get(Dictionary<string, List<Sprite>> groups, string action)
    {
        return groups.TryGetValue(action, out List<Sprite> frames) ? frames : null;
    }

    private static List<Sprite> LoadSprites()
    {
        string[] folders = { SpriteFolder };
        return AssetDatabase.FindAssets("t:Sprite", folders)
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.EndsWith(".png"))
            .SelectMany(p => AssetDatabase.LoadAllAssetsAtPath(p))
            .OfType<Sprite>()
            .ToList();
    }

    private static Dictionary<string, List<Sprite>> GroupByAction(List<Sprite> sprites)
    {
        Dictionary<string, List<(int index, Sprite sprite)>> temp = new Dictionary<string, List<(int, Sprite)>>();

        foreach (Sprite sprite in sprites)
        {
            string fileName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(sprite));
            int lastUnderscore = fileName.LastIndexOf('_');
            if (lastUnderscore <= 0) continue;

            string framePart = fileName.Substring(lastUnderscore + 1);
            if (!int.TryParse(framePart, out int frameIndex)) continue;

            string action = fileName.Substring(0, lastUnderscore).TrimEnd('_');
            if (!temp.TryGetValue(action, out List<(int, Sprite)> list))
            {
                list = new List<(int, Sprite)>();
                temp[action] = list;
            }
            list.Add((frameIndex, sprite));
        }

        Dictionary<string, List<Sprite>> result = new Dictionary<string, List<Sprite>>();
        foreach (KeyValuePair<string, List<(int, Sprite)>> pair in temp)
            result[pair.Key] = pair.Value.OrderBy(x => x.Item1).Select(x => x.Item2).ToList();

        return result;
    }

    private static AnimationClip CreateClip(string clipName, List<Sprite> frames, float frameRate, bool loop)
    {
        if (frames == null || frames.Count == 0) return null;

        AnimationClip clip = new AnimationClip { name = clipName, frameRate = frameRate };
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[frames.Count];
        for (int i = 0; i < frames.Count; i++)
            keyframes[i] = new ObjectReferenceKeyframe { time = i / frameRate, value = frames[i] };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        string path = Path.Combine(OutputFolder, clipName + ".anim").Replace('\\', '/');
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static AnimatorController CreateController(
        AnimationClip idleClip, AnimationClip runClip, AnimationClip attackClip,
        AnimationClip jumpClip, AnimationClip slideClip, AnimationClip throwClip, AnimationClip deadClip)
    {
        string path = Path.Combine(OutputFolder, "PlayerAnimator.controller").Replace('\\', '/');
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        AnimatorStateMachine sm = controller.layers[0].stateMachine;

        AnimatorControllerParameter speed = new AnimatorControllerParameter
        {
            name = "Speed",
            type = AnimatorControllerParameterType.Float
        };
        AnimatorControllerParameter isAttacking = new AnimatorControllerParameter
        {
            name = "IsAttacking",
            type = AnimatorControllerParameterType.Bool
        };
        AnimatorControllerParameter isJumping = new AnimatorControllerParameter
        {
            name = "IsJumping",
            type = AnimatorControllerParameterType.Bool
        };
        AnimatorControllerParameter isDead = new AnimatorControllerParameter
        {
            name = "IsDead",
            type = AnimatorControllerParameterType.Bool
        };
        controller.AddParameter(speed);
        controller.AddParameter(isAttacking);
        controller.AddParameter(isJumping);
        controller.AddParameter(isDead);

        AnimatorState idle = AddState(sm, "Idle", idleClip);
        AnimatorState run = AddState(sm, "Run", runClip);
        AnimatorState attack = AddState(sm, "Attack", attackClip);
        AnimatorState jump = AddState(sm, "Jump", jumpClip);
        AddState(sm, "Slide", slideClip);
        AddState(sm, "Throw", throwClip);
        AddState(sm, "Dead", deadClip);

        sm.defaultState = idle;

        AnimatorStateTransition idleToRun = idle.AddTransition(run);
        idleToRun.hasExitTime = false;
        idleToRun.duration = 0.1f;
        idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, speed.name);

        AnimatorStateTransition runToIdle = run.AddTransition(idle);
        runToIdle.hasExitTime = false;
        runToIdle.duration = 0.1f;
        runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, speed.name);

        AnimatorStateTransition anyToAttack = sm.AddAnyStateTransition(attack);
        anyToAttack.AddCondition(AnimatorConditionMode.If, 0, isAttacking.name);

        AnimatorStateTransition attackToIdle = attack.AddTransition(idle);
        attackToIdle.hasExitTime = false;
        attackToIdle.duration = 0.1f;
        attackToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, isAttacking.name);

        AnimatorStateTransition anyToJump = sm.AddAnyStateTransition(jump);
        anyToJump.AddCondition(AnimatorConditionMode.If, 0, isJumping.name);

        AnimatorStateTransition jumpToIdle = jump.AddTransition(idle);
        jumpToIdle.hasExitTime = false;
        jumpToIdle.duration = 0.1f;
        jumpToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, isJumping.name);

        AnimatorStateTransition anyToDead = sm.AddAnyStateTransition(sm.states.First(s => s.state.name == "Dead").state);
        anyToDead.AddCondition(AnimatorConditionMode.If, 0, isDead.name);

        return controller;
    }

    private static AnimatorState AddState(AnimatorStateMachine sm, string name, AnimationClip clip)
    {
        AnimatorState state = sm.AddState(name);
        state.motion = clip;
        return state;
    }

    private static void SetupSelectedGameObject(AnimatorController controller)
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("No GameObject selected. Assign the Animator Controller manually.");
            return;
        }

        if (selected.GetComponent<SpriteRenderer>() == null)
            selected.AddComponent<SpriteRenderer>();

        if (selected.GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = selected.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.freezeRotation = true;
        }

        if (selected.GetComponent<Collider2D>() == null)
            selected.AddComponent<BoxCollider2D>();

        Animator animator = selected.GetComponent<Animator>();
        if (animator == null) animator = selected.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        if (selected.GetComponent<PlayerController>() == null)
            selected.AddComponent<PlayerController>();
    }
}
