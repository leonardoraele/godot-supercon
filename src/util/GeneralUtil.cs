using System;
using System.Threading.Tasks;
using Godot;

namespace Raele.Supercon;

public static class GeneralUtil
{
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
