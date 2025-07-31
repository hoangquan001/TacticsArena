using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "TFT/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;
    public float damage;
    public float aoeRadius;
    public float duration;
    public SkillTarget target;
}

public enum SkillTarget
{
    ClosestEnemy,
    FarthestEnemy,
    Self,
    AllEnemies,
    AllyLowestHP,
    Area
}
