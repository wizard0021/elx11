// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 OpenEleven Contributors & ELX11 Team
//
// This file is part of OpenEleven. See the LICENSE file at the repository root.

using System.Text.Json.Serialization;

namespace OpenEleven.Server.Web;

public sealed record MloPlayerMarketInfo(
    int Id,
    string Name,
    string Position,
    string Club,
    string Nationality,
    long CurrentPrice,
    long LastWeekPrice,
    int PopularityRank
)
{
    public long PriceChange => CurrentPrice - LastWeekPrice;
    public double ChangePercent => LastWeekPrice > 0 ? Math.Round((double)PriceChange / LastWeekPrice * 100, 1) : 0.0;
}

public sealed record MloTransferDeal(
    string Manager,
    string Club,
    string Division,
    string PlayerName,
    long Price,
    string Type, // "BUY" or "SELL"
    DateTimeOffset Timestamp
);

public sealed class MloMarketService
{
    private readonly object _gate = new();
    private readonly Dictionary<int, MloPlayerMarketInfo> _players = new();
    private readonly List<MloTransferDeal> _recentTransfers = new();
    private readonly Random _rand = new();
    private DateTimeOffset _lastShift = DateTimeOffset.UtcNow;
    private int _pricesMovedCount = 42;

    public MloMarketService()
    {
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        var raw = new (int Id, string Name, string Pos, string Club, string Nat, long BasePrice)[]
        {
            // Superstars
            (7511, "L. Messi", "RWF / SS", "Barcelona", "Argentina", 48500000),
            (7512, "C. Ronaldo", "LWF / CF", "Real Madrid", "Portugal", 47200000),
            (7513, "W. Rooney", "CF / SS", "Manchester United", "England", 26000000),
            (7514, "A. Robben", "RWF / RMF", "Bayern München", "Netherlands", 24500000),
            (7515, "W. Sneijder", "AMF", "Inter", "Netherlands", 23800000),
            (7516, "Xavi", "CMF", "Barcelona", "Spain", 22500000),
            (7517, "A. Iniesta", "CMF / AMF", "Barcelona", "Spain", 23000000),
            (7518, "Z. Ibrahimovic", "CF", "Milan", "Sweden", 25500000),
            (7519, "D. Villa", "CF / LWF", "Barcelona", "Spain", 21000000),
            (7520, "D. Drogba", "CF", "Chelsea", "Ivory Coast", 19800000),
            (7521, "F. Torres", "CF", "Liverpool", "Spain", 20500000),
            (7522, "Kaka", "AMF", "Real Madrid", "Brazil", 18500000),
            (7523, "Ronaldinho", "AMF / LWF", "Milan", "Brazil", 16000000),
            (7524, "S. Gerrard", "CMF", "Liverpool", "England", 17500000),
            (7525, "F. Lampard", "CMF", "Chelsea", "England", 17000000),
            (7526, "S. Aguero", "SS / CF", "Atlético Madrid", "Argentina", 19200000),
            (7527, "C. Tevez", "CF / SS", "Manchester City", "Argentina", 18800000),
            (7528, "D. Forlan", "CF", "Atlético Madrid", "Uruguay", 17200000),
            (7529, "Mesut Özil", "AMF", "Real Madrid", "Germany", 16500000),
            (7530, "Thomas Müller", "SS / RWF", "Bayern München", "Germany", 15800000),
            (7531, "Gareth Bale", "LB / LMF", "Tottenham Hotspur", "Wales", 18000000),
            (7532, "E. Hazard", "AMF / LMF", "Lille", "Belgium", 14500000),
            (7533, "Neymar", "SS / LWF", "Santos", "Brazil", 16200000),
            (7534, "Iker Casillas", "GK", "Real Madrid", "Spain", 15000000),
            (7535, "G. Buffon", "GK", "Juventus", "Italy", 14200000),
            (7536, "C. Puyol", "CB", "Barcelona", "Spain", 13800000),
            (7537, "N. Vidic", "CB", "Manchester United", "Serbia", 14600000),
            (7538, "Maicon", "RB", "Inter", "Brazil", 13500000),
            (7539, "D. Alves", "RB", "Barcelona", "Brazil", 14000000),

            // Iconic Default Squad (The Legends of MLO)
            (1001, "Castolo", "CF", "Default XI", "Brazil", 780000),
            (1002, "Minanda", "AMF", "Default XI", "Portugal", 720000),
            (1003, "Espimas", "RMF", "Default XI", "France", 680000),
            (1004, "Ximelez", "LMF", "Default XI", "Spain", 690000),
            (1005, "Ivarov", "GK", "Default XI", "Russia", 650000),
            (1006, "Ordaz", "CF", "Default XI", "Spain", 620000),
            (1007, "Huylens", "CF", "Default XI", "Belgium", 610000),
            (1008, "Jaric", "CB / DMF", "Default XI", "Serbia", 590000),
            (1009, "Stremer", "CB", "Default XI", "Sweden", 600000),
            (1010, "Valeny", "CB", "Default XI", "France", 580000),
            (1011, "Ruskin", "LB", "Default XI", "Scotland", 560000),
            (1012, "El Moubarki", "RB", "Default XI", "Morocco", 550000),
            (1013, "Dodo", "DMF", "Default XI", "Italy", 540000),
            (1014, "Iouga", "DMF", "Default XI", "Romania", 530000),
            (1015, "Ceciu", "CB", "Default XI", "Romania", 520000),
            (1016, "Zamenhof", "GK", "Default XI", "Poland", 500000)
        };

        var rank = 1;
        foreach (var p in raw.OrderByDescending(x => x.BasePrice))
        {
            // Initial last week price with slight variation
            var delta = (long)(p.BasePrice * (_rand.Next(-50, 70) / 1000.0));
            var lastPrice = p.BasePrice - delta;
            _players[p.Id] = new MloPlayerMarketInfo(
                p.Id, p.Name, p.Pos, p.Club, p.Nat, p.BasePrice, lastPrice, rank++
            );
        }
    }

