using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BotAIController : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer;

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject fireBall;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private Animator _animator;
    
    private IEnumerator coroutine;

    private bool isPatrol=false;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        //if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange&&!isPatrol) ChasePlayer();
        if (playerInAttackRange && playerInSightRange ) AttackPlayer();
               

    }

    /*
    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }
    
    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange)+player.transform.position.z;
        float randomX = Random.Range(-walkPointRange, walkPointRange)+player.transform.position.x;

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 5f, whatIsGround))
            walkPointSet = true;
    }
*/
    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
        _animator.SetFloat("isWalk", 1);
    }

    private void AttackPlayer()
    {
        if (!alreadyAttacked&&!isPatrol)
        {
            alreadyAttacked = true;
            agent.SetDestination(transform.position);

            transform.LookAt(player);
        
            _animator.SetFloat("isWalk", 0);
            _animator.SetTrigger("isAttack");
        
            Vector3 playerPos = transform.position;
            Vector3 playerDirection = transform.forward;
            float spawnDistance = 2;
 
            Vector3 spawnPos = playerPos + new Vector3(0,0.5f,0) +playerDirection*spawnDistance;
            GameObject fb=Instantiate(fireBall,spawnPos,Quaternion.identity  );
            
            //tolerance
            Vector3 tolerance = new Vector3(0, 0, 0);
            if (!CanTolerance())
            {
                tolerance = new Vector3(Random.Range(-0.15f, 0.15f), 0, Random.Range(-0.15f, 0.15f));
            }

            fb.GetComponent<Rigidbody>().AddForce((transform.forward+tolerance)*200);
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
            coroutine = DestroyFireBall(fb);
            StartCoroutine(coroutine);
            
            Patroling();
            
        }
    }

    private void Patroling()
    {
        if (!isPatrol)
        {
            Vector3 deflection = new Vector3(Random.Range(-15f, 15f), 0, Random.Range(-15f, 15f));
            agent.SetDestination(player.position+deflection);
            _animator.SetFloat("isWalk", 1);
            isPatrol = true;
        }

        if (!agent.isStopped)
        {
            isPatrol = false;
        }
        
    }
    private bool CanTolerance ()
    {
        if((Random.Range(0, 10)>6))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    /*
    private bool SendRaycast()
    {
        
        Vector3 playerPos = transform.position;
        Vector3 playerDirection = transform.forward;
        float spawnDistance = 2;
 
        Vector3 spawnPos = playerPos + new Vector3(0,0.5f,0) +playerDirection*spawnDistance;
        Ray ray = new Ray(spawnPos, Vector3.forward);
        
        RaycastHit hit;
        */
        //Ray ray = new Ray(transform.position, Vector3.forward);
        /*
        RaycastHit hit;
        if (Physics.Raycast(RayTransform.position,Vector3.forward, out hit,40f))
        {
            if (hit.collider.tag == "Player")
            {
                Debug.Log("PlayerRaycast");
                return true;
            }
            else
            {
                Debug.Log("OtherRaycast");
                return false;
            }
        }
        else
        {
            Debug.Log("NoneRaycast");
            return false;
        }
        
    }
    */
    private IEnumerator DestroyFireBall(GameObject _fireball)
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(_fireball);
    }
    
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
    
    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
