using Godot;
using System;

public partial class GameManager : Node3D
{
  [Export] HUD HeadsUpDisplay;
  [Export] PlayerCharacterController PlayerCharacter;
  [Export] IOpponent TestGoon;
  [Export] OpponentBuffer OpponentBuffer;

  // character pool

  public override void _Ready()
  {
    PlayerCharacter.OnPlayerHealthUpdate += OnPlayerHealthUpdated;
    PlayerCharacter.OnPlayerMaxHealthUpdate += OnPlayerMaxHealthUpdated;
  }

  public override void _PhysicsProcess(double delta)
  {
    base._PhysicsProcess(delta);
    OpponentBuffer.UpdateTargetPosition(PlayerCharacter.Position);
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
