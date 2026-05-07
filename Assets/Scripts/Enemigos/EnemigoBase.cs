using UnityEngine;
using UnityEngine.AI;

public class EnemigoBase : MonoBehaviour, ISavable<EnemyState>
{
    [SerializeField] private float _life;
    [SerializeField] private float _damage;
    [SerializeField] protected float _speed;
    protected EnemyStateMachine _currentState;
    private string _saveId;
    protected NavMeshAgent _agent;
    public GameObject player;

    public string SaveId => _saveId;

    protected virtual void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = _speed;
    }

    protected virtual void Update()
    {
        
    }
    public virtual void TakeDamage(float damage)
    {
        _life -= damage;
        if (_life <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    public EnemyState SaveState()
    {
               return new EnemyState
        {
            IsDead = _life <= 0
        };
    }

    public void RestoreState(EnemyState state)
    {
        if (state.IsDead)
        {
            Die();
        }
    }
}
