using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Commune.Basis
{
  public class _ : CollectionHlp
  {
		public static Tuple<T1, T2> Tuple<T1, T2>(T1 first, T2 second)
		{
			return new Tuple<T1, T2>(first, second);
		}

		public static Tuple<T1, T2, T3> Tuple<T1, T2, T3>(T1 first, T2 second, T3 third)
		{
			return new Tuple<T1, T2, T3>(first, second, third);
		}

		public static Tuple<T1, T2, T3, T4> Tuple<T1, T2, T3, T4>(T1 first, T2 second, T3 third, T4 fourth)
		{
			return new Tuple<T1, T2, T3, T4>(first, second, third, fourth);
		}

		public static Tuple<T1, T2, T3, T4, T5> Tuple<T1, T2, T3, T4, T5>(T1 first, T2 second, T3 third, T4 fourth, T5 fifth)
		{
			return new Tuple<T1, T2, T3, T4, T5>(first, second, third, fourth, fifth);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6> Tuple<T1, T2, T3, T4, T5, T6>(T1 first, T2 second, T3 third,
			T4 fourth, T5 fifth, T6 sixth)
		{
			return new Tuple<T1, T2, T3, T4, T5, T6>(first, second, third, fourth, fifth, sixth);
		}
	}
}
