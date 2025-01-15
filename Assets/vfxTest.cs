using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;

public class vfxTest : MonoBehaviour
{
    [SerializeField] VisualEffect visualEffect;
    [SerializeField] int particleCount;
    [SerializeField] Vector3 test;

    void Update()
    {
        // パーティクルデータの配列を作成
        Vector3[] positions = new Vector3[particleCount];
        Color[] colors = new Color[particleCount];
        float[] sizes = new float[particleCount];

        test.x += 0.2f;
        // データを設定
        for (int i = 0; i < particleCount; i++)
        {
            test.y += 1;
            positions[i] = test;
            colors[i] = new Color(Random.value, Random.value, Random.value, 1f);
            sizes[i] = Random.Range(0.3f, 1f);
        }

        test.y = 0;

        // GraphicsBufferを作成し、データを設定
        GraphicsBuffer particleBuffer = new(GraphicsBuffer.Target.Structured, particleCount, Marshal.SizeOf(new Vector3()));
        particleBuffer.SetData(positions);
        GraphicsBuffer sizeBuffer = new(GraphicsBuffer.Target.Structured, particleCount, Marshal.SizeOf(new float()));
        sizeBuffer.SetData(sizes);
        GraphicsBuffer colorBuffer = new(GraphicsBuffer.Target.Structured, particleCount, Marshal.SizeOf(new Color()));
        colorBuffer.SetData(colors);

        // Visual Effectにバッファを渡す
        visualEffect.SetGraphicsBuffer("PositionBuffer", particleBuffer);
        visualEffect.SetGraphicsBuffer("SizeBuffer", sizeBuffer);
        visualEffect.SetGraphicsBuffer("ColorBuffer", colorBuffer);
    }
}
