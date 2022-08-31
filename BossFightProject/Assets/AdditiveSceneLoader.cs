using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditiveSceneLoader : MonoBehaviour
{
    bool m_IsSwitchingScenes;
    AsyncOperation m_SceneLoadOperation;

    [SerializeField]
    VoidEvent m_StartButtonPressed;

    [SerializeField]
    string m_SceneToLoad = "Level1";

    // Start is called before the first frame update
    void Start()
    {
        m_IsSwitchingScenes = false;
        m_StartButtonPressed.Register(LoadSceneWhenReady);
    }

    void LoadSceneWhenReady()
    {
        m_SceneLoadOperation = SceneManager.LoadSceneAsync(m_SceneToLoad);
        if (m_IsSwitchingScenes)
        {
            Debug.Log("The scene is already switching gawd damn! Calm down.");
            return;
        }

        m_IsSwitchingScenes = true;
        StartCoroutine(SetActiveAfterLoad());
    }

    IEnumerator SetActiveAfterLoad()
    {
        m_IsSwitchingScenes = true;
        yield return new WaitUntil(() => m_SceneLoadOperation.isDone);
        yield return null;
        //SceneManager.SetActiveScene(SceneManager.GetSceneByName(m_SceneToLoad));
    }
}
