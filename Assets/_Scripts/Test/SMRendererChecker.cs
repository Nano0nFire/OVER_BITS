using UnityEngine;

public class SMRendererChecker : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] bool Start, Reverse;
    [SerializeField] Transform[] bones;

    // Update is called once per frame
    void Update()
    {
        if (Start)
        {
            Start = false;
            bones = skinnedMeshRenderer.bones;
        }

        if (Reverse)
        {
            Reverse = false;
            skinnedMeshRenderer.bones = bones;
        }
    }
}
