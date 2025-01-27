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
    [SerializeField] Material _material = null;
    [SerializeField] Vector2Int _counts = new Vector2Int(10, 10);

    PositionBuffer _buffer;

    void Start()
      => _buffer = new PositionBuffer(_counts.x, _counts.y);

    void OnDestroy()
      => _buffer.Dispose();

    void Update()
    {
        _buffer.Update(Time.time);

        var rparams = new RenderParams(_material)
          { receiveShadows = true,
            shadowCastingMode = ShadowCastingMode.On };

        var matrices = _buffer.Matrices;
        for (var offs = 0; offs < matrices.Length; offs += 1023)
        {
            var count = Mathf.Min(1023, matrices.Length - offs);
            Graphics.RenderMeshInstanced(rparams, _mesh, 0, matrices, count, offs);
        }
        matrices = _buffer.Matrices2;
        for (var offs = 0; offs < matrices.Length; offs += 1023)
        {
            var count = Mathf.Min(1023, matrices.Length - offs);
            Graphics.RenderMeshInstanced(rparams, _mesh, 0, matrices, count, offs);
        }
    }
}

sealed class PositionBuffer : IDisposable
{
    public NativeArray<Matrix4x4> Matrices => _arrays.m.Reinterpret<Matrix4x4>();
    public NativeArray<Matrix4x4> Matrices2;
    public NativeArray<float3> scales;

    (NativeArray<float3> p, NativeArray<float4x4> m) _arrays;
    (int x, int y) _dims;

    public PositionBuffer(int xCount, int yCount)
    {
        _dims = (xCount, yCount);
        _arrays = (new NativeArray<float3>(_dims.x * _dims.y, Allocator.Persistent),
                   new NativeArray<float4x4>(_dims.x * _dims.y, Allocator.Persistent));
        scales = new NativeArray<float3>(_dims.x * _dims.y, Allocator.Persistent);
        Matrices2 = new NativeArray<Matrix4x4>(_dims.x * _dims.y, Allocator.Persistent);

        var offs = 0;
        for (var i = 0; i < _dims.x; i++)
        {
            var x = i - _dims.x * 0.2f + 0.2f;
            for (var j = 0; j < _dims.y; j++)
            {
                var z = j - _dims.y * 0.2f + 0.2f;
                var p = math.float3(x, UnityEngine.Random.Range(-0.5f, 0.5f), z);
                _arrays.p[offs] = p;
                var scale = math.float3(UnityEngine.Random.Range(0.5f, 1.5f), UnityEngine.Random.Range(0.3f, 2), UnityEngine.Random.Range(0.5f, 1.5f));
                scales[offs] = scale;

                var transformMatrix = math.mul(float4x4.Translate(p), float4x4.Scale(scale));

                _arrays.m[offs] = transformMatrix;
                offs++;
            }
        }
        offs = 0;
        for (var i = 0; i < _dims.x; i++)
        {
            var x = i - _dims.x * 0.2f + 0.2f;
            for (var j = 0; j < _dims.y; j++)
            {
                var z = j - _dims.y * 0.2f + 0.2f;
                var p = math.float3(x, UnityEngine.Random.Range(-1f, 0f), z);
                var scale = math.float3(UnityEngine.Random.Range(0.5f, 1.5f), UnityEngine.Random.Range(0.3f, 2), UnityEngine.Random.Range(0.5f, 1.5f));
                var transformMatrix = math.mul(float4x4.Translate(p), float4x4.Scale(scale));

                Matrices2[offs] = transformMatrix;
                offs++;
            }
        }
    }

    public void Dispose()
    {
        if (_arrays.p.IsCreated) _arrays.p.Dispose();
        if (_arrays.m.IsCreated) _arrays.m.Dispose();
        if (scales.IsCreated) scales.Dispose();
    }

    public void Update(float time)
    {
        var t = time;
        var handle = new CalJob
        {
            Positions = _arrays.p,
            Matrices = _arrays.m,
            scales = scales,
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
        public float t;
        public void Execute(int index)
        {
            // z方向のスケールを計算（sinカーブを使用）
            float scaleZ = 0.1f + Math.Abs(math.sin(t) * scales[index].z);
            var position = new float3(Positions[index].x, Positions[index].y, Positions[index].z + scaleZ / 2.0f);

            // 位置とスケールを掛け合わせて最終行列を計算
            var finalMatrix = math.mul(float4x4.Translate(position), float4x4.Scale(new float3(scales[index].x, scales[index].y, scaleZ)));

            // 結果を保存
            Matrices[index] = finalMatrix;
        }
    }
}
