

#Market Data

### This Section covers market data capabilities utilizing the tradorvate api. This is public information on their website: https://api.tradovate.com/

## Accessing Market Data
The Tradovate Market Data API provides a way to access market data such as quotes, DOM, charts and histograms. The API uses JSON format for request bodies and response data. The exchange of requests and responses are transmitted via the Tradovate WebSocket protocol. We have example projects available in both C# and JavaScript.

In a typical scenario using the Market Data API consists of the following steps:

### 1. Acquire An Access Token Using Credentials
Client uses the standard Access procedure to acquire an Access Token.

### 2. Open a WebSocket and Get Authorized
Client opens a WebSocket connection and sends their access token using the authorization procedure noted in the WebSockets section.

### 3. Build a Request
Request parameters are an object in JSON format. Each request for real-time data requires a symbol parameter that specifies the contract for which market data is requested. Contract can be specified either by the contract symbol string or by the contract ID integer:

```json
{
    "symbol":"ESM7" // Contract is specified by contract symbol
    ...
}
// or
{
    "symbol":123456 // Contract is specified by contract ID
    ...
}
```

Requests may have additional parameters, which are described in the corresponding sections.

### 4. Subscribe to Real-Time Data
The client sends the request parameters via WebSocket message to the server, specifying an endpoint such as `md/subscribeQuote`. The server sends back a response message:

- If a response has an error, client can perform error handling.
- If a response is successful, the corresponding subscription is activated. The client will begin to receive market data. In order to properly unsubscribe from market data, the client is responsible for keeping track of the contracts for which subscriptions are activated. A client can have a single subscription of each type (quotes, DOM, or charts) per contract.

---

## Handling Market Data
Market data arrives from the Tradovate server to the client asynchronously as event messages of `md` or `chart` types, for example:

```json
{
  "e":"md",
  "d": {
    "quotes": [
      {
        "timestamp":"2021-04-13T04:59:06.588Z",
        "contractId":123456,
        "entries": {
          "Bid": { "price":18405.123, "size":7.123 },
          "TotalTradeVolume": { "size":4118.123 },
          "Offer": { "price":18410.012, "size":12.35 },
          "LowPrice": { "price":18355.23 },
          "Trade": { "price":18405.023, "size":2.10 },
          "OpenInterest": { "size":40702.024 },
          "OpeningPrice": { "price":18515.123 },
          "HighPrice": { "price":18520.125 },
          "SettlementPrice": { "price":18520.257 }
        }
      }
    ]
  }
}
```

---

## Unsubscribing From Market Data
Mirroring the process for market data subscription, the client creates request parameters, specifies request endpoint such as `md/unsubscribeQuote` and sends the request to the Tradovate server. If the request is successful, the server will deactivate your subscription and the client will stop receiving real-time data.

---

## Request Reference

### Subscribe Quote
**Endpoint:** `md/subscribeQuote`

**Parameters:**
```json
{ "symbol": "ESM7" | 123456 }
```

**Data message:**
```json
{
  "e":"md",
  "d": {
    "quotes": [
      {
        "timestamp":"2021-04-13T04:59:06.588Z",
        "contractId":123456,
        "entries": {
          "Bid": { "price":18405.123, "size":7.123 },
          "TotalTradeVolume": { "size":4118.123 },
          "Offer": { "price":18410.012, "size":12.35 },
          "LowPrice": { "price":18355.23 },
          "Trade": { "price":18405.023, "size":2.10 },
          "OpenInterest": { "size":40702.024 },
          "OpeningPrice": { "price":18515.123 },
          "HighPrice": { "price":18520.125 },
          "SettlementPrice": { "price":18520.257 }
        }
      }
    ]
  }
}
```

---

### Unsubscribe Quote
**Endpoint:** `md/unsubscribeQuote`

**Parameters:**
```json
{ "symbol": "ESM7" | 123456 }
```

---

### Subscribe DOM
**Endpoint:** `md/subscribeDOM`

**Parameters:**
```json
{ "symbol": "ESM7" | 123456 }
```

**Data message:**
```json
{
  "e":"md",
  "d": {
    "doms": [
      {
        "contractId":123456,
        "timestamp":"2021-04-13T11:33:57.488Z",
        "bids": [
          {"price":2335.25,"size":33.54},
          {"price":2333,"size":758.21}
        ],
        "offers": [
          {"price":2335.5,"size":255.12},
          {"price":2337.75,"size":466.64}
        ]
      }
    ]
  }
}
```

---

### Unsubscribe DOM
**Endpoint:** `md/unsubscribeDOM`

**Parameters:**
```json
{ "symbol": "ESM7" | 123456 }
```

---

### Subscribe Histogram
**Endpoint:** `md/subscribeHistogram`

**Parameters:**
```json
{ "symbol": "ESM7" | 123456 }
```

**Data message:**
```json
{
  "e":"md",
  "d": {
    "histograms": [
      {
        "contractId":123456,
        "timestamp":"2017-04-13T11:33:57.412Z",
        "tradeDate": {
          "year":2022,
          "month":4,
          "day":13
        },
        "base":2338.75,
        "items": {
          "-14":5906.67,
          "2":1234.55
        },
        "refresh":false
      }
    ]
  }
}
```

---

### Unsubscribe Histogram
**Endpoint:** `md/unsubscribeHistogram`

**Parameters:**
```json
{ "symbol": "ESM7" | 123456 }
```

---

### Get Chart
**Description:** Client may have multiple charts for the same contract, so the response for `md/getChart` request contains a subscription ID to properly track and unsubscribe from a real-time chart subscription. If you're using JavaScript, don't forget to check the section on charts in our API's comprehensive JavaScript tutorial.

**Endpoint:** `md/getChart`

**Parameters:**
```json
{
  "symbol":"ESM7" | 123456,
  "chartDescription": {
    "underlyingType":"MinuteBar",
    "elementSize":15,
    "elementSizeUnit":"UnderlyingUnits",
    "withHistogram": true | false
  },
  "timeRange": {
    "closestTimestamp":"2017-04-13T11:33Z",
    "closestTickId":123,
    "asFarAsTimestamp":"2017-04-13T11:33Z",
    "asMuchAsElements":66
  }
}
```

**Available values — underlyingType:** `Tick`, `DailyBar`, `MinuteBar`, `Custom`, `DOM`

**Available values — elementSizeUnit:** `Volume`, `Range`, `UnderlyingUnits`, `Renko`, `MomentumRange`, `PointAndFigure`, `OFARange`

> All fields in `timeRange` are optional, but at least one is required.

**Response:**

A response for `md/getChart` request contains two subscription IDs, `historicalId` and `realtimeId`. Client needs to store the `realtimeId` value to properly cancel the real-time chart subscription via `md/cancelChart`.

```json
{
  "s":200,
  "i":13,
  "d":{
    "historicalId":32,
    "realtimeId":31
  }
}
```

**Data message:**
```json
{
  "e":"chart",
  "d": {
    "charts": [
      {
        "id":9,
        "td":20170413,
        "bars": [
          {
            "timestamp":"2017-04-13T11:00:00.000Z",
            "open":2334.25,
            "high":2334.5,
            "low":2333,
            "close":2333.75,
            "upVolume":4712.234,
            "downVolume":201.124,
            "upTicks":1333.567,
            "downTicks":82.890,
            "bidVolume":2857.123,
            "offerVolume":2056.224
          }
        ]
      }
    ]
  }
}
```

---

### Cancel Chart
**Endpoint:** `md/cancelChart`

**Parameters:**
```json
{
  "subscriptionId": 123456
}
```
