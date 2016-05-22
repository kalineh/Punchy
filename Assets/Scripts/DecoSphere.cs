using UnityEngine;
using System.Collections;

public class DecoSphere
    : MonoBehaviour
{
    private Vector3 startPosition;
    private float shuffle;

    public void Start()
    {
        OnScriptReload();
    }

    public void OnScriptReload()
    {
        startPosition = transform.position;
        shuffle = Random.Range(0.0f, 30.0f);
        transform.localScale = transform.localScale * Random.Range(0.7f, 1.3f);
    }

	void Update()
    {
        var t = Time.time + shuffle;
        var p = new Vector3(
            Mathf.Sin(t * 0.13f) * 0.04f,
            Mathf.Sin(t * 0.16f) * 0.3f,
            Mathf.Sin(t * 0.12f) * 0.06f
        );

        transform.position = startPosition + p;
	}
}
