namespace ASQ
{
    public class IdGenerator
    {
        private int _instanceId = 0;

        public int GenerateInstanceId()
        {
            return _instanceId++;
        }
    }
}
