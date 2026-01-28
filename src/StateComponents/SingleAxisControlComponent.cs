using System;
using Godot;

namespace Raele.Supercon2D.StateComponents;

[GlobalClass][Icon($"res://addons/{nameof(Supercon2D)}/icons/character_body_single_axis_control.png")]
public partial class SingleAxisControlComponent : SuperconStateComponent
{
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
	// COMPUTED PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	private float AxisInput => this.Character?.InputMapping == null ? 0
		: this.Axis == AxisEnum.Horizontal ? this.Character.InputMapping.MovementInput.X
		: this.Axis == AxisEnum.Vertical ? this.Character.InputMapping.MovementInput.Y
		: 0f;
	private float AxisVelocity
	{
		get => this.Character == null ? 0f
			: this.Axis == AxisEnum.Horizontal ? this.Character.Velocity.X
			: this.Axis == AxisEnum.Vertical ? this.Character.Velocity.Y
			: 0f;
	}

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------------------------------------
	// LOCAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum AxisEnum
	{
		Horizontal,
		Vertical,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _SuperconPhysicsProcess(double delta)
	{
		base._SuperconPhysicsProcess(delta);

		if (this.FaceMovingDirection && Math.Abs(this.AxisVelocity) >= this.FaceMinSpeed)
		{
			this.Character?.FacingDirection = this.Axis == AxisEnum.Horizontal
				? Vector2.Right * Math.Sign(this.AxisVelocity)
				: Vector2.Up * Math.Sign(this.AxisVelocity);
		}

		float targetVelocity = this.AxisInput * this.MaxSpeedPxPSec;
		float acceleration =
			Math.Abs(targetVelocity) < Mathf.Epsilon ? this.SoftDecelerationPxPSecSqr
			: this.AxisVelocity > 0 && targetVelocity < this.AxisVelocity
			|| this.AxisVelocity < 0 && targetVelocity > this.AxisVelocity
				? this.HardDecelerationPxPSecSqr
			: this.AccelerationPxPSecSqr;
		this.AccelerateAxis(targetVelocity, acceleration, (float) delta);
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void AccelerateAxis(float targetVelocity, float acceleration, float delta)
	{
		if (this.Axis == AxisEnum.Horizontal)
		{
			this.Character?.AccelerateX(targetVelocity, acceleration * delta);
		}
		else if (this.Axis == AxisEnum.Vertical)
		{
			this.Character?.AccelerateY(targetVelocity, acceleration * delta);
		}
	}
}
