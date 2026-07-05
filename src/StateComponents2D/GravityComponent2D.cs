using Godot;

namespace Raele.Supercon.StateComponents2D;

[Tool][GlobalClass][Icon($"res://addons/{nameof(Supercon)}/icons/character_body_gravity.png")]
public partial class GravityComponent2D : SuperconStateComponent2D
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export(PropertyHint.None, "suffix:px/s")] public float MaxFallSpeed = float.PositiveInfinity;
	[Export] public float Mass = 1f;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------------------------------------
	// VIRTUALS & OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	protected override void _ActivityPhysicsProcess(double delta)
	{
		base._ActivityPhysicsProcess(delta);
		this.Character2D?.ApplyForce(
			this.Character2D.GetGravity() * this.Mass * (float) delta,
			this.MaxFallSpeed
		);
	}
}
