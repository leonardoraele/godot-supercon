using Godot;

namespace Raele.Supercon2D.StateComponents;

public partial class ImpulseComponent : SuperconStateController
{
	// -----------------------------------------------------------------------------------------------------------------
	// LOCAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	// TODO
	// public enum DirectionReferenceEnum : byte
	// {
	// 	Absolute,
	// 	FacingDirection,
	// }

	public enum ImpulseTypeEnum : byte
	{
		Additive,
		Override,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Determines how the impulse is applied to the character's velocity.
	/// </summary>
	[Export] public ImpulseTypeEnum ImpulseType = ImpulseTypeEnum.Additive;

	/// <summary>
	/// The direction of the impulse to be applied to the character's velocity, counter clockwise relative to the
	/// positive right axis.
	/// </summary>
	[Export] public float AngleDeg = 0f;

	/// <summary>
	/// If true, the impulse direction is inverted when the character is facing left.
	/// </summary>
	[Export] public bool UseFacingDirection = true;

	/// <summary>
	/// The magnitude of the impulse to be applied to the character's velocity, in pixels per second.
	/// </summary>
	[Export] public float MagnitudePxPSec = 200f;

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	private Vector2 ImpulseDirection => Vector2.Right.Rotated(Mathf.DegToRad(this.AngleDeg))
		* new Vector2(this.UseFacingDirection ? this.Character.FacingDirection : 1, 1);

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _EnterState()
	{
		base._EnterState();
		this.Character.Velocity = this.Character.Velocity * (this.ImpulseType == ImpulseTypeEnum.Additive ? 1 : 0)
			+ this.ImpulseDirection * this.MagnitudePxPSec;
	}
}
