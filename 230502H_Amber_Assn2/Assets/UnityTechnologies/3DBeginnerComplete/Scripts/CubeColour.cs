using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HelloWorld
{
    public class CubeColour : MonoBehaviour
    {
        public PlayerMovement script;
        private void Start()
        {
            Color[] ColorCube = new[] {new Color(1.0f, 0.0f, 0.0f),
                                        new Color(0.0f, 1.0f, 0.0f),
                                        new Color(0.0f, 0.0f, 1.0f),
                                        new Color(1.0f, 1.0f, 0.0f),};
            script = transform.parent.GetComponent<PlayerMovement>();
            foreach(Renderer r in GetComponentsInChildren<Renderer>())
                r.material.color = ColorCube[script.PlayerNum.Value];
        }
    }
}
