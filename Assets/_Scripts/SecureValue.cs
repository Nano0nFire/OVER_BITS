using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// ハッシュチェック機構と相互依存関係を備えたfloat型変数
/// </summary>
public class SecureFloat
{
    public Action TamperAction;
    string baselineHash;
    int rA, rB, rC, rD, rE, rF;
    string str {get => a.ToString() + b.ToString() + Value.ToString();}
    float _value;
    public float Value
    {
        get
        {
            if (IsApproximatelyEqual(2 * _value, a + b))
                return _value;

            TamperAction.Invoke();
            return (a + b) / 2;
        }
        set
        {
            key = (int)(UnityEngine.Random.Range(value - 1, value * 2 + 1) / value) + 1;
            a = value;
            b = value;
            _value = value;
            baselineHash = CalculateHash();
        }
    }
    int key = 1;
    float _a;
    float a
    {
        get => _a / (key * (key % rA == 0 ? -rB : rC));
        set => _a = value * key * (key % rA == 0 ? -rB : rC);
    }
    float _b;
    float b
    {
        get => _b / (key * (key % rD == 0 ? rE : -rF));
        set => _b = value * key * (key % rD == 0 ? rE : -rF);
    }
    public SecureFloat(Action action)
    {
        TamperAction += action;
        rA = UnityEngine.Random.Range(1, 5);
        rB = UnityEngine.Random.Range(1, 5);
        rC = UnityEngine.Random.Range(1, 5);
        rD = UnityEngine.Random.Range(1, 5);
        rE = UnityEngine.Random.Range(1, 5);
        rF = UnityEngine.Random.Range(1, 5);

        Value = 0;
    }

    bool IsApproximatelyEqual(float a, float b, float tolerancePer = 0.01f)
    {
        return Math.Abs(a - b) <= Math.Abs(a) * tolerancePer;
    }
    string CalculateHash()
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(str));
        StringBuilder sb = new();
        foreach (byte b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
    public bool IsModified()
    {
        string currentHash = CalculateHash();
        return currentHash != baselineHash;
    }
}
