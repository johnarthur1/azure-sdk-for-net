// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System;
using System.Collections.Generic;
using System.Text.Json;
using Azure.Core;

namespace Azure.Iot.Hub.Service.Models
{
    public partial class MessageFeedbackBatch
    {
        internal static MessageFeedbackBatch DeserializeMessageFeedbackBatch(JsonElement element)
        {
            DateTimeOffset? enqueuedTime = default;
            string userId = default;
            string lockToken = default;
            IReadOnlyList<MessageResult> messageResult = default;
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals("enqueuedTime"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    enqueuedTime = property.Value.GetDateTimeOffset("O");
                    continue;
                }
                if (property.NameEquals("userId"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    userId = property.Value.GetString();
                    continue;
                }
                if (property.NameEquals("lockToken"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    lockToken = property.Value.GetString();
                    continue;
                }
                if (property.NameEquals("messageResult"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    List<MessageResult> array = new List<MessageResult>();
                    foreach (var item in property.Value.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.Null)
                        {
                            array.Add(null);
                        }
                        else
                        {
                            array.Add(Models.MessageResult.DeserializeMessageResult(item));
                        }
                    }
                    messageResult = array;
                    continue;
                }
            }
            return new MessageFeedbackBatch(enqueuedTime, userId, lockToken, messageResult);
        }
    }
}
