using UnityEngine;
using UnityEngine.AI;

public enum GolemAction
{
    Wander, Detect, Attack
}

public class Golem : CharacterBase
{
    [SerializeField] private Animator _animator;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Area wanderArea;
    [SerializeField] private Transform _handlerTransform;

    public State<Golem>[] _action;
    public StateMachine<Golem> _actionMachine;

    private Rigidbody _rigidbody;

    private GolemStateHandler _golemStateHandler;
    private GolemAttackHandler _golemAttackHandler;
    public Animator Animator => _animator;
    public NavMeshAgent Agent => _agent;
    public Rigidbody Rigidbody => _rigidbody;
    public NavMeshAgent NavMeshAgent => _agent;
    public Area WanderArea => wanderArea;
    public Transform HandlerTransform => _handlerTransform;
    public GolemStateHandler GolemStateHandler => _golemStateHandler;
    public GolemAttackHandler golemAttackHandler => _golemAttackHandler;

    protected override void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();

        _golemStateHandler = HandlerTransform.GetComponent<GolemStateHandler>();
        _golemAttackHandler = HandlerTransform.GetComponent<GolemAttackHandler>();

    }

    private void Start()
    {
        _golemStateHandler.Initialize(this);
        _golemAttackHandler.Initialize(this);

        SetUp();
    }

    public override void SetUp()
    {
        _action = new State<Golem>[3];

        _action[(int)GolemAction.Wander] = new WanderState();
        _action[(int)GolemAction.Detect] = new DetectState();
        _action[(int)GolemAction.Attack] = new AttackState();
        _actionMachine = new StateMachine<Golem>();
        _actionMachine.SetUp(this, _action[(int)GolemAction.Wander]);
    }

    public override void Updated()
    {
        if (_actionMachine != null)
            _actionMachine.Execute();
    }

    public override void FixedUpdated()
    {
        if (_actionMachine != null)
            _actionMachine.FixedExecute();
    }

    public void ChangeActionState(GolemAction state)
    {
        _actionMachine.ChangeState(_action[(int)state]);
    }
}
