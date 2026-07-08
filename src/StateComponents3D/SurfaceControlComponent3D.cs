using Godot;
using Godot.Collections;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon.StateComponents3D;

/// <summary>
/// Allows the player to control the character using directional input to move it along a surface. e.g. To move the
/// character over the floor or climbing a wall.
///
/// You can set different speed values for forward, lateral, and backward movement.
///
/// This component does not not update the character's rotation direction. For that, use the FacingComponent.
/// </summary>
[Tool][GlobalClass]
public partial class SurfaceControlComponent3D : SuperconStateComponent3D
{
	//==================================================================================================================
		#region STATICS
	//==================================================================================================================

	// public static readonly string MyConstant = "";

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region EXPORTS
	//==================================================================================================================

	[Export] public SurfaceTypeEnum Surface = SurfaceTypeEnum.Floor;
	[Export(PropertyHint.None, "suffix:m/s")] public float MaxSpeed
		{ get; set { if (value != field) field = /*this.MaxForwardSpeed =*/ value.Clamped(0f, value); } }
		= 5f;
	[Export(PropertyHint.None, "suffix:m/s²")] public float Acceleration
		{ get; set => field = value.Clamped(0f, value); }
		= 10f;
	[Export(PropertyHint.None, "suffix:m/s²")] public float Deceleration
		{ get; set { if (value != field) field = /*this.DragDeceleration =*/ value.Clamped(0f, value); } }
		= 15f;
	[Export] public bool PreserveOrthogonalVelocity = true; // TODO Is this option really needed? Is there a use case to disable it?

	[ExportGroup("Use Smooth Turning")]
	/// <summary>
	/// If true, the character will turn smoothly toward the input direction, at a rate determined by the
	/// <see cref="AngularVelocity"/> property. If false, the character will immediately turn toward the input
	/// direction at each frame.
	/// </summary>
	[Export(PropertyHint.GroupEnable)] public bool UseSmoothTurning = true;

	// TODO Implement velocity-based angular velocity, so that the character turns faster when moving faster, and slower
	// when moving slower.
	[Export(PropertyHint.Range, "0,1080,5,radians_as_degrees,or_greater,suffix:°/s")] public float AngularVelocity
	{
		get;
		set
		{
			if (value == field)
				return;
			// this.TurnMinAngularVelocity = this.TurnMinAngularVelocity.Clamped(0f, value);
			/*this.TurnMaxAngularVelocity =*/ field = value;
		}
	}
		= Mathf.Pi * 2;

	[ExportSubgroup("Require Forward Start")]
	/// <summary>
	/// If true, the character will be forced to start moving forward when it starts moving from the idle position. The
	/// character will then be able to turn toward any other direction after that, according to the
	/// <see cref="AngularVelocity"/> property. If false, the character immediately starts moving in the direction of
	/// the input.
	///
	/// This option is intented for entities that cannot change movement direction while still, only while moving, such
	/// as cars and other land vehicles. When used for humanoid characters, it might make the movement feel less
	/// responsive or harder to control, but perhaps the character animation will look more natural.
	/// </summary>
	[Export(PropertyHint.GroupEnable)] public bool RequireForwardStart = false;
	[Export] public Vector3 LocalForwardDirection = Vector3.Forward;

	[ExportGroup("Rotate Character", "Rotation")]
	[Export(PropertyHint.GroupEnable)] public bool RotationEnabled = false;
	[Export] public AlignmentOptionsEnum RotationForwardAlignment
		{ get; set { field = value; this.NotifyPropertyListChanged(); } }
		= AlignmentOptionsEnum.MovementDirection;
	[Export] public Vector3 RotationGlobalForwardDirection = Vector3.Forward;
	[Export] public Vector3 RotationLocalForwardDirection = Vector3.Forward;
	[Export] public AlignmentOptionsEnum RotationUpAlignment
		{ get; set { field = value; this.NotifyPropertyListChanged(); } }
		= AlignmentOptionsEnum.SurfaceNormal;
	[Export] public Vector3 RotationGlobalUpDirection = Vector3.Up;
	[Export] public Vector3 RotationLocalUpDirection = Vector3.Up;

