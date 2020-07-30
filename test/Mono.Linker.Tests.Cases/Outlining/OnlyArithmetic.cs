using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using Mono.Linker.Tests.Cases.Expectations.Assertions;

namespace Mono.Linker.Tests.Cases.Outlining
{
	[SkipKeptItemsValidation]
	public class OnlyArithmetic
	{
		public static void Main ()
		{
			Console.WriteLine (A ());
			Console.WriteLine (B ());
			Console.WriteLine (C ());
		}

		static int _integerField1 = 42;
		static int _integerField2 = 84;

		// IL_0000:  ldc.i4.0
		// IL_0001:  stloc.0
		// IL_0002:  ldloc.0
		// IL_0003:  ldsfld int32 test.Program::_integerField1
		// IL_0008:  add
		// IL_0009:  stloc.0
		// IL_000a:  ldloca.s V_0
		// IL_000c:  ldsfld int32 test.Program::_integerField2
		// IL_0011:  call instance bool[System.Runtime] System.Int32::Equals (int32)
		// IL_0016:  brfalse.s IL_001a
		// IL_0018:  ldc.i4.m1
		// IL_0019:  ret
		// IL_001a:  ldloc.0
		// IL_001b:  ldc.i4.3
		// IL_001c:  mul
		// IL_001d:  ret
		static int A ()
		{
			int x = 0;
			x += _integerField1;
			if (x.Equals (_integerField2))
				return -1;

			return x * 3;
		}

		static int B ()
		{
			int x = 0;
			x += _integerField1;
			if (x.Equals (_integerField2))
				return -1;

			return x * 3;
		}

		static int C ()
		{
			return A () + B ();
		}
	}
}
