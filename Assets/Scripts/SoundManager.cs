using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioSource audioSourceConstant, audioSourceOneShot, musicAudioSource;

    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider sliderMaster, sliderMusic, sliderSFX; //Sliders para el volumen

    public static SoundManager Instance;
    public GameObject menuButtonPanel;

    [SerializeField] GameObject audioPanel;
    [SerializeField] Button goBackButton;

    public PlayerMVC player;

    [SerializeField] AudioClip[] audioClips;

    public bool isResting = false;

    public void OnEnable()
    {
        menuButtonPanel = FindObjectOfType<MenuButton>()?.gameObject;
        //if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 0)
        //    menuButtonPanel = FindObjectOfType<MenuButton>().gameObject;
        //Debug.Log("OnEnable se ejecuta");
        CanvasGroupTurnOff();
    }

    public void CanvasGroupTurnOff()
    {
        audioPanel.SetActive(true);
        CanvasGroup cg = audioPanel.GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    public void CanvasGroupTurnOn()
    {
        CanvasGroup cg = audioPanel.GetComponent<CanvasGroup>();
        cg.alpha = 255f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
        //Para que no haya duplicados
    }
    private void Start()
    {
        goBackButton.onClick.AddListener(() =>
        {
            CanvasGroupTurnOff();
            if (menuButtonPanel != null) menuButtonPanel.SetActive(true);
            if (PauseManager.instance != null) PauseManager.instance.IsNotInVolumePanel();
        });

        if (PlayerPrefs.HasKey("masterVolume") && PlayerPrefs.HasKey("musicVolume") && PlayerPrefs.HasKey("sfxVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMaster();
            SetMusic();
            SetSFX();
        }

        if (player == null)
        {
            audioSourceConstant.mute = true;
        }
    }
    public void ChangeToMenuMusic() => musicAudioSource.clip = audioClips[13]; //Clip Menu Music
    public void ChangeToCaveAmbience() => musicAudioSource.clip = audioClips[14]; //Clip Cave Ambience
    /// <summary>
    /// Orden de los audio clips
    /// 0 Angeles - 1 Explosion - 2 FireEnter - 3 FireStay - 4 MonsterGrunt - 5 Run - 6 SwordSwing - 7 Walk
    /// 8 OnHit - 9 PlayerDeath - 10 Slam Ground - 11 Heal SFX - 12 SwordOnDirt - 13 MenuMusic - 14 CaveAmbience
    /// 15 Sword against rock
    /// </summary>
    /// <param name="index"></param>
    /// 
    public void PlayFootsteps()
    {
        if (isResting || player == null)
        {
            audioSourceConstant.mute = true;
            return;
        }

        bool isWalking = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;

        if (isWalking)
        {
            //Debug.Log("WALKING");
            audioSourceConstant.mute = false;
            audioSourceConstant.clip = audioClips[7]; //Clip Walking
            //audioSourceConstant.
        }
        else
        {
            audioSourceConstant.mute = true;
        }
    }
    public void PlayOneShotFromIndex(int index)
    {
        if (audioClips == null || index < 0 || index >= audioClips.Length)
            return; // <- salís si no es válido

        audioSourceOneShot.PlayOneShot(audioClips[index]);
    }
    public void ChangePitch(float add)
    {
        audioSourceOneShot.pitch += add;
    }
    public void PitchBackToOriginal()
    {
        audioSourceOneShot.pitch = 1;
    }
    public void SetMaster()
    {
        float volume = sliderMaster.value;
        audioMixer.SetFloat("MasterParam", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("masterVolume", volume); //guardo la key masterVolume para que ser guarde
    }
    public void SetMusic()
    {
        float volume = sliderMusic.value;
        audioMixer.SetFloat("MusicParam", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }
    public void SetSFX()
    {
        float volume = sliderSFX.value;
        audioMixer.SetFloat("SFXParam", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("sfxVolume", volume);
    }
    public void LoadVolume()
    {
        sliderMaster.value = PlayerPrefs.GetFloat("masterVolume", 1); //si no hay nada guardado, por defecto es 1
        sliderMusic.value = PlayerPrefs.GetFloat("musicVolume", 1);
        sliderSFX.value = PlayerPrefs.GetFloat("sfxVolume", 1);
    }
}
