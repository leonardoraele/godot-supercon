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
	[Export] public bool PreserveOrthogonalVelocity = true;

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

	[ExportSubgroup("Additional Options")]
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
	[Export] public bool RequireForwardStart = false;

	[ExportGroup("Rotate Character", "Rotation")]
	[Export(PropertyHint.GroupEnable)] public bool RotationEnabled = false;
	[Export] public AlignmentOptionsEnum RotationForwardAlignment
		{ get; set { field = value; this.NotifyPropertyListChanged(); } }
		= AlignmentOptionsEnum.MovementDirection;
	[Export] public Vector3 RotationGlobalForwardDirection = Vector3.Forward;
	// [Export] public Vector3 RotationLocalForwardDirection = Vector3.Forward;
	[Export] public AlignmentOptionsEnum RotationUpAlignment
		{ get; set { field = value; this.NotifyPropertyListChanged(); } }
		= AlignmentOptionsEnum.SurfaceNormal;
	[Export] public Vector3 RotationGlobalUpDirection = Vector3.Up;
	// [Export] public Vector3 RotationLocalUpDirection = Vector3.Up;

	[ExportSubgroup("Limits Directions", "RotationLimit")]
	[Export(PropertyHint.GroupEnable)] public bool RotationLimitEnabled = false;
	[Export] public int RotationLimitDirections
		{ get; set { field = Mathf.Max(4, value); } }
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
		}
	}

	protected override void _ActivityPhysicsProcess(double delta)
	{
		base._ActivityPhysicsProcess(delta);
		if (this.Character3D == null)
			return;
		if (this.ResolveGlobalMovementPlane() is not Plane plane)
		{
			if (this.TestSurfaceExit())
				this.EmitSignalSurfaceExit();
			return;
		}
		if (this.Character3D.GetViewport().GetCamera3D() is not Camera3D camera)
			return;

		Vector3 projectedVelocity = this.Character3D.Velocity.Project(plane with { D = 0 });
		float currentSpeed = projectedVelocity.Length();
		bool isMoving = currentSpeed > Mathf.Epsilon;
		Vector3 currentDirection = projectedVelocity.Normalized().DefaultIfZero(this.Character3D.GlobalBasis.Forward);
		Vector2 normalizedInput = this.Character3D.InputController?.NormalizedDirectionalInput ?? Vector2.Zero;
		float inputStrength = normalizedInput.Length();
		bool hasInput = inputStrength > Mathf.Epsilon;
		Vector3 inputDirection = normalizedInput.IsZeroApprox()
			? Vector3.Zero
			: Basis.LookingAt(plane.Normal * -1, camera.GlobalBasis.Up)
				* new Vector3(normalizedInput.X, normalizedInput.Y * -1, 0);
		Vector3 newGlobalDirection = isMoving
			? hasInput
				? this.UseSmoothTurning
					? currentDirection.RotateToward(inputDirection, this.AngularVelocity * delta)
					: inputDirection
				: currentDirection
			: hasInput && !this.RequireForwardStart
				? inputDirection
				: this.Character3D.Basis.Forward;
		float targetSpeed = this.MaxSpeed * inputStrength;
		float acceleration = targetSpeed > currentSpeed - Mathf.Epsilon
			? this.Acceleration
			: this.Deceleration;
		float newSpeed = currentSpeed.MoveToward(targetSpeed, acceleration * (float) delta);
		this.Character3D.Velocity = newGlobalDirection * newSpeed
			+ (this.PreserveOrthogonalVelocity ? this.Character3D.Velocity.Project(plane.Normal) : Vector3.Zero);

		if (!this.RotationEnabled)
			return;

		Vector3 forward = this.RotationForwardAlignment switch
			{
				AlignmentOptionsEnum.MovementDirection => newGlobalDirection,
				AlignmentOptionsEnum.SurfaceNormal => plane.Normal,
				AlignmentOptionsEnum.CameraForward => camera.GlobalBasis.Forward.Normalized(),
				AlignmentOptionsEnum.Gravity => this.Character3D.GetGravity().Normalized(),
				AlignmentOptionsEnum.Global => this.RotationGlobalForwardDirection,
				_ => this.Character3D.GlobalBasis.Forward,
			};
		Vector3 up = this.RotationUpAlignment switch
			{
				AlignmentOptionsEnum.MovementDirection => newGlobalDirection,
				AlignmentOptionsEnum.SurfaceNormal => plane.Normal,
				AlignmentOptionsEnum.CameraForward => camera.GlobalBasis.Forward.Normalized(),
				AlignmentOptionsEnum.Gravity => this.Character3D.GetGravity().Normalized(),
				AlignmentOptionsEnum.Global => this.RotationGlobalUpDirection,
				_ => this.Character3D.GlobalBasis.Up,
			};
		Vector3 back = forward * -1;
		Vector3 right = up.Cross(back).DefaultIfZero(Vector3.Right).Normalized();
		Basis globalBasis = new Basis(right, up, back);
		Basis newBasis = globalBasis.Orthonormalized();

		if (this.RotationLimitEnabled)
		{
			float stepAngle = Mathf.Pi * 2f / this.RotationLimitDirections;
			Vector3 originForward = this.RotationLimitAlignWithWorldAxis
				? Vector3.Forward
				: camera.GlobalBasis.Forward;
			Plane surfacePlane = new Plane(up, Vector3.Zero);
			Vector3 originDirection = originForward.Project(surfacePlane).Normalized();
			float currentAngle = originDirection.SignedAngleTo(forward, up);
			float targetAngle = Mathf.Round(currentAngle / stepAngle) * stepAngle;
			float rotationAngle = targetAngle - currentAngle;
			newBasis = newBasis.Rotated(newBasis.Up, rotationAngle);
		}

		this.Character3D.GlobalBasis = newBasis;
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

	private bool TestSurfaceExit()
		// Note: We assume that if this method is being called, then the character is not on the surface anymore. This
		// method only tests if the character has exit the surface this frame.
		=> this.Character3D != null && this.Surface switch
		{
			SurfaceTypeEnum.Floor => this.Character3D.TimeOnFloor > this.Character3D.GetPhysicsProcessDeltaTime() * -1 - Mathf.Epsilon,
			SurfaceTypeEnum.Wall => this.Character3D.TimeOnWall > this.Character3D.GetPhysicsProcessDeltaTime() * -1 - Mathf.Epsilon,
			SurfaceTypeEnum.Ceiling => this.Character3D.TimeOnCeiling > this.Character3D.GetPhysicsProcessDeltaTime() * -1 - Mathf.Epsilon,
			_ => false,
		};

	//==================================================================================================================
		#endregion
	//==================================================================================================================
}
