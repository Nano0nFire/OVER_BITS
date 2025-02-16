using UnityEngine;
using UnityEngine.UI;
using CLAPlus.Extension;
using TMPro;
using DACS.Inventory;

public class UI_ColorSelecter : MonoBehaviour
{
    [SerializeField] Image SampleImg;
    [SerializeField] Slider hueSlider;
    [SerializeField] Slider saturationSlider;
    [SerializeField] Slider valueSlider;
    [SerializeField] Image hueSliderBackground;
    [SerializeField] Image saturationSliderBackground;
    [SerializeField] Image valueSliderBackground;
    [SerializeField] TMP_InputField codeField;
    [SerializeField] Button ApplyChangesBtn;
    private const int GradientTextureWidth = 256; // グラデーションテクスチャの幅
    public static Color newColor = new();

    void Start()
    {
        // スライダーの背景テクスチャを更新
        UpdateHueSliderBackground();
        UpdateSaturationValueSliderBackgrounds(hueSlider.value);

        // スライダーの値が変更されたときのイベントリスナーを追加
        hueSlider.onValueChanged.AddListener(delegate { OnHueValueChanged(hueSlider.value); });
        saturationSlider.onValueChanged.AddListener(delegate { OnColorChanged(); });
        valueSlider.onValueChanged.AddListener(delegate { OnColorChanged(); });
        codeField.onEndEdit.AddListener(delegate { OnColorCodeChanged(codeField.text); });
        ApplyChangesBtn.onClick.AddListener(delegate { OnApply(); });
    }

    void OnHueValueChanged(float hue)
    {
        UpdateSaturationValueSliderBackgrounds(hue);
        OnColorChanged();
    }

    void OnColorChanged()
    {
        newColor = Color.HSVToRGB(hueSlider.value, saturationSlider.value, valueSlider.value);
        SampleImg.color = newColor;
        codeField.text = Extensions.ColorToHexString(newColor);
    }

    void OnColorCodeChanged(string Value)
    {
        Color color = Extensions.HexToColorConverter(Value);
        Color.RGBToHSV(color, out float hue, out float saturation, out float value);
        SetSlider(hue, saturation, value);
        OnColorChanged();

    }

    void OnApply()
    {
        ClientGeneralManager.customLifeAvatar.colors[UIGeneral.SelectedItem.FirstIndex - 19] = newColor; // 19はItemDataBase上のBaseBodyのインデックスを示している
        ClientGeneralManager.customLifeAvatar.ModelIDs[UIGeneral.SelectedItem.FirstIndex - 19] = UIGeneral.SelectedItem.SecondIndex;
        ClientGeneralManager.customLifeAvatar.Combiner();
    }

    void UpdateHueSliderBackground()
    {
        hueSliderBackground.sprite = Sprite.Create(CreateHueGradient(), new Rect(0, 0, GradientTextureWidth, 1), new Vector2(0.5f, 0.5f));
    }

    void UpdateSaturationValueSliderBackgrounds(float hue)
    {
        saturationSliderBackground.sprite = Sprite.Create(CreateSaturationGradient(hue), new Rect(0, 0, GradientTextureWidth, 1), new Vector2(0.5f, 0.5f));
        valueSliderBackground.sprite = Sprite.Create(CreateValueGradient(hue), new Rect(0, 0, GradientTextureWidth, 1), new Vector2(0.5f, 0.5f));
    }

    void SetSlider(float hue, float saturation, float value)
    {
        hueSlider.value = hue;
        saturationSlider.value = saturation;
        valueSlider.value = value;
        UpdateSaturationValueSliderBackgrounds(hue);
    }

    Texture2D CreateHueGradient()
    {
        Texture2D texture = new Texture2D(GradientTextureWidth, 1, TextureFormat.RGB24, false);
        for (int i = 0; i < GradientTextureWidth; i++)
        {
            Color color = Color.HSVToRGB(i / (float)GradientTextureWidth, 1f, 1f);
            texture.SetPixel(i, 0, color);
        }
        texture.Apply();
        return texture;
    }

    Texture2D CreateSaturationGradient(float hue)
    {
        Texture2D texture = new Texture2D(GradientTextureWidth, 1, TextureFormat.RGB24, false);
        for (int i = 0; i < GradientTextureWidth; i++)
        {
            Color color = Color.HSVToRGB(hue, i / (float)GradientTextureWidth, 1f);
            texture.SetPixel(i, 0, color);
        }
        texture.Apply();
        return texture;
    }

    Texture2D CreateValueGradient(float hue)
    {
        Texture2D texture = new Texture2D(GradientTextureWidth, 1, TextureFormat.RGB24, false);
        for (int i = 0; i < GradientTextureWidth; i++)
        {
            Color color = Color.HSVToRGB(hue, 1f, i / (float)GradientTextureWidth);
            texture.SetPixel(i, 0, color);
        }
        texture.Apply();
        return texture;
    }

}
