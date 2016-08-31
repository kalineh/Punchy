using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Bonsai3))]
public class Bonsai3Editor
    : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var bonsai = target as Bonsai3;

        if (GUILayout.Button("Grow"))
            bonsai.MakeBranch();
        if (GUILayout.Button("AutoGrow"))
            bonsai.StartAutoGrow();
        if (GUILayout.Button("Stop All Coroutines"))
            bonsai.StopAllCoroutines();
    }
}

#endif


[SelectionBase]
public class Bonsai3
    : MonoBehaviour
{
    public GameObject parent;

    public float MoveForce;
    public float MovePower;
    public float TorqueForce;

    public void Start()
    {
        OnScriptReload();
    }

    public void OnScriptReload()
    {
    }

    public void Attach(GameObject p, Vector3 ofs, Vector3 dir)
    {
        var attach = DoAttachment(p, ofs, dir);

        StartCoroutine(attach);
    }

    public IEnumerator DoAttachment(GameObject p, Vector3 ofs, Vector3 dir)
    {
        parent = p;

        var body = GetComponent<Rigidbody>();
        var bodyParent = parent.GetComponent<Rigidbody>();

        var baseRotOfsWithParent = Quaternion.LookRotation(dir, bodyParent.rotation * Vector3.up);
        var baseRotOfs = Quaternion.Inverse(bodyParent.rotation) * baseRotOfsWithParent;

        //var rotOfsSrcEuler = bodyParent.rotation.eulerAngles;
        //var rotOfsDstEuler = baseRotOfs.eulerAngles;
        //var ofsEulerX = Mathf.DeltaAngle(rotOfsSrcEuler.x, rotOfsDstEuler.x);
        //var ofsEulerY = Mathf.DeltaAngle(rotOfsSrcEuler.y, rotOfsDstEuler.y);
        //var ofsEulerZ = Mathf.DeltaAngle(rotOfsSrcEuler.z, rotOfsDstEuler.z);
        //Debug.LogFormat("parent: {0}, x:{1},y:{2},z:{3}", p.name, (int)ofsEulerX, (int)ofsEulerY, (int)ofsEulerZ);

        //var axis = AxisHelper.Create();
        //axis.transform.localScale = Vector3.one * 1.25f;

        body.MovePosition(bodyParent.position + bodyParent.rotation * ofs);
        body.MoveRotation(bodyParent.rotation * baseRotOfs);


        while (true)
        {
            Debug.DrawLine(transform.position, transform.position + baseRotOfs * Vector3.forward * 1.25f, Color.white);

            var targetPos = bodyParent.position + bodyParent.rotation * ofs;
            var targetRot = bodyParent.rotation * baseRotOfs;

            //Debug.DrawLine(transform.position, transform.position + targetRot * Vector3.forward * 0.7f, Color.blue + Color.white * 0.5f);
            //Debug.DrawLine(transform.position, transform.position + targetRot * Vector3.right * 0.7f, Color.red + Color.white * 0.5f);
            //Debug.DrawLine(transform.position, transform.position + targetRot * Vector3.up * 0.7f, Color.green + Color.white * 0.5f);

            //Debug.DrawLine(transform.position, transform.position + bodyParent.rotation * Vector3.forward, Color.blue);
            //Debug.DrawLine(transform.position, transform.position + bodyParent.rotation * Vector3.right, Color.red);
            //Debug.DrawLine(transform.position, transform.position + bodyParent.rotation * Vector3.up, Color.green);

            //axis.transform.position = targetPos;
            //axis.transform.rotation = targetRot;

            // testing
            //body.MovePosition(bodyParent.position + bodyParent.rotation * ofs);
            //body.MoveRotation(targetRot);

            var dt = Time.deltaTime;
            if (dt <= 0.0f)
            {
                yield return null;
                continue;
            }

            var moveOfs = targetPos - body.position;
            var moveDir = moveOfs.SafeNormalize();
            var moveLen = moveOfs.SafeMagnitude();

            var moveForce = moveDir * Mathf.Pow(moveLen, MovePower) * MoveForce;

            body.AddForce(moveForce * Time.deltaTime);
           
            var torque = Vector3.zero;

            var rotForward = targetRot * Vector3.forward;
            var rotRight = targetRot * Vector3.right;
            var rotUp = targetRot * Vector3.up;

            rotForward = rotForward.SafeNormalizeOr(Vector3.forward);
            rotRight = rotRight.SafeNormalizeOr(Vector3.right);
            rotUp = rotUp.SafeNormalizeOr(Vector3.up);

            var bodyForward = body.rotation * Vector3.forward;
            var bodyRight = body.rotation * Vector3.right;
            var bodyUp = body.rotation * Vector3.up;

            bodyForward = bodyForward.SafeNormalizeOr(Vector3.forward);
            bodyRight = bodyRight.SafeNormalizeOr(Vector3.right);
            bodyUp = bodyUp.SafeNormalizeOr(Vector3.up);

            var forwardAxis = Vector3.Cross(bodyForward, rotForward);
            var forwardTheta = Mathf.Asin(forwardAxis.SafeMagnitude());
            var forwardAngle = forwardAxis.normalized * forwardTheta / dt;
            var forwardBasis = targetRot * body.inertiaTensorRotation;
            var forwardTorque = forwardBasis * Vector3.Scale(body.inertiaTensor, Quaternion.Inverse(forwardBasis) * forwardAngle);

            var rightAxis = Vector3.Cross(bodyRight, rotRight);
            var rightTheta = Mathf.Asin(rightAxis.SafeMagnitude());
            var rightAngle = rightAxis.normalized * rightTheta / dt;
            var rightBasis = targetRot * body.inertiaTensorRotation;
            var rightTorque = rightBasis * Vector3.Scale(body.inertiaTensor, Quaternion.Inverse(rightBasis) * rightAngle);

            var upAxis = Vector3.Cross(bodyUp, rotUp);
            var upTheta = Mathf.Asin(upAxis.SafeMagnitude());
            var upAngle = upAxis.normalized * upTheta / dt;
            var upBasis = targetRot * body.inertiaTensorRotation;
            var upTorque = upBasis * Vector3.Scale(body.inertiaTensor, Quaternion.Inverse(upBasis) * upAngle);

            torque = forwardTorque + rightTorque + upTorque;
            torque = torque * TorqueForce;

            body.AddTorque(torque, ForceMode.Acceleration);

            yield return null;
        }
    }

    public GameObject MakeBranch()
    {
        var resource = Resources.Load<GameObject>("Bonsai3");
        var obj = GameObject.Instantiate(resource);
        var branch = obj.GetComponent<Bonsai3>();

        var ofs = Vector3.RotateTowards(Random.onUnitSphere, Vector3.up, 1.2f, 0.0f);
        var dir = Vector3.RotateTowards(Random.onUnitSphere, Vector3.up, 1.2f, 0.0f);

        dir = transform.rotation * Vector3.RotateTowards(Vector3.forward, Vector3.up, Mathf.Deg2Rad * Random.Range(5.0f, 15.0f), 0.0f);

        //ofs = Vector3.forward * 1.25f;
        //dir = Vector3.forward;
        //dir = transform.rotation * Vector3.RotateTowards(Vector3.forward, Vector3.up, Mathf.Deg2Rad * 45.0f, 0.0f);
        //dir = transform.rotation * Vector3.RotateTowards(Vector3.forward, (Vector3.up + Vector3.right).normalized, 0.5f, 0.0f);

        //dir = Vector3.RotateTowards(Vector3.forward, Vector3.right, 0.5f, 0.0f); // pyr=0,-28,0
        //dir = Vector3.RotateTowards(Vector3.right, Vector3.up, 0.5f, 0.0f); // pyr=0,-28,0

        branch.Attach(gameObject, ofs, dir);

        return obj;
    }

    public void StartAutoGrow()
    {
        var obj = MakeBranch();
        StartCoroutine(DoAutoGrow(obj));
    }

    public IEnumerator DoAutoGrow(GameObject obj)
    {
        yield return new WaitForSeconds(1.0f);
        obj.GetComponent<Bonsai3>().StartAutoGrow();
    }
}
