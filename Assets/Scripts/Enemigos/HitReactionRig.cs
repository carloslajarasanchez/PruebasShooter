using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections;
using System.Collections.Generic;

public class HitReactionRig : MonoBehaviour
{
    [System.Serializable]
    public struct BoneGroup
    {
        public enum ConstraintType { OverrideTransform, TwoBoneIK }

        public string label;
        public HumanBodyBones rootBone;
        public ConstraintType constraintType;
        [Range(0f, 90f)] public float maxAngle;
        [Range(1f, 50f)] public float stiffness;
        [Range(0.5f, 20f)] public float damping;
        [Range(0f, 0.5f)] public float pushDistance;
    }

    [SerializeField] private BoneGroup[] _boneGroups = new BoneGroup[]
    {
        new BoneGroup { label = "Head", rootBone = HumanBodyBones.Head, constraintType = BoneGroup.ConstraintType.OverrideTransform, maxAngle = 25f, stiffness = 22f, damping = 7f, pushDistance = 0.05f },
        new BoneGroup { label = "Torso", rootBone = HumanBodyBones.UpperChest, constraintType = BoneGroup.ConstraintType.OverrideTransform, maxAngle = 35f, stiffness = 14f, damping = 5f, pushDistance = 0.15f },
        new BoneGroup { label = "Left Arm", rootBone = HumanBodyBones.LeftUpperArm, constraintType = BoneGroup.ConstraintType.TwoBoneIK, maxAngle = 70f, stiffness = 7f, damping = 3f, pushDistance = 0.20f },
        new BoneGroup { label = "Right Arm", rootBone = HumanBodyBones.RightUpperArm, constraintType = BoneGroup.ConstraintType.TwoBoneIK, maxAngle = 70f, stiffness = 7f, damping = 3f, pushDistance = 0.20f },
        new BoneGroup { label = "Left Leg", rootBone = HumanBodyBones.LeftUpperLeg, constraintType = BoneGroup.ConstraintType.TwoBoneIK, maxAngle = 30f, stiffness = 20f, damping = 6f, pushDistance = 0.10f },
        new BoneGroup { label = "Right Leg", rootBone = HumanBodyBones.RightUpperLeg, constraintType = BoneGroup.ConstraintType.TwoBoneIK, maxAngle = 30f, stiffness = 20f, damping = 6f, pushDistance = 0.10f },
    };

    [SerializeField] private float _minForce = 1f;
    [SerializeField] private float _maxForce = 20f;
    [SerializeField][Range(0f, 1f)] private float _childFalloff = 0.5f;

    private Animator _animator;
    private RigBuilder _rigBuilder;

    private Dictionary<HumanBodyBones, OverrideTransform> _overrideConstraints;
    private Dictionary<HumanBodyBones, TwoBoneIKConstraint> _ikConstraints;
    private Dictionary<HumanBodyBones, Transform> _overrideTargets;
    private Dictionary<HumanBodyBones, Transform> _ikTargets;
    private Dictionary<HumanBodyBones, BoneGroup> _configs;
    private Dictionary<HumanBodyBones, HumanBodyBones> _boneToGroupRoot;
    private Dictionary<HumanBodyBones, Coroutine> _activeCoroutines;

    private static readonly Dictionary<HumanBodyBones, (HumanBodyBones mid, HumanBodyBones tip)> _ikChains = new()
    {
        { HumanBodyBones.LeftUpperArm, (HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand) },
        { HumanBodyBones.RightUpperArm, (HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand) },
        { HumanBodyBones.LeftUpperLeg, (HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot) },
        { HumanBodyBones.RightUpperLeg, (HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot) },
    };

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

        _overrideConstraints = new Dictionary<HumanBodyBones, OverrideTransform>();
        _ikConstraints = new Dictionary<HumanBodyBones, TwoBoneIKConstraint>();
        _overrideTargets = new Dictionary<HumanBodyBones, Transform>();
        _ikTargets = new Dictionary<HumanBodyBones, Transform>();
        _configs = new Dictionary<HumanBodyBones, BoneGroup>();
        _activeCoroutines = new Dictionary<HumanBodyBones, Coroutine>();

