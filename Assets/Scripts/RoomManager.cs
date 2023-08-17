using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager sharedInstance;

    private void Awake()
    {
        if (sharedInstance == null)
        {
            sharedInstance = this;
            
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Vector3 spawnPosition = new Vector3(Random.Range(-3f, 3f), 2, Random.Range(-3f, 3f));

        if(PhotonNetwork.InRoom)//si estamos en online - para instanciar cosa con photonnetwork tenemos que meter las cosas en una carpeta llamada Resources
        {
            PhotonNetwork.Instantiate("First_Person_Player", spawnPosition, Quaternion.identity);
        }
        else //si estamos offline
        {
            Instantiate(Resources.Load("First_Person_Player"), spawnPosition, Quaternion.identity);
        }
    }
}
