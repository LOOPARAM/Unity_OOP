using UnityEngine;

//제네릭 클래스로 만들어 어떤 타입도 싱글톤으로 동작하게 할 수 있음
//Singleton자체가 MonoBehaviour를 상속받으며 T타입은 유니티의 Component를 상속받아야함
public class Singleton<T> : MonoBehaviour where T : Component
{
    // 단 하나의 싱글톤 인스턴
    protected static T instance;
    // 싱글톤 인스턴스가 존재하는지 확인하는 프로퍼티
    public static bool HasInstance => instance != null;
    // 인스턴스를 가져오는 메소드, 있으면 인스턴스를 없으면 null 반환
    public static T TryGetInstance() => HasInstance ? instance : null;
    // 현재 인스턴스 직접 반환 / 없으면 null
    public static T Current => instance;

    // Instance를 호출할때 instance가 없으면 할당해주기, 근데 할당해줄 인스턴스가 없으면 임의로 생성해서 할당 후 무조건 instance 반환 // 앞선 Current와 차이가 있음
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name + "AutoCreated";
                    instance = obj.AddComponent<T>();
                }
            }

            return instance;
        }
    }

    // 자식 클래스에서 재정의할 수 있도록 virtual을 통해 유니티 이벤트 함수 Awake를 구성함
    // 기본적으로 싱글톤 초기화 메소드를 호출함
    protected virtual void Awake() => InitializeSingleton();
    
    // 싱글톤 초기화 메소드
    // 에디터모드에서 Awake의 호출로 인스턴스가 잘못 설정되는것 방지
    protected virtual void InitializeSingleton()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        instance = this as T;
    }
}