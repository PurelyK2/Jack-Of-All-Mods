using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InnerNet;

namespace JAM.Modules.ProjectorSpirit
{
    public class ProjSpiritObject : InnerNetObject
    {
        public PlayerControl? Owner;
        public Rigidbody2D Rigidbody;
        public ProjSpiritNetTransform NetTransform;
        public SpriteRenderer Rend;
    }
}