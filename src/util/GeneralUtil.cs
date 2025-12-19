using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace Raele.Supercon2D;

public static class GeneralUtil
{
	public static Vector2 MoveToward(Vector2 from, Vector2 to, Vector2 delta)
	{
		return new Vector2(
			Mathf.MoveToward(from.X, to.X, delta.X),
			Mathf.MoveToward(from.Y, to.Y, delta.Y)
		);
	}

	public static Vector2 MoveToward(float fromX, float fromY, float toX, float toY, float deltaX, float deltaY)
	{
		return new Vector2(
			Mathf.MoveToward(fromX, toX, deltaX),
			Mathf.MoveToward(fromY, toY, deltaY)
		);
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
}
