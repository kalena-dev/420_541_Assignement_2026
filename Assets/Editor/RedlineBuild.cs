using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class RedlineBuild
{
    [MenuItem("Tools/REDLINE/Refresh animation links")]
    public static void RefreshAnimations()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/Runner.controller");
        var states = controller.layers[0].stateMachine.states;
        AnimationClip Clip(string name) => AssetDatabase.LoadAllAssetsAtPath("Assets/Animations/" + name + ".fbx")
            .OfType<AnimationClip>().First(c => !c.name.StartsWith("__preview__"));
        BlendTree tree = (BlendTree)states.First(s => s.state.name == "Locomotion").state.motion;
        ChildMotion[] children = tree.children;
        string[] clips = { "Idle", "Walking", "Running" };
        for (int i = 0; i < children.Length; i++) children[i].motion = Clip(clips[i]);
        tree.children = children;
        EditorUtility.SetDirty(tree);
        foreach (ChildAnimatorState state in states)
        {
            if (state.state.name == "Flip") state.state.motion = Clip("Flip");
            if (state.state.name == "Airborne") state.state.motion = Clip("Falling");
            EditorUtility.SetDirty(state.state);
        }
        AssetDatabase.SaveAssets();
        Validate();
    }

    [MenuItem("Tools/REDLINE/Validate project")]
    public static void Validate()
    {
        string[] expected = { "MainMenu", "Level01", "Level02", "Level03", "EndScreen" };
        string[] enabled = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        Require(enabled.Length == expected.Length, "Build must contain five scenes.");
        for (int i = 0; i < expected.Length; i++)
        {
            Require(Path.GetFileNameWithoutExtension(enabled[i]) == expected[i], "Scene order is incorrect.");
            var scene = EditorSceneManager.OpenScene(enabled[i], OpenSceneMode.Single);
            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject root in roots)
                foreach (Component component in root.GetComponentsInChildren<Component>(true))
                    Require(component != null, "Missing component in " + scene.name + "/" + root.name);
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            Require(camera != null && camera.CompareTag("MainCamera"), "Main camera missing in " + scene.name);
            if (i < 1 || i > 3) continue;
            LevelInfo level = UnityEngine.Object.FindAnyObjectByType<LevelInfo>();
            CharacterMovement player = UnityEngine.Object.FindAnyObjectByType<CharacterMovement>();
            Require(level != null && level.levelNumber == i, "Level number mismatch.");
            Require(player != null, "Player missing in " + scene.name);
            Require(player.GetComponent<PlayerHealth>() != null, "Player health missing.");
            Require(player.GetComponent<PlayerPowerups>() != null, "Player power-ups missing.");
            Require(player.gameObject.layer == 8, "Player must be on its own physics layer.");
            Require(player.GetComponent<Rigidbody>().constraints == RigidbodyConstraints.FreezeRotation, "Player rotation is not constrained.");
            Require(UnityEngine.Object.FindObjectsByType<LevelGoal>().Length == 1, "Each level needs one goal.");
            Require(player.GetComponent<Animator>().runtimeAnimatorController != null, "Runner controller missing.");
        }
        foreach (string name in new[] { "Score", "Speed", "ScoreBoost", "Jump" })
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/" + name + ".prefab");
            Require(prefab != null && prefab.GetComponent<Pickup>() != null, name + " pickup prefab missing.");
            Require(prefab.GetComponent<Collider>().isTrigger, name + " pickup must use a trigger.");
            Require(prefab.GetComponent<Pickup>().visual != null, name + " pickup visual is not assigned.");
        }
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/Runner.controller");
        Require(controller != null, "Animation controller missing.");
        var states = controller.layers[0].stateMachine.states;
        BlendTree locomotion = states.First(s => s.state.name == "Locomotion").state.motion as BlendTree;
        Require(locomotion != null && locomotion.children.Length == 3, "Idle/walk/run Blend Tree is missing.");
        foreach (ChildMotion child in locomotion.children) Require(child.motion != null, "Locomotion animation reference is missing.");
        Require(states.First(s => s.state.name == "Flip").state.motion != null, "Flip animation reference is missing.");
        EditorSceneManager.OpenScene(enabled[0]);
        Debug.Log("REDLINE validation passed: scenes, player, camera, pickups and animations.");
    }

    [MenuItem("Tools/REDLINE/Build Windows")]
    public static void BuildWindows()
    {
        Validate();
        Directory.CreateDirectory("Builds/REDLINE");
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = "Builds/REDLINE/REDLINE.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("Windows build failed: " + report.summary.result);
        Debug.Log("REDLINE Windows build completed.");
    }

    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

