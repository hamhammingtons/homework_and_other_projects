using System;

public enum MovementState
{
    Idle,
    Flying,
    Swimming,
    Walking
}

public interface ICreatureAction
{
    void CheckMovementStatus();
}

public class Creature : ICreatureAction
{
    public MovementState CurrentState { get; set; } = MovementState.Idle;

    public void Fly() => CurrentState = MovementState.Flying;
    public void Swim() => CurrentState = MovementState.Swimming;
    public void Walk() => CurrentState = MovementState.Walking;

    public void CheckMovementStatus()
    {
        switch (CurrentState)
        {
            case MovementState.Flying:
                Console.WriteLine("The creature is flying.");
                break;
            case MovementState.Swimming:
                Console.WriteLine("The creature is swimming.");
                break;
            case MovementState.Walking:
                Console.WriteLine("The creature is walking.");
                break;
            default:
                Console.WriteLine("The creature is stationary.");
                break;
        }
    }
}

class Program
{
    static void Main()
    {
        Creature creature = new Creature();

        creature.Walk();
        creature.CheckMovementStatus();

        creature.Fly();
        creature.CheckMovementStatus();
        
        creature.Swim();
        creature.CheckMovementStatus();
    }
}
