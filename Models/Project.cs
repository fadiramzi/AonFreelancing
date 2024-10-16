namespace AonFreelancing.Models
{
    public class Project
    {
        private static int _projectCount = 0;
        private int _id = _projectCount;
        public string Title { get; set; }
        public int Id
        {
            get
            {
                return _id;
            }
        }
        public Project()
        {
            _projectCount++;
        }
        public Project(string titel)
        {
            _projectCount++;
            Title = titel;
        }
    }
}
