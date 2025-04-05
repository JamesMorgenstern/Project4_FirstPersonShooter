using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public ZombieHand zombieHand;
    public int zombieDamage;

    private void Start()
    {
        zombieHand.damage = zombieDamage;
    }
}
