// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System.Collections.Generic;
using Azure.Core;

namespace Azure.AI.Language.QuestionAnswering.Models
{
    /// <summary> filters over knowledge base. </summary>
    public partial class QueryFilters
    {
        /// <summary> Initializes a new instance of QueryFilters. </summary>
        public QueryFilters()
        {
            SourceFilter = new ChangeTrackingList<string>();
        }

        /// <summary> Find QnAs that are associated with the given list of metadata. </summary>
        public MetadataFilter MetadataFilter { get; set; }
        /// <summary> Find QnAs that are associated with the given list of sources in knowledge base. </summary>
        public IList<string> SourceFilter { get; }
        /// <summary> Logical operation used to join metadata filters with source filters. </summary>
        public LogicalOperationKind? LogicalOperation { get; set; }
    }
}
