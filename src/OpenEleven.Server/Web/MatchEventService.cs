// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 PES5 Indie Team & ELX11 Team
//
// This file is part of OpenEleven. See the LICENSE file at the repository root.

namespace OpenEleven.Server.Web;

public sealed record MatchEventRecord(
    long Id,
    string Type,
    DateTimeOffset Timestamp,
    IReadOnlyDictionary<string, object?> Data
);

public sealed class MatchEventService
{
    private readonly object _gate = new();
    private readonly List<MatchEventRecord> _events = new();
    private long _counter = 0;
    private const int MaxHistory = 500;

    public void Publish(string type, Dictionary<string, object?> data)
    {
        lock (_gate)
        {
            var record = new MatchEventRecord(
                Interlocked.Increment(ref _counter),
                type,
                DateTimeOffset.UtcNow,
                data
            );
            _events.Add(record);
            if (_events.Count > MaxHistory)
            {
                _events.RemoveRange(0, _events.Count - MaxHistory);
            }
        }
    }

    public IReadOnlyList<MatchEventRecord> GetSince(long sinceId)
    {
        lock (_gate)
        {
            return _events.Where(e => e.Id > sinceId).ToList();
        }
    }
}
