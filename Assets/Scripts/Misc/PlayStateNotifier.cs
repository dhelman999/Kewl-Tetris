using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[InitializeOnLoad]
public static class PlayStateNotifier
{

    static PlayStateNotifier()
    {
        EditorApplication.playModeStateChanged += ModeChanged;
    }

    static void ModeChanged(PlayModeStateChange playModeState)
    {
        if (playModeState == PlayModeStateChange.EnteredEditMode)
        {
            Debug.Log("Entered Edit mode.");
        }
    }

    public static bool ApplicationIsAboutToExitPlayMode()
    {
        return EditorApplication.isPlayingOrWillChangePlaymode && Application.isPlaying;
    }

    public static bool ApplicationIsAboutToEnterPlayMode()
    {
        return EditorApplication.isPlayingOrWillChangePlaymode && !Application.isPlaying;
    }
}

#endif