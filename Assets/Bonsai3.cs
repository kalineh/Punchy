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

    public Vector3 CalcTorqueTowards1(Rigidbody bodySrc, Rigidbody bodyDst, Quaternion ofsDst)
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

    private Vector3 RectifyAngleDifference(Vector3 angdiff)
    {
        if (angdiff.x > 180) angdiff.x -= 360;
        if (angdiff.y > 180) angdiff.y -= 360;
        if (angdiff.z > 180) angdiff.z -= 360;
        return angdiff;
    }

    public Vector3 CalcTorqueTowards4(Rigidbody bodySrc, Rigidbody bodyDst, Quaternion ofsDst)
    {
        var rotSrc = bodySrc.rotation;
        var rotDst = bodyDst.rotation;

        rotDst = ofsDst * rotDst;

        var srcUp = rotSrc * Vector3.up;
        var srcFwd = rotSrc * Vector3.forward;

        var dstUp = rotDst * Vector3.up;
        var dstFwd = rotDst * Vector3.forward;

        var diff = Quaternion.FromToRotation(srcUp, dstUp);
        var angle = Quaternion.Angle(rotDst, rotSrc);
        var perp = Vector3.Cross(dstUp, dstFwd);

        if (Vector3.Dot(srcFwd, perp) < 0.0f)
            angle *= -1.0f;

        var correction = Quaternion.AngleAxis(angle, dstUp);

        var rotation = RectifyAngleDifference(diff.eulerAngles);
        var corrective = RectifyAngleDifference(correction.eulerAngles);

        var torque = (rotation - corrective * 0.5f) - bodySrc.angularVelocity;

        return torque;
    }

    public Vector3 CalcTorqueTowards5(Rigidbody bodySrc, Rigidbody bodyDst, Quaternion ofsDst)
    {
        var rotSrc = bodySrc.rotation;
        var rotDst = bodyDst.rotation;

        rotDst = ofsDst * rotDst;

        var torque = rotSrc * Vector3.up * 0.5f;

        return torque;
    }

    /*
    public Vector3 CalcTorqueTowards4(Rigidbody bodySrc, Rigidbody bodyDst, Quaternion ofsDst)
    {
        Quaternion AngleDifference = Quaternion.FromToRotation(ObjectToAttract.transform.up, transform.up);

        float AngleToCorrect = Quaternion.Angle(transform.rotation, ObjectToAttract.transform.rotation);
        Vector3 Perpendicular = Vector3.Cross(transform.up, transform.forward);
        if (Vector3.Dot(ObjectToAttract.transform.forward, Perpendicular) < 0)
            AngleToCorrect *= -1;
        Quaternion Correction = Quaternion.AngleAxis(AngleToCorrect, transform.up);

        Vector3 MainRotation = RectifyAngleDifference((AngleDifference).eulerAngles);
        Vector3 CorrectiveRotation = RectifyAngleDifference((Correction).eulerAngles);
        ObjectToAttract.AddTorque((MainRotation - CorrectiveRotation / 2) - ObjectToAttract.angularVelocity, ForceMode.VelocityChange); ;
    }
    */

    public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
    {
        return Mathf.Atan2(
            Vector3.Dot(n, Vector3.Cross(v1, v2)),
            Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }


    static float GetPitch(Quaternion rotation)
    {
        var dir = rotation * Vector3.forward;
        var angle = Mathf.Atan2(dir.y, dir.z);
        return angle * Mathf.Rad2Deg;
    }

    static float GetYaw(Quaternion rotation)
    {
        var dir = rotation * Vector3.forward;
        var angle = Mathf.Atan2(dir.x, dir.z);
        return angle * Mathf.Rad2Deg;
    }

    static float GetRoll(Quaternion rotation)
    {
        var dir = rotation * Vector3.right;
        var angle = Mathf.Atan2(dir.y, dir.x);
        return angle * Mathf.Rad2Deg;
    }

    static float GetPitch360(Quaternion rotation)
    {
        var rot = GetPitch(rotation);
        if (rot < 0) rot += 360.0f;
        return rot;
    }

    static float GetYaw360(Quaternion rotation)
    {
        var rot = GetYaw(rotation);
        if (rot < 0) rot += 360.0f;
        return rot;
    }

    static float GetRoll360(Quaternion rotation)
    {
        var rot = GetRoll(rotation);
        if (rot < 0) rot += 360.0f;
        return rot;
    }

    public float PosNegAngle(Vector3 a1, Vector3 a2, Vector3 normal)
    {
        float angle = Vector3.Angle(a1, a2);
        float sign = Mathf.Sign(Vector3.Dot(normal, Vector3.Cross(a1, a2)));
        return angle * sign;
    }

    public IEnumerator DoAttachment(GameObject p, Vector3 ofs, Vector3 dir)
    {
        parent = p;

        var body = GetComponent<Rigidbody>();
        var bodyParent = parent.GetComponent<Rigidbody>();

        // get offset of dir to original rotation
        var upward = Vector3.Project(bodyParent.rotation * Vector3.up, Vector3.up).normalized;
        var invalid = upward.sqrMagnitude < Vector3.kEpsilon;
        if (invalid)
            upward = Vector3.up;

        Debug.LogFormat("upward: {0}", upward);

        var dirOfs = dir;
        var rotOfsWithParent = Quaternion.LookRotation(dirOfs, bodyParent.rotation * Vector3.up);

        // but we want to remove the parent rotation since it should be just an offset
        var rotOfs = Quaternion.Inverse(bodyParent.rotation) * rotOfsWithParent;

        var rotOfsSrcEuler = bodyParent.rotation.eulerAngles;
        var rotOfsDstEuler = rotOfs.eulerAngles;

        var ofsEulerX = Mathf.DeltaAngle(rotOfsSrcEuler.x, rotOfsDstEuler.x);
        var ofsEulerY = Mathf.DeltaAngle(rotOfsSrcEuler.y, rotOfsDstEuler.y);
        var ofsEulerZ = Mathf.DeltaAngle(rotOfsSrcEuler.z, rotOfsDstEuler.z);
        
        Debug.LogFormat("parent: {0}, x:{1},y:{2},z:{3}", p.name, (int)ofsEulerX, (int)ofsEulerY, (int)ofsEulerZ);

        var axis = AxisHelper.Create();

        axis.transform.localScale = Vector3.one * 1.25f;

        while (true)
        {
            Debug.DrawLine(transform.position, transform.position + rotOfs * Vector3.forward * 1.25f, Color.white);

            var targetRot = bodyParent.rotation * rotOfs;

            body.MovePosition(bodyParent.position + bodyParent.rotation * ofs);
            //body.MoveRotation(targetRot);

            axis.transform.position = body.position;
            axis.transform.rotation = targetRot;

            {
                var x = Vector3.Cross(body.transform.forward.normalized, axis.transform.forward.normalized);
                float theta = Mathf.Asin(x.magnitude);
                var w = x.normalized * theta / Time.fixedDeltaTime;
                var q = axis.transform.rotation * body.inertiaTensorRotation;
                var t = q * Vector3.Scale(body.inertiaTensor, Quaternion.Inverse(q) * w);
                body.AddTorque(t * 3.8f - body.angularVelocity, ForceMode.Acceleration);
            }
            {
                var x = Vector3.Cross(body.transform.right.normalized, axis.transform.right.normalized);
                float theta = Mathf.Asin(x.magnitude);
                var w = x.normalized * theta / Time.fixedDeltaTime;
                var q = axis.transform.rotation * body.inertiaTensorRotation;
                var t = q * Vector3.Scale(body.inertiaTensor, Quaternion.Inverse(q) * w);
                body.AddTorque(t * 3.8f - body.angularVelocity, ForceMode.Acceleration);
            }

            // get spherical coords

            /*
            var eulerX = Mathf.DeltaAngle(axis.transform.rotation.eulerAngles.x, body.transform.rotation.eulerAngles.x);
            var eulerY = Mathf.DeltaAngle(axis.transform.rotation.eulerAngles.y, body.transform.rotation.eulerAngles.y);
            var eulerZ = Mathf.DeltaAngle(axis.transform.rotation.eulerAngles.z, body.transform.rotation.eulerAngles.z);

            // check flip 2 axis at a time
            var flipPitch = Vector3.Dot(axis.transform.forward, Vector3.forward) < 0.0f && Vector3.Dot(axis.transform.up, Vector3.up) < 0.0f;
            var flipYaw = Vector3.Dot(axis.transform.forward, Vector3.forward) < 0.0f && Vector3.Dot(axis.transform.right, Vector3.right) < 0.0f;
            var flipRoll = Vector3.Dot(axis.transform.up, Vector3.up) < 0.0f && Vector3.Dot(axis.transform.right, Vector3.right) < 0.0f;

            if ((flipPitch || flipRoll) && !flipYaw)
            {
                //angle = (angle + 180.0f) % 360.0f;
                //eulerY = (eulerY + 180.0f) % 360.0f;
                //eulerZ = (eulerZ + 180.0f) % 360.0f;
            }
            else if (flipRoll)
            {
                //eulerY = (eulerY + 180.0f) % 360.0f;
                //eulerZ = (eulerZ + 180.0f) % 360.0f;
                //azimuth = (azimuth + 180.0f) % 360.0f;
                //angle = (angle + 180.0f) % 360.0f;
                //roll = (roll + 180.0f) % 360.0f;
            }

            Debug.LogFormat("angle: {0}, {1}, {2}, flip: {3}, {4}, {5}", (int)eulerX, (int)eulerY, (int)eulerZ, flipPitch, flipYaw, flipRoll);
            */


            /* fail #552344123
            var targetLocalEulerX = axis.transform.localRotation.eulerAngles.x;
            var targetLocalEulerY = axis.transform.localRotation.eulerAngles.y;
            var targetLocalEulerZ = axis.transform.localRotation.eulerAngles.z;

            var currLocalEulerX = body.transform.rotation.eulerAngles.x;
            var currLocalEulerY = body.transform.rotation.eulerAngles.y;
            var currLocalEulerZ = body.transform.rotation.eulerAngles.z;

            var angleOfsX = PosNegAngle(axis.transform.forward, body.transform.forward, Vector3.right);
            var angleOfsY = PosNegAngle(axis.transform.forward, body.transform.forward, Vector3.up);
            var angleOfsZ = PosNegAngle(axis.transform.up, body.transform.up, Vector3.forward);

            var localTorque = new Vector3(angleOfsX, angleOfsY, angleOfsZ);
            //body.AddRelativeTorque(localTorque * -0.1f);
            */

            /*
            var srcEuler = body.rotation.eulerAngles;
            var dstEuler = targetRot.eulerAngles;

            var srcRot = body.rotation;
            var dstRot = targetRot;

            var srcPos = bodyParent.position;
            var dstPos = bodyParent.position + targetRot * Vector3.forward;
            var ofsLook = Quaternion.LookRotation(dstPos - srcPos, Vector3.up);

            var flipped = Vector3.Dot(bodyParent.rotation * Vector3.forward, Vector3.forward) < 0.0f;

            var ofsLookEuler = ofsLook.eulerAngles;
            var ofsLookEulerX = ofsLookEuler.x;
            var ofsLookEulerY = ofsLookEuler.y;
            var ofsLookEulerZ = ofsLookEuler.z;

            if (flipped)
            {
                if (ofsLookEulerX > 270.0f && ofsLookEulerX <= 360.0f)
                    ofsLookEulerX = (270.0f + (270.0f - ofsLookEulerX)) % 360.0f;
                if (ofsLookEulerX < 90.0f && ofsLookEulerX >= 0.0f)
                    ofsLookEulerX = (90.0f + (90.0f - ofsLookEulerX)) % 360.0f;

                ofsLookEulerY = (ofsLookEulerY + 180.0f) % 360.0f;
            }

            Debug.DrawLine(srcPos, srcPos + ofsLook * Vector3.forward, Color.cyan);

            Debug.LogFormat("ofsEuler: {0}, {1}, {2}; flip: {3}", (int)ofsLookEulerX, (int)ofsLookEulerY, (int)ofsLookEulerZ, flipped);
            */

            // get the rotation of src on the dst axis
            /*
            var srcForward = srcRot * Vector3.forward;
            var srcRight = srcRot * Vector3.right;
            var srcUp = srcRot * Vector3.up;

            var dstForward = dstRot * Vector3.forward;
            var dstRight = dstRot * Vector3.right;
            var dstUp = dstRot * Vector3.up;

            var srcForwardPlanar = Vector3.ProjectOnPlane(srcForward, dstRight);
            var srcRightPlanar = Vector3.ProjectOnPlane(srcRight, dstForward);
            var srcUpPlanar = Vector3.ProjectOnPlane(srcUp, dstRight);

            var srcAngleX = PosNegAngle(srcForwardPlanar, dstForward, dstRight);
            var srcAngleY = PosNegAngle(srcRightPlanar, dstRight, dstForward);
            var srcAngleZ = PosNegAngle(srcUpPlanar, dstUp, dstRight);

            Debug.LogFormat("src: x: {0}, y: {1}, z: {2}", srcAngleX, srcAngleY, srcAngleZ);
            */

            // get euler manually
            /*

            var srcEulerX = srcEuler.x;
            var srcEulerY = srcEuler.y;
            var srcEulerZ = srcEuler.z;

            var dstEulerX = dstEuler.x;
            var dstEulerY = dstEuler.y;
            var dstEulerZ = dstEuler.z;

            var srcAngleFlip = false;
            var dstAngleFlip = false;

            // how do we get rotation around euler, we need angle between each axis and the target
            var angleDiffX = Mathf.DeltaAngle(srcEulerX, dstEulerX);
            var angleDiffY = Mathf.DeltaAngle(srcEulerY, dstEulerY);
            var angleDiffZ = Mathf.DeltaAngle(srcEulerZ, dstEulerZ);
            //var angleDiffX = dstEulerX - srcEulerX;
            //var angleDiffY = dstEulerY - srcEulerY;
            //var angleDiffZ = dstEulerZ - srcEulerZ;

            Debug.LogFormat("src: {0},{1},{2}; dst: {3},{4},{5}, flip: src: {6}, dst: {7}",
                (int)srcEulerX, (int)srcEulerY, (int)srcEulerZ, (int)dstEulerX, (int)dstEulerY, (int)dstEulerZ, srcAngleFlip, dstAngleFlip);

            var torqueDir = new Vector3(angleDiffX, angleDiffY, angleDiffZ).normalized;
            var torque = torqueDir * 2.5f;
            //body.AddTorque(torque, ForceMode.Acceleration);
            //Debug.LogFormat("> diff: {0}, {1}, {2}", angleDiffX, angleDiffY, angleDiffZ);
            */

            Debug.DrawLine(transform.position, transform.position + targetRot * Vector3.forward * 0.7f, Color.blue + Color.white * 0.5f);
            Debug.DrawLine(transform.position, transform.position + targetRot * Vector3.right * 0.7f, Color.red + Color.white * 0.5f);
            Debug.DrawLine(transform.position, transform.position + targetRot * Vector3.up * 0.7f, Color.green + Color.white * 0.5f);

            Debug.DrawLine(transform.position, transform.position + bodyParent.rotation * Vector3.forward, Color.blue);
            Debug.DrawLine(transform.position, transform.position + bodyParent.rotation * Vector3.right, Color.red);
            Debug.DrawLine(transform.position, transform.position + bodyParent.rotation * Vector3.up, Color.green);

            yield return null;
        }
    }


    public IEnumerator DoAttachmentOld(GameObject p, Vector3 ofs, Vector3 dir)
    {
        parent = p;

        var forward = dir;
        var right = Vector3.Cross(p.transform.up, forward);
        var up = Vector3.Cross(forward, right);

        var upBase = up;
        var upFlip = Vector3.Dot(upBase, Vector3.up) < 0.0f;

        if (upFlip)
        {
            GetComponentInChildren<Renderer>().material.color = Color.blue;
            up = -up;
        }

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

        Debug.LogFormat("parent: {0}, p:{1},y:{2},r:{3}", p.name, ofsPitch, ofsYaw, ofsRoll);
        Debug.LogFormat("   ofs: {0}", ofs);

        body.position = bodyParent.position;
        body.rotation = Quaternion.LookRotation(forward, up);
        
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
            //var eulerOfs = RectifyAngleDifference(new Vector3(ofsPitch, ofsRoll, ofsYaw));
            //var eulerOfsRot = Quaternion.Euler(-eulerOfs);

            Debug.DrawLine(transform.position, transform.position + targetRot * Vector3.forward, Color.blue);
            Debug.DrawLine(transform.position, transform.position + targetRot * Vector3.right, Color.red);
            Debug.DrawLine(transform.position, transform.position + targetRot * Vector3.up, Color.green);

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
        dir = transform.rotation * Vector3.RotateTowards(Vector3.forward, Vector3.up, Mathf.Deg2Rad * 45.0f, 0.0f);
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
