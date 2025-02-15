﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.AI.Embeddings;

namespace Microsoft.SemanticKernel.Memory;

/// <summary>
/// IMPORTANT: this is a storage schema. Changing the fields will invalidate existing metadata stored in persistent vector DBs.
/// </summary>
public class MemoryRecord : DataEntryBase
{
    /// <summary>
    /// Source content embeddings.
    /// </summary>
    [JsonPropertyName("embedding")]
    public Embedding<float> Embedding { get; }

    /// <summary>
    /// Metadata associated with a Semantic Kernel memory.
    /// </summary>
    [JsonPropertyName("metadata")]
    public MemoryRecordMetadata Metadata { get; }

    /// <summary>
    /// Constructor, use <see cref="ReferenceRecord"/> or <see cref="LocalRecord"/>
    /// </summary>
    [JsonConstructor]
    public MemoryRecord(
        MemoryRecordMetadata metadata,
        Embedding<float> embedding,
        string? key,
        DateTimeOffset? timestamp = null) : base(key, timestamp)
    {
        this.Metadata = metadata;
        this.Embedding = embedding;
    }

    /// <summary>
    /// Prepare an instance about a memory which source is stored externally.
    /// The universal resource identifies points to the URL (or equivalent) to find the original source.
    /// </summary>
    /// <param name="externalId">URL (or equivalent) to find the original source</param>
    /// <param name="sourceName">Name of the external service, e.g. "MSTeams", "GitHub", "WebSite", "Outlook IMAP", etc.</param>
    /// <param name="description">Optional description of the record. Note: the description is not indexed.</param>
    /// <param name="embedding">Source content embeddings</param>
    /// <param name="key">Optional existing database key</param>
    /// <param name="timestamp">optional timestamp</param>
    /// <returns>Memory record</returns>
    public static MemoryRecord ReferenceRecord(
        string externalId,
        string sourceName,
        string? description,
        Embedding<float> embedding,
        string? key = null,
        DateTimeOffset? timestamp = null)
    {
        return new MemoryRecord(
            new MemoryRecordMetadata
            (
                isReference: true,
                externalSourceName: sourceName,
                id: externalId,
                description: description ?? string.Empty,
                text: string.Empty
            ),
            embedding,
            key,
            timestamp
        );
    }

    /// <summary>
    /// Prepare an instance for a memory stored in the internal storage provider.
    /// </summary>
    /// <param name="id">Resource identifier within the storage provider, e.g. record ID/GUID/incremental counter etc.</param>
    /// <param name="text">Full text used to generate the embeddings</param>
    /// <param name="description">Optional description of the record. Note: the description is not indexed.</param>
    /// <param name="embedding">Source content embeddings</param>
    /// <param name="key">Optional existing database key</param>
    /// <param name="timestamp">optional timestamp</param>
    /// <returns>Memory record</returns>
    public static MemoryRecord LocalRecord(
        string id,
        string text,
        string? description,
        Embedding<float> embedding,
        string? key = null,
        DateTimeOffset? timestamp = null)
    {
        return new MemoryRecord
        (
            new MemoryRecordMetadata
            (
                isReference: false,
                id: id,
                text: text,
                description: description ?? string.Empty,
                externalSourceName: string.Empty
            ),
            embedding,
            key,
            timestamp
        );
    }

    /// <summary>
    /// Create a memory record from a serialized metadata string.
    /// </summary>
    /// <param name="json">Json string representing a memory record's metadata.</param>
    /// <param name="embedding">The embedding associated with a memory record.</param>
    /// <param name="key">Optional existing database key</param>
    /// <param name="timestamp">optional timestamp</param>
    /// <returns></returns>
    /// <exception cref="MemoryException"></exception>
    public static MemoryRecord FromJson(
        string json,
        Embedding<float> embedding,
        string? key = null,
        DateTimeOffset? timestamp = null)
    {
        var metadata = JsonSerializer.Deserialize<MemoryRecordMetadata>(json);
        if (metadata != null)
        {
            return new MemoryRecord(metadata, embedding, key, timestamp);
        }

        throw new MemoryException(
            MemoryException.ErrorCodes.UnableToDeserializeMetadata,
            "Unable to create memory record from serialized metadata");
    }

    /// <summary>
    /// Serialize the metadata of a memory record.
    /// </summary>
    /// <returns>The memory record's metadata serialized to a json string.</returns>
    public string GetSerializedMetadata()
    {
        return JsonSerializer.Serialize(this.Metadata);
    }
}
