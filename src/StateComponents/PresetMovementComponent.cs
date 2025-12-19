using System;
using Godot;

namespace Raele.Supercon2D.StateComponents;

public partial class PresetMovementComponent : SuperconStateComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	// TODO Implement handles to edit bezier curve movement in the editor, and draw the curve preview using _Draw().
	[Export] public MovementTypeEnum MovementType = MovementTypeEnum.Straight;
	[Export(PropertyHint.None, "suffix:px")] public Vector2 Destination;
	// [Export] public Vector2 HandleA; // TODO
	// [Export] public Vector2 HandleB; // TODO

	/// <summary>
	/// If true, the direction is mirrored horizontally when the character is facing left.
	/// </summary>
	[Export] public bool UseFacing = false;

	/// <summary>
	/// Determines the time it takes for the character to reach the jump's apex, in miliseconds. If IsCancelable is
	/// true, the player can cancel the jump earlier by releasing the jump button. In this case, the character might not
	/// reach this height.
	/// </summary>
	[Export(PropertyHint.None, "suffix:ms")] public float DurationMs = 400f;

	/// <summary>
	/// By default, the controller uses a simple sine-based easing function to calculate the character's jump height
	/// every frame and applying it to the JumpHeightPx property. Use this property to customize the jump height curve.
	/// The curve's X axis represents the jump progress, and the Y axis represents the jump height. The curve's Y axis
	/// values are relative to the JumpHeightPx property, while the curve's X axis values are relative to the
	/// JumpDurationMs property.
	/// </summary>
	[Export] public Curve? Curve;

	[ExportGroup("State Transitions")]
	/// <summary>
	/// If set, the controller will transition to the specified state when the jump ends (i.e., when the character
	/// reaches the apex or finishes the ascent).
	/// </summary>
	[Export] public SuperconState? TransitionOnMoveComplete;
	[Export] public SuperconState? TransitionOnCollision;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private Vector2 InternalVelocity;

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public TimeSpan Duration => TimeSpan.FromMilliseconds(this.DurationMs);

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void MovementCompletedEventHandler();
	[Signal] public delegate void MovementInterruptedEventHandler(KinematicCollision2D collision);

	// -----------------------------------------------------------------------------------------------------------------
	// LOCAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum MovementTypeEnum
	{
		Straight,
		// QuadraticBezier, // TODO
		// CubicBezier, // TODO
	}

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _SuperconStart()
	{
		base._SuperconStart();
		this.InternalVelocity = Vector2.Zero;
		this.SetPhysicsProcess(true);
	}

	public override void _SuperconPhysicsProcess(double delta)
	{
		base._SuperconPhysicsProcess(delta);

		// Remove any previously applied velocity.
		this.Character.Velocity -= this.InternalVelocity;

		// Handles instant movement when the duration is zero and prevents division by zero.
		if (Mathf.IsZeroApprox(this.DurationMs))
		{
			this.Character.MoveAndCollide(this.Destination);
			this.SetPhysicsProcess(false);
			this.EmitSignalMovementCompleted();
			return;
		}

		// Handles movement interruption on collision.
		if (
			!this.InternalVelocity.IsEqualApprox(Vector2.Zero)
			&& this.Character.GetLastSlideCollision() is KinematicCollision2D collision
			&& this.TransitionOnCollision != null
		)
		{
			this.Character.QueueTransition(this.TransitionOnCollision);
			this.SetPhysicsProcess(false);
			this.EmitSignalMovementInterrupted(collision);
			return;
		}

		// Handles ending the movement when the duration is exceeded.
		if (this.State.ActiveDuration >= this.Duration)
		{
			if (this.TransitionOnMoveComplete != null)
			{
				this.Character.QueueTransition(this.TransitionOnMoveComplete);
			}
			this.SetPhysicsProcess(false);
			this.EmitSignalMovementCompleted();
			return;
		}

		TimeSpan thisFrameActiveDuration = this.State.ActiveDuration;
		TimeSpan lastFrameActiveDuration = this.State.ActiveDuration.Subtract(TimeSpan.FromSeconds(delta));

		// TODO We could precalculate the jump height curve so that we don't need to read the curve twice every frame.
		// TODO We could read this.Character.GetPositionDelta and accumulate the movement instead of recalculing the
		// previous frame every time.
		Vector2 thisFramePosition = this.CalculateExpectedPosition(thisFrameActiveDuration / this.Duration);
		Vector2 lastFramePosition = this.CalculateExpectedPosition(lastFrameActiveDuration / this.Duration);
		this.Character.Velocity += this.InternalVelocity = (thisFramePosition - lastFramePosition) / (float) delta;
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private Vector2 CalculateExpectedPosition(double progress)
	{
		progress = Mathf.Clamp(progress, 0, 1);
		double distanceProgress = this.Curve?.SampleBaked((float) progress) ?? Math.Sin(progress * Math.PI / 2);
		return this.SamplePath(distanceProgress);
	}

	private Vector2 SamplePath(double distanceProgress)
	{
		return this.Destination * (float) distanceProgress
			* (this.UseFacing ? new Vector2(this.Character.HorizontalFacingDirection, 1) : Vector2.One);
	}
}
