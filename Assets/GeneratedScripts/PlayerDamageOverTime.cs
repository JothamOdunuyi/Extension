using UnityEngine;

public class PlayerDamageOverTime : MonoBehaviour
{
    public float damageInterval = 1f;
    public int damageAmount = 1;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= damageInterval)
        {
            timer = 0;
            ApplyDamage();
        }
    }

    void ApplyDamage()
    {
        // Apply damage to the player here
    }
}