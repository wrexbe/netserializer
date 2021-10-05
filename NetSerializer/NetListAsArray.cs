using System;
using System.Collections.Generic;

namespace NetSerializer
{
	/// <summary>
	/// Represents a type that can serialize a <see cref="List{T}"/> or array, but always deserializes as array.
	/// </summary>
	/// <typeparam name="T">The type of contents stored in the list or array.</typeparam>
	[Serializable]
	public readonly struct NetListAsArray<T>
	{
		/// <summary>
		/// The collection contained by this instance. This can either be an array or a <see cref="List{T}"/>.
		/// </summary>
		public readonly IReadOnlyCollection<T> Value;

		/// <summary>
		/// The array deserialized into this instance. Only use this if the instance was deserialized.
		/// </summary>
		/// <remarks>
		/// Never returns null, instead returns an empty array if the deserialized collection was null.
		/// </remarks>
		public T[] Array => (T[])Value ?? System.Array.Empty<T>();

		/// <summary>
		/// If true, <see cref="Value"/> is a non-empty collection.
		/// </summary>
		public bool HasContents => Value is { Count: > 0 };

		public NetListAsArray(T[] array)
		{
			Value = array;
		}

		public NetListAsArray(List<T> array)
		{
			Value = array;
		}

		public static implicit operator NetListAsArray<T>(T[] array) => new(array);
		public static implicit operator NetListAsArray<T>(List<T> array) => new(array);
	}
}
