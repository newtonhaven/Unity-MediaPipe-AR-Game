using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RocketProjectile : MonoBehaviour
{
    public float lifeTime = 5f;
    public GameObject hitEffectPrefab;

    void Start()
    {
        Debug.Log("Rocket started");
        // Auto-destroy after lifetime to prevent infinite rockets
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Rocket hit");
        HandleHit(other.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Rocket collided");
        HandleHit(collision.gameObject);
    }

    void HandleHit(GameObject otherObj)
    {
        // Ignore the player
        if (otherObj.CompareTag("Player")) return;

        // Ignore UI elements (Layer 5 is built-in Unity UI layer)
        // if (otherObj.layer == 5) 
        // {
        //     Debug.Log($"Rocket IGNORED hit on UI Layer object: {otherObj.name}");
        //     return;
        // }
        
        
        if (otherObj.name == "shootButton" || otherObj.name == "ShieldButton") 
        {
            Debug.Log($"Rocket IGNORED hit on Button object: {otherObj.name}");
            return;
        }

        Debug.Log($"Rocket hit: {otherObj.name}");

        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, transform.rotation);
        }

        // Destroy the rocket upon impact
        Destroy(gameObject);
    }
}
