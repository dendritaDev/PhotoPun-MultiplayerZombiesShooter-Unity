using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{
    public int enemiesAlive;
    public int round;
    public GameObject[] spawnPoints;
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI roundsSurvivedText;

    public GameObject gameOverPanel;
    public GameObject pausePanel;

    public Animator fadePanelAnimator;

    public bool isPaused;
    public bool isGameOver;

    public PhotonView photonView;


    private void Start()
    {
        isPaused = false;
        isGameOver = false;
        Time.timeScale = 1;

        spawnPoints = GameObject.FindGameObjectsWithTag("Spawner");
    }

    void Update()
    {
        if(!PhotonNetwork.InRoom || (PhotonNetwork.IsMasterClient && photonView.IsMine)) //el master que ha creado la sala esta online y este es su script cuando se procesa:
        {
            if (enemiesAlive == 0)
            {
                round++;
                NextWave(round);
                if(PhotonNetwork.InRoom)
                {
                    Hashtable hash = new Hashtable();
                    hash.Add("currentRound", round);
                    PhotonNetwork.LocalPlayer.SetCustomProperties(hash); //guardamos la lutima ronda y se la pasamos a todos los jugadores
                }
                else
                {
                    DisplayNextRound(round);

                }
            }

      
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }

    }

    private void DisplayNextRound(int round)
    {
        roundText.text = $"Round: {round}";
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if(photonView.IsMine)
        {
            if (changedProps["currentRound"] != null)
            {
                DisplayNextRound((int)(changedProps["currentRound"]));
            }

        }
    }

    public void NextWave(int round)
    {
        for (int i = 0; i < round; i++)
        {
            int randomPos = Random.Range(0, spawnPoints.Length);
            GameObject spawnPoint = spawnPoints[randomPos];

            GameObject enemyInstance;
            
            if(PhotonNetwork.InRoom) //online
            {
                enemyInstance = PhotonNetwork.Instantiate("Zombie", spawnPoint.transform.position, Quaternion.identity) as GameObject;
            }
            else //offlie, singleplayer
            {
                enemyInstance = Instantiate(Resources.Load("Zombie"), spawnPoint.transform.position, Quaternion.identity) as GameObject;

            }

            
            //enemyInstance.GetComponent<EnemyManager>().gameManager = GetComponent<GameManager>();
            enemiesAlive++;
        }
    }

    public void GameOver()
    {
        gameOverPanel.SetActive(true);
        roundsSurvivedText.text = round.ToString();
        
        if(!PhotonNetwork.InRoom)
        {
            Time.timeScale = 0;

        }
        Cursor.lockState = CursorLockMode.None;
        isGameOver = true;
    }

    public void RestartGame()
    {
        if (!PhotonNetwork.InRoom)
        {
            Time.timeScale = 1;

        }
        
        SceneManager.LoadScene(1);
    }

    public void BackToMainMenu()
    {
        if (!PhotonNetwork.InRoom)
        {
            Time.timeScale = 1;

        }
        AudioListener.volume = 1;
        fadePanelAnimator.SetTrigger("FadeIn");
        Invoke("LoadMainMenuScene", 0.5f);
    }

    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene(0);
    }

    public void Pause()
    {
        pausePanel.SetActive(true);
        AudioListener.volume = 0;
        if (!PhotonNetwork.InRoom)
        {
            Time.timeScale = 0;

        }
        Cursor.lockState = CursorLockMode.None;
        isPaused = true;
    }
    
    public void Resume()
    {
        pausePanel.SetActive(false);
        AudioListener.volume = 1;
        if (!PhotonNetwork.InRoom)
        {
            Time.timeScale = 1;

        }
        Cursor.lockState = CursorLockMode.Locked;
        isPaused = false;
    }
    

}
