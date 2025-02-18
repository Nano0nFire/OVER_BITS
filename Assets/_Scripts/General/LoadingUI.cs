using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] Slider slider;
    [SerializeField] GameObject obj;
    static TMP_Text _text;
    static Slider _slider;
    static GameObject _obj;
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _text = text;
        _slider = slider;
        _obj = obj;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="prog">0~100</param>
    public static void UpdateLoadPanel(string text, float prog)
    {
        _obj.SetActive(true);
        _text.text = $"{_text.text}\n{text}";
        _slider.value = prog;

        if (prog >= 100)
            _obj.SetActive(false);
    }
}
