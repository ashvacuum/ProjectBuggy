using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;

namespace ShipECS.Systems
{
    [UpdateInGroup(typeof(PausableSystemGroup))]
    public partial class InputSystem : SystemBase
    {
        private InputSystem_Actions inputs = null;
        protected override void OnCreate()
        {
            inputs = new InputSystem_Actions();
            inputs.Enable();
        }
        protected override void OnUpdate()
        {
            foreach (RefRW<InputsData> data in SystemAPI.Query<RefRW<InputsData>>())
            {
                data.ValueRW.move = inputs.Player.Move.ReadValue<Vector2>();
            }   
        }
    }
}
