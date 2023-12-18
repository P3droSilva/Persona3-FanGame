using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip titleTheme;
    [SerializeField] private AudioClip tartarus;
    [SerializeField] private AudioClip massDestruction;
    [SerializeField] private AudioClip itsGoingDownNow;
    [SerializeField] private AudioClip bossBattleTheme;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);

       GetAudioClip();

        audioSource.Play();
        audioSource.loop = true;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        audioSource.Stop();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GetAudioClip();

        audioSource.Play();
        audioSource.loop = true;
    }

    private void GetAudioClip()
    {
        if (SceneManager.GetActiveScene().name == "TurnBasedBattle")
        {
            if (GameManager.Instance.playerAdvantage == 1)
            {
                audioSource.clip = itsGoingDownNow;
            }
            else
            {
                audioSource.clip = massDestruction;
            }
        }
        else if (SceneManager.GetActiveScene().name == "BossBattle")
        {
            audioSource.clip = bossBattleTheme;
        }
        else if (SceneManager.GetActiveScene().name == "MainMenu" || SceneManager.GetActiveScene().name == "GameOver")
        {
            audioSource.clip = titleTheme;
        }
        else if (SceneManager.GetActiveScene().name == "OverWorld")
        {
            audioSource.clip = tartarus;
        }
    }
}
