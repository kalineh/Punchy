using UnityEngine;
using System.Collections;

public class PhysicsGrabber
    : MonoBehaviour
{
    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var screen = Input.mousePosition;
            var ray = Camera.main.ScreenPointToRay(screen);
            var info = new RaycastHit();
            var hit = Physics.Raycast(ray, out info, 10.0f);

            if (hit)
            {
                var p = info.point;
                var f = ray.direction * 20.0f;
                var b = info.collider.gameObject.GetComponent<Rigidbody>();
                if (b)
                    StartCoroutine(DoHit(b, p, f));
            }
        }
    }

    public IEnumerator DoHit(Rigidbody body, Vector3 pos, Vector3 force)
    {
        for (int i = 0; i < 8; ++i)
        {
            body.AddForceAtPosition(force, pos);
            force *= 0.8f;
            yield return null;
        }
    }
}
