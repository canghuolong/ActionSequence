namespace ASQ
{
    public class InfinityAction : IAction,ICustomComplete,IPool
    {
        public bool IsFromPool { get; set; }

        public void Reset()
        {
            CanComplete = false;
        }
        
        public bool CanComplete { get; set; }
    }
}