	[ExportSubgroup("Limits Directions", "RotationLimit")]
	[Export(PropertyHint.GroupEnable)] public bool RotationLimitEnabled = false;
	[Export] public int RotationLimitDirections
		{ get; set { field = value.AtLeast(4); } }
		= 8;
	[Export] public bool RotationLimitAlignWithWorldAxis = false;

	// [ExportGroup("Break MaxSpeed")]
	// [Export(PropertyHint.GroupEnable)] public bool MaxSpeedOptionsEnabled
	// 	{ get; set { field = value; this.NotifyPropertyListChanged(); } }
	// 	= false;
	// [Export(PropertyHint.None, "suffix:m/s")] public float MaxForwardSpeed
	// 	{ get; set { if (value != field) field = this.MaxSpeed = value.Clamped(0f, value); } }
	// 	= 5f;
	// [Export(PropertyHint.None, "suffix:m/s")] public float MaxLateralSpeed
	// 	{ get; set { field = value.Clamped(0f, value); } }
	// 	= 5f;
	// [Export(PropertyHint.None, "suffix:m/s")] public float MaxBackwardSpeed
	// 	{ get; set { field = value.Clamped(0f, value); } }
	// 	= 5f;
	// [Export(PropertyHint.None, "suffix:m/s")] public float MaxVerticalSpeed
	// 	{ get; set { field = value.Clamped(0f, value); } }
	// 	= 5f;

	// [ExportGroup("Break Deceleration")]
	// /// <summary>
	// /// Determines the deceleration behavior when the character's speed exceeds the maximum speed.
	// ///
	// /// If this is higher than <see cref="Deceleration"/>, the character will decelerate faster when above max speed.
	// /// </summary>
	// [Export(PropertyHint.GroupEnable)] public bool DecelerationOptionsEnabled
	// 	{ get; set { field = value; this.NotifyPropertyListChanged(); } }
	// 	= false;
	// [Export(PropertyHint.None, "suffix:m/s²")] public float DragDeceleration
	// 	{ get; set { if (value != field) field = this.Deceleration = value.Clamped(0f, value); } }
	// 	= 15f;
	// [Export(PropertyHint.None, "suffix:m/s²")] public float DecelerationAboveMaxSpeed
	// 	{ get; set { field = value.Clamped(0f, value); } }
	// 	= 5f;

	// [ExportGroup("Break Angular Velocity", "Turn")]
	// [Export(PropertyHint.GroupEnable)] public bool TurnEnabled
	// 	{ get; set { field = value; this.NotifyPropertyListChanged(); } }
	// 	= false;
	// // TODO Implement dynamic range min/max using _ValidateProperty() instead of setter
	// [Export(PropertyHint.Range, "0,1080,5,radians_as_degrees,or_greater,suffix:°/s")] public float TurnMinAngularVelocity
	// 	{ get; set { field = value.Clamped(0f, this.TurnMaxAngularVelocity); } }
	// 	= Mathf.Pi / 2;
	// // TODO Implement dynamic range min/max using _ValidateProperty() instead of setter
	// [Export(PropertyHint.Range, "0,1080,5,radians_as_degrees,or_greater,suffix:°/s")] public float TurnMaxAngularVelocity
	// 	{ get; set { if (value != field) this.AngularVelocity = field = value.Clamped(this.TurnMinAngularVelocity, float.PositiveInfinity); } }
	// 	= Mathf.Pi * 2;
	// [Export(PropertyHint.None, "m/s²")] public float TurnLowerMovementSpeed
	// 	{ get; set { field = value.Clamped(0f, this.TurnUpperMovementSpeed); } }
	// 	= 0f;
	// [Export(PropertyHint.None, "m/s²")] public float TurnUpperMovementSpeed
	// 	{ get; set { field = value.Clamped(this.TurnLowerMovementSpeed, float.PositiveInfinity); } }
	// 	= 5f;
	// // [Export] public bool TurnExtrapolateBeyondMaxSpeed = false;
	// [Export(PropertyHint.ExpEasing, "attenuation")] public float TurnCurve = 1f;

