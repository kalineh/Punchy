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
    private bool overridePhysics = false;
    private float overrideMass = 1.0f;
    private float overrideDrag = 1.0f;
    private float overrideAngularDrag = 1.0f;
    private float overrideMoveForce = 750.0f;
    private float overrideMovePower = 0.8f;
    private float overrideMoveLerp = 0.05f;
    private float overrideTorqueLerp = 0.05f;
    private float overrideTorqueForce = 5.0f;
    private float overrideBackMoveForce = 0.0f;
    private float overrideBackTorque = 0.0f;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var bonsai = target as Bonsai3;

        if (GUILayout.Button("Grow"))
            bonsai.MakeBranch();
        if (GUILayout.Button("AutoGrow"))
            bonsai.StartAutoGrow();
        if (GUILayout.Button("AutoTree"))
            bonsai.StartAutoTree("root", 0);
        if (GUILayout.Button("AutoTreeUp"))
            bonsai.StartAutoTreeUp("root", 8);
        if (GUILayout.Button("Stop All Coroutines"))
            bonsai.StopAllCoroutines();

        EditorGUILayout.Separator();

        var wasOverride = overridePhysics;
        overridePhysics = GUILayout.Toggle(overridePhysics, "Override Physics");

        if (!wasOverride && overridePhysics)
        {
            overrideMass = bonsai.GetComponent<Rigidbody>().mass;
            overrideDrag = bonsai.GetComponent<Rigidbody>().mass;
            overrideAngularDrag = bonsai.GetComponent<Rigidbody>().mass;
            overrideMoveForce = bonsai.MoveForce;
            overrideMovePower = bonsai.MovePower;
            overrideMoveLerp = bonsai.MoveLerp;
            overrideTorqueLerp = bonsai.TorqueLerp;
            overrideTorqueForce = bonsai.TorqueForce;
            overrideBackMoveForce = bonsai.BackMoveForce;
            overrideBackTorque = bonsai.BackTorque;
        }

        if (overridePhysics)
        {
            overrideMass = EditorGUILayout.FloatField("Mass", overrideMass);
            overrideDrag = EditorGUILayout.FloatField("Drag", overrideDrag);
            overrideAngularDrag = EditorGUILayout.FloatField("Angular Drag", overrideAngularDrag);
            overrideMoveForce = EditorGUILayout.FloatField("Move Force", overrideMoveForce);
            overrideMovePower = EditorGUILayout.FloatField("Move Power", overrideMovePower);
            overrideMoveLerp = EditorGUILayout.FloatField("Move Lerp", overrideMoveLerp);
            overrideTorqueLerp = EditorGUILayout.FloatField("Torque Lerp", overrideTorqueLerp);
            overrideTorqueForce = EditorGUILayout.FloatField("Torque Force", overrideTorqueForce);
            overrideBackMoveForce = EditorGUILayout.FloatField("Back Move Force", overrideBackMoveForce);
            overrideBackTorque = EditorGUILayout.FloatField("Back Torque", overrideBackTorque);

            var bonsais = FindObjectsOfType<Bonsai3>();
            foreach (var b in bonsais)
            {
                var rb = b.GetComponent<Rigidbody>();

                rb.mass = overrideMass;
                rb.drag = overrideDrag;
                rb.angularDrag = overrideAngularDrag;

                b.MoveForce = overrideMoveForce;
                b.MovePower = overrideMovePower;
                b.MoveLerp = overrideMoveLerp;
                b.TorqueLerp = overrideTorqueLerp;
                b.TorqueForce = overrideTorqueForce;
                b.BackMoveForce = overrideBackMoveForce;
                b.BackTorque = overrideBackTorque;
            }
        }
    }
}

#endif


