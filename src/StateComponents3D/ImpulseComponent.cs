using Godot;
using Godot.Collections;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon.StateComponents3D;

[Tool]
public partial class ImpulseComponent : SuperconStateComponent3D
{
	//==================================================================================================================
	// EXPORTS
	//==================================================================================================================

	[Export] public ImpulseTypeEnum ImpulseType = ImpulseTypeEnum.Add;

	/// <summary>
	/// The direction to which the force is applied.
	/// </summary>
	[Export] public Vector3 Direction = Vector3.Up;
	/*EditorOnly*/ [Export(PropertyHint.Range, "0,360,radians_as_degrees")] public float HorizontalAngle
	{
		get => this.Direction.SignedAngleTo(Vector3.Right, Vector3.Up);
		set => this.Direction = Vector3.Right.Rotated(Vector3.Up, value).RotateToward(Vector3.Up, this.VerticalAngle);
	}
	/*EditorOnly*/ [Export(PropertyHint.Range, "0,360,radians_as_degrees")] public float VerticalAngle
	{
		get => this.Direction.SignedAngleTo(Plane.PlaneXZ);
		set => this.Direction = Vector3.Right.Rotated(Vector3.Up, this.HorizontalAngle).RotateToward(Vector3.Up, value);
	}

	/// <summary>
	/// If true, the direction is considered in local space.
	/// </summary>
	[Export] public bool LocalDirection = false;
	[Export(PropertyHint.None, "suffix:m/s")] public float Magnitude = 1f;

	[ExportGroup("Additional Options")]
	[Export(PropertyHint.None, "suffix:m/s")] public float MaxSpeed = float.PositiveInfinity;

	//==================================================================================================================
	// FIELDS
	//==================================================================================================================

	//==================================================================================================================
	// COMPUTED FIELDS
	//==================================================================================================================

	public Vector3 GlobalDirection => this.LocalDirection && this.Character != null
		? (this.Character.GlobalBasis * this.Direction).Normalized()
		: this.Direction.Normalized();

	//==================================================================================================================
	// INTERNAL TYPES
	//==================================================================================================================

	public enum ImpulseTypeEnum : sbyte
	{
		/// <summary>
		/// Adds the impulse to the current velocity.
		/// </summary>
		Add = 16,
		/// <summary>
		/// Sets the character velocity to the impulse. The current velocity is completely overridden.
		/// </summary>
		Set = 32,
		/// <summary>
		/// Overrides the character velocity on the impulse axis by setting it to the impulse magnitude. The other axes
		/// are unaffected.
		/// </summary>
		SetAxis = 48,
	}

	//==================================================================================================================
	// VIRTUALS & OVERRIDES
	//==================================================================================================================

	public override void _ValidateProperty(Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.VerticalAngle):
			case nameof(this.HorizontalAngle):
				property["usage"] = (int) PropertyUsageFlags.Editor;
				break;
			case nameof(this.MaxSpeed):
				if (this.ImpulseType != ImpulseTypeEnum.Add)
					property["usage"] = (int) PropertyUsageFlags.None;
				break;
		}
	}

	protected override void _ActivityStarted(string mode, Variant argument)
	{
		base._ActivityStarted(mode, argument);
		switch (this.ImpulseType)
		{
			case ImpulseTypeEnum.Add:
				this.Character?.Velocity += this.GlobalDirection * this.Magnitude;
				break;
			case ImpulseTypeEnum.Set:
				this.Character?.Velocity = this.GlobalDirection * this.Magnitude;
				break;
			case ImpulseTypeEnum.SetAxis:
				this.Character?.Velocity =
					this.Character.Velocity.Project(new Plane(this.GlobalDirection))
					+ this.GlobalDirection * this.Magnitude;
				break;
		}
	}
}
