using UnityEngine;
using UnityEngine.AI;

public enum archerAction
{
    Wander, Detect, Attack
}

public class Archer : CharacterBase
{
    [SerializeField] private Animator _animator;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Area wanderArea;
    [SerializeField] private Transform _handlerTransform;
    [SerializeField] private GameObject _bow;

    public State<Archer>[] _action;
    public StateMachine<Archer> _actionMachine;

    private Rigidbody _rigidbody;
    private archerStatehandler _archerStatehandler;
    private monsterShoot _monsterShoot;
    public Animator Animator => _animator;
    public NavMeshAgent Agent => _agent;
    public Rigidbody Rigidbody => _rigidbody;
    public NavMeshAgent NavMeshAgent => _agent;
    public Area WanderArea => wanderArea;
    public Transform HandlerTransform => _handlerTransform;
    public GameObject bow => _bow;
    public archerStatehandler archerStatehandler => _archerStatehandler;
    public monsterShoot monsterShoot => _monsterShoot;

    protected override void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();

        _archerStatehandler = GetComponent<archerStatehandler>();
        _monsterShoot = bow.GetComponent<monsterShoot>();

    }

    private void Start()
    {
        archerStatehandler.Initialize(this);
        monsterShoot.Initialize(this);

        SetUp();
    }

    public override void SetUp()
    {
        _action = new State<Archer>[3];

        _action[(int)archerAction.Wander] = new archerWanderState();
        _action[(int)archerAction.Detect] = new archerDetectState();
        _action[(int)archerAction.Attack] = new archerAttackState();
        _actionMachine = new StateMachine<Archer>();
        _actionMachine.SetUp(this, _action[(int)archerAction.Wander]);
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

    public void ChangeActionState(archerAction state)
    {
        _actionMachine.ChangeState(_action[(int)state]);
    }
}
