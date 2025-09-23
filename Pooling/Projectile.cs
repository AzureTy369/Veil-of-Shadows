using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour, IPoolable
{
    public float speed = 5f;
    public float ttl = 3f;
    public Vector2 direction = Vector2.right;

    private float timer;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnSpawnFromPool()
    {
        timer = 0f;
        if (rb != null)
        {
            rb.velocity = direction.normalized * speed;
        }
        gameObject.SetActive(true);
    }

    public void OnReturnToPool()
    {
        if (rb != null)
            rb.velocity = Vector2.zero;
        StopAllCoroutines();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= ttl) Return();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(1f);
        }

        PoolManager.Instance.SpawnIfExists("FX_HitSmall", transform.position, Quaternion.identity);
        Return();
    }

    private void Return() => PoolManager.Instance.ReturnToPool(this.gameObject);
}


