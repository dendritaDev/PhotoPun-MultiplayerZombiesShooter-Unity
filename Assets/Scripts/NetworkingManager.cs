using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;


public class NetworkingManager : MonoBehaviourPunCallbacks
{
    public Button multiplayerButton;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Conexion a un servidor");
        PhotonNetwork.ConnectUsingSettings(); //esto nos conecta a uns ervidor.
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Unirnos a un lobby");
        PhotonNetwork.JoinLobby(); //Esto hace que los player sse unan a un lobby
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("Estamos listos para multijugador");
        multiplayerButton.interactable = true;
    }

    public void FindMatch()
    {
        Debug.Log("Buscando una sala");

        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        MakeRoom();
    }

    private void MakeRoom()
    {
        int randomRoomName = Random.Range(0, 5000);

        RoomOptions roomOptions = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 6,
            PublishUserId = true

        };

        PhotonNetwork.CreateRoom($"RoomName_{randomRoomName}", roomOptions);

        Debug.Log(randomRoomName);

    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Cargando escena de juego");

        PhotonNetwork.LoadLevel(1);
    }
}
