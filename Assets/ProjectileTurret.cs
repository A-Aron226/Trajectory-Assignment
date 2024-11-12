using Palmmedia.ReportGenerator.Core;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectileTurret : MonoBehaviour
{
    [SerializeField] float projectileSpeed = 1;
    [SerializeField] Vector3 gravity = new Vector3(0, -9.8f, 0);
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;
    [SerializeField] bool useLowAngle;

    List<Vector3> points = new List<Vector3>();

    // Update is called once per frame
    void Update()
    {
        TrackMouse();
        TurnBase();
        RotateGun();
        Line();

        if (Input.GetButtonDown("Fire1"))
            Fire();
    }

    void Fire()
    {
        GameObject projectile = Instantiate(projectilePrefab, barrelEnd.position, gun.transform.rotation);
        projectile.GetComponent<Rigidbody>().velocity = projectileSpeed * barrelEnd.transform.forward;
    }

    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if(Physics.Raycast(cameraRay, out hit, 1000, targetLayer))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
            //Debug.Log("hit ground");
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
        turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }

    void RotateGun()
    {
        float? angle = CalculateTrajectory(crosshair.transform.position, useLowAngle);
        if (angle != null)
            gun.transform.localEulerAngles = new Vector3(360f - (float)angle, 0, 0);
    }

    float? CalculateTrajectory(Vector3 target, bool useLow)
    {
        Vector3 targetDir = target - barrelEnd.position;
        
        float y = targetDir.y;
        targetDir.y = 0;

        float x = targetDir.magnitude;

        float v = projectileSpeed;
        float v2 = Mathf.Pow(v, 2);
        float v4 = Mathf.Pow(v, 4);
        float g = gravity.y;
        float x2 = Mathf.Pow(x, 2);

        float underRoot = v4 - g * ((g * x2) + (2 * y * v2));

        if (underRoot >= 0)
        {
            float root = Mathf.Sqrt(underRoot);
            float highAngle = v2 + root;
            float lowAngle = v2 - root;

            if (useLow)
                return (Mathf.Atan2(lowAngle, g * x) * Mathf.Rad2Deg);
            else
                return (Mathf.Atan2(highAngle, g * x) * Mathf.Rad2Deg);
        }
        else
            return null;
    }

    public void Line() //Needs work
    {
        //float t;
        //float a = gravity.y;
        //float vi = projectileSpeed;
        //float vf;
        //float d;

        //Calculating time
        //t = (-1f * vi) / a;
        //t = 2f * t;

        //Calculating final velocity and displacement
        //vf = vi + a * t;
        //d = ((vi + vf) / 2f) * t;
        /*float timeCount = t * time;
        if (timeCount > 10)
        {
            timeCount = 0;
        }*/

        //float dispX = v * t + (0.5f) * accelX * Mathf.Pow(t, 2);
        //float dispY = v * t + (0.5f) * accelY * Mathf.Pow(t, 2);
        //float dispY = (v * t) + (0.5f * accelY * Mathf.Pow(t, 2));
        //float dispZ = v * t + (0.5f) * accelZ * Mathf.Pow(t, 2);

        /*if (Physics.Raycast(barrelEnd.position, barrelEnd.forward, out RaycastHit hit, 1000.0f, targetLayer))
        {
            points.Add(hit.point);
        }*/

        /////////////////////////////
        points.Clear();
        float accelY = -gravity.y;
        float accelX = 0f;
        float accelZ = 0f;
        float v = projectileSpeed;

        
        points.Add(barrelEnd.position);

        Vector3 vi = barrelEnd.forward * v;
        float timeStep = 0.005f;
        float maxTime = 1.0f;
        Vector3 lastPosition = barrelEnd.position;

        for (float t = 0;  t <= maxTime; t += timeStep)
        {
            float dispX = (vi.x * t);
            float dispY = (vi.y * t) + (0.5f * accelY * Mathf.Pow(t, 2));
            float dispZ = (vi.z * t);


            Vector3 currDisp = new Vector3(dispX, dispY, dispZ);
            Vector3 positionAtT = barrelEnd.position + currDisp;

            if (Physics.Linecast(lastPosition, positionAtT, out RaycastHit hit, targetLayer))
            {
                points.Add(hit.point);
                break;
            }

            points.Add(positionAtT);
            lastPosition = positionAtT;
        }

        line.positionCount = points.Count;

        for (int i = 0; i < line.positionCount; i++)
        {
            line.SetPosition(i, points[i]);
        }
    }
}
