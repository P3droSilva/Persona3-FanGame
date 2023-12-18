using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Image fadeImage;

    public void LoadGame()
    {
        StartCoroutine(FadeOut("OverWorld", fadeImage));   
    }

    private IEnumerator FadeOut(string sceneName, Image fadeImage)
    {
        fadeImage.gameObject.SetActive(true);

        float alpha = 0f;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime * 1.5f;
            fadeImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        SceneManager.LoadScene(sceneName);

        StartCoroutine(FadeIn(fadeImage));
    }

    private IEnumerator FadeIn(Image fadeImage)
    {
        float alpha = 1f;

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * 1.5f;
            fadeImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        fadeImage.gameObject.SetActive(false);
    }
}