	// [ExportGroup("Brake On Sharp Turns")]
	// [Export(PropertyHint.GroupEnable)] public bool SharpTurnOptionsEnabled = false;
	// [Export(PropertyHint.Range, "0,180,5,radians_as_degrees,suffix:°")] public float SoftBrakeTurnAngle = Mathf.Pi / 3;
	// [Export(PropertyHint.ExpEasing, "attenuation")] public float SoftBrakeVelocityLoss = 1f;
	// [Export(PropertyHint.Range, "0,180,5,radians_as_degrees,suffix:°")] public float HardBrakeTurnAngle = Mathf.Pi / 3;
	// [Export(PropertyHint.None, "suffix:m/s²")] public float HardBrakeDeceleration
	// 	{ get; set { field = value.Clamped(0f, value); } }
	// 	= 50f;

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region FIELDS
	//==================================================================================================================

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region COMPUTED PROPERTIES
	//==================================================================================================================

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region EVENTS & SIGNALS
	//==================================================================================================================

	[Signal] public delegate void SurfaceExitEventHandler();

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region INTERNAL TYPES
	//==================================================================================================================

	public enum SurfaceTypeEnum : sbyte {
		Floor = 16,
		Wall = 20,
		Ceiling = 24,
	}

	public enum AlignmentOptionsEnum : sbyte {
		MovementDirection = 16,
		SurfaceNormal = 32,
		CameraForward = 48,
		Gravity = 64,
		Global = 112,
		NoAlignment = 120,
	}

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region OVERRIDES & VIRTUALS
	//==================================================================================================================

