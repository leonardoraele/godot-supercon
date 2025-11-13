using Godot;

namespace Raele.Supercon2D.StateControllers;

public partial class WallClimbController : StateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public float MaxUpwardSpeedPxPSec = 100f;
	[Export] public float UpwardAccelerationPxPSecSqr = 300f;
	[Export] public float MaxDownwardSpeedPxPSec = 100f;
	[Export] public float DownwardAccelerationPxPSecSqr = 300f;
	[Export] public float DecelerationPxPSecSqr = 600f;

	// -----------------------------------------------------------------------------------------------------------------
	// PUBLIC METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _PhysicsProcessActive(double delta)
	{
		base._PhysicsProcessActive(delta);
		float targetVelocityY = this.InputManager.MovementInput.Y
			* (
				this.InputManager.MovementInput.Y > 0 ? this.MaxDownwardSpeedPxPSec
					: this.InputManager.MovementInput.Y < 0 ? this.MaxUpwardSpeedPxPSec
					: 0
			);
		double accelerationY = targetVelocityY == 0
			|| 0 < targetVelocityY && targetVelocityY < this.Character.VelocityY
			|| this.Character.VelocityY < targetVelocityY && targetVelocityY < 0
				? this.DecelerationPxPSecSqr * delta
			: targetVelocityY < 0 ? this.UpwardAccelerationPxPSecSqr * delta
			: this.DownwardAccelerationPxPSecSqr * delta;
		this.Character.AccelerateY(targetVelocityY, (float) accelerationY);
	}
}
