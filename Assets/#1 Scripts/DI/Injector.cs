using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DependencyInjection
{
    // 필드와 메소드를 대상으로 지정할 수 있는 주입(Inject) Attribute 생성
    // InjectAttribute라는 이름으로 Attribute를 상속받았으나 실제 사용할때는 Inject, InjectAttribute 둘 중 아무거나 사용해도 됨
    // Inject Attribute를 지정해줌으로 인해 Injector 시스템이 주입할 멤버와 메소드를 판별할 수 있음
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
    public sealed class InjectAttribute : Attribute { }

    // 메소드만을 대상으로 지정할 수 있는 제공(Provide) Attribute 생성
    // 마찬가지로 사용할때는 Provide, ProvideAttribute 둘 중 아무거나 사용해도 됨
    // Provide Attribute를 지정해줌으로 인해 Inject 해줄 메소드를 판별할 수 있음
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ProvideAttribute : Attribute { }

    // 제공자 클래스에 부여할 인터페이스로 제공자 클래스를 탐색할 때 사용됨
    public interface IDependencyProvider { }

    // 싱글톤 클래스로 만들기 위해 Singleton을 상속받음
    // 타입을 Injector을 넘겨주는데 Injector는 Singleton을 상속받고, Singleton은 MonoBehaviour를 상속받고, MonoBehaviour는 Behaviour를, Behaviour는 Component를 상속받으므로
    // Singleton 클래스를 정의할때 지정했던 타입이 Component여야 한다는 조건을 만족시킴
    public class Injector : Singleton<Injector>
    {
        // Instance( 인스턴스 멤버만 가져옴 스태틱 멤버 X ), Public, NonPublic 필터링 플래그 ( Public과 NonPublic이 같이 있으니까 Static 제외하고 모든 멤버를 가져올 수 있는 필터라고 볼 수 있음 )
        // k_는 konstant ( 독일어 ) 에서 따와 const와 마찬가지로 상수의 의미를 지님
        private const BindingFlags k_bindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // 제공자 클래스로부터 제공받은 인스턴스를 그 타입과 함께 등록해둘 리스트 생성 및 초기화
        private readonly Dictionary<Type, object> registry = new Dictionary<Type, object>();

        // 싱글톤 클래스에서 정의된 Awake를 override로 재정의
        protected override void Awake()
        {
            // base.Awake()로 싱글톤 클래스에서 정의된 Awake를 호출함으로써 싱글톤 초기화작업 진행
            base.Awake();
            
            // 의존성 제공 역할을 하는 모든 모듈을 찾아서 인스턴스를 등록하기
            // 모든 MonoBehaviour 중에서 IDependencyProvider를 상속받은 모듈 가져오고 foreach로 하나씩 RegisterProvider 실행
            var providers = FindMonoBehaviours().OfType<IDependencyProvider>();
            foreach (var provider in providers)
            {
                RegisterProvider(provider);
            }
            
            // 제공받아 등록한 인스턴스를 실제로 주입시킬 대상들에 주입시키기
            // 모든 MonoBehaviour 중에서 Where를 사용해 IsInjectable을 만족시키는 MonoBehaviour만 가져오고 foreach로 각각 Inject 해주며 주입
            var injectables = FindMonoBehaviours().Where(IsInjectable);
            foreach (var injectable in injectables)
            {
                Inject(injectable);
            }
        }
        
        // 각각의 MonoBehaviour를 obj로 받아 Flags를 만족하는 멤버 ( 필드 & 메소드 )를 찾고, 모든 멤버중에서 단 하나라도 Inject Attribute를 가지고 있다면 참을 반환
        static bool IsInjectable(MonoBehaviour obj)
        {
            // k_bindingFlags에 의해 static을 제외하고 모든 멤버를 가져옴 ( MemberInfo[] 타입을 가짐 )
            var members = obj.GetType().GetMembers(k_bindingFlags);
            return members.Any(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
        }
        
        // 제공자 클래스에 존재하는 모든 제공자 메소드를 찾고 해당 메소드를 실행한 후 생기는 인스턴스들을 registry에 추가
        void RegisterProvider(IDependencyProvider provider)
        {
            var methods = provider.GetType().GetMethods(k_bindingFlags);

            foreach (var method in methods)
            {
                // Provide Attribute가 없으면 패스!
                if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;

                // 제공자 메소드의 반환 타입이자 주입될 객체의 타입을 지정하고 주입될 객체를 생성해 저장
                var returnType = method.ReturnType;
                var provideInstance = method.Invoke(provider, null);
                
                // 객체가 성공적으로 생성이 되었다면 그 타입과 함께 registry에 등록
                if (provideInstance != null)
                {
                    registry.Add(returnType, provideInstance);
                    Debug.Log($"Provider {returnType.Name} from {provider.GetType().Name}");
                }
                else
                {
                    throw new Exception($"Provider {provider.GetType().Name} returned null for {returnType.Name}");
                }
            }
        }
        
        // InstanceID를 기준으로 MonoBehaviour를 모두 찾아 정렬해 반환
        static MonoBehaviour[] FindMonoBehaviours()
        {
            return FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID);
        }
        
        // 제공자로부터 생성되어 registry에 있던 Instance들을 실제 주입대상들에게 주입해주는 메소드
        private void Inject(object instance)
        {
            // 주입받을 모듈의 타입
            var type = instance.GetType();
            
            // 주입받아야할 모든 필드 가져오기
            var injectableFields = type.GetFields(k_bindingFlags)
                .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            // 모든 필드를 순회하며 Resolve를 주입할 대상을 가져오고 SetValue로 주입해주기
            foreach (var injectableField in injectableFields)
            {
                var fieldType = injectableField.FieldType;
                var resolvedInstance = Resolve(fieldType);
                if (resolvedInstance == null)
                {
                    throw new Exception($"Failed to inject {fieldType.Name} into {type.Name}");
                }
                
                injectableField.SetValue(instance, resolvedInstance);
                Debug.Log($"Injected {fieldType.Name} into {type.Name}");
            }
            
            // 주입받아야할 모든 메소드 가져오기
            var injectableMethods = type.GetMethods(k_bindingFlags)
                .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            // 모든 메소드를 순회하며 필요한 파라미터의 타입들, 파라미터의 할당될 모듈들 가져와 Invoke로 실행
            foreach (var injectableMethod in injectableMethods)
            {
                var requiredPrameters = injectableMethod.GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .ToArray();
                var resolvedInstances = requiredPrameters.Select(Resolve).ToArray();
                if (resolvedInstances.Any(resolvedInstance => resolvedInstance == null))
                {
                    throw new Exception($"Failed to inject {type.Name}.{injectableMethod.Name}");
                }
                
                injectableMethod.Invoke(instance, resolvedInstances);
                Debug.Log($"Method injected {type.Name}.{injectableMethod.Name}");
            }
        }

        // 원하는 타입의 인스턴스를 가져와 반환
        object Resolve(Type type)
        {
            registry.TryGetValue(type, out var resolvedInstance);
            return resolvedInstance;
        }
    }
}