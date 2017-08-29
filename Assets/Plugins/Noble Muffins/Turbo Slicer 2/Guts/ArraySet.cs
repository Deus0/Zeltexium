using System;

namespace NobleMuffins.TurboSlicer.Guts
{
	public class ArraySet
	{
		public static void Set<TElement>(TElement[] array, int start, int length, TElement figure) {
			for(int i = 0; i < array.Length; i++) {
				array[i] = figure;
			}
		}
	}
}

