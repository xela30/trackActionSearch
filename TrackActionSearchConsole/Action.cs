using System;

namespace TrackActionSearchConsole
{
    public struct Action: IEquatable<Action>
    {
        public Action(int actionId, DateTime actionDateTime)
        {
            ActionId = actionId;
            ActionDateTime = actionDateTime;
        }

        public int ActionId { get; }

        public DateTime ActionDateTime { get; }

        public bool Equals(Action other)
        {
            return ActionId == other.ActionId && ActionDateTime.Equals(other.ActionDateTime);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;

            return obj is Action key && Equals(key);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ActionId * 397) ^ ActionDateTime.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"Id={ActionId} at {ActionDateTime}";
        }
    }
}