# Real-time WebSocket Protocol

Raw WebSockets (RFC 6455) — framework-agnostic, usable from Next.js, React, Vue, Flutter, or any
native client. No SignalR.

## Connecting

```
ws://localhost:5079/ws?token=<better-auth session token>
wss://api.anchormarket.com/ws?token=<better-auth session token>
```

- The connection is authenticated on the handshake using the Better Auth **session token**.
- WebSocket handshakes cannot send an `Authorization` header, so the token is passed as the
  `token` query parameter. A same-origin browser client may instead rely on the session cookie.
- An unauthenticated or invalid token yields HTTP `401` and the socket is not opened.
- The server sends a ping every 15s (keep-alive); standard clients respond automatically.

## Client → Server (subscribe / unsubscribe)

Send a JSON text frame:

```json
{ "action": "subscribe",   "channel": "price",        "outcomeId": "..." }
{ "action": "subscribe",   "channel": "orderbook",    "outcomeId": "..." }
{ "action": "subscribe",   "channel": "trades",       "marketId":  "..." }
{ "action": "subscribe",   "channel": "feed",         "marketId":  "..." }
{ "action": "subscribe",   "channel": "market",       "marketId":  "..." }
{ "action": "subscribe",   "channel": "group-market", "groupId":   "..." }
{ "action": "unsubscribe", "channel": "price",        "outcomeId": "..." }
```

| Channel        | Identifier  | Notes |
|----------------|-------------|-------|
| `price`        | `outcomeId` | Latest traded price for an outcome |
| `orderbook`    | `outcomeId` | Aggregated order book for an outcome |
| `trades`       | `marketId`  | Executed trades on a market |
| `feed`         | `marketId`  | Latest external feed value (e.g. underlying asset price) |
| `market`       | `marketId`  | Market lifecycle (e.g. resolved) |
| `group-market` | `groupId`   | Group-scoped events; **requires group membership** |

**Group privacy:** any subscription whose market is group-scoped (resolved via the requested
`outcomeId`/`marketId`/`groupId`) is only accepted from members of that group; public markets are open.

The server acknowledges each request:

```json
{ "type": "subscribed",   "topic": "price:<outcomeId>" }
{ "type": "unsubscribed", "topic": "price:<outcomeId>" }
{ "type": "error", "message": "Not a member of this group." }
```

## Server → Client (broadcasts)

```json
{ "type": "price-update",     "outcomeId": "...", "price": 0.67, "volume": 50, "timestamp": "..." }
{ "type": "trade-executed",   "marketId": "...", "outcomeId": "...", "price": 0.67, "shares": 50, "timestamp": "..." }
{ "type": "orderbook-update", "outcomeId": "...", "bids": [{"price":0.66,"quantity":120}], "asks": [{"price":0.68,"quantity":90}], "timestamp": "..." }
{ "type": "feed-update",      "marketId": "...", "feedRegistrationId": "...", "value": 62345.10, "timestamp": "..." }
{ "type": "market-resolved",  "marketId": "...", "winningOutcomeId": "...", "timestamp": "..." }
```

Producers: `price-update`/`trade-executed` from the matching engine; `orderbook-update` from the
order book snapshot service; `feed-update` from the feed polling service; `market-resolved` from
market resolution.

## How it works

```
Order matching / feeds  ──►  IRealtimePublisher  ──►  Redis pub/sub channels
                                                          │  (ws:price-updates, ws:trade-executions, …)
                                                          ▼
                                              RealtimeBackplaneService  ──►  WebSocketConnectionManager
                                                                                 │  (topic fan-out)
                                                                                 ▼
                                                                          subscribed clients
```

- Redis pub/sub is the **backplane**, so broadcasts reach clients connected to *any* API instance.
- Without Redis configured, the socket still connects and accepts subscriptions, but no events are
  delivered (nothing is published) — matching the rest of the real-time layer's fallback behaviour.
