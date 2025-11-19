using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ManaBlink : MonoBehaviour
{
    [SerializeField] private Image manaBar;
    [SerializeField] private Color blinkColor = Color.red;
    [SerializeField] private float blinkSpeed = 0.5f, timeBlinking = 1f;
    float counter = 0;
    private Color originalColor;
    private bool blinking = false;

    void Start()
    {
        originalColor = manaBar.color;

        if (ServiceLocator.Instance.TryGetDependency<SkillManager>(out SkillManager skill))
        {
            skill.OnCantUseSkill += StartBlink;
        }
    }
    public void StartBlink()
    {
        if (!blinking)
        {
            Debug.Log("Arranco blink blink");
            StartCoroutine(Blink());
        }
    }
    private void Update()
    {
        if(blinking)
        {
            counter += Time.deltaTime;
        }
    }
    public void StopBlink()
    {
        counter = 0;
        blinking = false;
        manaBar.color = originalColor;
    }
    private IEnumerator Blink()
    {
        blinking = true;

        while (blinking && counter <= timeBlinking)
        {
            manaBar.color = blinkColor;
            yield return new WaitForSeconds(blinkSpeed);
            manaBar.color = originalColor;
            yield return new WaitForSeconds(blinkSpeed);
        }
        StopBlink();
    }
}