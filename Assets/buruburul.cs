using Unity.Mathematics;
using UnityEngine;

public class buruburul : MonoBehaviour
{
    float t = 0;
    [SerializeField] float speed = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
        Vector3 pos = Vector3.zero;
        pos.z = math.sin(t * speed) * 5;
        transform.position = pos;

        Debug.DrawRay(transform.position, transform.forward * 3f, Color.red, 0.01f, true);
        Debug.DrawRay(transform.position, transform.rotation * Vector3.forward * 3f, Color.blue, 0.01f, true);
    }
}
