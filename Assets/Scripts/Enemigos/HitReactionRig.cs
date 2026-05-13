using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections;
using System.Collections.Generic;

public class HitReactionRig : MonoBehaviour
{
    [System.Serializable]
    public struct BoneGroup
    {
        public string label;
        public HumanBodyBones rootBone;
        [Range(0f, 90f)] public float maxAngle;
        [Range(1f, 50f)] public float stiffness;
        [Range(0.5f, 20f)] public float damping;
        [Range(0f, 0.5f)] public float pushDistance;
    }

    [SerializeField] private BoneGroup[] _boneGroups = new BoneGroup[]
    {
        new BoneGroup { label = "Head", rootBone = HumanBodyBones.Head, maxAngle = 25f, stiffness = 22f, damping = 7f, pushDistance = 0.05f },
        new BoneGroup { label = "Torso", rootBone = HumanBodyBones.UpperChest, maxAngle = 35f, stiffness = 14f, damping = 5f, pushDistance = 0.15f },
        new BoneGroup { label = "Left Arm", rootBone = HumanBodyBones.LeftUpperArm, maxAngle = 70f, stiffness = 7f, damping = 3f, pushDistance = 0.20f },
        new BoneGroup { label = "Right Arm", rootBone = HumanBodyBones.RightUpperArm, maxAngle = 70f, stiffness = 7f, damping = 3f, pushDistance = 0.20f },
        new BoneGroup { label = "Left Leg", rootBone = HumanBodyBones.LeftUpperLeg, maxAngle = 30f, stiffness = 20f, damping = 6f, pushDistance = 0.10f },
        new BoneGroup { label = "Right Leg", rootBone = HumanBodyBones.RightUpperLeg, maxAngle = 30f, stiffness = 20f, damping = 6f, pushDistance = 0.10f },
    };

    [SerializeField] private float _minForce = 1f;
    [SerializeField] private float _maxForce = 20f;
    [SerializeField][Range(0f, 1f)] private float _childFalloff = 0.5f;

    private Animator _animator;
    private RigBuilder _rigBuilder;

    private Dictionary<HumanBodyBones, OverrideTransform> _constraints;
    private Dictionary<HumanBodyBones, Transform> _targets;
    private Dictionary<HumanBodyBones, BoneGroup> _configs;
    private Dictionary<HumanBodyBones, HumanBodyBones> _boneToGroupRoot;
    private Dictionary<OverrideTransform, Coroutine> _activeCoroutines;

    private static readonly Dictionary<HumanBodyBones, HumanBodyBones[]> _childToParents = new()
    {
        { HumanBodyBones.LeftHand, new[] { HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftUpperArm } },
        { HumanBodyBones.LeftLowerArm, new[] { HumanBodyBones.LeftUpperArm } },
        { HumanBodyBones.RightHand, new[] { HumanBodyBones.RightLowerArm, HumanBodyBones.RightUpperArm } },
        { HumanBodyBones.RightLowerArm, new[] { HumanBodyBones.RightUpperArm } },
        { HumanBodyBones.LeftFoot, new[] { HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftUpperLeg } },
        { HumanBodyBones.LeftLowerLeg, new[] { HumanBodyBones.LeftUpperLeg } },
        { HumanBodyBones.RightFoot, new[] { HumanBodyBones.RightLowerLeg, HumanBodyBones.RightUpperLeg } },
        { HumanBodyBones.RightLowerLeg, new[] { HumanBodyBones.RightUpperLeg } },
        { HumanBodyBones.Neck, new[] { HumanBodyBones.Head } },
        { HumanBodyBones.Chest, new[] { HumanBodyBones.UpperChest } },
        { HumanBodyBones.Spine, new[] { HumanBodyBones.Chest, HumanBodyBones.UpperChest } },
    };

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigBuilder = GetComponent<RigBuilder>();

        if (_rigBuilder == null)
            _rigBuilder = gameObject.AddComponent<RigBuilder>();

