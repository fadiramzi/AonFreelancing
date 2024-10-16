namespace AonFreelancing.Models
{
    public class Project
    {
        private static int _projectCount = 0;
        private readonly int _id;
        public string Title { get; set; }
        public int Id => _id;
        public Project()
        {
            _id = _projectCount++;
        }
        public Project(string title)
        {
            _id = _projectCount++;
            Title = title;
        }
    }
}
