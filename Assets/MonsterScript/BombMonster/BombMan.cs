using UnityEngine;
using UnityEngine.AI;

public enum BombManAction
{
    idle, walk, explode
}

public class BombMan : CharacterBase
{
    [SerializeField] private BombManData _data;
    [SerializeField] private Animator _animator;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Area wanderArea;
    [SerializeField] private Transform _handlerTransform;

    public State<BombMan>[] _action;

    public StateMachine<BombMan> _actionMachine;


    private Rigidbody _rigidbody;

    private BombStateHandler _bombStateHandler;
    private BombhpHandler _bombhpHandler;
    
    public Animator Animator => _animator;
    public Rigidbody Rigidbody => _rigidbody;
    public NavMeshAgent NavMeshAgent => _agent;
    public Area WanderArea => wanderArea;
    public Transform HandlerTransform => _handlerTransform;
    public BombStateHandler BombStateHandler => _bombStateHandler;
    public BombhpHandler BombhpHandler => _bombhpHandler;

    protected override void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();

       _bombStateHandler = _handlerTransform.GetComponent<BombStateHandler>();
       _bombhpHandler = _handlerTransform.GetComponent<BombhpHandler>();
    }

    private void Start()
    {
        _bombStateHandler.Initialize(this);
        _bombhpHandler.Initialize(this);

        SetUp();
    }

    public override void SetUp()
    {
        _action = new State<BombMan>[3];

        _action[(int)BombManAction.idle] = new BombIdle();
        _action[(int)BombManAction.walk] = new BombWalk();
        _action[(int)(BombManAction.explode)] = new BombExplode();
        _actionMachine = new StateMachine<BombMan>();
        _actionMachine.SetUp(this, _action[(int)BombManAction.idle]);   
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

    public void ChangeActionState(BombManAction state)
    {
        _actionMachine.ChangeState(_action[(int)state]);
    }
}
