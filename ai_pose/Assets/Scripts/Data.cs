﻿using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
class BodyBone
{
    [SerializeField]
    private string name;
    [SerializeField]
    private Transform bone;
    [SerializeField]
    private Vector3 offset;
    [SerializeField]
    private float rate = 1;
    public string Name { get { return this.name; } }
    public Transform Bone { get { return this.bone; } }

    public void CreateBone(Transform root, string name, float scale = 1f, bool active = true)
    {
        if (this.bone == null)
        {
            this.name = name;
            this.bone = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            this.bone.localScale *= scale;
            this.bone.name = name;
            this.bone.SetParent(root);
            this.bone.gameObject.SetActive(active);
        }
    }

    public void SetPosition(Vector3 position, float duration = 0)
    {
        position += this.offset;
        if (this.bone != null && this.bone.gameObject.activeInHierarchy)
        {
            if (duration > 0)
                this.bone.DOMove(position, duration);
            else
                this.bone.position = position;
        }
    }

    public void SetLocalPosition(Vector3 localPosition, float duration = 0)
    {
        localPosition += this.offset;
        if (duration > 0)
            this.bone.DOLocalMove(localPosition, duration);
        else
            this.bone.localPosition = localPosition;
    }
}

[Serializable]
class GizmosData
{
    [SerializeField]
    private bool enable;
    [SerializeField]
    private List<BonePoints> datas;

    public bool Enable { get { return this.enable; } }
    public List<BonePoints> Datas { get { return this.datas; } }
}

[Serializable]
public class BonePoints
{
    [SerializeField]
    private Color lineColor;
    [SerializeField]
    private List<Transform> points;

    public Color LineColor { get { return this.lineColor; } }
    public List<Transform> Points { get { return this.points; } }
}

public class BoneData
{
    public string BoneName { get; set; }
    public Vector3 Position { get; set; }

    public BoneData(string boneName, float x, float y, float z)
    {
        this.BoneName = boneName;
        this.Position = new Vector3(x, y, z);
    }
}
