namespace Staffing.TableauAPI.Models
{
    /// <summary>
    /// Object has a Project Id
    /// </summary>
    public interface IHasProjectId
    {
        /// <summary>
        /// Tableau Project ID
        /// </summary>
        string ProjectId  {get;}
    }
}