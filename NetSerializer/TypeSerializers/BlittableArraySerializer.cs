using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetSerializer.TypeSerializers
{
	internal class BlittableArraySerializer : IStaticTypeSerializer
	{
		public bool Handles(Type type)
		{
			try
			{
				if (!type.IsArray)
					return false;
				var elementT = type.GetElementType();

				if (elementT.IsGenericType)
					return false;

				if (!elementT.IsClass)
				{
					// Throws if not blittable
					GCHandle.Alloc(Activator.CreateInstance(elementT), GCHandleType.Pinned).Free();
					var checkM = typeof(BlitterArray<>)
					  .MakeGenericType(type)
					  .GetMethod("Check");
					// Throws if we can't make it for some other reason
					checkM.Invoke(null, null);
					return true;
				}
			}
			catch(ArgumentException)
			{
			}
			return false;
		}

		public IEnumerable<Type> GetSubtypes(Type type)
		{
			return new[] { type.GetElementType() };
		}

		public MethodInfo GetStaticWriter(Type type)
		{
			return typeof(BlitterArray<>)
			  .MakeGenericType(type)
			  .GetMethod("Write");
		}

		public MethodInfo GetStaticReader(Type type)
		{
			return typeof(BlitterArray<>)
			  .MakeGenericType(type)
			  .GetMethod("Read");
		}

		public static class BlitterArray<T> where T : unmanaged
		{
			private static readonly int _size = Marshal.SizeOf<T>();
			public static int Size => _size;

			public static void Check() { }
			public static unsafe void Write(Stream stream, T[] blittable)
			{
				var ptr = Unsafe.AsPointer(ref blittable);
				stream.Write(BitConverter.GetBytes(blittable.Length));
				stream.Write(new ReadOnlySpan<byte>(ptr, _size * blittable.Length));
			}

			public static unsafe void Read(Stream stream, out T[] value)
			{
				Span<byte> lengthBytes = stackalloc byte[4];
				stream.Read(lengthBytes);
				var length = BitConverter.ToInt32(lengthBytes);
				byte[] data = ArrayPool<byte>.Shared.Rent(Size * length);
				stream.Read(data);
				var spanT = MemoryMarshal.Cast<byte, T>(data);
				value = spanT.ToArray();
				ArrayPool<byte>.Shared.Return(data);
			}
		}
	}
}
