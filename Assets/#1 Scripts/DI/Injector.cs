using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DependencyInjection
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
    public sealed class InjectAttribute : Attribute
    {
        public InjectAttribute () { }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ProvideAttribute : Attribute
    {
        public ProvideAttribute() { }
    }

    public interface IDependencyProvider { }

    // 싱글톤 클래스로 만들기 위해 Singleton을 상속받음
    // 타입을 Injector을 넘겨주는데 Injector는 Singleton을 상속받고, Singleton은 MonoBehaviour를 상속받고, MonoBehaviour는 Behaviour를, Behaviour는 Component를 상속받으므로
    // Singleton 클래스를 정의할때 지정했던 타입이 Component여야 한다는 조건을 만족시킴
    public class Injector : Singleton<Injector>
    {
        // Instance( 인스턴스 멤버만 가져옴 스태틱 멤버 X ), Public, NonPublic 필터링 플래그 ( Public과 NonPublic이 같이 있으니까 Static 제외하고 모든 멤버를 가져올 수 있는 필터라고 볼 수 있음 )
        private const BindingFlags k_bindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // 
        private readonly Dictionary<Type, object> registry = new Dictionary<Type, object>();

        // 싱글톤 클래스에서 정의된 Awake를 override로 재정의하며 base.Awake()로 싱글톤 클래스를 초기화 한 후 추가 기능 구현
        protected override void Awake()
        {
            base.Awake();
            
            // Find all modules implementing IDependencyProvider
            var providers = FindMonoBehaviours().OfType<IDependencyProvider>();
            foreach (var provider in providers)
            {
                RegisterProvider(provider);
            }
            
            // Find all injectable objects and inject their dependencies
            var injectables = FindMonoBehaviours().Where(IsInjectable);
            foreach (var injectable in injectables)
            {
                Inject(injectable);
            }
        }

        private void Inject(object instance)
        {
            var type = instance.GetType();
            var injectableFields = type.GetFields(k_bindingFlags)
                .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

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
            
            var injectableMethods = type.GetMethods(k_bindingFlags)
                .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

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

        object Resolve(Type type)
        {
            registry.TryGetValue(type, out var resolvedInstance);
            return resolvedInstance;
        }

        static bool IsInjectable(MonoBehaviour obj)
        {
            var members = obj.GetType().GetMembers(k_bindingFlags);
            return members.Any(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
        }
        
        void RegisterProvider(IDependencyProvider provider)
        {
            var methods = provider.GetType().GetMethods(k_bindingFlags);

            foreach (var method in methods)
            {
                if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;

                var returnType = method.ReturnType;
                var provideInstance = method.Invoke(provider, null);
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

        static MonoBehaviour[] FindMonoBehaviours()
        {
            return FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID);
        }
    }
}