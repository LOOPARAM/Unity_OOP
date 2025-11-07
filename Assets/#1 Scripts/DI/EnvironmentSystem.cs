using UnityEngine;

namespace DependencyInjection
{
    public interface IEnvironmentSystem
    {
        EnvironmentSystem ProvideEnvironmentSystem();
        void Initialize();
    }

    public class EnvironmentSystem : MonoBehaviour, IDependencyProvider, IEnvironmentSystem
    {
        [Provide]
        public EnvironmentSystem ProvideEnvironmentSystem()
        {
            return this;
        }

        public void Initialize()
        {
            Debug.Log("EnvironmentSystem.Initialize()");
        }
    }
}