using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using System;
using UnityEngine.Rendering;
using Unity.Jobs;
using Unity.Burst;

public class RMeshAPI : MonoBehaviour
{
    [SerializeField] Mesh _mesh = null;
    [SerializeField] Material _material = null, material2 = null;
    [SerializeField] Vector2Int _counts = new Vector2Int(10, 10);
    GraphicsBuffer _colorBuffer;

    bool isEnable = false;
    float t = 0;
    float stop = 0;
    PositionBuffer _buffer;
    MaterialPropertyBlock _matProps;

    void Start()
    {
        _buffer = new PositionBuffer(_counts.x, _counts.y);
        _colorBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _counts.x * _counts.y, sizeof(float) * 4);
        _matProps = new MaterialPropertyBlock();
    }

    void OnDestroy()
      => _buffer.Dispose();

    public void Play(float stop)
    {
        this.stop = stop;
        isEnable = true;
    }

    void Update()
    {
        if (t > stop)
            isEnable = false;
        else
            t += Time.deltaTime / 2.0f;

        _buffer.Update(t, isEnable);

        _colorBuffer.SetData(_buffer.Colors);
        _material.SetBuffer("_InstanceColorBuffer", _colorBuffer);

        var rparams = new RenderParams(_material)
          { receiveShadows = true,
            shadowCastingMode = ShadowCastingMode.On,
            matProps = _matProps };

        var matrices = _buffer.Matrices;
        for (var offs = 0; offs < matrices.Length; offs += 1023)
        {
            var count = Mathf.Min(1023, matrices.Length - offs);
            _matProps.SetInteger("_InstanceIDOffset", offs);
            Graphics.RenderMeshInstanced(rparams, _mesh, 0, matrices, count, offs);
        }
        rparams = new RenderParams(_material)
          { receiveShadows = true,
            shadowCastingMode = ShadowCastingMode.On };
        matrices = _buffer.Matrices2;
        for (var offs = 0; offs < matrices.Length; offs += 1023)
        {
            var count = Mathf.Min(1023, matrices.Length - offs);
            Graphics.RenderMeshInstanced(rparams, _mesh, 0, matrices, count, offs);
        }
        matrices = _buffer.Matrices3;
        rparams = new RenderParams(material2)
          { receiveShadows = true,
            shadowCastingMode = ShadowCastingMode.On };
        for (var offs = 0; offs < matrices.Length; offs += 1023)
        {
            var count = Mathf.Min(1023, matrices.Length - offs);
            Graphics.RenderMeshInstanced(rparams, _mesh, 0, matrices, count, offs);
        }
    }
    sealed class PositionBuffer : IDisposable
    {
        public NativeArray<Matrix4x4> Matrices => _arrays.m.Reinterpret<Matrix4x4>();
        public NativeArray<Matrix4x4> Matrices2, Matrices3;
        public NativeArray<Color> Colors;
        public NativeArray<float3> scales;

        NativeArray<float> zScale;

        (NativeArray<float3> p, NativeArray<float4x4> m) _arrays;
        (int x, int y) _dims;

        public PositionBuffer(int xCount, int yCount)
        {
            _dims = (xCount, yCount);
            _arrays = (new NativeArray<float3>(_dims.x * _dims.y, Allocator.Persistent),
                    new NativeArray<float4x4>(_dims.x * _dims.y, Allocator.Persistent));
            scales = new NativeArray<float3>(_dims.x * _dims.y, Allocator.Persistent);
            zScale = new NativeArray<float>(_dims.x * _dims.y, Allocator.Persistent);
            Matrices2 = new NativeArray<Matrix4x4>(_dims.x * _dims.y, Allocator.Persistent);
            Matrices3 = new NativeArray<Matrix4x4>(_dims.x / 10 * _dims.y / 10, Allocator.Persistent);
            Colors = new NativeArray<Color>(_dims.x * _dims.y, Allocator.Persistent);
            var offs = 0;
            int center = _dims.x * _dims.y / 2;
            float colorBuffer;
            for (var i = 0; i < _dims.x; i++)
            {
                var x = i - _dims.x * 0.5f;
                for (var j = 0; j < _dims.y; j++)
                {
                    var z = j - _dims.y * 0.2f;
                    var p = math.float3(x + UnityEngine.Random.Range(-0.2f, 0.2f), center == offs ? -10 : UnityEngine.Random.Range(-10f, 1f), z + UnityEngine.Random.Range(-0.4f, 0.4f));
                    _arrays.p[offs] = p;
                    float zs = UnityEngine.Random.Range(0.5f, 1.5f);
                    colorBuffer = UnityEngine.Random.Range(0f, 1f);
                    var scale = math.float3(UnityEngine.Random.Range(0.1f, 0.3f), UnityEngine.Random.Range(0.5f, 1f), zs);
                    zScale[offs] = zs;
                    scales[offs] = scale;
                    Colors[offs] = new Color(colorBuffer, colorBuffer, colorBuffer, 1);

                    var transformMatrix = math.mul(float4x4.Translate(p), float4x4.Scale(scale));

                    _arrays.m[offs] = transformMatrix;
                    offs++;
                }
            }
            offs = 0;
            for (var i = 0; i < _dims.x; i++)
            {
                var x = i - _dims.x * 0.5f + 0.2f;
                for (var j = 0; j < _dims.y; j++)
                {
                    var z = j - _dims.y * 0.2f + 0.2f;
                    var p = math.float3(x, UnityEngine.Random.Range(-2f, 0), z);
                    var scale = math.float3(UnityEngine.Random.Range(1f, 2f), UnityEngine.Random.Range(0.3f, 1.5f), UnityEngine.Random.Range(1f, 2f));
                    var transformMatrix = math.mul(float4x4.Translate(p), float4x4.Scale(scale));

                    Matrices2[offs] = transformMatrix;
                    offs++;
                }
            }
            offs = 0;
            for (var i = 0; i < _dims.x / 10; i++)
            {
                var x = i * 12 - _dims.x * 0.5f;
                for (var j = 0; j < _dims.y / 10; j++)
                {
                    var z = j  * 12 - _dims.y * 0.2f;
                    var p = math.float3(x + UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(0, 1), z + UnityEngine.Random.Range(-10f, 10f));
                    var scale = math.float3(0.3f);
                    var transformMatrix = math.mul(float4x4.Translate(p), float4x4.Scale(scale));

                    Matrices3[offs] = transformMatrix;
                    offs++;
                }
            }
        }

        public void Dispose()
        {
            if (_arrays.p.IsCreated) _arrays.p.Dispose();
            if (_arrays.m.IsCreated) _arrays.m.Dispose();
            if (scales.IsCreated) scales.Dispose();
            if (Matrices2.IsCreated) Matrices2.Dispose();
            if (Matrices3.IsCreated) Matrices3.Dispose();
            if (zScale.IsCreated) zScale.Dispose();
        }

        public void Update(float time, bool isEnable)
        {
            var t = time;
            var handle = new CalJob
            {
                Positions = _arrays.p,
                Matrices = _arrays.m,
                scales = scales,
                isEnable = isEnable,
                zScale = zScale,
                t = t
            }.Schedule(_arrays.p.Length, 64);

            handle.Complete();
        }

        [BurstCompile]
        struct CalJob : IJobParallelFor
        {
            public NativeArray<float3> Positions;
            public NativeArray<float4x4> Matrices;
            public NativeArray<float3> scales;
            public NativeArray<float> zScale;
            public float t;
            public bool isEnable;
            public void Execute(int index)
            {
                // z方向のスケールを計算（sinカーブを使用）
                float scaleZ = isEnable ? 0.1f + Math.Abs(math.sin(t) * scales[index].z * 2.5f) : zScale[index];
                zScale[index] = scaleZ;
                var position = new float3(Positions[index].x, Positions[index].y, Positions[index].z + scaleZ / 2.0f);

                // 位置とスケールを掛け合わせて最終行列を計算
                var finalMatrix = math.mul(float4x4.Translate(position), float4x4.Scale(new float3(scales[index].x, scales[index].y, scaleZ)));

                // 結果を保存
                Matrices[index] = finalMatrix;
            }
        }
    }

}