    public (IReadOnlyList<MloPlayerMarketInfo> Gainers, IReadOnlyList<MloPlayerMarketInfo> Droppers, int PricesMoved) GetTopMovers()
    {
        lock (_gate)
        {
            var sorted = _players.Values.ToList();
            var gainers = sorted.OrderByDescending(p => p.PriceChange).Take(5).ToList();
            var droppers = sorted.OrderBy(p => p.PriceChange).Take(5).ToList();
            return (gainers, droppers, _pricesMovedCount);
        }
    }

    public IReadOnlyList<MloPlayerMarketInfo> GetMostExpensive(int count = 10)
    {
        lock (_gate)
        {
            return _players.Values.OrderByDescending(p => p.CurrentPrice).Take(count).ToList();
        }
    }

    public MloPlayerMarketInfo? FindPlayer(string query)
    {
        lock (_gate)
        {
            var clean = query.Trim().ToLowerInvariant();
            if (int.TryParse(clean, out var id) && _players.TryGetValue(id, out var byId))
                return byId;

            return _players.Values.FirstOrDefault(p =>
                p.Name.ToLowerInvariant().Contains(clean) ||
                clean.Contains(p.Name.ToLowerInvariant()));
        }
    }

    public object GetMarketSummary()
    {
        lock (_gate)
        {
            var all = _players.Values.ToList();
            var totalVal = all.Sum(p => p.CurrentPrice);
            var (gainers, droppers, moved) = GetTopMovers();

            return new
            {
                status = "OPEN",
                currency = "EUR (€)",
                total_players = all.Count,
                prices_moved = moved,
                last_shift = _lastShift,
                total_market_value = totalVal,
                top_gainers = gainers,
                top_droppers = droppers
            };
        }
    }

    public MloTransferDeal RecordTransfer(string manager, string club, string division, string playerName, long price, string type = "BUY")
    {
        lock (_gate)
        {
            var deal = new MloTransferDeal(
                manager, club, division, playerName, price, type, DateTimeOffset.UtcNow
            );
            _recentTransfers.Insert(0, deal);
            if (_recentTransfers.Count > 100)
                _recentTransfers.RemoveAt(_recentTransfers.Count - 1);
            return deal;
        }
    }

    public IReadOnlyList<MloTransferDeal> GetRecentTransfers(int count = 10)
    {
        lock (_gate)
        {
            return _recentTransfers.Take(count).ToList();
        }
    }

    /// <summary>Simulates a market shift (fluctuations based on season match performance)</summary>
    public (IReadOnlyList<MloPlayerMarketInfo> Gainers, IReadOnlyList<MloPlayerMarketInfo> Droppers, int PricesMoved) SimulateMarketShift()
    {
        lock (_gate)
        {
            var moved = 0;
            var keys = _players.Keys.ToList();
            foreach (var k in keys)
            {
                var p = _players[k];
                // 70% chance price shifts slightly (-4% to +6%)
                if (_rand.Next(100) < 70)
                {
                    var percent = _rand.Next(-40, 65) / 1000.0;
                    var change = (long)(p.CurrentPrice * percent);
                    if (change != 0)
                    {
                        var newPrice = Math.Max(300000, p.CurrentPrice + change);
                        _players[k] = p with { LastWeekPrice = p.CurrentPrice, CurrentPrice = newPrice };
                        moved++;
                    }
                }
            }

            _pricesMovedCount = moved;
            _lastShift = DateTimeOffset.UtcNow;
            return GetTopMovers();
        }
    }
}
