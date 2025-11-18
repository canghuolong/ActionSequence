namespace ASQ
{
    public struct ActionSequenceModel
    {
        public string id;
        public ActionClip[] clips;
        public ActionSequenceModel(string id,ActionClip[] clips)
        {
            this.id = id;
            this.clips = clips;
        }

        public ActionSequenceModel(ActionClip[] clips)
        {
            this.id = string.Empty;
            this.clips = clips;
        }
    }
}