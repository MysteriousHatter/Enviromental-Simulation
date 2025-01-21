using UnityEngine;

public class ButtonController : MonoBehaviour
{
     [SerializeField] private GateController gate; // Reference to the gate to control
    [SerializeField] private string TriggerTag;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag(TriggerTag))
        {
            gate.OpenGate();
        }
    }


    //private void OnCollisionExit(Collision collision)
    //{
    //    if(collision.gameObject.CompareTag(TriggerTag))
    //    {
    //        gate.CloseGateWithDelay(1f);
    //    }
    //}
}
