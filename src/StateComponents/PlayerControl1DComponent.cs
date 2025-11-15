using System;
using Godot;

namespace Raele.Supercon2D.StateComponents;

public partial class PlayerControl1DComponent : SuperconStateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// LOCAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum AxisEnum
	{
		Horizontal,
		Vertical,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public AxisEnum Axis = AxisEnum.Horizontal;
	[Export] public float MaxSpeedPxPSec = 200f;
	[Export] public float AccelerationPxPSecSqr = 400f;
	[Export] public float SoftDecelerationPxPSecSqr = 400f;
	[Export] public float HardDecelerationPxPSecSqr = 1000f;
	[Export] public float DampingPxPSecSqr = 0f;

	// -----------------------------------------------------------------------------------------------------------------
	// LIFECYCLE METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _PhysicsProcessActive(double delta)
	{
		base._PhysicsProcessActive(delta);
		switch (this.Axis)
		{
			case AxisEnum.Horizontal:
				this.ProcessHorizontalMovement((float) delta);
				break;
			case AxisEnum.Vertical:
				this.ProcessVerticalMovement((float) delta);
				break;
		}
	}

	private void ProcessHorizontalMovement(float delta)
	{
		float targetVelocityX = this.InputManager.MovementInput.X * this.MaxSpeedPxPSec;
		float accelerationX = Math.Abs(targetVelocityX) < float.Epsilon
			|| 0 < targetVelocityX && targetVelocityX < this.Character.VelocityX
			|| this.Character.VelocityX < targetVelocityX && targetVelocityX < 0
				? this.HardDecelerationPxPSecSqr
			: this.AccelerationPxPSecSqr;
		this.Character.AccelerateX(targetVelocityX, accelerationX * (float) delta);
	}

	private void ProcessVerticalMovement(float delta)
	{
		float targetVelocityY = this.InputManager.MovementInput.Y * this.MaxSpeedPxPSec;
		float accelerationY = Math.Abs(targetVelocityY) < float.Epsilon
			|| 0 < targetVelocityY && targetVelocityY < this.Character.VelocityY
			|| this.Character.VelocityY < targetVelocityY && targetVelocityY < 0
				? this.HardDecelerationPxPSecSqr
			: this.AccelerationPxPSecSqr;
		this.Character.AccelerateY(targetVelocityY, accelerationY * (float) delta);
	}
}
