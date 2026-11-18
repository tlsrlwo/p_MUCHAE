namespace muchae
{
    public abstract class PlayerStateBase
    {
        public abstract void EnterState(PlayerMovement pMovement);

        public abstract void UpdateState(PlayerMovement pMovement);

        public abstract void ExitState(PlayerMovement pMovement);
    }
}