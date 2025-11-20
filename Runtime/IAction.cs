namespace ASQ
{
    
    public interface IAction
    {
    }
    public interface IActionWithId : IAction
    {
        string Identifier { get; set; }
    }
    
    public interface IModifyDuration
    {
        float Duration { get;}
    }

    public interface ICustomComplete : IModifyDuration
    {
        float IModifyDuration.Duration => float.PositiveInfinity;

        bool CanComplete{get; set; }
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

    public interface IParam
    {
        public void SetParams(object param);
    }

}