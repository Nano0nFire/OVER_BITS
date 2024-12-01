using System;
using Cysharp.Threading.Tasks;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;

public class DACS_Entities_PlayerComponent : DACS_Entities // プレイヤーオブジェクト直下
{
    SecureFloat sInt_HP;
    SecureFloat sInt_Def;
    SecureFloat sInt_Abno;
    SecureFloat sInt_DodgeChance;
    SecureFloat sInt_Atk;
    SecureFloat sInt_MP;
    SecureFloat sInt_CritChance;
    SecureFloat sInt_CritDamage;
    SecureFloat sInt_Penetration;
    SecureFloat sInt_HitChance;

    public void Setup()
    {
        sInt_HP = new(Tamper);
        sInt_Def = new(Tamper);
        sInt_Abno = new(Tamper);
        sInt_DodgeChance = new(Tamper);
        sInt_Atk = new(Tamper);
        sInt_MP = new(Tamper);
        sInt_CritChance = new(Tamper);
        sInt_CritDamage = new(Tamper);
        sInt_Penetration = new(Tamper);
        sInt_HitChance = new(Tamper);
    }
    public void OnActivate(BaseEntityStatus entityData)
    {

    }
    public override void OnDamage(DamageData damage, ulong nwID = 0)
    {

    }
    public override void OnDead()
    {

    }
    public override void OnDodge()
    {

    }

    public void Tamper()
    {

    }

}

/// <summary>
/// ハッシュチェック機構と相互依存関係を備えたfloat型変数
/// </summary>
public class SecureFloat
{
    Action TamperAction;
    string baselineHash;
    int rA, rB, rC, rD, rE, rF;
    string str {get => a.ToString() + b.ToString() + Value.ToString();}
    float _value;
    public float Value
    {
        get
        {
            if (2 * _value == a + b)
                return _value;

            TamperAction.Invoke();
            return (a + b) / 2;
        }
        set
        {
            key = UnityEngine.Random.Range(-(int)value, (int)value);
            a = value;
            b = value;
            _value = value;
            baselineHash = CalculateHash();
        }
    }
    int key;
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
        TamperAction +=action;
        rA = UnityEngine.Random.Range(2, 3);
        rB = UnityEngine.Random.Range(1, 3);
        rC = UnityEngine.Random.Range(1, 3);
        rD = UnityEngine.Random.Range(2, 3);
        rE = UnityEngine.Random.Range(1, 3);
        rF = UnityEngine.Random.Range(1, 3);
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
