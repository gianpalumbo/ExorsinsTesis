using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class SlowZone : MonoBehaviour
{   
    //PODRIA HACER QUE REALENTIZE ENEMIGOS Y AUMENTE LA VELOCIDAD DE LOS ALIADOS, SERIA CHANCEFACTOR
    private HashSet<ISpeedSlower> slowed = new HashSet<ISpeedSlower>(); //lista hash donde guardo enemigos sloweados
    [SerializeField] Vector3 targetScale;
    [SerializeField] float speed;
    CapsuleCollider col;

    private void Start()
    {
        Destroy(gameObject, 2);
    }
    private void Update()
    { 
        //slowed.RemoveWhere(s => s == null); // aunque se destruya el gameobject del enemigo se mantiene la referencia en el HashSet, pero ya no apunta a nada real, osea que queda como null y los destruyo
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * speed);
    }
    private void OnTriggerEnter(Collider other) 
    {
        if (other.TryGetComponent<ISlowable>(out var slowable))
            slowable.SlowEntity();

        VisualEffect chainsVFX = other.GetComponentsInChildren<VisualEffect>(true).FirstOrDefault(vfx => vfx.name == "VFX_Chains");
        if (chainsVFX != null) chainsVFX.SendEvent("OnPlay");

        //ISpeedSlower speedSlower = other.GetComponent<ISpeedSlower>();
        //FSMEnemy fsm = other.GetComponent<FSMEnemy>();

        //if (fsm != null)
        //{
        //    fsm.isSlowed = true;
        //    fsm._currentSpeed = fsm.slowSpeed;
        //    Debug.Log($"SlowZone: {other.name} _currentSpeed={fsm._currentSpeed}");

        //}
        //if (speedSlower != null) slowed.Add(speedSlower);

        //if (speedSlower != null)
        //{
        //    Debug.Log($"SLOWZONE: se aplico el slow al enemigo {other.name}", other.gameObject);
        //    slowed.Add(speedSlower);
        //    speedSlower.SpeedSlower();
        //}
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    if (!other.CompareTag("Enemy")) return;
    //    ISpeedSlower speedSlower = other.GetComponent<ISpeedSlower>();

    //    FSMEnemy fsm = other.GetComponent<FSMEnemy>();

    //    if (fsm != null)
    //    {
    //        fsm.isSlowed = false;
    //        fsm._currentSpeed = fsm.normalSpeed;
    //    }
    //    Debug.Log($"SLOWZONE: se reinicia la velocidad al enemigo {other.name}", other.gameObject);
    //    if (speedSlower != null)
    //    {
    //        slowed.Remove(speedSlower);
    //        speedSlower.SpeedReset();
    //    }
    //    if (other.TryGetComponent<ISlowable>(out var slowable))
    //        slowable.UnSlowEntity();
    //}


    //private void OnDestroy()
    //{
    //    foreach (var s in slowed)
    //    {
    //        s?.SpeedReset(); //si s no es null, entonces se ejecuta

    //    }
    //}
}
