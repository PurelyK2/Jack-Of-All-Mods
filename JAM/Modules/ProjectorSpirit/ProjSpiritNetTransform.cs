using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InnerNet;

namespace JAM.Modules.ProjectorSpirit
{
    public class ProjSpiritNetTransform : InnerNetObject
    {
        private bool isPaused;
        private Rigidbody2D body;
        private Queue<Vector2> sendQueue = new Queue<Vector2>();
        private Queue<Vector2> incomingPosQueue = new Queue<Vector2>();
        private float rubberbandModifier = 1f;
        private float idealSpeed;
        private ushort lastSequenceId;
        private Vector2 lastPosition;
        private Vector2 lastPosSent;
        private Vector2? tempSnapPosition;
    }
}