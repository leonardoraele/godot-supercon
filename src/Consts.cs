using Godot;

namespace Raele.Supercon;

public static class Consts
{
	public const string IconsDir = $"addons/{nameof(Supercon)}/icons";
	public const int MAX_SINGLE_LINE_COMMENT_LENGTH = 20;

	public static class FacingDirection
	{
		public static readonly Vector2 DEFAULT_GROUNDED_2D = Vector2.Right;
		public static readonly Vector2 DEFAULT_FLOATING_2D = Vector2.Down;
		public static readonly Vector3 DEFAULT_3D = Vector3.Forward;
	}
}
