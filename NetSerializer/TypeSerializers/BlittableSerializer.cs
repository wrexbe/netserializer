using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetSerializer.TypeSerializers
{
	internal class BlittableSerializer : IStaticTypeSerializer
	{
		public bool Handles(Type type)
		{
			try
			{
				if (type.IsGenericType)
					return false;
				if (!type.IsClass)
				{
					// Throws if not blittable
					GCHandle.Alloc(Activator.CreateInstance(type), GCHandleType.Pinned).Free();
					var checkM = typeof(Blitter<>)
					  .MakeGenericType(type)
					  .GetMethod("Check");
					// Throws if we can't make it for some other reason
					checkM.Invoke(null, null);
					return true;
				}
			}
			catch(Exception)
			{
			}
			return false;
		}

		public IEnumerable<Type> GetSubtypes(Type type)
		{
			return Array.Empty<Type>();
		}

		public MethodInfo GetStaticWriter(Type type)
		{
			return typeof(Blitter<>)
			  .MakeGenericType(type)
			  .GetMethod("Write");
		}

		public MethodInfo GetStaticReader(Type type)
		{
			return typeof(Blitter<>)
			  .MakeGenericType(type)
			  .GetMethod("Read");
		}

		public static class Blitter<T> where T : unmanaged
		{
			private static readonly int _size = Marshal.SizeOf<T>();
			public static int Size => _size;
			public static void Check() { }

			public static unsafe void Write(Stream stream, T blittable)
			{
				var ptr = Unsafe.AsPointer(ref blittable);
				stream.Write(new ReadOnlySpan<byte>(ptr, _size));
			}

			public static unsafe void Read(Stream stream, out T value)
			{
				Span<byte> data = stackalloc byte[_size];
				stream.Read(data);
				var spanT = MemoryMarshal.Cast<byte, T>(data);
				value = spanT[0];
			}
		}
	}
}
