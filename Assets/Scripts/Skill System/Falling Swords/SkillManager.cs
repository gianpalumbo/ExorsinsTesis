using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using System;
public class SkillManager : MonoBehaviour
{
    //ES PRIVADA, OSEA _currentSkill
    private ISkill currentSkill;

    // usa el mismo enum que ya esta en SkillButtonUI.cs
    private Dictionary<SkillType, ISkill> skillDictionary = new Dictionary<SkillType, ISkill>();



    //AGUS ADDON
    //[SerializeField] SkillWheelUI skillWheel;
    [SerializeField] List<SkillType> skillList;
    public event Func<float, bool> OnSkillUse;
    public event Action OnManaRecharge, OnCantUseSkill;
    [SerializeField] float manaCost;


    //OTRA FORMA DE HACER DICCIONARIO
    //private Dictionary<SkillType, ISkill> _skills;


    //CONSTRUCTOR PARA PASARLE PARAMETROS A LAS HABILIDADES
    [Header("SwordsFalling Skill Settings")]
    [SerializeField] private GameObject swordBulletPrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float spawnHeight = 10f;
    [SerializeField] private float outerRadius = 5f;
    [SerializeField] private float innerRadius = 1f;
    [SerializeField] private float spawnInterval = 0.2f;
    [SerializeField] private float spawnDuration = 5f;
    [SerializeField] private int spawnCount;
    [SerializeField] private float _cdSword;
    [SerializeField] private Transform swordSpawner; //NO LO ESTOY USANDO

    [Header("Slow Zone Skill Settings")]
    [SerializeField] private GameObject slowPrefab;
    [SerializeField] private Transform slowSpawner;
    [SerializeField] private float _cdSlow;
    //[SerializeField] private GameObject fireVfxPrefab;

    [Header("Fire Shoot Skill Settings")]
    [SerializeField] private GameObject _fireballPrefab;
    [SerializeField] private Transform _fireballSpawner;
    [SerializeField] private float _cdFireball;
    //[SerializeField] private GameObject fireVfxPrefab;



    //AGUS WAS HERE
    [SerializeField] GameObject[] skillIcons;
    [SerializeField] float _counter;



    private void Awake()
    {
        if (playerTransform == null)
        {
            //Debug.Log("SKILLMANAGER: NO HABIA PLAYER");
            playerTransform = GameObject.FindWithTag("Player").transform;
        }



        // AGUS WAS HERE
        ServiceLocator.Instance.RegisterDependency<SkillManager>(this);



        // agrego aca mas mappings si creo nuevas habilidades
        //skillDictionary.Add(SkillType.Fire, new FireSkill());
        
        skillDictionary.Add(SkillType.SlowZone, new SlowSkill(
                  slowPrefab,
                  playerTransform,
                  slowSpawner,
                  _cdSlow
                  //spawnDuration,
                  ));

        //NUEVO
        skillDictionary.Add(SkillType.SwordsFalling, new SwordFallingSkill(
                  swordBulletPrefab,
                  playerTransform,
                  swordSpawner,
                  spawnHeight,
                  outerRadius,
                  innerRadius,
                  spawnInterval,
                  spawnDuration,
                  spawnCount,
                  _cdSword
                  ));

        //    //LA OTRA FORMA DE HACERLO POR DICCIONARIO, SOLAMENTE ESCRIBO ESTO EN VEZ DEL ADD, COMO SETEARLO ANTES
        //    _skills = new Dictionary<SkillType, ISkill>
        //    {
        //        { SkillType.Fire, new FireSkill() },
        //        { SkillType.Ice, new IceSkill(
        //            iceBulletPrefab,
        //            iceSpawner,
        //            spawnHeight,
        //            outerRadius,
        //            innerRadius,
        //            spawnInterval,
        //            spawnDuration) }
        //    };

        //    // Skill por defecto
        //    _currentSkill = _skills[SkillType.Fire];
        //}

        skillDictionary.Add(SkillType.FireBall, new FireballSkill(
                  _fireballPrefab,
                  _fireballSpawner,
                  _cdFireball
                  ));

        //currentSkill = skillDictionary[SkillType.Ice]; //me aseguro que tiene una habilidad por defecto
        currentSkill = skillDictionary[SkillType.SlowZone]; //me aseguro que tiene una habilidad por defecto



        //AGUS WAS HERE
        //LE PASO DEL DICCIONARIO LOS SKILL TYPES PARA RECORRERLO CON TAB
        foreach (var skill in skillDictionary)
        {
            skillList.Add(skill.Key);
        }
    }

