using Cysharp.Threading.Tasks;
using UnityEngine;

public class ArmControl : MonoBehaviour
{
    [SerializeField] Transform[] arms;
    [SerializeField] bool PlayOnStart = false;

    // Update is called once per frame
    void Update()
    {
        if (PlayOnStart)
        {
            Play();
            PlayOnStart = false;
        }
    }

    public async void Play()
    {
        foreach (var arm in arms)
        {
            arm.transform.localPosition = new Vector3(0, 0, 0);
            arm.GetComponent<Animator>().SetTrigger("Play");
        }
        await UniTask.Delay(1500);
        for (int i = 0; i < 200; i++)
        {
            foreach (var arm in arms)
            {
                arm.transform.localPosition += new Vector3(0, 0.003f, 0);
            }
            await UniTask.Delay(10);
        }
    }
}
