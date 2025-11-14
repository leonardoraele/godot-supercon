using System;
using Godot;

namespace Raele.Supercon2D.StateControllers;

public partial class JumpComponent : SuperconStateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Determines the upmost height the character is able to reach, in pixels.
	/// </summary>
	[Export] public float JumpApexHeightPx = 200f;

	/// <summary>
	/// Determines the time it takes for the character to reach the jump's apex, in miliseconds. If IsCancelable is
	/// true, the player can cancel the jump earlier by releasing the jump button. In this case, the character might not
	/// reach this height.
	/// </summary>
	[Export] public float AscentDurationMs = 400f;

	/// <summary>
	/// By default, the controller uses a simple sine-based easing function to calculate the character's jump height
	/// every frame and applying it to the JumpHeightPx property. Use this property to customize the jump height curve.
	/// The curve's X axis represents the jump progress, and the Y axis represents the jump height. The curve's Y axis
	/// values are relative to the JumpHeightPx property, while the curve's X axis values are relative to the
	/// JumpDurationMs property.
	/// </summary>
	[Export] public Curve? AscentCurve;

	[ExportGroup("Jump End Options")]
	/// <summary>
	/// If set, the controller will transition to the specified state when the jump ends (i.e., when the character
	/// reaches the apex or finishes the ascent).
	/// </summary>
	[Export] public SuperconState? TransitionAtJumpEnd;
	/// <summary>
	/// The jump will end early if the character collides with a ceiling and remains colliding for at least this many
	/// miliseconds. If set to 0, the jump will end immediately upon colliding with a ceiling.
	/// </summary>
	[Export] public float EndJumpOnCeilingThresholdMs = 150f;

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public TimeSpan AscentDuration => TimeSpan.FromMilliseconds(this.AscentDurationMs);
	public TimeSpan EndJumpOnCeilingThreshold => TimeSpan.FromMilliseconds(this.EndJumpOnCeilingThresholdMs);

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _ProcessActive(double delta)
	{
		base._ProcessActive(delta);
		if (
			this.State.ActiveDuration >= this.AscentDuration
			|| this.Character.TimeOnCeiling > this.EndJumpOnCeilingThreshold
		)
		{
			if (this.TransitionAtJumpEnd != null)
			{
				this.StateMachine.QueueTransition(this.TransitionAtJumpEnd);
			}
			else
			{
				this.StateMachine.Reset();
			}
		}
	}

	public override void _PhysicsProcessActive(double delta)
	{
		base._PhysicsProcessActive(delta);
		// TODO We could precalculate the jump height curve so that we don't need to read the curve twice every frame.
		double currentAscentDurationProgress = this.State.ActiveDuration.TotalMilliseconds / this.AscentDurationMs;
		double currentApexHeightProgress = this.AscentCurve?.Sample((float)currentAscentDurationProgress)
			?? Math.Sin(currentAscentDurationProgress * Math.PI / 2);
		double previousAscentDurationProgress = Math.Max(0, (this.State.ActiveDuration.TotalMilliseconds - delta * 1000) / this.AscentDurationMs);
		double previousApexHeightProgress = this.AscentCurve?.Sample((float)previousAscentDurationProgress)
			?? Math.Sin(previousAscentDurationProgress * Math.PI / 2);
		double heightDiffPx = this.JumpApexHeightPx * (currentApexHeightProgress - previousApexHeightProgress);
		this.Character.VelocityY = (float)(heightDiffPx / delta * Vector2.Up.Y);
	}
}
