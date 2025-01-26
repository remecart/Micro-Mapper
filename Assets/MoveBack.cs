using System.Windows.Forms;
using UnityEngine;

public class MoveBack : MonoBehaviour
{
    public static MoveBack instance;
    public Transform grid;

    void Start()
    {
        instance = this;
    }

    public void MB()
    {
        transform.position =
            new Vector3(0, 0, -grid.localPosition.z);
    }
    // Update is called once per frame
    void Update()
    {
        MB();
    }
}
