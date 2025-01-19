using UnityEngine;
using System.Collections;

public class GateController : MonoBehaviour
{

    [SerializeField] private float openHeight = 3f; 
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private float closeSpeed = 2f;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpening = false;
    private bool isClosed = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Store the gate's inital and open postions
        closedPosition = transform.position;
        openPosition = closedPosition + new Vector3(0, openHeight, 0);
    }

    // Update is called once per frame
    void Update()
    {
        OpenAndCloseGate();
    }

    private void OpenAndCloseGate()
    {
        if (isOpening)
        {
            // Move the gate to the open position
            transform.position = Vector3.MoveTowards(transform.position, openPosition, openSpeed * Time.deltaTime);

            if (transform.position == openPosition)
                isOpening = false;
        }
        else if (isClosed)
        {
            // Move the gate to the closed position
            transform.position = Vector3.MoveTowards(transform.position, closedPosition, closeSpeed * Time.deltaTime);

            if (transform.position == closedPosition)
                isClosed = false;
        }
    }

    public void OpenGate()
    {
        isOpening = true;
        Debug.Log("Open Gate");
        isClosed = false; // Stop closing if it's already closing
    }

    public void CloseGateWithDelay(float delay)
    {
        StartCoroutine(CloseGateAfterDelay(delay));
    }

    private IEnumerator CloseGateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseGate();
    }

    public void CloseGate()
    {
        isClosed = true;
        Debug.Log("Close Gate");
        isOpening = false; // Stop opening if it's already opening
    }
}
