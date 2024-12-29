using CLAPlus.Face2Face;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Face2Face faceTracking;
    float ElapsedMilliseconds;
    int frameCount, fps;
    void Update()
    {
        ElapsedMilliseconds += Time.deltaTime;
        frameCount++;
        if (ElapsedMilliseconds >= 1)
        {
            ElapsedMilliseconds -= 1;
            fps = frameCount;
            frameCount = 0;
        }
        text.text = $"FPS : {fps}\n{faceTracking.suc} / {faceTracking.fail}\nRunning : {faceTracking.isRunning}\nLeft : {faceTracking.LeftEyeCloseness}\nRight : {faceTracking.RightEyeCloseness}\nMouth : {faceTracking.mouthOpenRatio}\nThread : {faceTracking.thread}";
    }

    public void OnPlus()
    {
        faceTracking.thread ++;
    }

    public void OnMinus()
    {
        faceTracking.thread --;
    }
}
