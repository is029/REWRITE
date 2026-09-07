using UnityEngine;

public enum UpgradeType
{
    AttackUp,
    SpeedUp,
    MaxHPUp,
    Heal
}

[CreateAssetMenu(
    fileName = "NewUpgrade",
    menuName = "REWRITE/Upgrade Data"
)]
public class UpgradeData : ScriptableObject
{
    [Header("基本情報")]
    public string upgradeName;

    [TextArea(2, 4)]
    public string description;

    [Header("強化タイプ")]
    public UpgradeType upgradeType;

    [Header("効果量")]
    public int value;

    [Header("価格")]
    public int price;
}