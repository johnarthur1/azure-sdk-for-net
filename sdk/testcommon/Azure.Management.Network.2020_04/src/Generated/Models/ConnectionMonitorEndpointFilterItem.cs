// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

namespace Azure.Management.Network.Models
{
    /// <summary> Describes the connection monitor endpoint filter item. </summary>
    public partial class ConnectionMonitorEndpointFilterItem
    {
        /// <summary> Initializes a new instance of ConnectionMonitorEndpointFilterItem. </summary>
        public ConnectionMonitorEndpointFilterItem()
        {
        }

        /// <summary> Initializes a new instance of ConnectionMonitorEndpointFilterItem. </summary>
        /// <param name="type"> The type of item included in the filter. Currently only &apos;AgentAddress&apos; is supported. </param>
        /// <param name="address"> The address of the filter item. </param>
        internal ConnectionMonitorEndpointFilterItem(ConnectionMonitorEndpointFilterItemType? type, string address)
        {
            Type = type;
            Address = address;
        }

        /// <summary> The type of item included in the filter. Currently only &apos;AgentAddress&apos; is supported. </summary>
        public ConnectionMonitorEndpointFilterItemType? Type { get; set; }
        /// <summary> The address of the filter item. </summary>
        public string Address { get; set; }
    }
}
