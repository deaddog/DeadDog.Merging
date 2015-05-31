using System;

namespace DeadDog.Merging
{
    /// <summary>
    /// Determines if a pair of <see cref="Delete{T}"/>/<see cref="Insert{T}"/> elements are similar by comparing their edit-distance.
    /// </summary>
    /// <typeparam name="T">The type of the elements being merged.</typeparam>
    public class EditDistanceMoveIdentifier<T> : IMoveIdentifier<T> where T : IEquatable<T>
    {
        private readonly double maxMoveDistance;
        private readonly int minMoveLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditDistanceMoveIdentifier{T}"/> class.
        /// </summary>
        /// <param name="maxMoveDistance">The maximum normalized distance (0-1) tolerated. Distances above this value are not considered moves.</param>
        /// <param name="minMoveLength">The minimum number of items required for two elements to be considered a move.</param>
        public EditDistanceMoveIdentifier(double maxMoveDistance = 0.2, int minMoveLength = 10)
        {
            if (maxMoveDistance < 0 || maxMoveDistance > 1)
                throw new ArgumentOutOfRangeException("maxMoveDistance", "The distance must be a normalized value (0-1).");
            if (minMoveLength < 0)
                throw new ArgumentOutOfRangeException("minMoveLength");

            this.maxMoveDistance = maxMoveDistance;
            this.minMoveLength = minMoveLength;
        }

        public double? MoveWeight(Delete<T> delete, Insert<T> insert)
        {
            if (Math.Max(delete.Value.Length, insert.Value.Length) < minMoveLength)
                return null;

            double normalized_dist = EditDistance.GetDistance(delete.Value, insert.Value) / Math.Max(delete.Value.Length, insert.Value.Length);

            if (normalized_dist <= maxMoveDistance)
                return normalized_dist;
            else
                return null;
        }
    }
}
