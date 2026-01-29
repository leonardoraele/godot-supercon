using Godot;

namespace Raele.Supercon.StateComponents2D;

[Tool][GlobalClass][Icon($"res://addons/{nameof(Supercon)}/icons/character_body_gravity.png")]
public partial class GravityComponent : SuperconStateComponent2D
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export(PropertyHint.None, "suffix:px/s")] public float MaxFallSpeed = float.PositiveInfinity;
	[Export] public float GravityMultiplier = 5f;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public Vector2 GravityDirection;
	public float GravityMagnitudePxPSecSq;

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------------------------------------
	// VIRTUALS & OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _Ready()
	{
		base._Ready();
		this.GravityDirection = ProjectSettings.GetSetting("physics/2d/default_gravity_vector").AsVector2().Normalized();
		this.GravityMagnitudePxPSecSq = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	}

	protected override void _ActivityPhysicsProcess(double delta)
	{
		base._ActivityPhysicsProcess(delta);
		this.Character?.ApplyForce(
			this.GravityDirection * this.GravityMagnitudePxPSecSq * (float) delta * this.GravityMultiplier,
			this.MaxFallSpeed
		);
	}
}
