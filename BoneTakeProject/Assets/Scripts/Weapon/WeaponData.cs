using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum Weapon_Type
{
    Basic,
    Knife,
    Bow,
    etc
}

[System.Serializable]
public enum Weapon_Name
{
    Basic,
    Wp01, //KnifeBunnyKnife
    Wp02,  //BowowBow
    Wp03, //보스무기 1
    Wp04, //보스무기 2
    Wp05, //Enemy02 무기
    Wp06, //Enemy03 무기
}

[System.Serializable]
public class WeaponGFXSource
{
    public List<Sprite> freshIcon;
    public List<Sprite> rottenIcon;
}

public class WeaponData : MonoBehaviour
{
    public static WeaponData instance;
    public WeaponGFXSource weaponGFXSource;
    
    // 무기 이름에 따라 무기 ID 부여
    public Dictionary<Weapon_Name, int> weaponName_ID = new Dictionary<Weapon_Name, int>
    {
        { Weapon_Name.Basic, 0 },
        { Weapon_Name.Wp01, 1 },
        { Weapon_Name.Wp02, 2 },
        { Weapon_Name.Wp03, 3 },
        { Weapon_Name.Wp04, 4 },
        { Weapon_Name.Wp05, 5 },
        { Weapon_Name.Wp06, 6 }
    };
    
    // 무기 이름에 따라 무기 형식 부여
    public Dictionary<Weapon_Name, Weapon_Type> weaponName_Type = new Dictionary<Weapon_Name, Weapon_Type>
    {
        { Weapon_Name.Basic, Weapon_Type.Basic },
        { Weapon_Name.Wp01, Weapon_Type.Knife },
        { Weapon_Name.Wp02, Weapon_Type.Bow },
        { Weapon_Name.Wp03, Weapon_Type.Knife },
        { Weapon_Name.Wp04, Weapon_Type.Bow },
        { Weapon_Name.Wp05, Weapon_Type.etc },
        { Weapon_Name.Wp06, Weapon_Type.etc }
    };
    
    // 무기 형식에 따라 최대 타수 부여
    public Dictionary<Weapon_Type, int> weaponType_AttackCounts = new Dictionary<Weapon_Type, int>
    {
        { Weapon_Type.Basic, 2 },
        { Weapon_Type.Knife, 2 },
        { Weapon_Type.Bow, 1 },
        { Weapon_Type.etc, 0 }
    };
    
    //무기마다의 공격력 부여
    public Dictionary<Weapon_Name, int> weaponName_Damage = new Dictionary<Weapon_Name, int>
    {
        { Weapon_Name.Basic, 0 },
        { Weapon_Name.Wp01, 5 },
        { Weapon_Name.Wp02, 5 },
        { Weapon_Name.Wp03, 7 },
        { Weapon_Name.Wp04, 7 },
        { Weapon_Name.Wp05, 5 },
        { Weapon_Name.Wp06, 5 }
    };
    
    //무기마다의 무기HP 부여
    public Dictionary<Weapon_Name, int> weaponName_Life = new Dictionary<Weapon_Name, int>
    {
        { Weapon_Name.Basic, -1 },
        { Weapon_Name.Wp01, 8 },
        { Weapon_Name.Wp02, 15 },
        { Weapon_Name.Wp03, 10 },
        { Weapon_Name.Wp04, 17 },
        { Weapon_Name.Wp05, 8 },
        { Weapon_Name.Wp06, 8 }
    };
    
    //활 무기마다 조준 공격 쿨타임을 부여
    public Dictionary<Weapon_Name, float> bowAimCoolTime = new Dictionary<Weapon_Name, float>
    {
        { Weapon_Name.Wp02, 0.5f },
        { Weapon_Name.Wp04, 0.2f }
        //여기에 추가 활의 쿨타임 데이터를 입력
    };
    
    private void Awake()
    {
        #region 싱글톤
        
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(instance.gameObject);
        }
        
        #endregion
    }
    
    /// <summary>
    /// 무기 타입에따른 무기 타수 찾기
    /// </summary>
    public int GetType_AttackCount(Weapon_Type weaponType)
    {
        if (weaponType_AttackCounts.TryGetValue(weaponType, out int attackCount))
        {
            return attackCount;
        }
        else
        {
            Debug.LogError("정의되어 있지 않은 WeaponType");
            return 0;
        }
    }
    
    /// <summary>
    /// 무기 이름에따른 무기 데미지 찾기
    /// </summary>
    public int GetName_DamageCount(Weapon_Name weaponName)
    {
        if (weaponName_Damage.TryGetValue(weaponName, out int attackDamage))
        {
            return attackDamage;
        }
        else
        {
            Debug.LogError("정의되어 있지 않은 WeaponName");
            return 0;
        }
    }
    
    /// <summary>
    /// 무기 이름에따른 초기 무기 HP 찾기
    /// </summary>
    public int GetName_WeaponLifeCount(Weapon_Name weaponName)
    {
        if (weaponName_Life.TryGetValue(weaponName, out int weaponLife))
        {
            return weaponLife;
        }
        else
        {
            Debug.LogError("정의되어 있지 않은 WeaponName");
            return 0;
        }
    }
    
    /// <summary>
    /// 무기 이름에따른 무기 ID 찾기
    /// </summary>
    public int GetName_WeaponID(Weapon_Name weaponName)
    {
        if (weaponName_ID.TryGetValue(weaponName, out int weaponId))
        {
            return weaponId;
        }
        else
        {
            Debug.LogError("정의되어 있지 않은 WeaponName");
            return 0;
        }
    }
    
    /// <summary>
    /// 무기 이름에따른 무기 형식(Type) 찾기
    /// </summary>
    public Weapon_Type GetName_WeaponType(Weapon_Name weaponName)
    {
        if (weaponName_Type.TryGetValue(weaponName, out Weapon_Type weaponType))
        {
            return weaponType;
        }
        else
        {
            Debug.LogError("정의되어 있지 않은 WeaponName");
            return 0;
        }
    }
    
    /// <summary>
    /// 활 무기 이름에 따른 조준 공격 쿨타임 찾기
    /// </summary>
    public float GetName_BowAimCoolTime(Weapon_Name weaponName)
    {
        if (bowAimCoolTime.TryGetValue(weaponName, out float aimAttackCool))
        {
            return aimAttackCool;
        }
        else
        {
            Debug.LogError("정의되어 있지 않은 WeaponName");
            return 0;
        }
    }
    
    
}
