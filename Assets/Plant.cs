using UnityEngine;

public class Plant : MonoBehaviour
{
    public GameObject plantobj;
private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            Debug.Log("Hit");
            GameObject plant = Instantiate(plantobj, gameObject.transform.position, gameObject.transform.rotation);
            Destroy(this.gameObject);
        }
    }
}
