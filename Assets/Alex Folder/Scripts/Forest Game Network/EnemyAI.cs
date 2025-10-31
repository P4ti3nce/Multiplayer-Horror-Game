using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent (typeof(NetworkTransformReliable))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : NetworkBehaviour
{
    public enum EnemyState { Idle, Walk, Run, Taunt, Attack }

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animStateParameter = "State";
    [SerializeField] private ParticleSystem smokeEffect;

    [Header("Movement")]
    [SerializeField] private float speed = 3.5f;
    [SerializeField] private float chaseSpeedMultiplier = 1.5f;
    [SerializeField] private float roamUpdateRate = 0.5f;
    [SerializeField] private float roamMinDistance = 10f;
    [SerializeField] private float roamMaxDistance = 25f;
    [SerializeField] private float roamConeAngle = 60f;

    [Header("Vision")]
    public float viewRange = 20f;
    public float fieldOfView = 90f;
    public LayerMask obstructionMask;
    public LayerMask playerMask;

    [Header("Chase & Taunt")]
    public float chaseDuration = 5f;
    public float postChaseTeleportDuration = 2f;
    public float tauntDuration = 1.5f;
    [Range(0f, 1f)] public float teleportChance = 0.5f;
    public float teleportDistanceBehindPlayer = 3f;

    [Header("Attack")]
    public SphereCollider attackTrigger;

    private NavMeshAgent agent;
    private Transform target;
    private Coroutine roamingCoroutine;
    private bool isChasing = false;

    [SyncVar(hook = nameof(OnStateChanged))]
    private EnemyState currentState = EnemyState.Idle;

    #region Unity
    public override void OnStartServer()
    {
        base.OnStartServer();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        attackTrigger.isTrigger = true;
        InvokeRepeating(nameof(ServerUpdateAI), 0.1f, 0.1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        if (((1 << other.gameObject.layer) & playerMask) == 0) return;

        var reaction = other.GetComponentInParent<PlayerEnemyReaction>();
        if (reaction != null)
        {
            SetState(EnemyState.Attack);
            reaction.CaughtByChaser();
        }
    }
    #endregion

    #region State & Animation
    [Server]
    public void SetState(EnemyState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
    }

    void OnStateChanged(EnemyState oldState, EnemyState newState)
    {
        if (!animator) return;
        animator.SetInteger(animStateParameter, (int)newState);
    }
    #endregion

    #region AI
    [Server]
    void ServerUpdateAI()
    {
        if (!CanSeeAPlayer())
        {
            if (roamingCoroutine == null)
                StartRoaming(10f);
            return;
        }

        if (CanSeeAPlayer())
        {
            Chase();
        }
    }

    bool CanSeeAPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, viewRange, playerMask);
        Transform closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (var hit in hits)
        {
            Transform player = hit.transform;
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToPlayer);
            float distance = Vector3.Distance(transform.position, player.position);

            if (angle < fieldOfView / 2f &&
                !Physics.Raycast(transform.position + Vector3.up * 1.5f, dirToPlayer, distance, obstructionMask))
            {
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }
        }

        if (closestPlayer != null)
        {
            target = closestPlayer;
            return true;
        }

        target = null;
        return false;
    }
    #endregion

    #region Roaming
    [Server]
    public void StartRoaming(float duration)
    {
        if (roamingCoroutine != null) StopCoroutine(roamingCoroutine);
        roamingCoroutine = StartCoroutine(RoamingRoutine(duration));
    }

    private IEnumerator RoamingRoutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            SetState(EnemyState.Idle);
            yield return new WaitForSeconds(1f); // 1s idle before choosing new target

            PickRoamingTarget();
            SetState(EnemyState.Walk);

            yield return new WaitForSeconds(roamUpdateRate);
            elapsed += roamUpdateRate + 1f; // include idle
        }
        roamingCoroutine = null;
    }

    private void PickRoamingTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) return;

        Transform closestPlayer = null;
        float closestDist = Mathf.Infinity;
        foreach (var p in players)
        {
            float d = Vector3.Distance(transform.position, p.transform.position);
            if (d < closestDist)
            {
                closestDist = d;
                closestPlayer = p.transform;
            }
        }

        target = closestPlayer;

        Vector3 dirToPlayer = (target.position - transform.position).normalized;
        float randomAngle = Random.Range(-roamConeAngle, roamConeAngle);
        Vector3 roamDir = Quaternion.Euler(0, randomAngle, 0) * dirToPlayer;

        float roamDistance = Random.Range(roamMinDistance, roamMaxDistance);
        Vector3 roamTarget = transform.position + roamDir * roamDistance;

        if (NavMesh.SamplePosition(roamTarget, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            agent.speed = speed;
        }
    }
    #endregion

    #region Chase & Taunt
    [Server]
    public void Chase()
    {
        if (!isChasing && target != null)
            StartCoroutine(ChaseRoutine());
    }

    private IEnumerator ChaseRoutine()
    {
        isChasing = true;
        float originalSpeed = agent.speed;
        agent.speed = originalSpeed * chaseSpeedMultiplier;
        SetState(EnemyState.Run);

        float chaseEndTime = Time.time + chaseDuration;
        while (Time.time < chaseEndTime && target != null)
        {
            agent.SetDestination(target.position);
            yield return null;
        }

        agent.speed = originalSpeed;

        // Taunt
        SetState(EnemyState.Taunt);
        if (smokeEffect) smokeEffect.Play();
        yield return new WaitForSeconds(tauntDuration);

        // Decide teleport
        if (Random.value <= teleportChance && target != null)
        {
            Vector3 behindPlayer = target.position - target.forward * teleportDistanceBehindPlayer;
            if (NavMesh.SamplePosition(behindPlayer, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                agent.Warp(hit.position);
            }
            // Chase 2 seconds after teleport
            agent.speed = originalSpeed * chaseSpeedMultiplier;
            SetState(EnemyState.Run);
            float teleportChaseEnd = Time.time + postChaseTeleportDuration;
            while (Time.time < teleportChaseEnd && target != null)
            {
                agent.SetDestination(target.position);
                yield return null;
            }
        }

        // Spawn smoke at end
        if (smokeEffect) smokeEffect.Play();

        // Destroy enemy
        NetworkServer.Destroy(gameObject);
        isChasing = false;
    }
    #endregion
}