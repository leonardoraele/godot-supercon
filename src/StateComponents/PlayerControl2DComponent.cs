using Godot;

namespace Raele.Supercon2D.StateComponents;

public partial class PlayerControl2DComponent : SuperconStateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public float MaxSpeedPxPSec = 200f;
	[Export] public float AccelerationPxPSecSqr = 400f;
	[Export] public float DecelerationPxPSecSqr = 800f;

	// -----------------------------------------------------------------------------------------------------------------
	// LIFECYCLE METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _PhysicsProcessActive(double delta)
	{
		base._PhysicsProcessActive(delta);
		float currentVelocityPxPSec = this.Character.Velocity.Length();
		float targetVelocityPxPSec = this.InputManager.MovementInput.Length() * this.MaxSpeedPxPSec;
		float accelerationPxPSecSqr = targetVelocityPxPSec > currentVelocityPxPSec
			? this.AccelerationPxPSecSqr
			: this.DecelerationPxPSecSqr;
		float newVelocity = Mathf.MoveToward(
			currentVelocityPxPSec,
			targetVelocityPxPSec,
			accelerationPxPSecSqr * (float) delta
		);
		this.Character.Velocity = this.InputManager.MovementInput.Normalized() * newVelocity;
	}
}
