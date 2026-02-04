using UnityEngine;

public class MinimapIcon : MonoBehaviour
{

    public Transform target;

    private void LateUpdate()
    {
        transform.position = new Vector3(target.transform.position.x, transform.position.y, target.position.z);
        transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0);
    }
}
