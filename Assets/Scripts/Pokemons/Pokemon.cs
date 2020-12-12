using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon 
{
   public PokemonBase Base { get; set; }
   public int Level { get; set; }
   public int exp { get; set; }
   public int exp2NextLevel { get; set; }

    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    
    public Dictionary<PokemonStat, int> Stats { get; private set; }
    public Dictionary<PokemonStat, int> StatBoost { get; private set; }

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        Base = pBase;
        Level = pLevel;
        exp2NextLevel = (int)Mathf.Pow(Level, 3);

        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.Base));
            if (Moves.Count == 4)
                break;
        }

        CalculateStats();
        HP = MaxHp;

        StatBoost = new Dictionary<PokemonStat, int>()
        {
            {PokemonStat.Attack, 0 },
            {PokemonStat.Defense, 0 },
            {PokemonStat.SpcAttack, 0 },
            {PokemonStat.SpcDefense, 0 },
            {PokemonStat.Speed, 0 },
        };
    }

    public void CalculateStats()
    {
        Stats = new Dictionary<PokemonStat, int>();
        Stats.Add(PokemonStat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100.0f) + 5);
        Stats.Add(PokemonStat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100.0f) + 5);
        Stats.Add(PokemonStat.SpcAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100.0f) + 5);
        Stats.Add(PokemonStat.SpcDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100.0f) + 5);
        Stats.Add(PokemonStat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100.0f) + 5);

        MaxHp = Mathf.FloorToInt((Base.MaxHP * Level) / 100.0f) + 10;
    }

    int GetStat(PokemonStat stat)
    {
        int statVal = Stats[stat];
        //Stat boosts
        int boost = StatBoost[stat];
        var boostVal = new float[] { 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 3.5f, 4.0f };

        if (boost >= 0)
            statVal = Mathf.FloorToInt(statVal * boostVal[boost]);
        else
            statVal = Mathf.FloorToInt(statVal * boostVal[-boost]);
        return statVal;
    }
    public int Attack {
        get { return GetStat(PokemonStat.Attack); }
    }
    public int Defense
    {
        get { return GetStat(PokemonStat.Defense); }
    }
    public int SpAttack
    {
        get { return GetStat(PokemonStat.SpcAttack); }
    }
    public int SpDefense
    {
        get { return GetStat(PokemonStat.SpcDefense); }
    }
    public int Speed
    {
        get { return GetStat(PokemonStat.Speed); }
    }
    public int MaxHp
    {
        get; private set;
    }

    public void ApplyStatus (List<MoveBase.StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            //Boost Stat to a max of 6
            StatBoost[statBoost.stat] = Mathf.Clamp(StatBoost[statBoost.stat] + statBoost.boost, -6, 6);
        }
    }
    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
        {
            critical = 2f;
        }
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };

        float attack = (move.Base.Category == MoveBase.MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveBase.MoveCategory.Special) ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        HP -= damage;
        if (HP <= 0)
        {
            HP = 0;
            damageDetails.Fainted = true;
        }

        return damageDetails;
    }

    public Move GetRandomMove()
    {
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }

    public Move GetAIMove(Pokemon target, int depth = 0)
    {
        //Test random Move
        int r = Random.Range(0, Moves.Count);
        if (TypeChart.GetEffectiveness(Moves[r].Base.Type, target.Base.Type1) >= 1.0f)
        {
            return Moves[r]; // Move is Effective
        }
        else if (depth > 4)
        {
            return Moves[r]; // Take it or leave it
        }
        else
        {
            return GetAIMove(target, ++depth); // Try Again
        }
    }

    /** SAVING **/
    public static string CreatePokemonString(Pokemon pokemon)
    {
        var pokeSave = new SavePokemon();
        pokeSave.pokemonBaseName = pokemon.Base.name;
        pokeSave.lvl = pokemon.Level;
        pokeSave.hp = pokemon.HP;
        string strMon = JsonUtility.ToJson(pokeSave);

        return strMon;
    }

    public static Pokemon ReadPokemonString(string strMon)
    {
        var pokeSave = JsonUtility.FromJson<SavePokemon>(strMon); //Load SavePokemon
        var baseMon = PokemonBase.ReadBaseMonString(pokeSave.pokemonBaseName); //Load PokemonBase SO
        var pokemon = new Pokemon(baseMon, pokeSave.lvl); //Build Pokemon
        pokemon.HP = pokeSave.hp;

        return pokemon;
    }

}

public class DamageDetails
{ 
    public bool Fainted { get; set; }
    public float Critical { get; set; }

    public float TypeEffectiveness { get; set; }
}

