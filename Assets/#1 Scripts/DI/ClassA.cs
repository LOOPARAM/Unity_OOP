using System;
using UnityEngine;

namespace DependencyInjection
{
    public class ClassA : MonoBehaviour
    {
        ServiceA serviceA;

        [Inject]
        public void Init(ServiceA serviceA)
        {
            this.serviceA = serviceA;
        }

        private void Start()
        {
            serviceA.Initialize("ServiceA initialized from ClassA");
        }
    }
}