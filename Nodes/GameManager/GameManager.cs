using Godot;
using System;

public partial class GameManager : Node3D
{
  [Export] HUD HeadsUpDisplay;
  [Export] PlayerCharacterController PlayerCharacter;

  public override void _Ready()
  {
    PlayerCharacter.OnPlayerHealthUpdate += OnPlayerHealthUpdated;
    PlayerCharacter.OnPlayerMaxHealthUpdate += OnPlayerMaxHealthUpdated;
  }

  public override void _ExitTree()
  {
    PlayerCharacter.OnPlayerHealthUpdate -= OnPlayerHealthUpdated;
    PlayerCharacter.OnPlayerMaxHealthUpdate -= OnPlayerMaxHealthUpdated;
  }

  private void OnPlayerHealthUpdated(float newHealth)
  {
    HeadsUpDisplay.UpdateHealth(newHealth);
  }
  private void OnPlayerMaxHealthUpdated(float newHealth)
  {
    HeadsUpDisplay.UpdateMaxHealth(newHealth);
  }
}
