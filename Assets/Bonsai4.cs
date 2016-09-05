using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Bonsai4))]
public class Bonsai4Editor
    : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var bonsai = target as Bonsai4;

        if (GUILayout.Button("MakeTower"))
            bonsai.StartCoroutine(Bonsai4Builder.DoBuildTower(bonsai.gameObject, "Tower"));
        if (GUILayout.Button("MakeOneUp"))
            bonsai.StartCoroutine(Bonsai4Builder.DoBuildOneUp(bonsai.gameObject, "OneUp"));
    }
}

#endif

[SelectionBase]
public class Bonsai4
    : MonoBehaviour
{
    public Bonsai4Settings settings;

    public static Bonsai4 MakeBranch(Bonsai4Settings settings = null)
    {
        var resource = Resources.Load<GameObject>("Bonsai4");
        var obj = GameObject.Instantiate(resource);
        var bonsai = obj.GetComponent<Bonsai4>();

        bonsai.settings = settings;
        settings.transform.SetParent(bonsai.transform);

        return bonsai;
    }

    public void Start()
    {
        OnScriptReload();
    }

    public void OnScriptReload()
    {
    }

    public IEnumerator DoAttachment(GameObject parent, Vector3 ofs, Vector3 dir)
    {
        if (settings == null)
        {
            settings = Bonsai4Settings.Get("Bonsai4SettingsSrc");
            settings.transform.SetParent(transform, false);
        }

        var body = GetComponent<Rigidbody>();
        var bodyParent = parent.GetComponent<Rigidbody>();

        body.mass = settings.Mass;
        body.drag = settings.Drag;
        body.angularDrag = settings.AngularDrag;

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

            var moveForce = moveDir * Mathf.Pow(moveLen, settings.MovePower) * settings.MoveForce;

            moveForce = Vector3.ClampMagnitude(moveForce, settings.LimitForce);

            body.MovePosition(Vector3.Lerp(body.position, targetPos, settings.MoveLerp));
           
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
            torque = torque * settings.TorqueForce;

            // TODO: fix
            if (float.IsNaN(torque.x) || float.IsNaN(torque.y) || float.IsNaN(torque.z))
            {
                yield return new WaitForFixedUpdate();
                continue;
            }

            torque = Vector3.ClampMagnitude(torque, settings.LimitTorque);

            body.MoveRotation(Quaternion.Lerp(body.rotation, targetRot, settings.TorqueLerp));
            body.AddTorque(torque, ForceMode.Force);

            //bodyParent.AddForceAtPosition(body.velocity * BackMoveForce, body.position, ForceMode.Force);
            //bodyParent.AddTorque(body.angularVelocity * BackTorque, ForceMode.Force);

            var velSelf = body.velocity;
            var velParent = bodyParent.velocity;
            var avelSelf = body.angularVelocity;
            var avelParent = bodyParent.angularVelocity;

            body.velocity = Vector3.Lerp(velSelf, velParent, settings.BackMoveForce);
            body.angularVelocity = Vector3.Lerp(avelSelf, avelParent, settings.BackTorque);
            bodyParent.velocity = Vector3.Lerp(velParent, velSelf, settings.BackMoveForce);
            bodyParent.angularVelocity = Vector3.Lerp(avelParent, avelSelf, settings.BackTorque);

            yield return new WaitForFixedUpdate();
        }
    }
}
