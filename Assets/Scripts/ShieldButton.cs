using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(BoxCollider), typeof(AudioSource))] 
public class PhysicalButton : MonoBehaviour
{
    public GameObject ForceShield;
    public AudioClip pressedSound; // Sound to play when pressed
    private Button _uiButton;
    private AudioSource _audioSource;
    private bool _isShieldActive = false;

    public float buttonCooldown = 4.5f; // Cooldown delay
    private float _lastPressTime = -999f;

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
            return;
        }

        _lastPressTime = Time.time;

        if (_uiButton != null)
        {
            _uiButton.onClick.Invoke();
            
            // Play sound if assigned
            if (_audioSource != null && pressedSound != null)
            {
                _audioSource.PlayOneShot(pressedSound);
            }

            Debug.Log("Shield Activated!");
            
            if (!_isShieldActive)
            {
                StartCoroutine(HandleShield());
            }
        }
    }

    private IEnumerator HandleShield()
    {
        if (ForceShield == null) yield break;

        _isShieldActive = true;
        ForceShield.SetActive(true);
        
        yield return new WaitForSeconds(5);
        
        ForceShield.SetActive(false);
        _isShieldActive = false;
    }
}