namespace ActionSequence
{
    public interface IModifyDuration
    {
        float Duration { get;}
    }
    
    public interface ICompleteAction
    {
        void Complete();
    }
    public interface IUpdateAction
    {
        void Update(float localTime,float duration);
    }
    public interface IStartAction
    {
        void Start();
    }
    
    
    public interface IAction
    {
        void Reset();

    }
    
    public interface IAction<out T> : IAction
    {
        public void SetParams(object param);
    }

    internal interface IPoolAction<out T> : IAction<T>, IPool
    {
        
    }
    
}