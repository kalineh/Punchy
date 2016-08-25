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


public class Bonsai3
    : MonoBehaviour
{
    public GameObject parent;

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

    public Vector3 CalcTorqueTowards(Rigidbody bodySrc, Rigidbody bodyDst, Quaternion ofsDst)
    {
        var rotSrc = bodySrc.rotation;
        var rotDst = bodyDst.rotation * ofsDst;

        var eulerCurrent = rotSrc.eulerAngles;
        var eulerTarget = rotDst.eulerAngles;

        var flipCurrent = Vector3.Dot(rotSrc * Vector3.up, Vector3.up) < 0.0f;
        var flipTarget = Vector3.Dot(rotDst * Vector3.up, Vector3.up) < 0.0f;

        if (flipTarget)
        {
            //rotDst = Quaternion.LookRotation(rotDst * Vector3.forward, rotDst * -Vector3.up);
            //eulerTarget = rotDst.eulerAngles;
        }

        var eulerDeltaX = Mathf.DeltaAngle(eulerCurrent.x, eulerTarget.x);
        var eulerDeltaY = Mathf.DeltaAngle(eulerCurrent.y, eulerTarget.y);
        var eulerDeltaZ = Mathf.DeltaAngle(eulerCurrent.z, eulerTarget.z);

        var eulerOfs = new Vector3(eulerDeltaX, eulerDeltaY, eulerDeltaZ);

        //Debug.LogFormat("eulerofs: {0}, eulerCurrent: {1}, eulerTarget: {2}, sign: {3}", eulerOfs, eulerCurrent, eulerTarget, Vector3.Dot(parent.transform.forward, Vector3.forward));

        var localEulerOfs = rotSrc * eulerOfs;
        var torque = localEulerOfs * 0.5f;

        return torque;
    }

    public Vector3 CalcTorqueTowards2(Rigidbody bodySrc, Rigidbody bodyDst, Quaternion ofsDst)
    {
        var rotSrc = bodySrc.rotation;
        var rotDst = bodyDst.rotation * ofsDst;

        var difference = Quaternion.Inverse(rotSrc) * rotDst;
        var srcDir = rotSrc * Vector3.forward;
        var dstDir = rotDst * Vector3.forward;
        var axis = Vector3.Cross(srcDir, dstDir);
        var theta = Mathf.Asin(axis.magnitude);
        var w = axis.normalized * theta;
        var q = bodySrc.rotation * bodySrc.inertiaTensorRotation;
        var torque = q * Vector3.Scale(bodySrc.inertiaTensor, Quaternion.Inverse(q) * w);

        return torque / Time.fixedDeltaTime;
    }

    public Vector3 CalcTorqueTowards3(Rigidbody bodySrc, Rigidbody bodyDst, Quaternion ofsDst)
    {
        var rotSrc = bodySrc.rotation;
        var rotDst = bodyDst.rotation * ofsDst;

        var z = Vector3.Cross(transform.forward, rotDst * Vector3.forward);
        var y = Vector3.Cross(transform.up, rotDst * Vector3.up);

        var thetaZ = Mathf.Asin(z.magnitude);
        var thetaY = Mathf.Asin(y.magnitude);

        var dt = Time.fixedDeltaTime;
        var wZ = z.normalized * (thetaZ / dt);
        var wY = y.normalized * (thetaY / dt);

        var q = transform.rotation * bodySrc.inertiaTensorRotation;
        var T = q * Vector3.Scale(bodySrc.inertiaTensor, Quaternion.Inverse(q) * (wZ + wY));

        // too wobbly
        //rigidbody.AddTorque(T, ForceMode.VelocityChange);

        // stable, but still laggy
        //rigidbody.angularVelocity = T;
        //rigidbody.maxAngularVelocity = T.magnitude;

        return T;
    }

    public IEnumerator DoAttachment(GameObject p, Vector3 ofs, Vector3 dir)
    {
        parent = p;

        var forward = dir;
        var right = Vector3.Cross(p.transform.up, forward);
        var up = Vector3.Cross(forward, right);

        var rotSrc = Quaternion.LookRotation(forward, up);
        var rotDst = Quaternion.LookRotation(p.transform.forward, p.transform.up);

        var body = GetComponent<Rigidbody>();
        var bodyParent = parent.GetComponent<Rigidbody>();

        // get component-wise angle offsets

        var eulerSrc = rotSrc.eulerAngles;
        var eulerDst = rotDst.eulerAngles;

        var ofsPitch = eulerDst.x - eulerSrc.x;
        var ofsYaw = eulerDst.y - eulerSrc.y;
        var ofsRoll = eulerDst.z - eulerSrc.z;

        Debug.LogFormat("p:{0},y:{1},r:{2}", ofsPitch, ofsYaw, ofsRoll);
        
        while (true)
        {
            var ofsLocal = bodyParent.rotation * ofs;
            var targetPos = bodyParent.position + ofsLocal;
            var targetRot = Quaternion.LookRotation(forward, up);

            targetRot *= bodyParent.rotation;

            body.MovePosition(Vector3.Lerp(transform.position, targetPos, 0.05f));
            //body.MoveRotation(Quaternion.Slerp(transform.rotation, targetRot, 0.1f));
            body.useGravity = false;

            var moveOfs = targetPos - body.position;

            var moveForce = moveOfs * 20.0f;

            body.AddForce(moveForce, ForceMode.Acceleration);

            // why negative?
            var eulerOfsRot = Quaternion.Euler(-ofsPitch, -ofsRoll, -ofsYaw);
            var torque = CalcTorqueTowards(body, bodyParent, eulerOfsRot);
            var torque2 = CalcTorqueTowards2(body, bodyParent, eulerOfsRot);
            var torque3 = CalcTorqueTowards3(body, bodyParent, eulerOfsRot);

            //body.AddTorque(torque * 2.0f, ForceMode.Acceleration);
            //body.AddTorque(torque2 * 10.0f, ForceMode.Acceleration);

            body.angularVelocity = torque3;
            body.maxAngularVelocity = torque3.magnitude;

            Debug.DrawLine(transform.position, transform.position + forward, Color.blue);
            Debug.DrawLine(transform.position, transform.position + right, Color.red);
            Debug.DrawLine(transform.position, transform.position + up, Color.green);

            //Debug.DrawLine(transform.position, transform.position + forward, Color.blue);
            //Debug.DrawLine(transform.position, transform.position + right, Color.red);
            //Debug.DrawLine(transform.position, transform.position + up, Color.green);

            yield return null;
        }

        yield break;
    }

    public GameObject MakeBranch()
    {
        var resource = Resources.Load<GameObject>("Bonsai3");
        var obj = GameObject.Instantiate(resource);
        var branch = obj.GetComponent<Bonsai3>();

        var ofs = Vector3.RotateTowards(Random.onUnitSphere, Vector3.up, 0.5f, 0.0f);
        var dir = Vector3.RotateTowards(Random.onUnitSphere, Vector3.up, 0.5f, 0.0f);

        ofs = Vector3.forward * 1.25f;
        dir = Vector3.forward;
        dir = Vector3.RotateTowards(Vector3.forward, Vector3.up, 0.5f, 0.0f); // pyr=-331,0,0
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
