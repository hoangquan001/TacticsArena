namespace TacticsArena.Champions
{

    [System.Serializable]
    public class AttributeData
    {
        public float health;
        public float attackDamage;
        public float attackSpeed;
        public float armor;
        public float magicResist;
        public float maxMana;
        public int range; // Placeholder for range, can be used in future expansions


        public static AttributeData operator +(AttributeData a, AttributeData b)
        {
            return new AttributeData
            {
                health = a.health + b.health,
                attackDamage = a.attackDamage + b.attackDamage,
                attackSpeed = a.attackSpeed + b.attackSpeed,
                armor = a.armor + b.armor,
                magicResist = a.magicResist + b.magicResist,
                maxMana = a.maxMana + b.maxMana,
                range = a.range + b.range

            };
        }
    }

}