        BuildRigHierarchy();
        BuildBoneToGroupMap();
    }

    private void Start()
    {
        _rigBuilder.Build();
    }

    private void BuildRigHierarchy()
    {
        var rigGO = new GameObject("HitReactionRig");
        rigGO.transform.SetParent(transform, false);
        rigGO.transform.localPosition = Vector3.zero;
        rigGO.transform.localRotation = Quaternion.identity;

        var rig = rigGO.AddComponent<Rig>();
        rig.weight = 1f;

        if (_rigBuilder.layers == null)
            _rigBuilder.layers = new List<RigLayer>();
        _rigBuilder.layers.Add(new RigLayer(rig, true));

        _constraints = new Dictionary<HumanBodyBones, OverrideTransform>();
        _targets = new Dictionary<HumanBodyBones, Transform>();
        _configs = new Dictionary<HumanBodyBones, BoneGroup>();
        _activeCoroutines = new Dictionary<OverrideTransform, Coroutine>();

        foreach (var group in _boneGroups)
        {
            var boneTransform = _animator.GetBoneTransform(group.rootBone);
            if (boneTransform == null) continue;

            // Target transform (initially at bone's world position)
            var targetGO = new GameObject($"Target_{group.rootBone}");
            targetGO.transform.SetParent(rigGO.transform, false);
            targetGO.transform.position = boneTransform.position;
            targetGO.transform.rotation = boneTransform.rotation;

            // OverrideTransform constraint
            var constraintGO = new GameObject($"Override_{group.rootBone}");
            constraintGO.transform.SetParent(rigGO.transform, false);

            var constraint = constraintGO.AddComponent<OverrideTransform>();
            var data = constraint.data;
            data.constrainedObject = boneTransform;
            data.sourceObject = targetGO.transform;
            data.positionWeight = 1f;
            data.rotationWeight = 1f;
            data.space = OverrideTransformData.Space.World;
            constraint.data = data;
            constraint.weight = 0f;

            _constraints[group.rootBone] = constraint;
            _targets[group.rootBone] = targetGO.transform;
            _configs[group.rootBone] = group;
        }
    }

    private void BuildBoneToGroupMap()
    {
        _boneToGroupRoot = new Dictionary<HumanBodyBones, HumanBodyBones>();

        foreach (var group in _boneGroups)
            _boneToGroupRoot[group.rootBone] = group.rootBone;

        _boneToGroupRoot[HumanBodyBones.Neck] = HumanBodyBones.Head;
        _boneToGroupRoot[HumanBodyBones.Chest] = HumanBodyBones.UpperChest;
        _boneToGroupRoot[HumanBodyBones.Spine] = HumanBodyBones.UpperChest;
        _boneToGroupRoot[HumanBodyBones.LeftShoulder] = HumanBodyBones.LeftUpperArm;
        _boneToGroupRoot[HumanBodyBones.LeftLowerArm] = HumanBodyBones.LeftUpperArm;
        _boneToGroupRoot[HumanBodyBones.LeftHand] = HumanBodyBones.LeftUpperArm;
        _boneToGroupRoot[HumanBodyBones.RightShoulder] = HumanBodyBones.RightUpperArm;
        _boneToGroupRoot[HumanBodyBones.RightLowerArm] = HumanBodyBones.RightUpperArm;
        _boneToGroupRoot[HumanBodyBones.RightHand] = HumanBodyBones.RightUpperArm;
        _boneToGroupRoot[HumanBodyBones.LeftUpperLeg] = HumanBodyBones.LeftUpperLeg;
        _boneToGroupRoot[HumanBodyBones.LeftLowerLeg] = HumanBodyBones.LeftUpperLeg;
        _boneToGroupRoot[HumanBodyBones.LeftFoot] = HumanBodyBones.LeftUpperLeg;
        _boneToGroupRoot[HumanBodyBones.LeftToes] = HumanBodyBones.LeftUpperLeg;
        _boneToGroupRoot[HumanBodyBones.RightUpperLeg] = HumanBodyBones.RightUpperLeg;
        _boneToGroupRoot[HumanBodyBones.RightLowerLeg] = HumanBodyBones.RightUpperLeg;
        _boneToGroupRoot[HumanBodyBones.RightFoot] = HumanBodyBones.RightUpperLeg;
        _boneToGroupRoot[HumanBodyBones.RightToes] = HumanBodyBones.RightUpperLeg;
    }

    public void TriggerHit(HumanBodyBones hitBone, Vector3 force)
    {
        if (!_boneToGroupRoot.TryGetValue(hitBone, out var groupRoot))
            groupRoot = HumanBodyBones.UpperChest;

        ApplyHitToConstraint(groupRoot, force, 1f);

        if (_childToParents.TryGetValue(hitBone, out var parentBones))
        {
            float falloff = _childFalloff;
            foreach (var parentBone in parentBones)
            {
                if (!_boneToGroupRoot.TryGetValue(parentBone, out var parentGroup)) continue;
                if (parentGroup == groupRoot) continue;
                ApplyHitToConstraint(parentGroup, force, falloff);
                falloff *= _childFalloff;
            }
        }
    }

    private void ApplyHitToConstraint(HumanBodyBones groupRoot, Vector3 force, float intensity)
    {
        if (!_constraints.TryGetValue(groupRoot, out var constraint))
        {
            Debug.LogWarning($"[HitReactionRig] No constraint found for {groupRoot}");
            return;
        }
        if (!_configs.TryGetValue(groupRoot, out var config)) return;
        if (!_targets.TryGetValue(groupRoot, out var target)) return;

        float t = Mathf.InverseLerp(_minForce, _maxForce, force.magnitude);

        Vector3 localForce = transform.InverseTransformDirection(-force.normalized);
        Quaternion fullRotation = Quaternion.FromToRotation(Vector3.forward, localForce);
        fullRotation.ToAngleAxis(out float originalAngle, out Vector3 axis);
        float degrees = Mathf.Lerp(0f, config.maxAngle, t) * intensity;
        degrees = Mathf.Min(originalAngle * intensity, degrees);
        Quaternion hitRotation = Quaternion.AngleAxis(degrees, axis);

        var boneTransform = constraint.data.constrainedObject;
        target.position = boneTransform.position + force.normalized * config.pushDistance * t * intensity;
        target.rotation = boneTransform.rotation * hitRotation;

        if (_activeCoroutines.TryGetValue(constraint, out var existing) && existing != null)
            StopCoroutine(existing);

        _activeCoroutines[constraint] = StartCoroutine(SpringDamper(constraint, config.stiffness, config.damping));
    }

    private IEnumerator SpringDamper(OverrideTransform constraint, float stiffness, float damping)
    {
        float weight = 1f;
        float velocity = 0f;

        while (Mathf.Abs(weight) > 0.001f || Mathf.Abs(velocity) > 0.001f)
        {
            float springForce = (0f - weight) * stiffness;
            velocity += (springForce - velocity * damping) * Time.deltaTime;
            weight += velocity * Time.deltaTime;
            constraint.weight = Mathf.Clamp01(weight);
            yield return null;
        }

        constraint.weight = 0f;
        _activeCoroutines[constraint] = null;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
