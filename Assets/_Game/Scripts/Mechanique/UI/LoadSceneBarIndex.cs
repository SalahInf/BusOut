using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LoadSceneBarIndex : MonoBehaviour
{
    public int indexScene;
    public LoadSceneMode mode = LoadSceneMode.Single;
    public float timeToStar = 2f;
    public bool loadOnstar;

    public Image progressValue;
    public GameObject root;

    void Start()
    {
        if (loadOnstar)
            LoadScene();
    }

    public void LoadScene()
    {
        StartCoroutine(LoadYourAsyncScene(indexScene));
    }

    IEnumerator LoadYourAsyncScene(int indexScene)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(indexScene, mode);
        yield return new WaitForSeconds(timeToStar);

        float t = 0;
        while (t < 1 || !asyncLoad.isDone)
        {
            t += Time.deltaTime;
            progressValue.fillAmount = Mathf.Lerp(0, asyncLoad.progress, t);
            yield return null;
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(indexScene));

        CanvasGroup canvasGroup = root.GetComponentInChildren<CanvasGroup>();

        t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / 0.2f);
            yield return null;
        }

        Destroy(root);
    }

    private void OnValidate()
    {
        timeToStar = Mathf.Max(0.3f, timeToStar);
    }
}
