using System;
using UnityEngine;

public enum PlayerStats
{
    BodyIsGround=0,
    HeadIsGround,
    CanControlArrow,
    IsOnClick,
    IsFly,
    IsArrowOnWall,
    IsCollisionMethod2,
    IsCollision,
    CanShoot,
    CanCombine,
    IsCombine,
    Push,
    HasArrow,
}

public class Player : MonoBehaviour
{
    //플레이어가 가질 수 있는 모든 상태 개수
    private static readonly int StateCount = Enum.GetValues(typeof(PlayerStats)).Length;
    //플레이어가 가질 수 있는 모든 상태들 배열
    private State<Player>[] _states;
    private StateManager<Player> _stateManager;
    
    //머리,몸,화살 오브젝트
    public GameObject Body;
    public GameObject Head;
    public GameObject Arrow;
    
    //기본 설정
    public void Start()
    {
        //_states 초기화
        _states = new State<Player>[StateCount];
        _states[(int)PlayerStats.BodyIsGround] = new PlayerOwnedStates.BodyIsGround();
        _states[(int)PlayerStats.HeadIsGround] = new PlayerOwnedStates.HeadIsGround();
        _states[(int)PlayerStats.CanControlArrow] = new PlayerOwnedStates.CanControlArrow();
        _states[(int)PlayerStats.IsOnClick] = new PlayerOwnedStates.IsOnClick();
        _states[(int)PlayerStats.IsFly] = new PlayerOwnedStates.IsFly();
        _states[(int)PlayerStats.IsArrowOnWall] = new PlayerOwnedStates.IsArrowOnWall();
        _states[(int)PlayerStats.IsCollisionMethod2] = new PlayerOwnedStates.IsCollisionMethod2();
        _states[(int)PlayerStats.IsCollision] = new PlayerOwnedStates.IsCollision();
        _states[(int)PlayerStats.CanShoot] = new PlayerOwnedStates.CanShoot();
        _states[(int)PlayerStats.CanCombine] = new PlayerOwnedStates.CanCombine();
        _states[(int)PlayerStats.IsCombine] = new PlayerOwnedStates.IsCombine();
        _states[(int)PlayerStats.Push] = new PlayerOwnedStates.Push();
        _states[(int)PlayerStats.HasArrow] = new PlayerOwnedStates.HasArrow();
        
        //상태 매니저 생성 및 초기화
        _stateManager = new StateManager<Player>();
        _stateManager.Setup(this,StateCount,_states);
        
        //바로 등록해야할 상태 등록
        AddState(PlayerStats.CanShoot);
    }
    
    //업데이트 메소드
    public void Update()
    {
        //상태 매니저의 Execute실행
        _stateManager.Execute();
    }

    //상태 추가 메소드
    public void AddState(PlayerStats ps)
    {
        State<Player> newState = _states[(int)ps];
        _stateManager.AddState(newState);
    }
    
    //상태 제거 메소드
    public void RemoveState(PlayerStats ps)
    {
        State<Player> remState = _states[(int)ps];
        _stateManager.RemoveState(remState);
    }
    //상태 있는지 체크
    public bool IsContainState(PlayerStats ps)
    {
        return _stateManager._currentState.Contains(_states[(int)ps]);
    }
}