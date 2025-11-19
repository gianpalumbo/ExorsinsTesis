using UnityEngine.VFX;
using UnityEngine;
using UnityEngine.UIElements;

public class AcidLaser : MonoBehaviour
{
    [SerializeField] VisualEffect _vfx;
    [SerializeField] float _maxDistance = 100f;

    void Update()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        Ray ray = new Ray(origin, direction);
        if (Physics.Raycast(ray, out RaycastHit hit, _maxDistance))
        {
            float lenght = Vector3.Distance(hit.point, origin);
            _vfx.SetFloat("LaserLenght", lenght);
            _vfx.SetVector3("ImpactPoint", hit.point);
            _vfx.SetVector3("InpactNormal", hit.normal);

        }
        else
        {
            _vfx.SetFloat("LaserLenght", _maxDistance);
            _vfx.SetVector3("ImpactPoint", direction * _maxDistance);
            _vfx.SetVector3("InpactNormal", -direction);
        }
    }
}

