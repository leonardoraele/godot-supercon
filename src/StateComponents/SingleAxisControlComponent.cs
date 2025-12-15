using System;
using Godot;

namespace Raele.Supercon2D.StateComponents;

public partial class SingleAxisControlComponent : SuperconStateComponent
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
	[Export(PropertyHint.None, "suffix:px/s")] public float MaxSpeedPxPSec = 200f;
	[Export(PropertyHint.None, "suffix:px/s²")] public float AccelerationPxPSecSqr = 400f;
	[Export(PropertyHint.None, "suffix:px/s²")] public float SoftDecelerationPxPSecSqr = 650f; // Rename to DampPxPSecSq?
	// [Export] public float DampMultiplierPFrame = 1f;
	[Export(PropertyHint.None, "suffix:px/s²")] public float HardDecelerationPxPSecSqr = 2000f; // Rename to BrakePxPSecSq?

	[ExportGroup("Facing")]
	[Export(PropertyHint.GroupEnable)] public bool FaceMovingDirection = true;
	[Export(PropertyHint.None, "suffix:px/s")] public float FaceMinSpeed = 10f;

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

	public override void _SuperconProcess(double delta)
	{
		base._SuperconProcess(delta);
		if (this.FaceMovingDirection && Math.Abs(this.AxisVelocity) >= this.FaceMinSpeed)
		{
			this.Character.FacingDirection = this.Axis == AxisEnum.Horizontal
				? Vector2.Right * Math.Sign(this.AxisVelocity)
				: Vector2.Up * Math.Sign(this.AxisVelocity);
		}
	}

	public override void _SuperconPhysicsProcess(double delta)
	{
		base._SuperconPhysicsProcess(delta);
		float targetVelocity = this.AxisInput * this.MaxSpeedPxPSec;
		float acceleration =
			Math.Abs(targetVelocity) < Mathf.Epsilon ? this.SoftDecelerationPxPSecSqr
			: this.AxisVelocity > 0 && targetVelocity < this.AxisVelocity
			|| this.AxisVelocity < 0 && targetVelocity > this.AxisVelocity
				? this.HardDecelerationPxPSecSqr
			: this.AccelerationPxPSecSqr;
		this.AccelerateAxis(targetVelocity, acceleration, (float) delta);
		// if (Math.Abs(targetVelocity) < Mathf.Epsilon)
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
