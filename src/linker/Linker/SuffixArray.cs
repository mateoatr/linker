using System;
using System.Collections.Generic;
using System.Linq;

namespace Mono.Linker
{
	public class SuffixArray<T>
	{
		private readonly Dictionary<T, int> integerMapping;
		private readonly List<int> integerRepresentation;
		private readonly List<T> originalSequence;
		private bool IsBuilt { get; set; }
		private int usedSeparators;

		public int[] SortedCyclicShifts;
		public int[] LongestCommonPrefix;

		public SuffixArray (List<T> sequence)
		{
			integerMapping = new Dictionary<T, int> ();
			integerRepresentation = new List<int> ();
			originalSequence = sequence;
			IsBuilt = false;
			// We always use at least '$' at the end.
			usedSeparators = 1;
		}

		public List<T> GetLongestNonOverlappingSubsequence (int minimSubsequenceSize = 2)
		{
			Build ();
			int candidateIndex = -1;
			int candidateLength = 0;
			for (int i = 1; i < LongestCommonPrefix.Length; i++) {
				int subsequenceSize = LongestCommonPrefix[i];
				if (subsequenceSize > LongestCommonPrefix[i - 1]) {
					// Break if there's overlaps or the substring does not meet the size threshold
					if (SortedCyclicShifts[i - 1] - SortedCyclicShifts[i] < subsequenceSize ||
						subsequenceSize < minimSubsequenceSize)
						continue;

					if (subsequenceSize > candidateLength) {
						candidateLength = subsequenceSize;
						candidateIndex = i - 1;
					}
				}
			}

			if (candidateIndex == -1)
				return null;

			return GetSubsequence (candidateIndex, candidateLength);
		}

		public List<T> GetLongestRepeatedSubsequence ()
		{
			Build ();
			int longestPrefixSize = LongestCommonPrefix.Max ();
			int longestPrefix = Array.IndexOf (LongestCommonPrefix, longestPrefixSize);
			return GetSubsequence (longestPrefix, longestPrefixSize);
		}

		void Build ()
		{
			if (IsBuilt)
				return;

			for (int i = 0; i < originalSequence.Count; i++) {
				if (originalSequence[i] == null) {
					integerRepresentation.Add ((++usedSeparators) * -1);
				} else {
					if (!integerMapping.TryGetValue (originalSequence[i], out int strRepresentation)) {
						strRepresentation = integerMapping.Count + 1;
						integerMapping[originalSequence[i]] = strRepresentation;
					}

					integerRepresentation.Add (strRepresentation);
				}
			}

			integerRepresentation.Add (0);
			BuildSuffixArray ();
			BuildLongestCommonPrefixArray ();
			IsBuilt = true;

			// Remove the terminator
			SortedCyclicShifts = SortedCyclicShifts.Skip (1).ToArray ();
			integerRepresentation.RemoveAt (integerRepresentation.Count - 1);
		}

		List<T> GetSubsequence (int index, int size)
		{
			// We go back from our integer representation to a string...
			// The mapping is bijective so this is fine.
			var reverseDict = integerMapping.ToDictionary (k => k.Value, v => v.Key);
			List<T> subsequence = new List<T> ();
			for (int i = SortedCyclicShifts[index];
				i < SortedCyclicShifts[index] + size; i++)
				subsequence.Add (reverseDict[integerRepresentation[i]]);

			return subsequence;
		}

		void BuildSuffixArray ()
		{
			int alphabetSize = integerMapping.Count + usedSeparators;
			int stringSize = integerRepresentation.Count;
			int[] elementCount = new int[Math.Max (alphabetSize, stringSize)];
			int[] permutations = new int[stringSize];
			int[] eqClasses = new int[stringSize];

			for (int i = 0; i < stringSize; i++)
				elementCount[usedSeparators + integerRepresentation[i]]++;

			for (int i = 1; i < alphabetSize; i++)
				elementCount[i] += elementCount[i - 1];

			for (int i = 0; i < stringSize; i++)
				permutations[--elementCount[usedSeparators + integerRepresentation[i]]] = i;

			eqClasses[permutations[0]] = 0;
			int classes = 1;
			for (int i = 1; i < stringSize; i++) {
				if ((usedSeparators + integerRepresentation[permutations[i]]) != (usedSeparators + integerRepresentation[permutations[i - 1]]))
					classes++;

				eqClasses[permutations[i]] = classes - 1;
			}

			int[] sndPermutation = new int[stringSize];
			int[] sndEqClasses = new int[stringSize];
			for (int j = 0; (1 << j) < stringSize; ++j) {
				for (int i = 0; i < stringSize; i++) {
					sndPermutation[i] = permutations[i] - (1 << j);
					if (sndPermutation[i] < 0)
						sndPermutation[i] += stringSize;
				}

				Array.Fill (elementCount, 0, 0, classes);
				for (int i = 0; i < stringSize; i++)
					elementCount[eqClasses[sndPermutation[i]]]++;

				for (int i = 1; i < classes; i++)
					elementCount[i] += elementCount[i - 1];

				for (int i = stringSize - 1; i >= 0; i--)
					permutations[--elementCount[eqClasses[sndPermutation[i]]]] = sndPermutation[i];

				sndEqClasses[permutations[0]] = 0;
				classes = 1;
				for (int i = 1; i < stringSize; i++) {
					var currentPair = (eqClasses[permutations[i]], eqClasses[(permutations[i] + (1 << j)) % stringSize]);
					var previousPair = (eqClasses[permutations[i - 1]], eqClasses[(permutations[i - 1] + (1 << j)) % stringSize]);
					if (currentPair != previousPair)
						classes++;

					sndEqClasses[permutations[i]] = classes - 1;
				}

				var holdMyArray = eqClasses;
				eqClasses = sndEqClasses;
				sndEqClasses = holdMyArray;
			}

			SortedCyclicShifts = permutations;
		}

		void BuildLongestCommonPrefixArray ()
		{
			int size = integerRepresentation.Count;
			int[] rank = new int[size];
			for (int i = 0; i < size; i++)
				rank[SortedCyclicShifts[i]] = i;

			LongestCommonPrefix = new int[size - 1];
			for (int i = 0, k = 0; i < size; i++) {
				if (rank[i] == size - 1) {
					k = 0;
					continue;
				}

				int j = SortedCyclicShifts[rank[i] + 1];
				while (i + k - j > 0 &&
					i + k < size &&
					j + k < size &&
					integerRepresentation[i + k] == integerRepresentation[j + k])
					k++;

				LongestCommonPrefix[rank[i]] = k;
				if (k > 0)
					k--;
			}
		}
	}
}