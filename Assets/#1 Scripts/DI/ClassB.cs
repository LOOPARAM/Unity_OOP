using UnityEngine;

namespace DependencyInjection
{
    public class ClassB : MonoBehaviour
    {
        [Inject] ServiceA serviceA;
        [Inject] ServiceB serviceB;
        FactoryA factoryA;

        [Inject]
        public void Init(FactoryA factoryA)
        {
            this.factoryA = factoryA;
        }
    }
}