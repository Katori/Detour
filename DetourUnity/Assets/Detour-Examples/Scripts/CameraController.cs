using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Detour.Examples.Client
{
    public class CameraController : MonoBehaviour
    {
        internal static CameraController Instance { get; private set; }

        readonly Vector3 StartForOffset = new Vector3(0, 0, -3);

        Vector3 Offset;

        internal Transform PlayerToWatch;

        // Use this for initialization
        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            Offset = transform.position - StartForOffset;
        }

        // Update is called once per frame
        void Update()
        {
            if (PlayerToWatch != null)
            {
                transform.position = PlayerToWatch.position + Offset;
            }
        }
    }
}