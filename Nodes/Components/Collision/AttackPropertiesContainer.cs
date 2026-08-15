using Godot;

public partial class AttackPropertiesContainer : Area3D
{
	[Export] float AttackPotency;
	[Export] float LauncherForce;
	[Export] bool Launcher;
	[Export] float KnockbackForce;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AttackPotency = 0;
		LauncherForce = 0;
		KnockbackForce = 0;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public float GetLauncherForce() { return LauncherForce; }
	public float GetKnockbackForce() { return KnockbackForce; }
	public float GetAttackPotency() { return AttackPotency; }
	public bool IsLauncher() { return Launcher; }
}
