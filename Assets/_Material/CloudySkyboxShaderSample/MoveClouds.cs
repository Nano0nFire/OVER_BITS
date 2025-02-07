using UnityEngine;

public class MoveClouds : MonoBehaviour
{
    public Vector4 _cloudsSpeed = new Vector4(0.5f, 0.5f);

    private const string CLOUDS_OFFSET = @"_CloudsOffset";

    private void Update()
    {
        var skybox = RenderSettings.skybox;

        Vector4 current = skybox.GetVector(CLOUDS_OFFSET);
        Vector4 next = current + (_cloudsSpeed * Time.deltaTime);
        skybox.SetVector(CLOUDS_OFFSET, next);
    }
}
