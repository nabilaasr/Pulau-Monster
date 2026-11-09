using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class SceneInstaller : MonoBehaviour
{
    public abstract void Install(GameServices services);

    protected List<T> FindInScene<T>() where T : Component
    {
        var list = new List<T>();
        Scene currentScene = gameObject.scene;
        GameObject[] roots = currentScene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            T[] found = roots[i].GetComponentsInChildren<T>(true);
            list.AddRange(found);
        }
        return list;
    }
}
