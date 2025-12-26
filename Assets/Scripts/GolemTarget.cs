using UnityEngine;
using System.Collections;
using UnityEngine.XR.ARFoundation;

public class GolemTarget : MonoBehaviour
{
    public float respawnDelay = 3f;
    public int pointsValue = 1;
    public GameObject deathEffectPrefab;
    public AudioClip dieSound;  
    public AudioSource _audioSource;


    public Vector3 respawnAreaMin = new Vector3(-5, 0, 5);
    public Vector3 respawnAreaMax = new Vector3(5, 0, 15);

    private bool _isDead = false;
    private Renderer[] _renderers;
    private Collider[] _colliders;
    private ARAnchor _anchor;

    void Start()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _colliders = GetComponentsInChildren<Collider>();

        // Auto-fetch audio source if not assigned
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null) 
            {
               _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Add AR Anchor to stick it to the world
        _anchor = GetComponent<ARAnchor>();
        if (_anchor == null) _anchor = gameObject.AddComponent<ARAnchor>();
    }

    void OnCollisionEnter(Collision collision)
    {
        CheckHit(collision.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        CheckHit(other.gameObject);
    }

    void CheckHit(GameObject hittingObject)
    {
        if (_isDead) return;

        // Check if the hitting object is a rocket
        RocketProjectile rocket = hittingObject.GetComponent<RocketProjectile>();
        
        if (rocket != null)
        {
            Debug.Log("Golem hit by rocket!");
            Die();
        }
    }

    void Die()
    {
        _isDead = true;
        
        // Add Score
        if (GameScoreManager.Instance != null)
        {
            GameScoreManager.Instance.AddScore(pointsValue);
        }
        else
        {
            Debug.LogWarning("GameScoreManager missing from scene! Score not added.");
        }

        // Show Effect
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, transform.rotation);
            Destroy(effect, 3f); // Auto-destroy the "dead prefab" (Ghost) after 3 seconds
        }
        
        if (_audioSource != null && dieSound != null)
        {
            _audioSource.PlayOneShot(dieSound);
        }


        // Hide Golem
        SetVisibility(false);

        // Start Respawn
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        Respawn();
    }

    void Respawn()
    {
        // Anchor Management: We must destroy the old anchor to move the object
        if (_anchor != null) Destroy(_anchor);
        
        // Move to random position (optional logic)
        float x = Random.Range(respawnAreaMin.x, respawnAreaMax.x);
        float y = Random.Range(respawnAreaMin.y, respawnAreaMax.y);
        float z = Random.Range(respawnAreaMin.z, respawnAreaMax.z);
        
        // Keep the same Y if you don't want them floating/sinking, or use the random Y
        transform.position = new Vector3(x, transform.position.y, z); 
        
        // Add new Anchor at new position
        _anchor = gameObject.AddComponent<ARAnchor>(); 
        // For now, let's keep it simple or use the range if configured
        
        // If the user wants specific logic, we can adjust. For now, let's just make it reappear at the same spot 
        // OR a slightly randomized offset to simulate "new enemy". 
        // Let's stick to simple Reappear for now as per request "disapear and re apear"
        
        SetVisibility(true);
        _isDead = false;
    }

    void SetVisibility(bool isVisible)
    {
        foreach (var r in _renderers) r.enabled = isVisible;
        foreach (var c in _colliders) c.enabled = isVisible;
    }
}