    private void Start()
    {
        ServiceLocator.Instance.GetDependency<ControllerPlayer>().OnKeyPressed += CanIUseSkill;
    }
    bool CanIUseSkill(KeyCode keyCode) => Input.GetKeyDown(keyCode) ? true : false;

    private void OnDestroy()
    {
        ServiceLocator.Instance.RemoveDependency<SkillManager>();
    }

    public void ChangeSkill()
    {
        if (currentSkill == skillDictionary[SkillType.SlowZone])
        {
            TurnOffIcons();
            skillIcons[0].SetActive(true);
            SetSkill(SkillType.SwordsFalling);
        }
        else if (currentSkill == skillDictionary[SkillType.SwordsFalling])
        {
            TurnOffIcons();
            skillIcons[1].SetActive(true);
            SetSkill(SkillType.FireBall);
        }
        else if (currentSkill == skillDictionary[SkillType.FireBall])
        {
            TurnOffIcons();
            skillIcons[2].SetActive(true);
            SetSkill(SkillType.SlowZone);
        }
    }


    public void SetSkill(SkillType skillType)
    {
        if (skillDictionary.TryGetValue(skillType, out var skill))
        {
            currentSkill = skill;

            //Debug.Log("Habilidad seleccionada: " + skillType);
        }
        //else Debug.LogWarning("SkillManager: Habilidad no encontrada - " + skillType);        
    }
    public void UseSkill()
    {
        if (currentSkill == null || OnSkillUse == null) return;

        //Debug.Log("CurrentSkill existe");

        bool canTryToUseSkill = !currentSkill.IsCooling(); //SI ESTOY EN COOLING NI CHEQUEO PARA SACAR MANA, SINO CHEQUEO PARA SACAR MANA Y USAR SKILL

        if (!canTryToUseSkill) //ACA NO PUEDO POR CD
        {
            OnCantUseSkill?.Invoke();
            return;
        }

        bool canUseSkill = OnSkillUse != null && OnSkillUse.Invoke(manaCost); //CHEQUEO NULL DE EVENTO Y SI TENGO MANA

        //Debug.Log(canUseSkill);
        if (canUseSkill)
        {
            //Debug.Log($"intento usar skill con {manaCost} de mana y pude? {canUseSkill}");
            if (currentSkill == skillDictionary[SkillType.FireBall])
            {
                StartCoroutine(ServiceLocator.Instance.GetDependency<AttackEFSM>().RotateToCamera(.2f));
                ServiceLocator.Instance.GetDependency<PlayerMVC>().AnimatorPlayer.SetTrigger("Fireball");
                StartCoroutine(DoFireball());
            }
            else
                currentSkill.Activate(this);
        }
        else
            OnCantUseSkill?.Invoke();

        //if (currentSkill != null) currentSkill.Activate();
        //if (currentSkill != null)
        //{
        //    //SoundManager.Instance.PlayOneShotFromIndex(0);
        //    if(currentSkill == skillDictionary[SkillType.FireBall])
        //    {
        //        StartCoroutine(ServiceLocator.Instance.GetDependency<AttackEFSM>().RotateToCamera(.2f));
        //        ServiceLocator.Instance.GetDependency<PlayerMVC>().AnimatorPlayer.SetTrigger("Fireball");
        //        StartCoroutine(DoFireball());
        //    }
        //    else
        //    currentSkill.Activate(this);

        //}// Paso this para runner (coroutines/Instantiate)
    }

    IEnumerator DoFireball()
    {
        yield return new WaitForSeconds(.55f);
        currentSkill.Activate(this);

        ServiceLocator.Instance.GetDependency<AttackEFSM>().Think();
    }

    void TurnOffIcons()
    {
        for (int i = 0; i < skillIcons.Length; i++)
        {
            skillIcons[i].SetActive(false);
        }
    }

    //// Dibuja en escena el anillo de spawn y exclusión
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
    //    Gizmos.DrawWireSphere(playerTransform.position + Vector3.up * spawnHeight, outerRadius);
    //    Gizmos.color = new Color(0f, 0f, 1f, 0.3f);
    //    Gizmos.DrawWireSphere(playerTransform.position + Vector3.up * spawnHeight, innerRadius);
    //}
}