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

        var body = GetComponent<Rigidbody>();

        while (parent != null)
        {
            var lp = 0.9f;
            var lr = 0.2f;

            var targetPos = p.transform.position + p.transform.rotation * ofs;
            var targetRot = Quaternion.LookRotation(dir) * p.transform.rotation;

            var rotFix = Quaternion.Euler(90.0f, 0.0f, 0.0f);

            targetRot *= rotFix;

            body.MovePosition(Vector3.Lerp(transform.position, targetPos, lp));
            body.MoveRotation(Quaternion.Lerp(transform.rotation, targetRot, lr));

            Debug.DrawLine(p.transform.position, targetPos, Color.red);
            Debug.DrawLine(targetPos, targetRot * Vector3.forward, Color.blue);

            yield return null;
        }
    }

    public GameObject MakeBranch()
    {
        var resource = Resources.Load<GameObject>("Bonsai2");
        var obj = GameObject.Instantiate(resource);
        var branch = obj.GetComponent<Bonsai2>();

        var ofs = Random.onUnitSphere;
        var dir = Random.onUnitSphere;

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
