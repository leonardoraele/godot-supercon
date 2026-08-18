using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon;

[Tool][GlobalClass][Icon($"res://addons/{nameof(Supercon)}/icons/character_body_bg.png")]
public abstract partial class SuperconStateComponent3D : SuperconStateComponent
{
	public FreakController3D? Controller => field ??= this.GetFirstAncestorOrDefault<FreakController3D>();
	public CharacterBody3D? Character3D => this.Controller?.Character;
}
