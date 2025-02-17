using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class NameTag : MonoBehaviour
{
    [SerializeField] PlayerStatus playerStatus;
    Camera _Camera;
    Transform _CameraPos;
    [SerializeField] RectTransform _targetUI;
    [SerializeField] private Vector3 _worldOffset;// オブジェクト位置のオフセット
    public static float NameTagDistance = 7;
    RectTransform _parentUI;
    bool IsLoaded = false;

    async void Start()
    {
        if (playerStatus.IsOwner)
            Destroy(this);
        await UniTask.WaitUntil( () => playerStatus.IsLoaded, cancellationToken : destroyCancellationToken);

        _Camera = Camera.main;
        _CameraPos = ClientGeneralManager.Instance.CameraPos;
        _targetUI = await UIGeneral.CreateAndGetNameTag();
        _targetUI.GetComponent<TMP_Text>().text = playerStatus.PlayerName;
        _parentUI = _targetUI.parent.GetComponent<RectTransform>();

        OnUpdatePosition();
        IsLoaded = true;
    }



    void OnDestroy()
    {
        Destroy(_targetUI.gameObject);
    }
    // UIの位置を毎フレーム更新
    private void Update()
    {
        if (!IsLoaded)
            return;

        OnUpdatePosition();
    }

    // UIの位置を更新する
    private void OnUpdatePosition()
    {
        var cameraTransform = _CameraPos.transform;

        // カメラの向きベクトル
        var cameraDir = cameraTransform.forward;
        // オブジェクトの位置
        var targetWorldPos = transform.position + _worldOffset;
        // カメラからターゲットへのベクトル
        var targetDir = targetWorldPos - _Camera.transform.position;

        // 内積を使ってカメラ前方かどうかを判定
        var isFront = Vector3.Dot(cameraDir, targetDir) > 0;

        // カメラ前方ならUI表示、後方なら非表示
        _targetUI.gameObject.SetActive(isFront);
        if (!isFront)
            return;

        // オブジェクトのワールド座標→スクリーン座標変換
        var targetScreenPos = _Camera.WorldToScreenPoint(targetWorldPos);

        // スクリーン座標変換→UIローカル座標変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _parentUI,
            targetScreenPos,
            null,
            out var uiLocalPos
        );

        // RectTransformのローカル座標を更新
        _targetUI.localPosition = uiLocalPos;

        float dis = Vector3.Distance(cameraTransform.position, transform.position);
        _targetUI.localScale = dis < NameTagDistance ? Vector3.one : Vector3.one * NameTagDistance / dis;
    }
}