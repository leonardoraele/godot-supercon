using Godot;

namespace Raele.Supercon.StateComponents3D;

public partial class GravityComponent : SuperconStateComponent3D
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
		this.Character?.ApplyForceAndLimitSpeed(
			this.Character.GetGravity() * this.Mass * (float) delta,
			this.MaxFallSpeed
		);
	}
}
