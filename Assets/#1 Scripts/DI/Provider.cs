using UnityEngine;
namespace DependencyInjection
{
    public class Provider : MonoBehaviour, IDependencyProvider
    {
        [Provide]
        public ServiceA ProvideServiceA()
        {
            return new ServiceA();
        }
        
        [Provide]
        public ServiceB ProvideServiceB()
        {
            return new ServiceB();
        }
        
        [Provide]
        public FactoryA ProvideFactoryA()
        {
            return new FactoryA();
        }
    }

    public class ServiceA
    {
        public void Initialize(string message = null)
        {
            Debug.Log($"ServiceA Initialize({message})");
        }
    }
    
    public class ServiceB
    {
        public void Initialize(string message = null)
        {
            Debug.Log($"ServiceB Initialize({message})");
        }
    }

    public class FactoryA
    {
        ServiceA cachedServiceA;

        public ServiceA CreateServiceA()
        {
            if (cachedServiceA == null)
            {
                cachedServiceA = new ServiceA();
            }
            return cachedServiceA;
        }
    }
}