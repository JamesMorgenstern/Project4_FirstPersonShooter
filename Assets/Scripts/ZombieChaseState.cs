using UnityEngine;
using UnityEngine.AI;

public class ZombieChaseState : StateMachineBehaviour
{
    NavMeshAgent agent;
    Transform player;

    public float chaseSpeed = 6f;
    public float stopChasingDistance = 21f;
    public float attackingDistance = 2.5f;
    
    // Zigzag logic
    public float erraticPointDistance = 4f;
    public float switchDistance = 1.5f;

    private Vector3 currentWaypoint;
    private bool hasWaypoint;
    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Initialization //
        
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = animator.GetComponent<NavMeshAgent>();
        
        agent.speed = chaseSpeed;
        
        hasWaypoint = false;
    }
    
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (SoundManager.Instance.zombieChannel.isPlaying == false)
        {
            SoundManager.Instance.zombieChannel.PlayOneShot(SoundManager.Instance.zombieChase);
        }
        
        // Choose new erratic waypoint if needed
        if (!hasWaypoint || Vector3.Distance(animator.transform.position, currentWaypoint) < switchDistance)
        {
            currentWaypoint = GenerateErraticPoint(animator.transform.position, player.position);
            hasWaypoint = true;
        }

        // Set destination to current erratic point
        agent.SetDestination(currentWaypoint);
        animator.transform.LookAt(player);
        
        float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);
        
        // Checking if the agent should stop chasing //

        if (distanceFromPlayer > stopChasingDistance)
        {
            animator.SetBool("isChasing", false);
        }
        
        // Checking if the agent should attack //

        if (distanceFromPlayer < attackingDistance)
        {
            animator.SetBool("isAttacking", true);
        }
    }
    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent.SetDestination(animator.transform.position);
        
        SoundManager.Instance.zombieChannel.Stop();
    }
    
    private Vector3 GenerateErraticPoint(Vector3 zombiePos, Vector3 playerPos)
    {
        Vector3 toPlayer = (playerPos - zombiePos).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, toPlayer);

        for (int i = 0; i < 5; i++)
        {
            float sidewaysOffset = Random.Range(-1f, 1f) * erraticPointDistance;
            Vector3 offsetDir = right * sidewaysOffset;
            
            float forwardProgress = Random.Range(3f, 6f);
            Vector3 forwardOffset = toPlayer * forwardProgress;
            float randomForwardFactor = Random.Range(0.8f, 1.2f);
            forwardOffset *= randomForwardFactor;

            Vector3 targetPoint = zombiePos + forwardOffset + offsetDir;

            // Snap to NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPoint, out hit, 2f, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    return hit.position;
                }
            }
        }
        
        return zombiePos + toPlayer * 2f;
    }
}
