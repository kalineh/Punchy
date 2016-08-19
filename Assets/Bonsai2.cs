using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Bonsai2))]
public class Bonsai2Editor
    : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var bonsai = target as Bonsai2;

        if (GUILayout.Button("Grow"))
            bonsai.MakeBranch();
        if (GUILayout.Button("AutoGrow"))
            bonsai.StartAutoGrow();
    }
}

#endif


public class Bonsai2
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

    public IEnumerator DoAttachment(GameObject p, Vector3 ofs, Vector3 dir)
    {
        parent = p;

        var rotFix = Quaternion.Euler(90.0f, 0.0f, 0.0f);

        var body = GetComponent<Rigidbody>();
        var local = p.transform.InverseTransformDirection(dir);

        var localUp = Vector3.up;
        var localRight = p.transform.InverseTransformDirection(dir);

        while (parent != null)
        {
            var lp = 0.2f;
            var lr = 0.01f;

            var targetPos = p.transform.position + p.transform.rotation * ofs;
            var targetDir = p.transform.rotation * local;
            var targetRot = Quaternion.LookRotation(targetDir, p.transform.up);

            //Debug.DrawLine(transform.position, transform.position + local, Color.yellow);
            //Debug.DrawLine(p.transform.position, targetPos, Color.magenta);

            //body.MovePosition(Vector3.Lerp(body.position, targetPos, lp));
            //body.MoveRotation(Quaternion.Slerp(body.rotation, targetRot, lr));

            var toTargetPos = targetPos - transform.position;
            var toTargetRot = Quaternion.FromToRotation(transform.rotation * Vector3.forward, targetRot * Vector3.forward);

            var force = toTargetPos.magnitude * 200.0f + 25.0f;

            body.AddForce(toTargetPos * force * Time.deltaTime, ForceMode.Acceleration);

            var fwdAngle = Vector3.Angle(transform.forward, targetRot * Vector3.forward);
            var fwdCross = Vector3.Cross(transform.up, targetRot * Vector3.forward);

            body.AddTorque(fwdCross * fwdAngle * 8.0f * Time.deltaTime);

            yield return null;
        }
    }

    public GameObject MakeBranch()
    {
        var resource = Resources.Load<GameObject>("Bonsai2");
        var obj = GameObject.Instantiate(resource);
        var branch = obj.GetComponent<Bonsai2>();

        var ofs = Vector3.RotateTowards(Random.onUnitSphere, Vector3.up, 1.5f, 0.0f);
        var dir = Vector3.RotateTowards(Random.onUnitSphere, Vector3.up, 1.5f, 0.0f);

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
        obj.GetComponent<Bonsai2>().StartAutoGrow();
    }
}
