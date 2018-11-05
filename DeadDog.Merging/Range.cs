using System;
using System.Collections.Generic;

namespace DeadDog.Merging
{
    public struct Range : IEquatable<Range>
    {
        public Range(int start, int end)
        {
            if (end < start)
                throw new ArgumentException("End must be greater than or equal to start.");

            Start = start;
            End = end;
        }

        public static bool operator ==(Range r1, Range r2)
        {
            return EqualityComparer<Range>.Default.Equals(r1, r2);
        }
        public static bool operator !=(Range r1, Range r2)
        {
            return !EqualityComparer<Range>.Default.Equals(r1, r2);
        }

        public static Range FromStartLength(int start, int length)
        {
            return new Range(start, start + length);
        }

        public bool OverlapsWith(Range range)
        {
            return (Start <= range.Start && End > range.Start) ||
                   (Start < range.End && End > range.End) ||
                   range.Contains(this);
        }
        public bool Contains(Range range, bool includeStart = true)
        {
            return (includeStart ? (Start <= range.Start) : (Start < range.Start)) && End >= range.End;
        }
        public bool Contains(int position, bool includeStart = true)
        {
            return Contains(new Range(position, position), includeStart);
        }

        public static Range Join(Range range1, Range range2)
        {
            return new Range(Math.Min(range1.Start, range2.Start), Math.Max(range1.End, range2.End));
        }

        public int Start { get; }
        public int End { get; }
        public int Length => End - Start;

        public override string ToString()
        {
            return $"({Start}, {End})";
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode() ^ End.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return obj is Range && Equals((Range)obj);
        }
        public bool Equals(Range other)
        {
            return Start == other.Start && End == other.End;
        }
    }
}
