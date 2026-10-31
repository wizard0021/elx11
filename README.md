# ELX11 (OpenEleven)

A modern, high-performance reverse-engineered server for **Pro Evolution Soccer 2011 (PC & PlayStation 3)**, built in C# and .NET 9.

ELX11 provides full online multiplayer capability for PES 2011, including account authentication, lobby management, room matchmaking, live match telemetry, and an integrated web hub.

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Architecture](#architecture)
- [Project Layout](#project-layout)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Build and Run](#build-and-run)
  - [Configuration](#configuration)
  - [Environment Variables](#environment-variables)
- [Connecting Clients](#connecting-clients)
  - [PC Client](#pc-client)
  - [PlayStation 3 (Retail / CFW)](#playstation-3-retail--cfw)
- [Docker Deployment](#docker-deployment)
- [Contributing](#contributing)
- [License and Disclaimer](#license-and-disclaimer)

## Overview

Pro Evolution Soccer 2011's official online servers were shut down years ago. ELX11 emulates the Konami binary network protocol, enabling original retail PC and PS3 game clients to authenticate and play online multiplayer matches without modifying the game executable.

Matches run directly peer-to-peer (UDP port 5730) between clients, ensuring minimum latency and low server resource overhead.

## Key Features

- **Cross-Platform Support**: Unified lobby and matchmaking for both PC and PlayStation 3 clients.
- **Protocol Emulation**: Complete implementation of the Konami binary protocol framing, XOR cipher, Blowfish-ECB encryption, and key-value message codecs.
- **Web Hub and Admin Portal**: Built-in ASP.NET Core Razor Pages web interface with player registration, ranking ladders, match histories, live match ticker, and administrative controls.
- **P2P Coordination**: Reliable NAT and IP/port exchange for direct UDP 5730 peer-to-peer gameplay.
- **Scalable Architecture**: Low memory footprint using asynchronous socket I/O, bounded channels, and response caching.

## Architecture

```
[ PES 2011 PC Client ] <----\
                             +---> [ Gate / Account / Menu / Lobby ] (TCP: 4010, 28021-28025)
[ PES 2011 PS3 Client ] <---/      [ Web Hub / REST API ]           (HTTP/HTTPS: 80, 443)
                                            |
                                            v
                                   [ SQLite / MySQL DB ]

[ Client A (Home) ] <========== UDP 5730 (P2P Gameplay) ==========> [ Client B (Away) ]
```

## Project Layout

- `src/OpenEleven.Protocol/`: XOR cipher, Blowfish-ECB, framing codecs, and key-value protocol grammar.
- `src/OpenEleven.Data/`: EF Core entity definitions, database context, and schema migrations.
- `src/OpenEleven.Server/`: Core server dispatch, socket listeners, protocol handlers, and web interface.
- `src/OpenEleven.Profiles/`: Title profile markers and command mappings.
- `src/OpenEleven.Pes2011Pc/`: PES 2011 entrypoint executable and default configuration.
- `tests/`: Comprehensive unit tests and golden packet vector verifications.

## Getting Started

### Prerequisites

- .NET 9.0 SDK or runtime
- Git
- SQLite (default) or MySQL

### Build and Run

Clone the repository:

```bash
git clone https://github.com/<username>/<repo-name>.git
cd <repo-name>
```

Build the solution:

```bash
dotnet build src/OpenEleven.Pes2011Pc
```

Run the server:

```bash
dotnet run --project src/OpenEleven.Pes2011Pc
```

### Configuration

Copy the sample configuration file:

```bash
cp conf/server.sample.yaml conf/server.yaml
```

To override settings locally without altering tracked files, create `conf/server.local.yaml`.

### Environment Variables

Settings can be overridden via environment variables using the `OPENELEVEN_` prefix:

```bash
OPENELEVEN_Server__AdvertiseIp=your.server.ip.or.domain
OPENELEVEN_Server__Admin__Password=your_secure_password
ELX11_BOT_API_KEY=your_bot_secret
```

## Connecting Clients

### PC Client

Redirect the following domain names to your server IP address via `hosts` or DNS:

- `pes11web-gate.winning-eleven.net`
- `pes2011pc.konamionline.com`

Alternatively, use the ELX11 Launcher to handle redirects and game launch automatically.

### PlayStation 3 (Retail / CFW)

Set your PS3 console DNS to a server redirecting the Konami gate hostname to your server instance. Both retail disc copies and digital installations are supported.

## Docker Deployment

A production-ready `Dockerfile` and `docker-compose.yml` are provided under the `docker/` directory:

```bash
cd docker
docker compose up -d --build
```

## License and Disclaimer

This project is licensed under the **GNU Affero General Public License v3.0 or later** (AGPL-3.0-or-later). See the [LICENSE](LICENSE) file for the full license text.

**Disclaimer:** OpenEleven and ELX11 are independent, fan-made open-source projects created for historical preservation and educational purposes. This project is not affiliated with, endorsed by, or associated with Konami Digital Entertainment Co., Ltd. Pro Evolution Soccer and PES are registered trademarks of Konami. No copyrighted game files, proprietary game binaries, or assets are distributed with this software.
