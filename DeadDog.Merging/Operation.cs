namespace DeadDog.Merging
{
    public class Operation
    {
        public Operation(int position, OperationType type)
        {
            Position = position;
            Type = type;
        }

        public int Position { get; }
        public OperationType Type { get; }
    }
}
