using Godot;

namespace Raele.Supercon2D.StateComponents;

[GlobalClass][Icon($"res://addons/{nameof(Supercon2D)}/icons/character_body_impulse.png"]
public partial class ImpulseComponent : SuperconStateComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// LOCAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	// TODO
	// public enum DirectionReferenceEnum : sbyte
	// {
	// 	Absolute,
	// 	FacingDirection,
	// }

	public enum ImpulseTypeEnum : sbyte
	{
		Add = 1,
		OverrideSingleAxis = 2,
		OverrideBothAxis = 3,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Determines how the impulse is applied to the character's velocity.
	/// </summary>
	[Export] public ImpulseTypeEnum ImpulseType = ImpulseTypeEnum.Add;

	/// <summary>
	/// The direction of the impulse to be applied to the character's velocity, counter clockwise relative to the
	/// positive right axis.
	/// </summary>
	[Export(PropertyHint.Range, "-180,180,5,radians_as_degrees")] public float Angle = 0f;

	/// <summary>
	/// The magnitude of the impulse to be applied to the character's velocity, in pixels per second.
	/// </summary>
	[Export] public float MagnitudePxPSec = 200f;

	/// <summary>
	/// If true, the impulse direction is inverted when the character is facing left.
	/// </summary>
	[Export] public bool UseFacingDirection = true;

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	private Vector2 ImpulseDirection => Vector2.Right.Rotated(this.Angle)
		* (this.UseFacingDirection ? new Vector2(this.Character?.HorizontalFacingDirection ?? 0, 1f) : Vector2.One);

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _SuperconStart()
	{
		base._SuperconStart();
		switch (this.ImpulseType)
		{
			case ImpulseTypeEnum.Add:
				this.Character?.Velocity = this.Character.Velocity + this.ImpulseDirection * this.MagnitudePxPSec;
				break;
			case ImpulseTypeEnum.OverrideSingleAxis:
				this.Character?.SetDirectionalVelocity(this.ImpulseDirection * this.MagnitudePxPSec);
				break;
			case ImpulseTypeEnum.OverrideBothAxis:
				this.Character?.Velocity = this.ImpulseDirection * this.MagnitudePxPSec;
				break;
		}
	}
}
