using Cysharp.Threading.Tasks;
using UnityEngine;

public class LobbyAudioControl : MonoBehaviour
{
    public AudioSource _introAudioSource;
    public AudioSource _loopAudioSource;
    [SerializeField] float introLength;
    [SerializeField] bool start;

    async void Awake()
    {
        _introAudioSource.clip.LoadAudioData();
        _loopAudioSource.clip.LoadAudioData();

        await UniTask.Delay(1000);

        AudioStart();
    }
    void AudioStart()
    {
        //イントロ部分の再生開始
        _introAudioSource.PlayScheduled(AudioSettings.dspTime);

        //イントロ終了後にループ部分の再生を開始
        _loopAudioSource.PlayScheduled (AudioSettings.dspTime + introLength);
    }

    void Update()
    {
        if (start)
        {
            start = false;
            AudioStart();
        }
    }

    void OnDestroy()
    {
        _introAudioSource.clip.UnloadAudioData();
        _loopAudioSource.clip.UnloadAudioData();
    }
}