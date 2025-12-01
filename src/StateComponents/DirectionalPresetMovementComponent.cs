using System;
using Godot;

namespace Raele.Supercon2D.StateComponents;

public partial class DirectionalPresetMovementComponent : SuperconStateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public Vector2 Direction = Vector2.Down;

	/// <summary>
	/// If true, the direction is mirrored horizontally when the character is facing left.
	/// </summary>
	[Export] public bool UseFacing = false;

	/// <summary>
	/// Determines the upmost height the character is able to reach, in pixels.
	/// </summary>
	[Export] public float DistancePx = 200f;

	/// <summary>
	/// Determines the time it takes for the character to reach the jump's apex, in miliseconds. If IsCancelable is
	/// true, the player can cancel the jump earlier by releasing the jump button. In this case, the character might not
	/// reach this height.
	/// </summary>
	[Export] public float DurationMs = 400f;

	/// <summary>
	/// By default, the controller uses a simple sine-based easing function to calculate the character's jump height
	/// every frame and applying it to the JumpHeightPx property. Use this property to customize the jump height curve.
	/// The curve's X axis represents the jump progress, and the Y axis represents the jump height. The curve's Y axis
	/// values are relative to the JumpHeightPx property, while the curve's X axis values are relative to the
	/// JumpDurationMs property.
	/// </summary>
	[Export] public Curve? Curve;

	/// <summary>
	/// If set, the controller will transition to the specified state when the jump ends (i.e., when the character
	/// reaches the apex or finishes the ascent).
	/// </summary>
	[Export] public SuperconState? TransitionOnEnd;

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public TimeSpan Duration => TimeSpan.FromMilliseconds(this.DurationMs);
	public Vector2 ResolvedDirection => this.Direction * (this.UseFacing ? new Vector2(this.Character.FacingDirection, 1) : Vector2.One);

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _ProcessActive(double delta)
	{
		base._ProcessActive(delta);
		if (this.State.ActiveDuration >= this.Duration)
		{
			if (this.TransitionOnEnd != null)
			{
				this.StateMachine.QueueTransition(this.TransitionOnEnd);
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
		double thisFrameDurationProgress = this.State.ActiveDuration.TotalMilliseconds / this.DurationMs;
		double thisFrameDistanceProgress = this.Curve?.Sample((float) thisFrameDurationProgress)
			?? Math.Sin(thisFrameDurationProgress * Math.PI / 2);
		double prevFrameDurationProgress = Math.Max(0, (this.State.ActiveDuration.TotalMilliseconds - delta * 1000) / this.DurationMs);
		double prevFrameDistanceProgress = this.Curve?.Sample((float) prevFrameDurationProgress)
			?? Math.Sin(prevFrameDurationProgress * Math.PI / 2);
		double distanceDiffPx = this.DistancePx * (thisFrameDistanceProgress - prevFrameDistanceProgress);
		this.Character.Velocity = this.ResolvedDirection * (float) (distanceDiffPx / delta);
	}
}
