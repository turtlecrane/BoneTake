public interface IEnemyAI
{
    bool canMove { get; set; }
    bool canRotation { get; set; }
    bool canAttack { get; set; }
    bool isGrounded { get; set; }
    bool facingRight { get; set; }
    bool canTracking { get; set; }
}