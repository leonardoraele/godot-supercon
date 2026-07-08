using Godot;

namespace Raele.Supercon.StateComponents3D;

public partial class GravityComponent3D : SuperconStateComponent3D
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

	protected override void _ActivityPhysicsProcessActive(double delta)
	{
		base._ActivityPhysicsProcessActive(delta);
		this.Character3D?.ApplyForceAndLimitSpeed(
			this.Character3D.GetGravity() * this.Mass * (float) delta,
			this.MaxFallSpeed
		);
	}
}
