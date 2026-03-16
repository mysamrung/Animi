using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CharacterMovementStats
{
    public float moveSpeed;
    public float rotateSpeed;
}

[System.Serializable]
public class CharacterAttackStats {
    public int damageMin;
    public int damageMax;

    public int CaculateDamage() {
        return Random.RandomRange(damageMin, damageMax);
    }
}

[System.Serializable]
public class CharacterWeapon {
    [System.Serializable]
    public class Weapon {
        public string key;
        public WeaponColliderDetector colliderDetector;
    }

    [SerializeField]
    private List<Weapon> weapons = new List<Weapon>();

    public Weapon GetWeapon(string name) {
        return weapons.Find(s => s.key == name);
    }

}
