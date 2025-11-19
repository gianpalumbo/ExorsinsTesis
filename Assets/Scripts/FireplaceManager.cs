using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireplaceManager : MonoBehaviour
{
    public static FireplaceManager Instance;
    public AudioSource audioSource;
    [SerializeField] float fadeSpeed = 1f;
    private float targetVolume = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource.volume = 0f;
    }

    public void SetVolume(float volume)
    {
        if (volume > targetVolume)
        {
            targetVolume = volume;
            //Debug.Log(volume);
        }
    }

    void LateUpdate()
    {
        audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);
        targetVolume = 0f;
    }
}
