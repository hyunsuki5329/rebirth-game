namespace Roguelite.Core
{
    public static class EnemySteering
    {
        public static int RangedAxis(float distance, float preferred, float tolerance)
            => distance < preferred - tolerance ? -1 : distance > preferred + tolerance ? 1 : 0;
    }
}
