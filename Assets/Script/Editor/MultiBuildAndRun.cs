using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;

public class MultiBuildAndRun
{
    [MenuItem("Tools/Run 2P")]
    static void Perfom()
    {
        Win64(1);
        
    }

    static void Win64(int players = 2)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);

        for (int i = 1; i <= players; i++)
        {
            BuildPipeline.BuildPlayer(
                    GetScenePaths(),
                    "Builds/Win64/"+GetProject() + i.ToString()
                    +"/"+GetProject() +i.ToString()+".exe",
                    BuildTarget.StandaloneWindows64, BuildOptions.AutoRunPlayer
                );
        }
    }

    static string GetProject()
    {
        string[] s = Application.dataPath.Split('/');
        return s[s.Length - 2];
    }

    static string[] GetScenePaths()
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }
        return scenes;
    }
}
