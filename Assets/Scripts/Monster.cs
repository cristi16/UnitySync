using UnityEngine;
using System.Collections;

public class Monster : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private NavMeshAgent agent;
    private Transform player;
    private bool playerInRange = false;
    private PhotonView photonView;
    private Animation animation;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        photonView = GetComponent<PhotonView>();
        player = FindObjectOfType<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().transform;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        animation = GetComponent<Animation>();
    }

    void Update()
    {
        if (PlayMode.isPlayMode)
        {
            agent.enabled = true;
            photonView.enabled = false;
            if(playerInRange)
            {
                agent.SetDestination(player.position);
                if(animation.clip != animation["walk"].clip || animation.isPlaying == false)
                {
                    animation.clip = animation["walk"].clip;
                    animation.Play();
                }

                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                if(distanceToPlayer < transform.localScale.x)
                {
                    animation.CrossFade("attack");
                }
            }
        }
        else
        {
            agent.enabled = false;
            photonView.enabled = true;
            playerInRange = false;
        }
           
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "FPS")
        {
            playerInRange = true;
        }

    }

    void OnTriggerStay(Collider col)
    {
        if(col.gameObject.tag == "FPS")
        {

        }

    }

    void OnTriggerExit(Collider col)
    {
        if(col.gameObject.tag == "FPS")
        {
            playerInRange = false;
            if (animation.clip != animation["walk"].clip)
            {
                animation.clip = animation["walk"].clip;
                animation.Play();
            }
            agent.SetDestination(initialPosition);
            StartCoroutine(ReachDestination());
        }
    }

    IEnumerator ReachDestination()
    {
        while(Vector3.Distance(transform.position, agent.destination) > 1)
        {
            if(playerInRange)
                yield break;
            yield return null;
        }

        if (animation.clip != animation["idle"].clip)
        {
            animation.clip = animation["idle"].clip;
            animation.Play();
        }

        float timer = 0;
        float spinTime = 1f;
        while(timer < spinTime)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, initialRotation, Time.deltaTime);
            yield return null;
        }
    }
}
