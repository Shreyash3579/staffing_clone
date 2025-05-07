using System.Collections.Generic;

namespace Staffing.TableauAPI.Models
{
    /// <summary>
    /// Object Allows the setting of the Data Connections
    /// </summary>
    internal interface IEditDataConnectionsSet
    {
        /// <summary>
        /// Set data connections on the Data Connection set
        /// </summary>
        /// <param name="connections"></param>
        void SetDataConnections(IEnumerable<SiteConnection> connections);
    }
}