using Godot;
using System;

public partial class WorldResourceBar : Node3D
{
  [Export] ShaderMaterial ResourceMat;

  public override void _Ready()
  {
    //ResourceMat = (ShaderMaterial)Material;
  }

  public void SetResourceValue(float newValue)
  {
    GD.Print(newValue);
    ResourceMat.SetShaderParameter("ResourceValue", newValue);
  }

  public void SetResourceMax(float newMax)
  {
    ResourceMat.SetShaderParameter("ResourceMax", newMax);
  }

  public void RevealHealthBar()
  {
    Visible = true;
  }
}

