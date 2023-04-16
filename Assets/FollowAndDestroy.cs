using UnityEngine;

public class FollowAndDestroy : MonoBehaviour
{
    public Transform player;
    public float speed;
    public float attackRange;
    public float attackDamage;

    void Update()
    {
        // Move the object towards the player
        transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

        // Get the distance between the object and the player
        float distance = Vector2.Distance(transform.position, player.position);

        // If the distance is less than attackRange, attack the player
        if (distance <= attackRange)
        {
            Attack();
        }
    }

    void Attack()
    {
        // Destroy all enemies in the attack range
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (Collider2D enemy in enemiesInRange)
        {
            //enemy.GetComponent<Health>().TakeDamage(attackDamage);
        }
    }
}