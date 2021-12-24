using Assets.Scripts.Character;
using System;
using UnityEngine;
public enum ItemCategory
{
    offensive, curative
}
public enum ItemType
{
    health, shield, mana, tension, attack, defense
}
[CreateAssetMenu]
[Serializable]
public class Item : ScriptableObject
{
    public int id;
    public string name;
    public string description;
    public ItemCategory category;
    public ItemType itemType;
    public float value;  //l'ammontare offensivo o curativo

    public float buyValue;
    public float sellValue;

    public int priority;

    public void Use(GameObject c)
    {
            var character = c.GetComponent<Character>();
            if (!character) return;

            switch (itemType)
            {
                case ItemType.health:
                    _ = category == ItemCategory.curative ? character.health += value : character.health -= value;
                    break;
                case ItemType.shield:
                    break;
                case ItemType.mana:
                    break;
                case ItemType.tension:
                    break;
                case ItemType.attack:
                    break;
                case ItemType.defense:
                    break;
                default:
                    break;
            }
    }
}
