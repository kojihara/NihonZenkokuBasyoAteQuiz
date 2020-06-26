using UnityEngine;

public class LoadingIconScript : MonoBehaviour
{
    private Vector3 angle;

    void Awake()
    {
        angle = new Vector3(0, 0, -1);
    }

    void Update()
    {
        transform.Rotate(angle);
    }
}
