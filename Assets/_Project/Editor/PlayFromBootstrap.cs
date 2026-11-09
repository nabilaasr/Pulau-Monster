using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PlayFromBootstrap
{
    private const string BootstrapPath = "Assets/_Project/Scenes/Scn_Bootstrap.unity";

    static PlayFromBootstrap()
    {
        EditorApplication.delayCall += Apply;
    }

    private static void Apply()
    {
        SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapPath);
        if (scene != null)
        {
            EditorSceneManager.playModeStartScene = scene;
        }
    }
}
