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

    public IEnumerator DoAttachment(GameObject p, Vector3 ofs, Vector3 dir)
    {
        parent = p;

        var forward = dir;
        var right = Vector3.Cross(p.transform.up, forward);
        var up = Vector3.Cross(forward, right);

        var rotSrc = Quaternion.LookRotation(forward, up);
        var rotDst = Quaternion.LookRotation(p.transform.forward, p.transform.up);

        var body = GetComponent<Rigidbody>();

        while (true)
        {
            var targetPos = p.transform.position + ofs;
            var targetRot = Quaternion.LookRotation(forward, up);

            body.MovePosition(Vector3.Lerp(transform.position, targetPos, 0.1f));
            body.MoveRotation(Quaternion.Slerp(transform.rotation, targetRot, 0.1f));

            Debug.DrawLine(transform.position, transform.position + forward, Color.blue);
            Debug.DrawLine(transform.position, transform.position + right, Color.red);
            Debug.DrawLine(transform.position, transform.position + up, Color.green);

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
