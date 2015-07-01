// 
// Copyright (c) Microsoft and contributors.  All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// 
// See the License for the specific language governing permissions and
// limitations under the License.
// 

// Warning: This code was generated by a tool.
// 
// Changes to this file may cause incorrect behavior and will be lost if the
// code is regenerated.

using System;
using System.Collections.Generic;
using System.Linq;
using Hyak.Common;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Management.RemoteApp.Models;

namespace Microsoft.WindowsAzure.Management.RemoteApp.Models
{
    /// <summary>
    /// The collection usage billing summary.
    /// </summary>
    public partial class CollectionUsageSummaryListResult : AzureOperationResponse
    {
        private IList<BillingUsageSummary> _usageSummaryList;
        
        /// <summary>
        /// Optional. The list of collection usage summary.
        /// </summary>
        public IList<BillingUsageSummary> UsageSummaryList
        {
            get { return this._usageSummaryList; }
            set { this._usageSummaryList = value; }
        }
        
        /// <summary>
        /// Initializes a new instance of the CollectionUsageSummaryListResult
        /// class.
        /// </summary>
        public CollectionUsageSummaryListResult()
        {
            this.UsageSummaryList = new LazyList<BillingUsageSummary>();
        }
    }
}
