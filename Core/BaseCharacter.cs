using UnityEngine;

public abstract class BaseCharacter : MonoBehaviour
{
    public ReactVariable<CharacterConfig> SaveConfig;

    public virtual void MakeAsPlayer() { }
    public virtual void MakeAsBot() { }
    public virtual void Init() { }
    public virtual void Update() { }

    private void Start()
    {
        Init();
    }
}
