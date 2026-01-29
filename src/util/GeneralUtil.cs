using System;
using System.Threading.Tasks;
using Godot;

namespace Raele.Supercon;

public static class GeneralUtil
{
	public static Vector3 RotateToward(Vector3 from, Vector3 to, float angleRad, Vector3? defaultAxis = null)
	{
		if (angleRad < 0) {
			to *= -1;
			angleRad *= -1;
		}
		float angle = from.AngleTo(to);
		if (angle < Mathf.Epsilon) {
			return to;
		} else if (Mathf.Pi - angle < Mathf.Epsilon) {
			return from.Rotated(defaultAxis ?? Vector3.Up, Math.Min(angleRad, Mathf.Pi));
		}
		float weight = Mathf.Clamp(angleRad / angle, 0, 1);
		return from.Slerp(to, weight);
	}

    public static bool TestIsParallel(Vector3 a, Vector3 b, float epsilon = Mathf.Epsilon)
    {
		float dot = a.Dot(b);
		return dot <= -1 + epsilon || dot >= 1 - epsilon;
    }

	public static Task<T> RequestSelectNode<T>(T? currentValue = default) where T : Node
	{
		TaskCompletionSource<T> tcs = new();
		EditorInterface.Singleton.PopupNodeSelector(
			Callable.From((NodePath path) =>
			{
				if (EditorInterface.Singleton.GetEditedSceneRoot()?.GetNodeOrNull(path) is T state)
				{
					tcs.SetResult(state);
				}
				else
				{
					tcs.SetCanceled();
				}
			}),
			[typeof(T).Name],
			currentValue
		);
		return tcs.Task;
	}

	public static void DebugLog<T>(params object[] items)
	{
		GD.PrintS(
			DateTime.Now.ToString("HH:mm:ss.fff"), $"(#{Time.GetTicksMsec() / (1000 / 60d):0})", $"by {typeof(T).Name}:",
			"\n\t", string.Join(" ", items)
		);
	}
}
