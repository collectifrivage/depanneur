namespace Depanneur.App.Entities
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
    }
}