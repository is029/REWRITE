using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyDatabase",
    menuName = "REWRITE/Enemy Database"
)]
public class EnemyDatabase : ScriptableObject
{
    [Header("通常敵")]
    public List<CharacterData> normalEnemies =
        new List<CharacterData>();

    [Header("エリート敵")]
    public List<CharacterData> eliteEnemies =
        new List<CharacterData>();

    [Header("ボス")]
    public List<CharacterData> bossEnemies =
        new List<CharacterData>();

    public CharacterData GetRandomNormalEnemy()
    {
        if (normalEnemies == null ||
            normalEnemies.Count == 0)
        {
            return null;
        }

        return normalEnemies[
            Random.Range(0, normalEnemies.Count)
        ];
    }

    public CharacterData GetRandomEliteEnemy()
    {
        if (eliteEnemies == null ||
            eliteEnemies.Count == 0)
        {
            return null;
        }

        return eliteEnemies[
            Random.Range(0, eliteEnemies.Count)
        ];
    }

    public CharacterData GetRandomBossEnemy()
    {
        if (bossEnemies == null ||
            bossEnemies.Count == 0)
        {
            return null;
        }

        return bossEnemies[
            Random.Range(0, bossEnemies.Count)
        ];
    }
}