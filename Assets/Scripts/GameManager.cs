using System.Collections;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Vector3 playerPosition = Vector3.zero;

    public int enemyInBattle;

    public int makotoHealth;
    public int junpeiHealth;
    public int mitsuruHealth;
    public int yukariHealth;

    public int playerAdvantage;

    [Header("Fade")]
    public Canvas fadeCanvas;
    public Image playerAdvImg;
    public Image enemyAdvImg;
    public Image fadeOutImg;
    public float fadeSpeed = 1.5f;

    private bool coroutineRunning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            makotoHealth = -1;
            junpeiHealth = -1;
            mitsuruHealth = -1;
            yukariHealth = -1;
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(fadeCanvas);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadBattleScene(bool playerAdv, GameObject enemy)
    {
        playerAdvantage = playerAdv ? 1 : 0;
        playerPosition = GameObject.Find("Player").transform.position;

        string enemyName = enemy.name;
        char enemyIdx = enemyName[enemyName.Length - 1];
        enemyInBattle = int.Parse(enemyIdx.ToString());

        if(!coroutineRunning)
        {
            coroutineRunning = true;
            Image fadeImage = playerAdv ? playerAdvImg : enemyAdvImg;
            StartCoroutine(FadeOut("TurnBasedBattle", fadeImage));
        }
    }

    public void LoadOverworldScene()
    {
        if(SceneManager.GetActiveScene().name == "OverWorld")
        {
            return;
        }

        playerAdvantage = -1;

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (!coroutineRunning)
        {
            coroutineRunning = true;
            StartCoroutine(FadeOut("OverWorld", fadeOutImg));
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (scene.name == "OverWorld")
        {
            DestroyEnemyInBattle();
        }
    }

    private void DestroyEnemyInBattle()
    {
        string enemyName = "Enemy" + enemyInBattle.ToString();
        GameObject enemy = GameObject.Find(enemyName);

        if (enemy != null)
        {
            Destroy(enemy);
        }
    }

    private IEnumerator FadeOut(string sceneName, Image fadeImage)
    {
        fadeImage.gameObject.SetActive(true);

        float alpha = 0f;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
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
            alpha -= Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        fadeImage.gameObject.SetActive(false);
        coroutineRunning = false;
    }



}
