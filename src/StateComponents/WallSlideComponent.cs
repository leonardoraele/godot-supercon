using Godot;

namespace Raele.Supercon2D.StateControllers;

public partial class WallSlideComponent : SuperconStateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public float MaxDownwardSpeedPxPSec = 100f;
	[Export] public float AccelerationPxPSecSqr = 300f;

	// -----------------------------------------------------------------------------------------------------------------
	// PUBLIC METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _PhysicsProcessActive(double delta)
	{
		base._PhysicsProcessActive(delta);
		this.Character.AccelerateY(this.MaxDownwardSpeedPxPSec, this.AccelerationPxPSecSqr * (float) delta);
	}
}
