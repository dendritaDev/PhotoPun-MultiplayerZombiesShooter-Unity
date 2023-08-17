using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public float health = 100f;
    public float healthCap;
    public TextMeshProUGUI healthText;

    public GameManager gameManager;
    public GameObject playerCamera;

    public CanvasGroup hitPanel;

    private float shakeTime = 1;
    private float shakeDuration = 0.5f;
    private Quaternion playerCameraOriginalRotation;

    public GameObject weaponHolder;
    private int activeWeaponIndex;
    private GameObject activeWeapon;

    public int totalPoints;
    public TextMeshProUGUI pointsText;

    public PhotonView photonView;

    private void Start()
    {
        playerCameraOriginalRotation = playerCamera.transform.localRotation;
        
        WeaponSwitch(0);
        totalPoints = 0;
        UpdatePoints(0);

        healthCap = health;
    }

    private void Update()
    {
        //para evitar que mi input afecte a los scripts de los otros jugadores o al revés, tenemos que hacer esta comprobación:
        if (PhotonNetwork.InRoom && !photonView.IsMine) { 
            
            playerCamera.gameObject.SetActive(false); //para no estar viendo camaras de los otros jugadores
            return; 
        
        }

        if (hitPanel.alpha > 0)
        {
            hitPanel.alpha -= Time.deltaTime;
        }
        
        if (shakeTime < shakeDuration)
        {
            shakeTime += Time.deltaTime;
            CameraShake();
        }else if (playerCamera.transform.localRotation != playerCameraOriginalRotation)
        {
            playerCamera.transform.localRotation = playerCameraOriginalRotation;
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0 || Input.GetKeyDown(KeyCode.Q))
        {
            WeaponSwitch(activeWeaponIndex + 1);
        }
        
    }

    public void Hit(float damage)
    {
       if(PhotonNetwork.InRoom)
        {
            photonView.RPC("PlayerTakeDamage", RpcTarget.All, damage, photonView.ViewID);
        }
        else
        {
            PlayerTakeDamage(damage, photonView.ViewID);
        }
    }

    [PunRPC]
    public void PlayerTakeDamage(float damage, int viewID)
    {
        if(photonView.ViewID == viewID)
        {
            health -= damage;
            healthText.text = $"{health} HP";

            if (health <= 0)
            {
                gameManager.GameOver();
            }
            else
            {
                shakeTime = 0;
                hitPanel.alpha = 1f;
            }
        }
    }

    public void CameraShake()
    {
        playerCamera.transform.localRotation = Quaternion.Euler(Random.Range(-2f, 2f), 0, 0);
    }

    public void WeaponSwitch(int weaponIndex)
    {
        int index = 0;
        int amountOfWeapons = weaponHolder.transform.childCount;

        if (weaponIndex > amountOfWeapons - 1)
        {
            weaponIndex = 0;
        }

        foreach (Transform child in weaponHolder.transform)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
            }

            if (index == weaponIndex)
            {
                child.gameObject.SetActive(true);
                activeWeapon = child.gameObject;
            }

            index++;
        }

        activeWeaponIndex = weaponIndex;

        if(photonView.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("weaponIndex", weaponIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if(!photonView.IsMine && targetPlayer == photonView.Owner && changedProps["weaponIndex"] != null)
        {
            WeaponSwitch((int)changedProps["weaponIndex"]);
        }
    }

    public void UpdatePoints(int pointsToAdd)
    {
        totalPoints += pointsToAdd;
        pointsText.text = $"Points: {totalPoints}";
    }

    [PunRPC]
    public void WeaponShootSFX(int viewID)
    {
        activeWeapon.GetComponent<WeaponManager>().ShootVFX(viewID);
    }

}
