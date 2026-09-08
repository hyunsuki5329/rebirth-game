namespace Roguelite.Core
{
    public readonly struct MoveVector
    {
        public readonly float X, Y;
        public MoveVector(float x, float y) { X = x; Y = y; }
    }

    public static class MovementRules
    {
        public static MoveVector Velocity(float x, float y, float speed, float sprintMultiplier, bool sprint)
        {
            var magnitude = System.Math.Sqrt(x * x + y * y);
            if (magnitude > 1) { x /= (float)magnitude; y /= (float)magnitude; }
            var rate = System.Math.Max(0, speed) * (sprint ? System.Math.Max(1, sprintMultiplier) : 1);
            return new MoveVector(x * rate, y * rate);
        }
    }
}
