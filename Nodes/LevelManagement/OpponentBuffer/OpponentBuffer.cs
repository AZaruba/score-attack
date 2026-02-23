using Godot;
using Godot.Collections;


public partial class OpponentBuffer : Node
{
  [Export] Array<IOpponent> Opponents;

  public override void _Ready()
  {
    foreach(IOpponent opponent in Opponents)
    {
      opponent.OnHealthZero += OnOpponentHealthZero;
    }
  }

  public void UpdateTargetPosition(Vector3 PositionIn)
  {
    foreach(IOpponent opponent in Opponents)
    {
      opponent.UpdateTargetPosition(PositionIn);
    }
  }

  private void OnOpponentHealthZero(IOpponent opRef)
  {
    Opponents.Remove(opRef);
    opRef.QueueFree();
  }
}
