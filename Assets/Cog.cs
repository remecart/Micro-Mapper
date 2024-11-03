using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cog : MonoBehaviour
{
    public Animator animatior;

    public void OnHover()
    {
        animatior.SetTrigger("Selected");
    }

    public void OnHoverExit()
    {
        animatior.SetTrigger("Deselected");
    }
}
