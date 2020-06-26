using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneBase : MonoBehaviour
{
    public virtual void OnLoad(object options = null) { }

    protected virtual void Back() { }
}
