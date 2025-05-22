using UniRx;

public class HealthStatus
{
    public ReactiveProperty<float> Current = new ReactiveProperty<float>(30f);
    public ReactiveProperty<float> Max = new ReactiveProperty<float>(30f);
    public ReactiveProperty<bool> IsDead = new ReactiveProperty<bool>(false);

    public bool TakeDamage(float amount)
    {
        if (IsDead.Value) return false;
        Current.Value -= amount;
        if (Current.Value <= 0f)
        {
            Current.Value = 0f;
            IsDead.Value = true;
            return true;
        }
        return false;
    }   
}
