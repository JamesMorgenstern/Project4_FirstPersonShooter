using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int HP = 100;
    private Animator animator;
    
    private NavMeshAgent navAgent;

    public bool isDead;

    private void Start()
    {
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
    }

    public void TakeDamage(int damageAmount)
    {
        HP -= damageAmount;

        if (HP <= 0)
        {
            int randomValue = Random.Range(0, 2); // 0 or 1

            if (randomValue == 0)
            {
                animator.SetTrigger("Death1");
            }
            else
            {
                animator.SetTrigger("Death2");
            }
            
            isDead = true;
            
            //Death Sound
            SoundManager.Instance.zombieChannel2.PlayOneShot(SoundManager.Instance.zombieDeath);
        }
        else
        {
            animator.SetTrigger("Damage");
            
            //Hurt Sound
            SoundManager.Instance.zombieChannel2.PlayOneShot(SoundManager.Instance.zombieHurt);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2.5f); //attacking // stop attacking
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 200f); // detection (start chasing)
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 201f); // stop chasing
    }
}
