using Godot;
using Godot.Collections;

public partial class HealthComponent : Node
{
  [Signal] public delegate void OnHealEventHandler(float newValue);
  [Signal] public delegate void OnDamageEventHandler(float newValue);
  [Signal] public delegate void OnHealthZeroEventHandler();

  private float MaxHealth;
  private float CurrentHealth;

  //private Array<Vector2> CurrentDOTs;

  public override void _Ready()
  {
    //CurrentDOTs = [];
    MaxHealth = 1;
    CurrentHealth = 1;
  }

  public override void _PhysicsProcess(double delta)
  {
    // for(int i = 0; i < CurrentDOTs.Count; i++)
    // {
    //   Vector2 DOT = CurrentDOTs[i];
    //   Damage(DOT.X);
    //   DOT.Y -= (float)delta;
    //   CurrentDOTs[i] = DOT;
    //   if (DOT.Y < 0)
    //   {
    //     // oops
    //     CurrentDOTs.RemoveAt(i);
    //     i--;
    //   }
    // }
  }

  public void InitHealth(float MaxHealth)
  {
    this.MaxHealth = MaxHealth;
    CurrentHealth = MaxHealth;
  }

  public void Heal(float Potency)
  {
    CurrentHealth += Potency;
    CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
    EmitSignal(SignalName.OnHeal, CurrentHealth);
  }

  public void Damage(float Potency)
  {
    CurrentHealth -= Potency;
    CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
    EmitSignal(SignalName.OnDamage, CurrentHealth);
    if (CurrentHealth <= 0)
    {
      EmitSignal(SignalName.OnHealthZero);
    }
  }

  public float GetMaxHealth()
  {
    return MaxHealth;
  }

  public float GetCurrentHealth()
  {
    return CurrentHealth;
  }
}
