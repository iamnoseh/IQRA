namespace Domain.Entities.Education;

public class Subject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    
    public ICollection<Topic> Topics { get; set; } = new List<Topic>();
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<Domain.Entities.Reference.ClusterSubject> ClusterSubjects { get; set; } = new List<Domain.Entities.Reference.ClusterSubject>();
}
