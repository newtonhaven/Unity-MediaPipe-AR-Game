using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider), typeof(AudioSource))]
public class ShootButton : MonoBehaviour
{
    public GameObject rocketSpherePrefab;
    public float shootForce = 500f;
    public float spawnDistance = 0.8f; // Distance from camera to spawn missile
    public float buttonCooldown = 1.0f; // Time in seconds between shots
    public AudioClip pressedSound; // Sound to play when pressed
    
    [Header("Visual Effects")]
    public GameObject shootEffectPrefab;
    public Transform effectSpawnPoint; // Optional: Assign gun muzzle here

    private Button _uiButton;
    private AudioSource _audioSource;
    private float _lastPressTime = -999f; // Initialize to allow immediate first shot

    void Start()
    {
        _uiButton = GetComponent<Button>();
        _audioSource = GetComponent<AudioSource>();
        GetComponent<BoxCollider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // Cooldown Check
        if (Time.time < _lastPressTime + buttonCooldown)
        {
            return; // Ignore press if still cooling down
        }

        _lastPressTime = Time.time; // Update last press time

        if (_uiButton != null)
        {
            _uiButton.onClick.Invoke();
        }

        // Play sound if assigned
        if (_audioSource != null && pressedSound != null)
        {
            _audioSource.PlayOneShot(pressedSound);
        }
        
        Shoot();
    }

    void Shoot()
    {
        Debug.Log("Attempting to shoot..."); // Debug 1

        if (rocketSpherePrefab == null)
        {
            Debug.LogError("FAIL: Rocket Sphere Prefab is NOT assigned in the Inspector!");
            return;
        }

        if (Camera.main == null)
        {
            Debug.LogError("FAIL: Main Camera could not be found! Ensure your AR Camera has the tag 'MainCamera'.");
            return;
        }

        // Spawn slightly in front of the camera using the configurable distance
        Vector3 spawnPos = Camera.main.transform.position + (Camera.main.transform.forward * spawnDistance);
        GameObject rocket = Instantiate(rocketSpherePrefab, spawnPos, Camera.main.transform.rotation);
        
        Rigidbody rb = rocket.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log("Success: Rocket instantiated and Rigidbody found. Adding force.");
            rb.AddForce(Camera.main.transform.forward * shootForce);
        }
        else
        {
            Debug.LogError("FAIL: The instantiated Rocket Prefab does NOT have a Rigidbody component!");
        }

        // VFX
        if (shootEffectPrefab != null)
        {
            Vector3 effectPos = (effectSpawnPoint != null) ? effectSpawnPoint.position : spawnPos;
            Quaternion effectRot = (effectSpawnPoint != null) ? effectSpawnPoint.rotation : Camera.main.transform.rotation;
            
            GameObject vfx = Instantiate(shootEffectPrefab, effectPos, effectRot);
            Destroy(vfx, 1.0f);
        }
    }
}