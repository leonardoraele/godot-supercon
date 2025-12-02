using System;
using Godot;

namespace Raele.Supercon2D.StateComponents;

public partial class SingleAxisControlComponent : SuperconStateController
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
	[Export] public float SoftDecelerationPxPSecSqr = 650f; // Rename to DampPxPSecSq?
	// [Export] public float DampMultiplierPFrame = 1f;
	[Export] public float HardDecelerationPxPSecSqr = 2000f; // Rename to BrakePxPSecSq?

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	private float AxisInput => this.Axis == AxisEnum.Horizontal ? this.InputMapping.MovementInput.X
		: this.Axis == AxisEnum.Vertical ? this.InputMapping.MovementInput.Y
		: 0f;
	private float AxisVelocity
	{
		get => this.Axis == AxisEnum.Horizontal ? this.Character.Velocity.X
			: this.Axis == AxisEnum.Vertical ? this.Character.Velocity.Y
			: 0f;
		// set
		// {
		// 	if (this.Axis == AxisEnum.Horizontal)
		// 	{
		// 		this.Character.VelocityX = value;
		// 	}
		// 	else if (this.Axis == AxisEnum.Vertical)
		// 	{
		// 		this.Character.VelocityY = value;
		// 	}
		// }
	}

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------------------------------------
	// LIFECYCLE METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _PhysicsProcessActive(double delta)
	{
		base._PhysicsProcessActive(delta);
		float targetVelocity = this.AxisInput * this.MaxSpeedPxPSec;
		float acceleration =
			Math.Abs(targetVelocity) < float.Epsilon ? this.SoftDecelerationPxPSecSqr
			: this.AxisVelocity > 0 && targetVelocity < this.AxisVelocity
			|| this.AxisVelocity < 0 && targetVelocity > this.AxisVelocity
				? this.HardDecelerationPxPSecSqr
			: this.AccelerationPxPSecSqr;
		this.AccelerateAxis(targetVelocity, acceleration, (float) delta);
		// if (Math.Abs(targetVelocity) < float.Epsilon)
		// {
		// 	this.Character.AxisVelocity *= this.DampMultiplierPFrame;
		// }
	}

	private void AccelerateAxis(float targetVelocity, float acceleration, float delta)
	{
		if (this.Axis == AxisEnum.Horizontal)
		{
			this.Character.AccelerateX(targetVelocity, acceleration * delta);
		}
		else if (this.Axis == AxisEnum.Vertical)
		{
			this.Character.AccelerateY(targetVelocity, acceleration * delta);
		}
	}
}
