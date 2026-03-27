using System;
using System.Collections.Generic;

class GameState
{
    public int health = 100;
    public List<string> inventory = new List<string>();
}

interface ICommand
{
    string Name { get; }
    void Execute(GameState state);
}

interface ICondition
{
    bool Check(GameState state);
}

interface IEffect
{
    void Apply(GameState state);
}

interface IInteractable
{
    string Id { get; }
    void Interact(GameState state);
}

abstract class CommandBase : ICommand
{
    public string Name { get; protected set; }

    public abstract void Execute(GameState state);
}

abstract class ConditionBase : ICondition
{
    public abstract bool Check(GameState state);
}

abstract class EffectBase : IEffect
{
    public abstract void Apply(GameState state);
}

abstract class GameEventBase
{
    public ICondition condition;
    public List<IEffect> effects = new List<IEffect>();

    public bool once = false;
    public bool triggered = false;

    public void TryExecute(GameState state)
    {
        if (once && triggered)
            return;

        if (condition.Check(state))
        {
            foreach (var e in effects)
            {
                e.Apply(state);
            }

            triggered = true;
        }
    }
}

class LookCommand : CommandBase
{
    public LookCommand()
    {
        Name = "look";
    }

    public override void Execute(GameState state)
    {
        Console.WriteLine("Ярик тихо осматривается вокруг...");
    }
}

class HelpCommand : CommandBase
{
    public HelpCommand()
    {
        Name = "help";
    }

    public override void Execute(GameState state)
    {
        Console.WriteLine("Команды:");
        Console.WriteLine("look");
        Console.WriteLine("inv");
        Console.WriteLine("status");
    }
}

class InventoryCommand : CommandBase
{
    public InventoryCommand()
    {
        Name = "inv";
    }

    public override void Execute(GameState state)
    {
        Console.WriteLine("Инвентарь:");

        foreach (var item in state.inventory)
        {
            Console.WriteLine(item);
        }
    }
}

class StatusCommand : CommandBase
{
    public StatusCommand()
    {
        Name = "status";
    }

    public override void Execute(GameState state)
    {
        Console.WriteLine("Здоровье: " + state.health);
    }
}

class HasItemCondition : ConditionBase
{
    string item;

    public HasItemCondition(string item)
    {
        this.item = item;
    }

    public override bool Check(GameState state)
    {
        return state.inventory.Contains(item);
    }
}

class HealthCondition : ConditionBase
{
    int minHealth;

    public HealthCondition(int h)
    {
        minHealth = h;
    }

    public override bool Check(GameState state)
    {
        return state.health >= minHealth;
    }
}

class AndCondition : ConditionBase
{
    ICondition a;
    ICondition b;

    public AndCondition(ICondition a, ICondition b)
    {
        this.a = a;
        this.b = b;
    }

    public override bool Check(GameState state)
    {
        return a.Check(state) && b.Check(state);
    }
}

class OrCondition : ConditionBase
{
    ICondition a;
    ICondition b;

    public OrCondition(ICondition a, ICondition b)
    {
        this.a = a;
        this.b = b;
    }

    public override bool Check(GameState state)
    {
        return a.Check(state) || b.Check(state);
    }
}

class NotCondition : ConditionBase
{
    ICondition cond;

    public NotCondition(ICondition c)
    {
        cond = c;
    }

    public override bool Check(GameState state)
    {
        return !cond.Check(state);
    }
}

class DamageEffect : EffectBase
{
    public int damage;

    public DamageEffect(int dmg)
    {
        damage = dmg;
    }

    public override void Apply(GameState state)
    {
        state.health = state.health - damage;
        Console.WriteLine("Ярик получил урон!");
    }
}

class HealEffect : EffectBase
{
    int heal;

    public HealEffect(int h)
    {
        heal = h;
    }

    public override void Apply(GameState state)
    {
        state.health += heal;
        Console.WriteLine("Ярик восстановил здоровье");
    }
}

class AddItemEffect : EffectBase
{
    string item;

    public AddItemEffect(string item)
    {
        this.item = item;
    }

    public override void Apply(GameState state)
    {
        state.inventory.Add(item);
        Console.WriteLine("Ярик нашел " + item);
    }
}

class RemoveItemEffect : EffectBase
{
    string item;

    public RemoveItemEffect(string item)
    {
        this.item = item;
    }

    public override void Apply(GameState state)
    {
        state.inventory.Remove(item);
        Console.WriteLine(item + " использован");
    }
}

class LogEffect : EffectBase
{
    string message;

    public LogEffect(string msg)
    {
        message = msg;
    }

    public override void Apply(GameState state)
    {
        Console.WriteLine(message);
    }
}

class Chest : IInteractable
{
    public string Id { get; private set; }
    string item;

    public Chest(string id, string item)
    {
        Id = id;
        this.item = item;
    }

    public void Interact(GameState state)
    {
        Console.WriteLine("Ярик открыл сундук");
        state.inventory.Add(item);
        Console.WriteLine("Внутри была " + item);
    }
}

class Door : IInteractable
{
    public string Id { get; private set; }
    string key;

    public Door(string id, string key)
    {
        Id = id;
        this.key = key;
    }

    public void Interact(GameState state)
    {
        if (state.inventory.Contains(key))
        {
            Console.WriteLine("Ярик открыл дверь");
        }
        else
        {
            Console.WriteLine("Нужен ключ");
        }
    }
}

class NPC : IInteractable
{
    public string Id { get; private set; }

    public NPC(string id)
    {
        Id = id;
    }

    public void Interact(GameState state)
    {
        Console.WriteLine("Персонаж спит... лучше не шуметь");
    }
}

class Trap : IInteractable
{
    public string Id { get; private set; }
    int damage;

    public Trap(string id, int dmg)
    {
        Id = id;
        damage = dmg;
    }

    public void Interact(GameState state)
    {
        state.health -= damage;
        Console.WriteLine("Ярик наступил на ловушку!");
    }
}

class Program
{
    static void Main(string[] args)
    {
        GameState state = new GameState();

        ICommand look = new LookCommand();
        ICommand help = new HelpCommand();
        ICommand inv = new InventoryCommand();
        ICommand status = new StatusCommand();

        look.Execute(state);

        IEffect dmg = new DamageEffect(10);
        dmg.Apply(state);

        IEffect add = new AddItemEffect("pizza");
        add.Apply(state);

        inv.Execute(state);

        status.Execute(state);

        IInteractable chest = new Chest("chest1", "burger");
        chest.Interact(state);

        status.Execute(state);

        Console.ReadLine();
    }
}