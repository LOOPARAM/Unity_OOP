//어떤 객체의 상태도 의미할 수 있도록 제네릭 클래스이자 추상 클래스 사용
//추후 실제 상태의 Enter, Execute, Exit을 구현할 부분에서 관리하기 편하게 생성해두는 추상 클래스
public abstract class State<T> where T : class
{
    /// <summary>
    /// 상태가 추가될때 1회 호출되는 메소드
    /// </summary>
    /// <param name="entity"></param>
    public abstract void Enter(T entity);
    /// <summary>
    /// 상태 유지중일때 매 프레임 호출되는 메소드
    /// </summary>
    /// <param name="entity"></param>
    public abstract void Execute(T entity);
    /// <summary>
    /// 상태가 제거될때 1회 호출되는 메소드 
    /// </summary>
    /// <param name="entity"></param>
    public abstract void Exit(T entity);
}