	public override void _ValidateProperty(Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			// case nameof(this.RotationLocalForwardDirection):
			// 	if (this.RotationForwardAlignment == AlignmentOptionsEnum.NoAlignment)
			// 	{
			// 		this.RotationLocalForwardDirection = Vector3.Forward;
			// 		property["usage"] = (long) PropertyUsageFlags.None;
			// 	}
			// 	if (this.RotationLocalForwardDirection.IsZeroApprox())
			// 		property["error"] = "The Forward direction cannot be a zero vector.";
			// 	break;
			case nameof(this.RotationGlobalForwardDirection):
				if (this.RotationForwardAlignment != AlignmentOptionsEnum.Global)
					property["usage"] = (long) PropertyUsageFlags.None;
				if (this.RotationGlobalForwardDirection.IsZeroApprox())
					property["error"] = "The Forward direction cannot be a zero vector.";
				break;
			// case nameof(this.RotationLocalUpDirection):
			// 	if (this.RotationUpAlignment == AlignmentOptionsEnum.NoAlignment)
			// 	{
			// 		this.RotationLocalUpDirection = Vector3.Up;
			// 		property["usage"] = (long) PropertyUsageFlags.None;
			// 		break;
			// 	}
			// 	if (this.RotationLocalUpDirection.IsZeroApprox())
			// 		property["error"] = "The Up direction cannot be a zero vector.";
			// 	break;
			case nameof(this.RotationGlobalUpDirection):
				if (this.RotationUpAlignment != AlignmentOptionsEnum.Global)
					property["usage"] = (long) PropertyUsageFlags.None;
				if (this.RotationGlobalUpDirection.IsZeroApprox())
					property["error"] = "The Up direction cannot be a zero vector.";
				break;
			case nameof(this.LocalForwardDirection):
				if (this.LocalForwardDirection.IsZeroApprox())
					property["error"] = "The Forward direction cannot be a zero vector.";
				break;
		}
	}

	protected override void _ActivityPhysicsProcessActive(double delta)
	{
		base._ActivityPhysicsProcessActive(delta);
		if (this.Character3D == null)
			return;

		// Should not control the character if it is not on the surface.
		if (this.ResolveGlobalMovementPlane() is not Plane movementPlane)
		{
			if (this.TestJustExitSurface())
				this.EmitSignalSurfaceExit();
			return;
		}

		// Cannot control the character if there is no active camrea, since the character control is based on the camera
		// orientation.
		if (this.Character3D.GetViewport().GetCamera3D() is not Camera3D camera)
			return;

		Vector3 projectedVelocity = this.Character3D.Velocity.Project(movementPlane with { D = 0 });
		float currentSpeed = projectedVelocity.Length();
		bool isMoving = currentSpeed > Mathf.Epsilon;
		Vector3 currentDirection = projectedVelocity.Normalized();
		Vector2 normalizedInput = this.Character3D.InputController?.NormalizedDirectionalInput ?? Vector2.Zero;
		float inputStrength = normalizedInput.Length();
		bool hasInput = inputStrength > Mathf.Epsilon;
		Vector3 inputDirection = normalizedInput.IsZeroApprox()
			? Vector3.Zero
			: Basis.LookingAt(movementPlane.Normal * -1, camera.GlobalBasis.Up)
				* new Vector3(normalizedInput.X, normalizedInput.Y * -1, 0);
		Vector3 newDirection = hasInput
			? isMoving && this.UseSmoothTurning
					? currentDirection.RotateToward(inputDirection, this.AngularVelocity * delta)
				: !isMoving && this.RequireForwardStart
					? movementPlane.Project(this.Character3D.GlobalBasis * this.LocalForwardDirection).Normalized()
				: inputDirection
			: currentDirection;
		float targetSpeed = this.MaxSpeed * inputStrength;
		float acceleration = targetSpeed > currentSpeed - Mathf.Epsilon
			? this.Acceleration
			: this.Deceleration;
		float newSpeed = currentSpeed.MoveToward(targetSpeed, acceleration * (float) delta);
		this.Character3D.Velocity = newDirection * newSpeed
			+ (this.PreserveOrthogonalVelocity ? this.Character3D.Velocity.Project(movementPlane.Normal) : Vector3.Zero);

		if (!this.RotationEnabled)
			return;

		Vector3 localBack = this.RotationLocalForwardDirection.Normalized() * -1;
		Vector3 localUp = this.RotationLocalUpDirection.Normalized();
		Basis localTargetBasis = new Basis(localUp.Cross(localBack), localUp, localBack).Orthonormalized();

		Vector3 forward = this.RotationForwardAlignment switch
			{
				AlignmentOptionsEnum.MovementDirection when !newDirection.IsZeroApprox() => newDirection,
				AlignmentOptionsEnum.SurfaceNormal => movementPlane.Normal,
				AlignmentOptionsEnum.CameraForward => camera.GlobalBasis.Forward.Normalized(),
				AlignmentOptionsEnum.Gravity => this.Character3D.GetGravity().Normalized(),
				AlignmentOptionsEnum.Global => this.RotationGlobalForwardDirection.Normalized(),
				_ => Vector3.Zero,
			};
		Vector3 up = this.RotationUpAlignment switch
			{
				AlignmentOptionsEnum.MovementDirection when !newDirection.IsZeroApprox() => newDirection,
				AlignmentOptionsEnum.SurfaceNormal => movementPlane.Normal,
				AlignmentOptionsEnum.CameraForward => camera.GlobalBasis.Forward.Normalized(),
				AlignmentOptionsEnum.Gravity => this.Character3D.GetGravity().Normalized(),
				AlignmentOptionsEnum.Global => this.RotationGlobalUpDirection.Normalized(),
				_ => Vector3.Zero,
			};
		Vector3 back = forward * -1;
		Vector3 right = up.Cross(back).Normalized();
		up = back.Cross(right);
		Basis globalTargetBasis = new Basis(right, up, back).Orthonormalized();

		if (!globalTargetBasis.IsOrthonormalized())
			return;

		Basis targetBasis = globalTargetBasis * localTargetBasis;

		if (this.RotationLimitEnabled)
		{
			float stepAngle = Mathf.Pi * 2f / this.RotationLimitDirections;
			Vector3 originForward = this.RotationLimitAlignWithWorldAxis
				? Vector3.Forward
				: camera.GlobalBasis.Forward.Normalized();
			Plane surfacePlane = new Plane(up, Vector3.Zero);
			Vector3 originDirection = originForward.Project(surfacePlane).Normalized();
			float currentAngle = originDirection.SignedAngleTo(forward, up);
			float targetAngle = Mathf.Round(currentAngle / stepAngle) * stepAngle;
			float rotationAngle = targetAngle - currentAngle;
			targetBasis = targetBasis.Rotated(targetBasis.Up, rotationAngle);
		}

		this.Character3D.GlobalBasis = targetBasis;
	}

	//==================================================================================================================
		#endregion
	//==================================================================================================================
		#region METHODS
	//==================================================================================================================

	// private void SnapToSurface()
	// {
	// 	if (this.Character == null)
	// 		return;
	// 	switch (this.Surface)
	// 	{
	// 		case SurfaceTypeEnum.Floor when this.Character.IsOnFloor():
	// 			this.Character.ApplyFloorSnap();
	// 			break;
	// 		case SurfaceTypeEnum.Wall when this.Character.IsOnWall():
	// 			this.Character.MoveAndCollide(this.Character.GetWallNormal() * -1);
	// 			break;
	// 		case SurfaceTypeEnum.Ceiling when this.Character.IsOnCeiling():
	// 			this.Character.MoveAndCollide(this.Character.GlobalBasis.Up);
	// 			break;
	// 	}
	// }

	/// <summary>
	/// Returns the global movement plane of the character, based on the surface type and the last collision normal.
	///
	/// Returns null if the character is not on the surface.
	/// </summary>
	public Plane? ResolveGlobalMovementPlane()
		=> this.Surface switch
		{
			SurfaceTypeEnum.Floor when this.Character3D?.IsOnFloor() == true
				&& this.Character3D.GetLastSlideCollision() is KinematicCollision3D collision
					=> new Plane(collision.GetNormal(), collision.GetPosition()),
			SurfaceTypeEnum.Wall when this.Character3D?.IsOnWall() == true
				&& this.Character3D.GetLastSlideCollision() is KinematicCollision3D collision
					=> new Plane(collision.GetNormal(), collision.GetPosition()),
			SurfaceTypeEnum.Ceiling when this.Character3D?.IsOnCeiling() == true
				&& this.Character3D.GetLastSlideCollision() is KinematicCollision3D collision
					=> new Plane(collision.GetNormal(), collision.GetPosition()),
			_ => null,
		};

	/// <summary>
	/// Return true if the character has exited the surface in the last physics frame.
	/// </summary>
	private bool TestJustExitSurface()
		=> this.Character3D != null && this.Surface switch
		{
			SurfaceTypeEnum.Floor =>
				this.Character3D.TimeAwayFromFloorSec >= 0
				&& this.Character3D.TimeAwayFromFloorSec
					< this.Character3D.GetPhysicsProcessDeltaTime() + Mathf.Epsilon,
			SurfaceTypeEnum.Wall =>
				this.Character3D.TimeAwayFromWallSec >= 0
				&& this.Character3D.TimeAwayFromWallSec
					< this.Character3D.GetPhysicsProcessDeltaTime() + Mathf.Epsilon,
			SurfaceTypeEnum.Ceiling =>
				this.Character3D.TimeAwayFromCeilingSec >= 0
				&& this.Character3D.TimeAwayFromCeilingSec
					< this.Character3D.GetPhysicsProcessDeltaTime() + Mathf.Epsilon,
			_ => false,
		};

	//==================================================================================================================
		#endregion
	//==================================================================================================================
}
