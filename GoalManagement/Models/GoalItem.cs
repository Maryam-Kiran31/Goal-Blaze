namespace GoalManagement.Models
{
    public class GoalItem
    {
        public int Id { get; set; } // Unique identifier
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; } = false; // For completion status

       // public string Priority { get; set; } = "Medium";
        public string Complexity { get; set; } = "Medium";
    }

   
}
