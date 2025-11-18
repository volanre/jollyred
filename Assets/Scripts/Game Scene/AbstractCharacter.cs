using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AbstractCharacter : MonoBehaviour
{
    /* 
    For all multipliers:
        If bool is true then it is an MULTIPLICATIVE bonus
        If bool is false then it is a ADDITIVE bonus
    */
    [NonSerialized] public List<Tuple<bool, float>> damageMultipliers = new List<Tuple<bool, float>>();
    [NonSerialized] public List<Tuple<bool, float>> defenseMultipliers = new List<Tuple<bool, float>>();
    [NonSerialized] public List<Tuple<bool, float>> speedMultipliers = new List<Tuple<bool, float>>();
    [NonSerialized] public List<Tuple<bool, float>> fireRateMultipliers = new List<Tuple<bool, float>>();

    [Header("Character Properties")]
    [NonSerialized] public int health = 999999999;
    [SerializeField] public string title;
    [SerializeField] public int maxHealth = 500;
    [SerializeField] public int attack = 100, defense = 100;
    [SerializeField] public float speed = 5f;
    [SerializeField] public float jumpForce = 10f;
    [SerializeField] public float fireRate = 0.3f;

    /// <summary>
    /// Call this whenever the character takes damage
    /// </summary>
    public void TakeDamage(int attackedValue, bool ignoreDefense = false)
    {
        int effectiveDamage = ignoreDefense ? attackedValue : CalculateEffectiveDamage(attackedValue);
        health -= effectiveDamage;
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        DamageEffects();
        Invoke("ResetColor", 0.07f);
    }

    /// <summary>
    /// Extra events that occur after taking damage
    /// </summary>
    public abstract void DamageEffects();
    private void ResetColor()
    {
        gameObject.GetComponent<SpriteRenderer>().color = Color.white;
    }

    /// <summary>
    /// Calculates how much damage is actually dealt to the character using a formula <br/>
    /// This is NOT for calculating how much damage the character does to others
    /// </summary>
    /// <returns></returns>
    public int CalculateEffectiveDamage(int attackedValue)
    {
        if (attackedValue <= 0) return 0;
        float currentDamage = (attackedValue * attackedValue) / (attackedValue + GetEffectiveDefense());
        currentDamage = (int)currentDamage;
        currentDamage = currentDamage <= 0 ? 1 : currentDamage;
        return (int)currentDamage;
    }

    /// <summary>
    /// Gets the character's effective attack value.
    /// </summary>
    /// <returns></returns>
    public int GetEffectiveAttack()
    {
        float newDamage = attack;
        var multiplicativeOnly = damageMultipliers.Where(x => x.Item1 == true).ToList();
        var additiveOnly = damageMultipliers.Where(x => x.Item1 == false).ToList();
        foreach ((bool type, float value) in multiplicativeOnly)
        {
            newDamage = newDamage * value;
        }
        foreach ((bool type, float value) in additiveOnly)
        {
            newDamage += value;
        }
        return (int)newDamage;
    }
    /// <summary>
    /// Gets the character's effective move speed.
    /// </summary>
    /// <returns></returns>
    public float GetEffectiveSpeed()
    {
        float newSpeed = speed;
        var multiplicativeOnly = speedMultipliers.Where(x => x.Item1 == true).ToList();
        var additiveOnly = speedMultipliers.Where(x => x.Item1 == false).ToList();
        foreach ((bool type, float value) in multiplicativeOnly)
        {
            newSpeed = newSpeed * value;
        }
        foreach ((bool type, float value) in additiveOnly)
        {
            newSpeed += value;
        }
        return newSpeed;
    }
    /// <summary>
    /// Gets the character's effective firerate.
    /// </summary>
    /// <returns></returns>
    public float GetEffectiveFireRate()
    {
        float newFire = fireRate;
        var multiplicativeOnly = fireRateMultipliers.Where(x => x.Item1 == true).ToList();
        var additiveOnly = fireRateMultipliers.Where(x => x.Item1 == false).ToList();
        foreach ((bool type, float value) in multiplicativeOnly)
        {
            newFire = newFire * value;
        }
        foreach ((bool type, float value) in additiveOnly)
        {
            newFire += value;
        }
        return newFire;
    }
    /// <summary>
    /// Gets the character's effective defense value.
    /// </summary>
    /// <returns></returns>
    public int GetEffectiveDefense()
    {
        float newD = defense;
        var multiplicativeOnly = defenseMultipliers.Where(x => x.Item1 == true).ToList();
        var additiveOnly = defenseMultipliers.Where(x => x.Item1 == false).ToList();
        foreach ((bool type, float value) in multiplicativeOnly)
        {
            newD = newD * value;
        }
        foreach ((bool type, float value) in additiveOnly)
        {
            newD += value;
        }
        return (int)newD;
    }

}