        foreach (var group in _boneGroups)
        {
            var boneTransform = _animator.GetBoneTransform(group.rootBone);
            if (boneTransform == null) continue;
            _configs[group.rootBone] = group;

            if (group.constraintType == BoneGroup.ConstraintType.TwoBoneIK)
            {
                if (!_ikChains.TryGetValue(group.rootBone, out var chain)) continue;
                var midBone = _animator.GetBoneTransform(chain.mid);
                var tipBone = _animator.GetBoneTransform(chain.tip);
                if (midBone == null || tipBone == null) continue;

                var targetGO = new GameObject($"Target_{group.rootBone}_IK");
                targetGO.transform.SetParent(rigGO.transform, false);
                targetGO.transform.position = tipBone.position;
                targetGO.transform.rotation = tipBone.rotation;

                var hintGO = new GameObject($"Hint_{group.rootBone}");
                hintGO.transform.SetParent(rigGO.transform, false);
                Vector3 rootToMid = (midBone.position - boneTransform.position).normalized;
                Vector3 midToTip = (tipBone.position - midBone.position).normalized;
                Vector3 hintDir = Vector3.Cross(rootToMid, midToTip);
                if (hintDir.sqrMagnitude < 0.001f) hintDir = transform.up;
                else hintDir.Normalize();
                if (group.rootBone is HumanBodyBones.LeftUpperArm or HumanBodyBones.LeftUpperLeg)
                    hintDir = -hintDir;
                hintGO.transform.position = midBone.position + hintDir * 0.3f;

                var constraintGO = new GameObject($"IK_{group.rootBone}");
                constraintGO.transform.SetParent(rigGO.transform, false);
                var ikConstraint = constraintGO.AddComponent<TwoBoneIKConstraint>();
                var ikData = ikConstraint.data;
                ikData.root = boneTransform;
                ikData.mid = midBone;
                ikData.tip = tipBone;
                ikData.target = targetGO.transform;
                ikData.hint = hintGO.transform;
                ikData.targetPositionWeight = 1f;
                ikData.targetRotationWeight = 1f;
                ikData.hintWeight = 1f;
                ikData.maintainTargetPositionOffset = false;
                ikConstraint.data = ikData;
                ikConstraint.weight = 0f;

                _ikConstraints[group.rootBone] = ikConstraint;
                _ikTargets[group.rootBone] = targetGO.transform;
            }
            else
            {
                var targetGO = new GameObject($"Target_{group.rootBone}");
                targetGO.transform.SetParent(rigGO.transform, false);
                targetGO.transform.position = boneTransform.position;
                targetGO.transform.rotation = boneTransform.rotation;

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

                _overrideConstraints[group.rootBone] = constraint;
                _overrideTargets[group.rootBone] = targetGO.transform;
            }
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

    public virtual void TriggerHit(HumanBodyBones hitBone, Vector3 force)
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
        if (!_configs.TryGetValue(groupRoot, out var config)) return;

        float t = Mathf.InverseLerp(_minForce, _maxForce, force.magnitude);

        if (config.constraintType == BoneGroup.ConstraintType.TwoBoneIK)
        {
            if (!_ikConstraints.TryGetValue(groupRoot, out var ikConstraint)) return;
            if (!_ikTargets.TryGetValue(groupRoot, out var ikTarget)) return;

            var tipTransform = ikConstraint.data.tip;
            Vector3 offset = force.normalized * config.pushDistance * t * intensity * 3f;
            ikTarget.position = tipTransform.position + offset;
            ikTarget.rotation = tipTransform.rotation;

            if (_activeCoroutines.TryGetValue(groupRoot, out var existing) && existing != null)
                StopCoroutine(existing);
            _activeCoroutines[groupRoot] = StartCoroutine(SpringDamper(w => ikConstraint.weight = w, config.stiffness, config.damping));
        }
        else
        {
            if (!_overrideConstraints.TryGetValue(groupRoot, out var constraint)) return;
            if (!_overrideTargets.TryGetValue(groupRoot, out var target)) return;

            Vector3 localForce = transform.InverseTransformDirection(-force.normalized);
            Quaternion fullRotation = Quaternion.FromToRotation(Vector3.forward, localForce);
            fullRotation.ToAngleAxis(out float originalAngle, out Vector3 axis);
            float degrees = Mathf.Lerp(0f, config.maxAngle, t) * intensity;
            degrees = Mathf.Min(originalAngle * intensity, degrees);
            Quaternion hitRotation = Quaternion.AngleAxis(degrees, axis);

            var boneTransform = constraint.data.constrainedObject;
            target.position = boneTransform.position + force.normalized * config.pushDistance * t * intensity;
            target.rotation = boneTransform.rotation * hitRotation;

            if (_activeCoroutines.TryGetValue(groupRoot, out var existing) && existing != null)
                StopCoroutine(existing);
            _activeCoroutines[groupRoot] = StartCoroutine(SpringDamper(w => constraint.weight = w, config.stiffness, config.damping));
        }
    }

    private IEnumerator SpringDamper(System.Action<float> setWeight, float stiffness, float damping)
    {
        float weight = 1f;
        float velocity = 0f;

        while (Mathf.Abs(weight) > 0.001f || Mathf.Abs(velocity) > 0.001f)
        {
            float springForce = (0f - weight) * stiffness;
            velocity += (springForce - velocity * damping) * Time.deltaTime;
            weight += velocity * Time.deltaTime;
            setWeight(Mathf.Clamp01(weight));
            yield return null;
        }

        setWeight(0f);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
