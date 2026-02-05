using Godot;
using System;

public partial class HUD : CanvasLayer
{
  [Export] ResourceBar HealthBar;
  [Export] ResourceBar AltBar;

  public void UpdateHealth(float newHealth)
  {
    HealthBar.SetResourceValue(newHealth);
  }

  public void UpdateMaxHealth(float newMax)
  {
    HealthBar.SetResourceMax(newMax);
  }
}
