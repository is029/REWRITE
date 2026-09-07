using NUnit.Framework.Interfaces;
using System.Collections.Generic;

[System.Serializable]
public class PlayerRunData
{
    // ========================================
    // ローグライク中の恒久強化
    // ========================================

    public int maxHPBonus = 0;
    public int attackBonus = 0;
    public int speedBonus = 0;


    // ========================================
    // 現在HP
    // ========================================

    public int currentHP;


    // ========================================
    // 基本最大HP
    // CharacterDataから取得
    // ========================================

    public int baseMaxHP;


    // ========================================
    // 現在の最大HP
    // ========================================

    public int MaxHP
    {
        get
        {
            return baseMaxHP + maxHPBonus;
        }
    }


    // ========================================
    // コイン
    // ========================================

    public int coins = 0;


    // ========================================
    // 獲得したスキル
    // ========================================

    public List<SkillData> skills =
        new List<SkillData>();


    // ========================================
    // 所持アイテム
    // ========================================

    public List<ItemData> items =
        new List<ItemData>();

    public int itemCooldown = 0;


    // ========================================
    // 初期化
    // ========================================

    public void Initialize(int baseMaxHP)
    {
        this.baseMaxHP = baseMaxHP;

        maxHPBonus = 0;
        attackBonus = 0;
        speedBonus = 0;

        currentHP = baseMaxHP;

        coins = 0;

        skills.Clear();
        items.Clear();

        itemCooldown = 0;
    }


    // ========================================
    // 攻撃力アップ
    // ========================================

    public void AddAttack(int amount)
    {
        attackBonus += amount;
    }


    // ========================================
    // Speedアップ
    // ========================================

    public void AddSpeed(int amount)
    {
        speedBonus += amount;
    }


    // ========================================
    // 最大HPアップ
    // ========================================

    public void AddMaxHP(int amount)
    {
        maxHPBonus += amount;

        // 最大HPが増えた分だけ現在HPも増やす
        currentHP += amount;
    }


    // ========================================
    // 回復
    // ========================================

    public void Heal(int amount)
    {
        currentHP += amount;

        if (currentHP > MaxHP)
        {
            currentHP = MaxHP;
        }
    }


    // ========================================
    // コイン追加
    // ========================================

    public void AddCoins(int amount)
    {
        coins += amount;
    }


    // ========================================
    // コイン消費
    // ========================================

    public bool SpendCoins(int amount)
    {
        if (coins < amount)
        {
            return false;
        }

        coins -= amount;
        return true;
    }


    // ========================================
    // アイテム追加
    // ========================================

    public void AddItem(ItemData item)
    {
        if (item == null)
        {
            return;
        }

        items.Add(item);
    }

    // ========================================
    // アイテム使用可能か確認
    // ========================================
    public bool CanUseItem()
    {
        return itemCooldown <= 0;
    }

    // ========================================
    // アイテム使用時クールタイムを持つ
    // ========================================
    public void UseItemCooldown()
    {
        itemCooldown = 2;
    }

    // ========================================
    // アイテムクールタイムを減らす
    // ========================================
    public void ReduceItemCooldown()
    {
        if (itemCooldown > 0)
        {
            itemCooldown--;
        }
    }
}