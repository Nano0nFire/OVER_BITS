using UnityEngine;

public class particletest : MonoBehaviour
{
    public ParticleSystem particleSystem; // パーティクルシステム
    private ParticleSystem.Particle[] particles; // パーティクルデータ
    private Vector3[] velocities; // 各パーティクルの速度

    private void Start()
    {
        int maxParticles = particleSystem.main.maxParticles;
        particles = new ParticleSystem.Particle[maxParticles];
        velocities = new Vector3[maxParticles];
    }

    private void Update()
    {
        // 現在のパーティクル数を取得
        int particleCount = particleSystem.GetParticles(particles);

        for (int i = 0; i < particleCount; i++)
        {
            // パーティクルに速度を適用
            velocities[i] += Physics.gravity * Time.deltaTime; // 重力を適用
            particles[i].position += velocities[i] * Time.deltaTime;
        }

        // 更新後のパーティクルデータを反映
        particleSystem.SetParticles(particles, particleCount);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnParticle(transform.position, transform.forward);
            Debug.Log("hi");
        }
            
   }

    public void SpawnParticle(Vector3 position, Vector3 initialVelocity)
    {
        // 新しいパーティクルを生成
        int particleCount = particleSystem.GetParticles(particles);
        if (particleCount < particles.Length)
        {
            particles[particleCount].position = position;
            particles[particleCount].startLifetime = 5.0f; // 寿命を設定
            particles[particleCount].remainingLifetime = 5.0f;
            particles[particleCount].startSize = 0.2f;
            particles[particleCount].startColor = Color.white;

            velocities[particleCount] = initialVelocity;

            particleCount++;
            particleSystem.SetParticles(particles, particleCount);
        }
    }
}