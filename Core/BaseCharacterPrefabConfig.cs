using UnityEngine;
using UnityEngine.AI;

public class BaseCharacterPrefabConfig : MonoBehaviour, IPrefabConfig
{
    public SideType SideType;
    [Header("Scene References")]
    public BaseCharacter Character;
    public Rigidbody Rb;
    public Animator Animator;
    public Weapon Weapon;
    public NavMeshAgent Agent;
    public Detector Detector;
    public HealthBar HealthBar;
    public AnimationEventController AnimEvent;

    [Header("Movement Settings")]
    public float WalkSpeed = 3.5f;
}
