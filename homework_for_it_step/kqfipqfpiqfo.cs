using System;

namespace GameApp
{
    class Character
    {
        public string Name { get; set; }
        public float Health { get; set; }
        public float Damage { get; set; }
        public float Armor { get; set; }
        public float Mana { get; set; }

        public Character(string name, float health, float damage, float armor, float mana)
        {
            Name = name;
            Health = health;
            Damage = damage;
            Armor = armor;
            Mana = mana;
        }

        public void Hit(ref Character enemy)
        {
            float actualDamage = Damage - enemy.Armor;
            if (actualDamage < 0) actualDamage = 0;

            enemy.Health -= actualDamage;
            Mana += 10;
        }

        public void CastSpell(ref Character enemy)
        {
            if (Mana >= 20)
            {
                enemy.Health -= Damage;
                Mana -= 20;
            }
        }
    }

    class Program
    {
        static void Main()
        {
            Character hero = new Character("Warrior", 100, 20, 5, 10);
            Character boss = new Character("Dragon", 200, 30, 10, 50);

            hero.Hit(ref boss);
            hero.CastSpell(ref boss);

            Console.WriteLine($"{boss.Name} Health: {boss.Health}");
            Console.WriteLine($"{hero.Name} Mana: {hero.Mana}");
            
            Console.ReadKey();
        }
    }
}
