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
    private GameObject[] enemies;

    [Header("Battle")]
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
    private bool loading = false;
    private int win = -1;

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

    public void Heal()
    {
        makotoHealth = -1;
        junpeiHealth = -1;
        mitsuruHealth = -1;
        yukariHealth = -1;
    }

    public void LoadGameOver(bool win)
    {
        this.win = win ? 1 : 0;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("GameOver");

    }

    public void LoadMainMenu()
    {
        Debug.Log("Loading main menu");
        SceneManager.LoadScene("MainMenu");
        DestroyDontDestroyOnLoadObjects();
    }

    public void LoadBattleScene(bool playerAdv, GameObject enemy)
    {
        if(loading)
        {
            return;
        }

        loading = true;
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

    public void LoadBossBattle()
    {
        playerAdvantage = -1;
        playerPosition = GameObject.Find("Player").transform.position;

        if (!coroutineRunning)
        {
            coroutineRunning = true;
            StartCoroutine(FadeOut("BossBattle", fadeOutImg));
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

    public void LoadSecondFloor()
    {
        GameObject teleportPoint = GameObject.Find("TeleportPoint2nd");
        GameObject.Find("Player").transform.position = teleportPoint.transform.position;
        enemies = GameObject.FindGameObjectsWithTag("Shadow");
        DisableShadows();
    }

    public void LoadFirstFloor()
    {
        GameObject teleportPoint = GameObject.Find("TeleportPoint1st");
        GameObject.Find("Player").transform.position = teleportPoint.transform.position;
        EnableShadows();
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (scene.name == "OverWorld")
        {
            DestroyEnemyInBattle();
        }
        else if(scene.name == "GameOver")
        {
            if(win == 1)
            {
                GameObject.Find("LoseText").SetActive(false);
            }
            else
            {
                GameObject.Find("WinText").SetActive(false);
            }

            GameObject bt = GameObject.Find("MenuButton");
            Button button = bt.GetComponent<Button>();
            if(button == null)
            {
                Debug.Log("Button is null");
            }
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(LoadMainMenu);
            
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
        loading = false;
    }

    private void DisableShadows()
    {
        foreach (GameObject shadow in enemies)
        {
            shadow.SetActive(false);
        }
    }

    private void EnableShadows()
    {
        foreach (GameObject shadow in enemies)
        {
            shadow.SetActive(true);
        }
    }

    void DestroyDontDestroyOnLoadObjects()
    {
        Destroy(fadeCanvas.gameObject);
        Destroy(GameObject.Find("AudioManager"));
        Destroy(GameObject.Find("GameManager"));
    }



}
