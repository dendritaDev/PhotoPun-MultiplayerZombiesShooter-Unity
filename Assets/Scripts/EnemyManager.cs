using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Photon.Pun;

public class EnemyManager : MonoBehaviour
{
    public GameObject player;
    private GameObject[] playersInScene;
    public Animator enemyAnimator;

    public float damage = 20f;
    public float health = 100f;

    public GameManager gameManager;

    public Slider healthBar;

    public bool playerInReach;
    public float attackDelayTimer;
    public float howMuchEarlierStartAttackAnim;
    public float delayBetweenAttacks;

    public AudioSource enemyAudioSource;
    public AudioClip[] growlAudioClips;

    public int points = 20;

    public PhotonView photonView;

    private NavMeshAgent navMeshAgent;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        enemyAudioSource = GetComponent<AudioSource>();
        healthBar.maxValue = health;
        healthBar.value = health;
        navMeshAgent = GetComponent<NavMeshAgent>();

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        playersInScene = GameObject.FindGameObjectsWithTag("Player");
    }

    void Update()
    {
        if (!enemyAudioSource.isPlaying)
        {
            enemyAudioSource.clip = growlAudioClips[Random.Range(0, growlAudioClips.Length)];
            enemyAudioSource.Play();
        }

        if(PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient) //si estamos online pero no somos el master no lo hacemos, solo el master actaulziaria zombi para que este sincronizado para todos
        {
            return;
        }

        GetClosestPlayer();

        if(player != null)
        {
            navMeshAgent.destination = player.transform.position;

            healthBar.transform.LookAt(player.transform);
        }

      

        if (navMeshAgent.velocity.magnitude > 1)
        {
            enemyAnimator.SetBool("isRunning", true);
        }
        else
        {
            enemyAnimator.SetBool("isRunning", false);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == player)
        {
            playerInReach = true;
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (playerInReach)
        {
            attackDelayTimer += Time.deltaTime;

            if (attackDelayTimer >= delayBetweenAttacks - howMuchEarlierStartAttackAnim && attackDelayTimer <= delayBetweenAttacks)
            {
                enemyAnimator.SetTrigger("isAttacking");
            }

            if (attackDelayTimer >= delayBetweenAttacks)
            {
                player.GetComponent<PlayerManager>().Hit(damage);
                attackDelayTimer = 0;
            }
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject == player)
        {
            playerInReach = false;
            attackDelayTimer = 0;
        }
    }

    public void Hit(float damage)
    {
        if(PhotonNetwork.InRoom)
        {
            photonView.RPC("TakeDamage", RpcTarget.All, damage, photonView.ViewID); //llama la funcion para todos los players pero solo se ejecutará una vez lo de eliminar daño en el master, pero la animacion y todo lo demas se msotrará en todos

        }
        else
        {
            TakeDamage(damage, photonView.ViewID);
        }

    }

    [PunRPC]
    public void TakeDamage(float damage, int viewID)
    {
        if(photonView.ViewID == viewID)
        {
            health -= damage;
            healthBar.value = health;

            if (health <= 0)
            {
                enemyAnimator.SetTrigger("isDead");
                //gameManager.enemiesAlive--;             


                player.GetComponent<PlayerManager>().UpdatePoints(points);

                Destroy(gameObject, 10f);
                Destroy(GetComponent<NavMeshAgent>());
                Destroy(GetComponent<EnemyManager>());
                Destroy(GetComponent<CapsuleCollider>());

                if (!PhotonNetwork.InRoom || (PhotonNetwork.IsMasterClient && photonView.IsMine)) //solo en el script del master de la sala se hará esto pa q todo este scrinronizao o si estamos offline
                {
                    gameManager.enemiesAlive--;
                }
            }
        }

       
    }

    private void GetClosestPlayer()
    {
        float minDistance = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (var p in playersInScene)
        {
            if(p!=null)
            {
                float distance = Vector3.Distance(p.transform.position, currentPos);
                if(distance < minDistance)
                {
                    player = p;
                    minDistance = distance;
                }
            }

        }
    }
}
