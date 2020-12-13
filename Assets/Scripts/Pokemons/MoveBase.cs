using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] PokemonType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;
    [SerializeField] MoveCategory category;
    [SerializeField] MoveTarget target;
    [SerializeField] MoveEffects effects;

    public string Name {
        get { return name; }
    }
    public string Description {
        get { return description; }
    }
    public PokemonType Type {
        get {return type;}
    }
    public int Power { 
        get { return power; }
    }
    public int Accuracy { 
        get { return accuracy; }
    }
    public int PP  {
        get { return pp; }
    }
    public MoveCategory Category
    {
        get { return category; }
    }
    public MoveTarget Target
    {
        get { return target; }
    }
    public MoveEffects Effects
    {
        get { return effects; }
    }

    [System.Serializable]
    public class MoveEffects
    {
        [SerializeField] List<StatBoost> statBoosts;

        public List<StatBoost> Boosts
        {
            get { return statBoosts; }
        }
    }

    [System.Serializable]
    public class StatBoost
    {
        public PokemonStat stat;
        public int boost;
    }

    public enum MoveCategory
    {
        Physical,
        Special,
        Status,
    }

    public enum MoveTarget
    {
        Enemy,
        Self,
    }
}

public static class MoveBaseList
{
    static public MoveBase[] baseList = Resources.LoadAll<MoveBase>("Scriptable Objects/Moves");

    public static MoveBase GetMoveBase(string moveName)
    {
        foreach (var moveBase in baseList)
        {
            //Debug.Log(pokemonBase.name);
            if (moveBase.Name == moveName)
            {
                return moveBase;
            }
        }

        return null;
    }

}