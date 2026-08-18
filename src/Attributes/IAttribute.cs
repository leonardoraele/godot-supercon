using System;
using Godot;
using Raele.GodotUtils.Adapters;

namespace Raele.Supercon;

/// <summary>
/// An attribute definition is an object that defines an attribute in an attribute profile, including its name, type,
/// and any associated getter or setter logic.
/// </summary>
public interface IAttribute
{
	public string Name { get; }
	public Variant.Type Type { get; }
	public PropertyHint Hint { get; }
	public string HintString { get; }
	public bool IsReadOnly { get; }

	public GodotPropertyInfo ToPropertyInfo()
		=> new GodotPropertyInfo
		{
			Name = this.Name,
			Type = this.Type,
			Hint = this.Hint,
			HintString = this.HintString,
		};

	/// <summary>
	/// A mapping function that should be called whenever the attribute is read from a container.
	/// </summary>
	public Variant RunGetter(IReadOnlyAttributeContainer container, Variant rawValue)
		=> rawValue;

	/// <summary>
	/// A mapping function that should be called whenever the attribute is stored in a container.
	/// </summary>
	public Variant RunSetter(IReadOnlyAttributeContainer container, Variant input)
		=> throw new NotSupportedException($"Attribute \"{this.Name}\" is read-only and cannot be set.");
}

public static class IAttributeExtensions
{
	extension (IAttribute self)
	{
		public IAttribute AsAttributeDefinition() => self;
	}
}
