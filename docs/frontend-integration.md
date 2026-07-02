# Frontend Integration Guide

How to build the market UI against this backend. Two parts:

1. **REST** — use the generated **`anchormarket-sdk-typescript`** package. Every endpoint below is already a method on the SDK; call those, don't hand-write fetches.
2. **WebSocket** — live updates are **not** in the SDK (WebSockets aren't part of OpenAPI). Use the small client in the [WebSocket](#websocket-live-updates) section.

---

## REST (via `anchormarket-sdk-typescript`)

Install and use the generated SDK for all of these. Each route maps to one SDK method.

### Market list / discovery
| Need | Endpoint |
|------|----------|
| Markets grid | `GET /api/markets` |
| Categories (filters) | `GET /api/categories` |

> `GET /api/markets` returns markets **without** outcomes or prices. Load those per market on the detail page.

### Market detail (the hinge call is **outcomes** — everything else is keyed by `outcomeId`)
Call `getMarket` + `getMarketOutcomes` first, then fan out the per-outcome calls.

| Section | Endpoint |
|---------|----------|
| Market header | `GET /api/markets/{id}` |
| **Outcomes (gives the `outcomeId`s)** | `GET /api/markets/{id}/outcomes` |
| Current price (per outcome) | `GET /api/orderbooks/market/{id}/outcome/{outcomeId}/price` |
| Order book (per outcome) | `GET /api/orderbooks/market/{id}/outcome/{outcomeId}` |
| Price chart (per outcome) | `GET /api/outcomes/{outcomeId}/price-history` |
| Trade feed | `GET /api/trades/flow/{id}` |
| Order-book history (optional) | `GET /api/orderbook/history/{outcomeId}` |
| Underlying feed value | `GET /api/feeds/market/{id}` |
| Winner (when `status === "Resolved"`) | `GET /api/markets/{id}/resolution` |

### Trading + user
| Action | Endpoint |
|--------|----------|
| Place order | `POST /api/limitorders` |
| Cancel order | `DELETE /api/limitorders/{orderId}` |
| My orders in a market | `GET /api/limitorders/market/{marketId}` |
| My positions (with PnL) | `GET /api/positions/with-pnl` |
| Positions in a market | `GET /api/positions/by-market/{marketId}` |
| Wallet balance | `GET /api/wallets/user/{userId}` |

---

## WebSocket (live updates)

The same backend exposes a raw WebSocket for live data. It is **not** in the SDK — wire it yourself with the client below.

### Connect

```
ws(s)://<api-host>/ws?token=<session token>
```

- `<session token>` is the **same Better Auth session token** the SDK authenticates with (the value of the `better-auth.session_token` cookie / your bearer token). Pass it as the `token` query parameter — WebSocket handshakes can't send headers.
- Unauthenticated → the socket is rejected (HTTP 401).

### Protocol

**You send** (subscribe / unsubscribe):

```json
{ "action": "subscribe", "channel": "price",     "outcomeId": "..." }
{ "action": "subscribe", "channel": "orderbook", "outcomeId": "..." }
{ "action": "subscribe", "channel": "trades",    "marketId":  "..." }
{ "action": "subscribe", "channel": "feed",      "marketId":  "..." }
{ "action": "subscribe", "channel": "market",    "marketId":  "..." }
```

| Channel | Identifier | What you get |
|---------|-----------|--------------|
| `price` | `outcomeId` | latest traded price |
| `orderbook` | `outcomeId` | full order book (bids/asks) |
| `trades` | `marketId` | executed trades |
| `feed` | `marketId` | underlying feed value (e.g. BTC price) |
| `market` | `marketId` | lifecycle, incl. resolution |

**Server sends** (each message has a `type`):

```json
{ "type": "subscribed",       "topic": "price:<outcomeId>" }
{ "type": "price-update",     "outcomeId": "...", "price": 0.67, "volume": 50, "timestamp": "..." }
{ "type": "orderbook-update", "outcomeId": "...", "bids": [{"price":0.66,"quantity":120}], "asks": [{"price":0.68,"quantity":90}], "timestamp": "..." }
{ "type": "trade-executed",   "marketId": "...", "outcomeId": "...", "price": 0.67, "shares": 50, "timestamp": "..." }
{ "type": "feed-update",      "marketId": "...", "value": 62345.10, "timestamp": "..." }
{ "type": "market-resolved",  "marketId": "...", "winningOutcomeId": "...", "timestamp": "..." }
{ "type": "error",            "message": "..." }
```

### Drop-in client

```ts
type Handler = (msg: any) => void;

export class AnchorMarketSocket {
  private ws?: WebSocket;
  private handlers = new Map<string, Set<Handler>>();
  private subscriptions: object[] = [];

  constructor(private baseUrl: string, private token: string) {}

  connect() {
    const url = `${this.baseUrl.replace(/^http/, "ws")}/ws?token=${encodeURIComponent(this.token)}`;
    this.ws = new WebSocket(url);

    this.ws.onopen = () => this.subscriptions.forEach((s) => this.send(s));
    this.ws.onmessage = (e) => {
      const msg = JSON.parse(e.data);
      this.handlers.get(msg.type)?.forEach((h) => h(msg));
    };
    // auto-reconnect; subscriptions are replayed on reopen
    this.ws.onclose = () => setTimeout(() => this.connect(), 1000);
  }

  /** Listen for a server message type, e.g. on("price-update", fn). */
  on(type: string, handler: Handler) {
    (this.handlers.get(type) ?? this.handlers.set(type, new Set()).get(type)!).add(handler);
    return () => this.handlers.get(type)?.delete(handler);
  }

  subscribe(channel: string, ids: { marketId?: string; outcomeId?: string }) {
    const sub = { action: "subscribe", channel, ...ids };
    this.subscriptions.push(sub);
    this.send(sub);
  }

  private send(payload: object) {
    if (this.ws?.readyState === WebSocket.OPEN) this.ws.send(JSON.stringify(payload));
  }

  close() { this.ws?.close(); }
}
```

### Usage on the market page

```ts
const socket = new AnchorMarketSocket(API_BASE, sessionToken);
socket.connect();

// keyed by marketId
socket.subscribe("trades", { marketId });
socket.subscribe("feed",   { marketId });
socket.subscribe("market", { marketId });

// per outcome (from getMarketOutcomes)
for (const o of outcomes) {
  socket.subscribe("price",     { outcomeId: o.id });
  socket.subscribe("orderbook", { outcomeId: o.id });
}

socket.on("price-update",     (m) => store.setPrice(m.outcomeId, m.price));
socket.on("orderbook-update", (m) => store.setBook(m.outcomeId, m.bids, m.asks));
socket.on("trade-executed",   (m) => store.addTrade(m));
socket.on("feed-update",      (m) => store.setFeedValue(m.marketId, m.value));
socket.on("market-resolved",  (m) => store.setResolved(m.marketId, m.winningOutcomeId));
```

**Pattern:** REST loads the initial snapshot; WebSocket frames patch the same store. Use the same keys (`outcomeId`, `marketId`) for both so updates merge cleanly.

---

## Notes

- **Outcomes are a separate call.** `MarketDto` has no nested outcomes — always call `/markets/{id}/outcomes` to get the `outcomeId`s the rest of the page (and the WS subscriptions) depend on.
- **Group markets:** same endpoints, but WS subscriptions to a group-scoped market require group membership (non-members get an `error` frame). Gate group-market visibility in the UI accordingly.
- **Live data needs Redis** configured on the backend. Without it the socket still connects and accepts subscriptions, but no frames are pushed — so always render from the REST snapshot first.
