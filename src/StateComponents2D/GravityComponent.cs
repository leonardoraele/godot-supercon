using Godot;

namespace Raele.Supercon2D.StateComponents2D;

[Tool][GlobalClass][Icon($"res://addons/{nameof(Supercon2D)}/icons/character_body_gravity.png")]
public partial class GravityComponent : SuperconStateComponent
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

	public override void _SuperconPhysicsProcess(double delta)
	{
		base._SuperconPhysicsProcess(delta);
		this.Character?.ApplyForce(
			this.GravityDirection * this.GravityMagnitudePxPSecSq * (float) delta * this.GravityMultiplier,
			this.MaxFallSpeed
		);
	}
}
