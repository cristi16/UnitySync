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
    private Transform offlineTransform;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        photonView = GetComponent<PhotonView>();
        player = FindObjectOfType<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().transform;
        animation = GetComponent<Animation>();
        offlineTransform = new GameObject(name + "_OfflineTransform").transform;
        offlineTransform.position = transform.position;
        offlineTransform.localScale = transform.localScale;
        offlineTransform.rotation = transform.rotation;
    }

    void Update()
    {
        if (PlayMode.isPlayMode)
        {
            if(agent.enabled == false)
            {
                initialPosition = transform.position;
                initialRotation = transform.rotation;
                agent.enabled = true;
                photonView.ObservedComponents.Clear();
                photonView.ObservedComponents.Add(offlineTransform);
            }

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
            if(agent.enabled == true)
            {
                agent.enabled = false;
                playerInRange = false;
                transform.position = initialPosition;
                transform.rotation = initialRotation;
                photonView.ObservedComponents.Clear();
                photonView.ObservedComponents.Add(transform);
                transform.position = offlineTransform.position;
                transform.rotation = offlineTransform.rotation;
                transform.localScale = offlineTransform.localScale;
                animation.Stop();
            }

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