[SelectionBase]
public class Bonsai3
    : MonoBehaviour
{
    public GameObject parent;

    public float LimitForce;
    public float LimitTorque;

    public float MoveForce;
    public float MovePower;
    public float MoveLerp;
    public float TorqueForce;
    public float TorqueLerp;

    public float BackMoveForce;
    public float BackTorque;

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

        var cube = transform.FindChild("Cube");
        var parentTip = bodyParent.transform.FindChild("Cube/Tip");

        if (body.isKinematic == false)
        {
            //cube.transform.position = body.position;
            //cube.transform.rotation = body.rotation;
            //cube.transform.localScale = Vector3.zero;
        }

        while (true)
        {
            var bodyPos = body.position;
            var bodyRot = body.rotation;

            var targetPos = bodyParent.position + bodyParent.rotation * ofs;
            var targetRot = bodyParent.rotation * baseRotOfs;

            if (body.isKinematic == false)
            {
                var parentTipPos = parentTip.transform.position;
                var parentTipPosOfs = (parentTipPos - targetPos);
                var parentTipPosLen = parentTipPosOfs.SafeMagnitude();

                //Debug.DrawLine(transform.position, parentTipPos, Color.blue);

                cube.transform.position = (parentTipPos + targetPos) * 0.5f;
                cube.transform.localScale = new Vector3(0.1f, parentTipPosLen, 0.1f);
                cube.transform.rotation = Quaternion.LookRotation(parentTipPosOfs.SafeNormalize(), Vector3.up) * Quaternion.Euler(-90.0f, 0.0f, 0.0f);
            }

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

            var dt = Time.fixedDeltaTime;

            var moveOfs = targetPos - bodyPos;
            var moveDir = moveOfs.SafeNormalize();
            var moveLen = moveOfs.SafeMagnitude();

            var moveForce = moveDir * Mathf.Pow(moveLen, MovePower) * MoveForce;

            moveForce = Vector3.ClampMagnitude(moveForce, LimitForce);

            body.MovePosition(Vector3.Lerp(body.position, targetPos, MoveLerp));
           
            body.AddForce(moveForce, ForceMode.Force);

            var torque = Vector3.zero;

            var rotForward = targetRot * Vector3.forward;
            var rotRight = targetRot * Vector3.right;
            var rotUp = targetRot * Vector3.up;

            rotForward = rotForward.SafeNormalizeOr(Vector3.forward);
            rotRight = rotRight.SafeNormalizeOr(Vector3.right);
            rotUp = rotUp.SafeNormalizeOr(Vector3.up);

            var bodyForward = bodyRot * Vector3.forward;
            var bodyRight = bodyRot * Vector3.right;
            var bodyUp = bodyRot * Vector3.up;

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

            // TODO: fix
            if (float.IsNaN(torque.x) || float.IsNaN(torque.y) || float.IsNaN(torque.z))
            {
                yield return new WaitForFixedUpdate();
                continue;
            }

            torque = Vector3.ClampMagnitude(torque, LimitTorque);

            body.MoveRotation(Quaternion.Lerp(body.rotation, targetRot, TorqueLerp));
            body.AddTorque(torque, ForceMode.Force);

            //bodyParent.AddForceAtPosition(body.velocity * BackMoveForce, body.position, ForceMode.Force);
            //bodyParent.AddTorque(body.angularVelocity * BackTorque, ForceMode.Force);

            var velSelf = body.velocity;
            var velParent = bodyParent.velocity;
            var avelSelf = body.angularVelocity;
            var avelParent = bodyParent.angularVelocity;

            body.velocity = Vector3.Lerp(velSelf, velParent, BackMoveForce);
            body.angularVelocity = Vector3.Lerp(avelSelf, avelParent, BackTorque);
            bodyParent.velocity = Vector3.Lerp(velParent, velSelf, BackMoveForce);
            bodyParent.angularVelocity = Vector3.Lerp(avelParent, avelSelf, BackTorque);

            yield return new WaitForFixedUpdate();
        }
    }

    public GameObject MakeBranchOfsDir(Vector3 ofs, Vector3 dir)
    {
        var resource = Resources.Load<GameObject>("Bonsai3");
        var obj = GameObject.Instantiate(resource);
        var branch = obj.GetComponent<Bonsai3>();

        branch.Attach(gameObject, ofs, dir);

        return obj;
    }

    public GameObject MakeBranch()
    {
        var resource = Resources.Load<GameObject>("Bonsai3");
        var obj = GameObject.Instantiate(resource);
        var branch = obj.GetComponent<Bonsai3>();

        var ofs = Vector3.RotateTowards(Random.onUnitSphere, Vector3.up, 2.0f, 0.0f);
        var dir = Vector3.RotateTowards(Random.onUnitSphere, Vector3.forward, 1.2f, 0.0f);

        //dir = transform.rotation * Vector3.RotateTowards(Vector3.forward, Vector3.up, Mathf.Deg2Rad * Random.Range(5.0f, 15.0f), 0.0f);

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

    public void StartAutoTreeUp(string name, int depth)
    {
        StartCoroutine(DoAutoTreeSingleUp(gameObject, depth));
    }

    public IEnumerator DoAutoTreeSingleUp(GameObject obj, int depth)
    {
        while (depth > 0)
        {
            var branch = obj.GetComponent<Bonsai3>().MakeBranchOfsDir(Vector3.up * 0.5f, Vector3.up);
            yield return new WaitForSeconds(0.5f);
            obj = branch.gameObject;
            depth--;
        }
    }
    public void StartAutoTree(string name, int depth)
    {
        StartCoroutine(DoAutoTreeSingle(gameObject, depth));
    }

    public IEnumerator DoAutoTreeSingle(GameObject obj, int depth)
    {
        yield return new WaitForSeconds(0.5f);

        var layers = Random.Range(2, 5) - depth;
        for (int i = 0; i < layers; ++i)
        {
            var branch = MakeBranch();
            var name = string.Format("Branch{0}.{1}", depth, i);
            branch.GetComponent<Bonsai3>().StartAutoTree(name, depth + 1);
            yield return new WaitForSeconds(0.3f);
        }
    }
}
