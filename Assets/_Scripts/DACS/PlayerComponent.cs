using Unity.Netcode;
using UnityEngine;
using SecureCollections;

namespace DACS.Entities
{
    public class PlayerComponent : Entity // プレイヤーオブジェクト直下
    {
        float t = 0;
        bool[] hasChange = new bool[13]; // 0番目の要素で全体で変更があったかを記録
        sfloat s_MaxHP;
        sfloat s_HP;
        sfloat s_Def;
        sfloat s_Abno;
        sfloat s_DodgeChance;
        sfloat s_Atk;
        sfloat s_MaxMP;
        sfloat s_MP;
        sfloat s_CritChance;
        sfloat s_CritDamage;
        sfloat s_Penetration;
        sfloat s_HitChance;

        public override float MaxHP
        {
            get => IsServer ? base.MaxHP : s_MaxHP.Value;
            set
            {
                if (IsServer)
                    base.MaxHP = value;
                else
                {
                    hasChange[1] = true;
                    s_MaxHP.Value = value;
                }
            }
        }
        public override float HP{get => IsServer ? base.HP : s_HP.Value; set => s_HP.Value = value;}
        public override float Def{get => IsServer ? base.Def : s_Def.Value; set => s_Def.Value = value;}
        public override float Abno{get => IsServer ? base.Abno : s_Abno.Value; set => s_Abno.Value = value;}
        public override float DodgeChance{get => IsServer ? base.DodgeChance : s_DodgeChance.Value; set => s_DodgeChance.Value = value;}
        public override float Atk{get => IsServer ? base.Atk : s_Atk.Value; set => s_Atk.Value = value;}
        public override float MaxMP{get => IsServer ? base.MaxMP : s_MaxMP.Value; set => s_MaxMP.Value = value;}
        public override float MP{get => IsServer ? base.MP : s_MP.Value; set => s_MP.Value = value;}
        public override float CritChance{get => IsServer ? base.CritChance : s_CritChance.Value; set => s_CritChance.Value = value;}
        public override float CritDamage{get => IsServer ? base.CritDamage : s_CritDamage.Value; set => s_CritDamage.Value = value;}
        public override float Penetration{get => IsServer ? base.Penetration : s_Penetration.Value; set => s_Penetration.Value = value;}
        public override float HitChance{get => IsServer ? base.HitChance : s_HitChance.Value; set => s_HitChance.Value = value;}


        public void Setup()
        {
            s_HP = new(Tamper);
            s_Def = new(Tamper);
            s_Abno = new(Tamper);
            s_DodgeChance = new(Tamper);
            s_Atk = new(Tamper);
            s_MP = new(Tamper);
            s_CritChance = new(Tamper);
            s_CritDamage = new(Tamper);
            s_Penetration = new(Tamper);
            s_HitChance = new(Tamper);
        }

        void Update()
        {
            if (t < 2)
                t += Time.deltaTime;

            if (hasChange[0] && t > 1)
            {
                float[] changed =
                {
                    MaxHP,
                    HP,
                    Def,
                    Abno,
                    DodgeChance,
                    Atk,
                    MaxMP,
                    MP,
                    CritChance,
                    CritDamage,
                    Penetration,
                    HitChance
                };
                UpdateValueServerRpc(changed);

                for(int i = 0; i < 13; i ++)
                    hasChange[i] = false;

                t = 0;
            }
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

        [ServerRpc]
        void UpdateValueServerRpc(float[] changedValue)
        {

        }
    }
}