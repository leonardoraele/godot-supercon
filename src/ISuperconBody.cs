using Raele.GodotUtils.Adapters;

namespace Raele.Supercon;

public interface ISuperconBody : INode
{
	public SuperconState? RestState { get; }
	public SuperconInputController? InputController { get; }

	public SuperconBody2D? As2D => this as SuperconBody2D;
	public SuperconBody3D? As3D => this as SuperconBody3D;
}
