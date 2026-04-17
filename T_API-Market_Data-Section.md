

# Market Data

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

# Replay

## changeSpeed
Change the playback speed of a Market Replay session.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /replay/changespeed`

**Request Body schema:** `application/json`

| Field | Required | Type | Constraints |
|-------|----------|------|-------------|
| `speed` | required | integer \<int32\> | [ 0 .. 400 ] |

**Request sample:**
```json
{
  "speed": 400
}
```

**Response sample (200):**
```json
{
  "errorText": "string",
  "ok": true
}
```

---

## checkReplaySession
Before beginning a Market Replay session, call this endpoint to check that the given timeframe is valid within the scope of the user's entitlements.

> You should use this endpoint from a WebSocket hooked up to the Market Replay URL.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /replay/checkreplaysession`

**Example:**
```js
const URL = 'wss://replay.tradovateapi.com/v1/websocket'

const myMarketReplaySocket = new WebSocket(URL)

// Simple WebSocket authorization procedure
myMarketReplaySocket.onopen = function() {
    myMarketReplaySocket.send(`authorize\n0\n\n${accessToken}`)
}

// JSON string for midnight April 30th 2018
const startTimestamp = new Date('2018-04-30').toJSON()
myMarketReplaySocket.send(`replay/checkreplaysession\n1\n\n${JSON.stringify({startTimestamp})}`)

// Listen for response
myMarketReplaySocket.addEventListener('message', msg => {
    const datas = JSON.parse(msg.data.slice(1)) // chop off leading 'frame' char
    // datas looks like this: [{ s: 200, i: 1, d: { checkStatus: 'OK' } }]
    if(datas) {
        datas.forEach(({i, d}) => {
            if(i && i === 1) { // id of our sent message is 1, response's `i` field will be 1
                console.log(d) // => { checkStatus: 'OK' }
                // if the status is OK we can send the initializeClock message
            }
        })
    }
})
```

**Request Body schema:** `application/json`

| Field | Required | Type |
|-------|----------|------|
| `startTimestamp` | required | string \<date-time\> |

**Request sample:**
```json
{
  "startTimestamp": "2019-08-24T14:15:22Z"
}
```

**Response sample (200):**
```json
{
  "checkStatus": "Ineligible",
  "startTimestamp": "2019-08-24T14:15:22Z"
}
```

---

## initializeClock
Set the initial date and time for a market replay session.

Using a WebSocket connected to the Tradovate Market Replay URL, we can start a Market Replay session which will simulate a given timeframe as if it were happening live. Each replay session creates a new replay account which gets discarded at the end of the replay session.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /replay/initializeclock`

**Example:**
```js
const URL = 'wss://replay.tradovateapi.com/v1/websocket'

const myMarketReplaySocket = new WebSocket(URL)

// Simple WebSocket authorization procedure
myMarketReplaySocket.onopen = function() {
    myMarketReplaySocket.send(`authorize\n0\n\n${accessToken}`)
}

const requestBody = {
    startTimestamp: new Date('2018-04-30').toJSON(),
    speed: 100,        // 100%, range is from 0-400%
    initialBalance: 50000  // account balance for replay session
}

myMarketReplaySocket.send(`replay/initializeclock\n1\n\n${JSON.stringify(requestBody)}`)

myMarketReplaySocket.addEventListener('message', msg => {
    const datas = JSON.parse(msg.data.slice(1))
    if(datas) {
        datas.forEach(({i, d}) => {
            if(i && i === 1) { // sent id is 1, response id will be 1
                console.log(d) // => { ok: true }
            }
        })
    }
})
```

**Request Body schema:** `application/json`

| Field | Required | Type | Constraints |
|-------|----------|------|-------------|
| `startTimestamp` | required | string \<date-time\> | |
| `speed` | required | integer \<int32\> | [ 0 .. 400 ] |
| `initialBalance` | optional | number \<double\> | |

**Request sample:**
```json
{
  "startTimestamp": "2019-08-24T14:15:22Z",
  "speed": 400,
  "initialBalance": 50000
}
```

**Response sample (200):**
```json
{
  "errorText": "string",
  "ok": true
}
```
# Configuration

## adminAlertFind
Retrieves an entity of AdminAlert type by its name.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /adminAlert/find`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `name` | required | string |

**Response sample (200):**
```json
{
"id": 0,
"name": "string",
"timestamp": "2019-08-24T14:15:22Z"
}

---

## adminAlertItem
Retrieves an entity of AdminAlert type by its id.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /adminAlert/item`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `id` | required | integer \<int64\> |

**Response sample (200):**
```json{
"id": 0,
"name": "string",
"timestamp": "2019-08-24T14:15:22Z"
}

---

## adminAlertItems
Retrieves multiple entities of AdminAlert type by its ids.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /adminAlert/items`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `ids` | required | Array of integers \<int64\> |

**Response sample (200):**
```json[
{
"id": 0,
"name": "string",
"timestamp": "2019-08-24T14:15:22Z"
}
]

---

## adminAlertList
Retrieves all entities of AdminAlert type.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /adminAlert/list`

**Response sample (200):**
```json[
{
"id": 0,
"name": "string",
"timestamp": "2019-08-24T14:15:22Z"
}
]

---

## adminAlertSuggest
Retrieves entities of AdminAlert type filtered by an occurrence of a text in one of its fields.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /adminAlert/suggest`

**Query Parameters:**

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| `t` | required | string | Text |
| `l` | required | integer \<int32\> | Max number of entities |

**Response sample (200):**
```json[
{
"id": 0,
"name": "string",
"timestamp": "2019-08-24T14:15:22Z"
}
]

---

## clearingHouseFind
Retrieves an entity of ClearingHouse type by its name.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /clearingHouse/find`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `name` | required | string |

**Response sample (200):**
```json{
"id": 0,
"name": "string"
}

---

## clearingHouseItem
Retrieves an entity of ClearingHouse type by its id.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /clearingHouse/item`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `id` | required | integer \<int64\> |

**Response sample (200):**
```json{
"id": 0,
"name": "string"
}

---

## clearingHouseItems
Retrieves multiple entities of ClearingHouse type by its ids.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /clearingHouse/items`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `ids` | required | Array of integers \<int64\> |

**Response sample (200):**
```json[
{
"id": 0,
"name": "string"
}
]

---

## clearingHouseList
Retrieves all entities of ClearingHouse type.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /clearingHouse/list`

**Response sample (200):**
```json[
{
"id": 0,
"name": "string"
}
]

---

## clearingHouseSuggest
Retrieves entities of ClearingHouse type filtered by an occurrence of a text in one of its fields.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /clearingHouse/suggest`

**Query Parameters:**

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| `t` | required | string | Text |
| `l` | required | integer \<int32\> | Max number of entities |

**Response sample (200):**
```json[
{
"id": 0,
"name": "string"
}
]

---

## entitlementItem
Retrieves an entity of Entitlement type by its id.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /entitlement/item`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `id` | required | integer \<int64\> |

**Response sample (200):**
```json
{
"id": 0,
"title": "string",
"price": 0.1,
"startDate": { "year": 0, "month": 0, "day": 0 },
"discontinuedDate": { "year": 0, "month": 0, "day": 0 },
"name": "string",
"duration": 0,
"durationUnits": "Lifetime",
"autorenewal": true
}

---

## entitlementItems
Retrieves multiple entities of Entitlement type by its ids.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /entitlement/items`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `ids` | required | Array of integers \<int64\> |

**Response sample (200):**
```json[
{
"id": 0,
"title": "string",
"price": 0.1,
"startDate": {},
"discontinuedDate": {},
"name": "string",
"duration": 0,
"durationUnits": "Lifetime",
"autorenewal": true
}
]

---

## entitlementList
Retrieves all entities of Entitlement type.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /entitlement/list`

**Response sample (200):**
```json[
{
"id": 0,
"title": "string",
"price": 0.1,
"startDate": {},
"discontinuedDate": {},
"name": "string",
"duration": 0,
"durationUnits": "Lifetime",
"autorenewal": true
}
]

---

## orderStrategyTypeFind
Retrieves an entity of OrderStrategyType type by its name.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /orderStrategyType/find`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `name` | required | string |

**Response sample (200):**
```json{
"id": 0,
"name": "string",
"enabled": true
}

---

## orderStrategyTypeItem
Retrieves an entity of OrderStrategyType type by its id.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /orderStrategyType/item`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `id` | required | integer \<int64\> |

**Response sample (200):**
```json{
"id": 0,
"name": "string",
"enabled": true
}

---

## orderStrategyTypeItems
Retrieves multiple entities of OrderStrategyType type by its ids.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /orderStrategyType/items`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `ids` | required | Array of integers \<int64\> |

**Response sample (200):**
```json[
{
"id": 0,
"name": "string",
"enabled": true
}
]

---

## orderStrategyTypeList
Retrieves all entities of OrderStrategyType type.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /orderStrategyType/list`

**Response sample (200):**
```json[
{
"id": 0,
"name": "string",
"enabled": true
}
]

---

## orderStrategyTypeSuggest
Retrieves entities of OrderStrategyType type filtered by an occurrence of a text in one of its fields.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /orderStrategyType/suggest`

**Query Parameters:**

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| `t` | required | string | Text |
| `l` | required | integer \<int32\> | Max number of entities |

**Response sample (200):**
```json[
{
"id": 0,
"name": "string",
"enabled": true
}
]

---

## propertyFind
Retrieves an entity of Property type by its name.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /property/find`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `name` | required | string |

**Response sample (200):**
```json{
"id": 0,
"name": "string",
"propertyType": "Boolean",
"enumOptions": "string",
"defaultValue": "string"
}

---

## propertyItem
Retrieves an entity of Property type by its id.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /property/item`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `id` | required | integer \<int64\> |

**Response sample (200):**
```json{
"id": 0,
"name": "string",
"propertyType": "Boolean",
"enumOptions": "string",
"defaultValue": "string"
}

---

## propertyItems
Retrieves multiple entities of Property type by its ids.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /property/items`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `ids` | required | Array of integers \<int64\> |

**Response sample (200):**
```json[
{
"id": 0,
"name": "string",
"propertyType": "Boolean",
"enumOptions": "string",
"defaultValue": "string"
}
]

---

## propertyList
Retrieves all entities of Property type.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /property/list`

**Response sample (200):**
```json[
{
"id": 0,
"name": "string",
"propertyType": "Boolean",
"enumOptions": "string",
"defaultValue": "string"
}
]

---

## propertySuggest
Retrieves entities of Property type filtered by an occurrence of a text in one of its fields.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /property/suggest`

**Query Parameters:**

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| `t` | required | string | Text |
| `l` | required | integer \<int32\> | Max number of entities |

---

# Users

## contactInfoDependents
Retrieves all entities of ContactInfo type related to User entity.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /contactInfo/deps`

**Query Parameters:**

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| `masterid` | required | integer \<int64\> | id of User entity |

**Response sample (200):**
```json[
{
"id": 0,
"userId": 0,
"firstName": "string",
"lastName": "string",
"streetAddress1": "string",
"streetAddress2": "string",
"city": "string",
"state": "string",
"postCode": "string",
"country": "st",
"phone": "string",
"mailingIsDifferent": true,
"mailingStreetAddress1": "string",
"mailingStreetAddress2": "string",
"mailingCity": "string",
"mailingState": "string",
"mailingPostCode": "string",
"mailingCountry": "st",
"jointFirstName": "string",
"jointLastName": "string",
"iraCustodianName": "string"
}
]

---

## contactInfoItem
Retrieves an entity of ContactInfo type by its id.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /contactInfo/item`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `id` | required | integer \<int64\> |

**Response sample (200):**
```json{
"id": 0,
"userId": 0,
"firstName": "string",
"lastName": "string",
"streetAddress1": "string",
"streetAddress2": "string",
"city": "string",
"state": "string",
"postCode": "string",
"country": "st",
"phone": "string",
"mailingIsDifferent": true,
"mailingStreetAddress1": "string",
"mailingStreetAddress2": "string",
"mailingCity": "string",
"mailingState": "string",
"mailingPostCode": "string",
"mailingCountry": "st",
"jointFirstName": "string",
"jointLastName": "string",
"iraCustodianName": "string"
}

---

## contactInfoItems
Retrieves multiple entities of ContactInfo type by its ids.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /contactInfo/items`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `ids` | required | Array of integers \<int64\> |

---

## contactInfoLDependents
Retrieves all entities of ContactInfo type related to multiple entities of User type.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /contactInfo/ldeps`

**Query Parameters:**

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| `masterids` | required | Array of integers \<int64\> | ids of User entities |

---

## marketDataSubscriptionCreate
Creates a new entity of MarketDataSubscription.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /marketDataSubscription/create`

**Request Body schema:** `application/json`

| Field | Required | Type | Constraints |
|-------|----------|------|-------------|
| `id` | optional | integer \<int64\> | |
| `userId` | required | integer \<int64\> | > 0 |
| `timestamp` | required | string \<date-time\> | |
| `planPrice` | required | number \<double\> | |
| `creditCardTransactionId` | optional | integer \<int64\> | > 0 |
| `cashBalanceLogId` | optional | integer \<int64\> | > 0 |
| `creditCardId` | optional | integer \<int64\> | > 0 |
| `accountId` | optional | integer \<int64\> | > 0 |
| `marketDataSubscriptionPlanId` | required | integer \<int64\> | > 0 |
| `year` | required | integer \<int32\> | [ 2015 .. 2030 ] |
| `month` | required | integer \<int32\> | [ 1 .. 12 ] |
| `expired` | required | boolean | |
| `renewalCreditCardId` | optional | integer \<int64\> | > 0 |
| `renewalAccountId` | optional | integer \<int64\> | > 0 |

**Request sample:**
```json{
"id": 0,
"userId": 0,
"timestamp": "2019-08-24T14:15:22Z",
"planPrice": 0.1,
"creditCardTransactionId": 0,
"cashBalanceLogId": 0,
"creditCardId": 0,
"accountId": 0,
"marketDataSubscriptionPlanId": 0,
"year": 2015,
"month": 1,
"expired": true,
"renewalCreditCardId": 0,
"renewalAccountId": 0
}

**Response sample (200):**
```json{
"id": 0,
"userId": 0,
"timestamp": "2019-08-24T14:15:22Z",
"planPrice": 0.1,
"creditCardTransactionId": 0,
"cashBalanceLogId": 0,
"creditCardId": 0,
"accountId": 0,
"marketDataSubscriptionPlanId": 0,
"year": 2015,
"month": 1,
"expired": true,
"renewalCreditCardId": 0,
"renewalAccountId": 0
}

---

## marketDataSubscriptionDependents
Retrieves all entities of MarketDataSubscription type related to User entity.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /marketDataSubscription/deps`

**Query Parameters:**

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| `masterid` | required | integer \<int64\> | id of User entity |

---

## marketDataSubscriptionItem
Retrieves an entity of MarketDataSubscription type by its id.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /marketDataSubscription/item`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `id` | required | integer \<int64\> |

---

## marketDataSubscriptionItems
Retrieves multiple entities of MarketDataSubscription type by its ids.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /marketDataSubscription/items`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `ids` | required | Array of integers \<int64\> |

---

## marketDataSubscriptionLDependents
Retrieves all entities of MarketDataSubscription type related to multiple entities of User type.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /marketDataSubscription/ldeps`

**Query Parameters:**

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| `masterids` | required | Array of integers \<int64\> | ids of User entities |

---

## marketDataSubscriptionList
Retrieves all entities of MarketDataSubscription type.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /marketDataSubscription/list`

---

## marketDataSubscriptionUpdate
Updates an existing entity of MarketDataSubscription.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /marketDataSubscription/update`

**Request sample:**
```json{
"id": 0,
"userId": 0,
"timestamp": "2019-08-24T14:15:22Z",
"planPrice": 0.1,
"creditCardTransactionId": 0,
"cashBalanceLogId": 0,
"creditCardId": 0,
"accountId": 0,
"marketDataSubscriptionPlanId": 0,
"year": 2015,
"month": 1,
"expired": true,
"renewalCreditCardId": 0,
"renewalAccountId": 0
}

---

## organizationFind
Retrieves an entity of Organization type by its name.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /organization/find`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `name` | required | string |

**Response sample (200):**
```json{
"id": 0,
"name": "string"
}

---

## organizationItem
Retrieves an entity of Organization type by its id.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /organization/item`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `id` | required | integer \<int64\> |

---

## organizationItems / organizationList / organizationSuggest

**Endpoints:**
- `GET /organization/items` — multiple by ids
- `GET /organization/list` — all entities
- `GET /organization/suggest` — filtered by text (`t`, `l` query params)

**Response sample (200):**
```json[
{
"id": 0,
"name": "string"
}
]

---

## secondMarketDataSubscriptionDependents
Retrieves all entities of SecondMarketDataSubscription type related to User entity.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /secondMarketDataSubscription/deps`

**Response sample (200):**
```json[
{
"id": 0,
"userId": 0,
"timestamp": "2019-08-24T14:15:22Z",
"year": 2015,
"month": 1,
"cancelledRenewal": true,
"cancellationTimestamp": "2019-08-24T14:15:22Z"
}
]

> Also available: `GET /secondMarketDataSubscription/item`, `/items`, `/ldeps`, `/list`

---

## tradovateSubscriptionCreate
Creates a new entity of TradovateSubscription.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /tradovateSubscription/create`

**Request sample:**
```json{
"id": 0,
"userId": 0,
"timestamp": "2019-08-24T14:15:22Z",
"planPrice": 0.1,
"creditCardTransactionId": 0,
"cashBalanceLogId": 0,
"creditCardId": 0,
"accountId": 0,
"tradovateSubscriptionPlanId": 0,
"startDate": { "year": 0, "month": 0, "day": 0 },
"expirationDate": { "year": 0, "month": 0, "day": 0 },
"paidAmount": 0.1,
"cancelledRenewal": true,
"cancelReason": "string"
}

> Also available: `GET /tradovateSubscription/deps`, `/item`, `/items`, `/ldeps`, `/list` and `POST /tradovateSubscription/update`

---

## acceptTradingPermission
Called to accept a given trading permission granted by another party.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/accepttradingpermission`

**Request sample:**
```json{
"tradingPermissionId": 0
}

**Response sample (200):**
```json{
"errorText": "string",
"tradingPermission": {
"id": 0,
"userId": 0,
"accountId": 0,
"accountHolderContact": "string",
"accountHolderEmail": "string",
"ctaContact": "string",
"ctaEmail": "string",
"status": "Accepted",
"updated": "2019-08-24T14:15:22Z",
"approvedById": 0
}
}

---

## activateSecondMarketDataSubscriptionRenewal
Used to setup a second market data subscription with active auto-renewal.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/activatesecondmarketdatasubscriptionrenewal`

**Request sample:**
```json{
"secondMarketDataSubscriptionId": 0
}

---

## addMarketDataSubscription
Add a subscription to Market Data for a user.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/addmarketdatasubscription`

**Request sample:**
```json{
"marketDataSubscriptionPlanIds": [0],
"year": 2015,
"month": 1,
"creditCardId": 0,
"accountId": 0,
"userId": 0
}

---

## addSecondMarketDataSubscription
Add a second market data subscription for a user.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/addsecondmarketdatasubscription`

**Request sample:**
```json{
"year": 2015,
"month": 1,
"creditCardId": 0,
"accountId": 0,
"userId": 0
}

---

## addTradovateSubscription
Used to add a Tradovate Trader membership plan for a user.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/addtradovatesubscription`

**Request sample:**
```json{
"tradovateSubscriptionPlanId": 0,
"creditCardId": 0,
"accountId": 0,
"userId": 0
}

---

## cancelEverything
Cancel all subscriptions, end plugin subscriptions, and revoke trading permissions.

> Higher level operation intended for B2B Partner applications. To fully revoke accounts and cancel all subscriptions, call this endpoint from both `live.tradovateapi.com/v1` and `demo.tradovateapi.com/v1`.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/canceleverything`

**Request sample:**
```json{
"userIds": [0],
"tradovateSubscriptions": true,
"tradingPermissions": true,
"userPlugins": true,
"marketDataSubscriptions": true
}

**Response sample (200):**
```json{
"tradovateSubscriptionIds": [0],
"tradingPermissionIds": [0],
"userPluginIds": [0],
"marketDataSubscriptionIds": [0]
}

---

## cancelSecondMarketDataSubscription
Cancel a second market data subscription for a user.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/cancelsecondmarketdatasubscription`

**Request sample:**
```json{
"secondMarketDataSubscriptionId": 0
}

---

## cancelSecondMarketDataSubscriptionRenewal
Cancel the auto-renewal for a second market data subscription for a user.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/cancelsecondmarketdatasubscriptionrenewal`

**Request sample:**
```json{
"secondMarketDataSubscriptionId": 0
}

---

## cancelTradovateSubscription
Cancel a Tradovate Trader membership plan.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/canceltradovatesubscription`

**Request sample:**
```json{
"tradovateSubscriptionId": 0,
"cancelReason": "string",
"expire": true
}

---

## createEvaluationAccounts
Batch create up to 100 simulation accounts at once.

> Preferred endpoint when creating many accounts to minimize API traffic. Supports inline `preTradeRisk` and `postTradeRisk` configuration.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/createevaluationaccounts`

**Request sample:**
```json{
"accounts": [{}]
}

**Response sample (200):**
```json{
"errorText": "string",
"results": [{}]
}

---

## createEvaluationUsers
Batch create up to 100 Organization users at once.

> Supports inline `tradovateSubscriptionPlanId` and `entitlementIds` assignment.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/createevaluationusers`

**Request sample:**
```json{
"users": [{}]
}

---

## createTradingPermission
Create a trading permission. Intended for use by B2B Partners.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/createtradingpermission`

**Request sample:**
```json{
"accountId": 0,
"userId": 0
}

---

## userFind
Retrieves an entity of User type by its name.

**Authorization:** `bearer_access_token`

**Endpoint:** `GET /user/find`

**Query Parameters:**

| Parameter | Required | Type |
|-----------|----------|------|
| `name` | required | string |

**Response sample (200):**
```json{
"id": 0,
"name": "string",
"timestamp": "2019-08-24T14:15:22Z",
"email": "string",
"status": "Active",
"professional": true,
"organizationId": 0,
"introducingPartnerId": 0
}

---

## getAccountTradingPermissions
Query the granted trading permissions associated with this account.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/getaccounttradingpermissions`

**Request sample:**
```json{
"accountId": 0
}

**Response sample (200):**
```json{
"tradingPermissions": [{}]
}

---

## getSecondMarketDataSubscriptionCost
Query the current price of a second market data subscription for a user.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/getsecondmarketdatasubscriptioncost`

**Request sample:**
```json{
"year": 2015,
"month": 1,
"userId": 0
}

**Response sample (200):**
```json{
"errorText": "string",
"monthlyCost": 0.1
}

---

## userItem / userItems / userList / userSuggest

**Endpoints:**
- `GET /user/item` — single by id
- `GET /user/items` — multiple by ids
- `GET /user/list` — all entities
- `GET /user/suggest` — filtered by text (`t`, `l` query params)

---

## modifyCredentials
Used to modify account username and password.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/modifycredentials`

**Request sample:**
```json{
"userId": 0,
"name": "string",
"password": "stringst",
"currentPassword": "string"
}

**Response sample (200):**
```json{
"errorText": "string",
"accessToken": "string",
"expirationTime": "2019-08-24T14:15:22Z",
"passwordExpirationTime": "2019-08-24T14:15:22Z",
"userStatus": "Active",
"userId": 0,
"name": "string",
"hasLive": true
}

---

## modifyEmailAddress
Change account email address information.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/modifyemailaddress`

**Request sample:**
```json{
"userId": 0,
"email": "string"
}

**Response sample (200):**
```json{
"errorText": "string",
"status": "Active"
}

---

## modifyPassword
Change account password information.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/modifypassword`

**Request sample:**
```json{
"userId": 0,
"password": "stringst",
"currentPassword": "string"
}

---

## openDemoAccount
Request to open a Demo account for a user. Typically used by B2B Partners.

> Prefer `/user/createEvaluationAccounts` when creating many accounts at once.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/opendemoaccount`

**Request sample:**
```json{
"templateAccountId": 0,
"name": "string",
"initialBalance": 0.1,
"defaultAutoLiq": {
"marginPercentageAlert": 0.1,
"dailyLossPercentageAlert": 0.1,
"dailyLossAlert": 0.1,
"marginPercentageLiqOnly": 0.1,
"dailyLossPercentageLiqOnly": 0.1,
"dailyLossLiqOnly": 0.1,
"marginPercentageAutoLiq": 0.1,
"dailyLossPercentageAutoLiq": 0.1,
"dailyLossAutoLiq": 0.1,
"weeklyLossAutoLiq": 0.1,
"flattenTimestamp": "2019-08-24T14:15:22Z",
"trailingMaxDrawdown": 0.1,
"trailingMaxDrawdownLimit": 0.1,
"trailingMaxDrawdownMode": "EOD",
"dailyProfitAutoLiq": 0.1,
"weeklyProfitAutoLiq": 0.1,
"doNotUnlock": true
},
"preTradeRisk": [{}]
}
**Response sample (200):**
```json
{
  "errorText": "string",
  "accountId": 0
}
```

---

## requestTradingPermission
Send a request to grant trading permission for your account to another party.

> B2B Partners should prefer `/user/createTradingPermission` or `/user/createEvaluationAccounts` instead.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/requesttradingpermission`

**Request sample:**
```json
{
  "accountId": 0,
  "ctaContact": "string",
  "ctaEmail": "string"
}
```

---

## revokeTradingPermission
Revoke an existing trading permission granted to another party.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/revoketradingpermission`

**Request sample:**
```json
{
  "tradingPermissionId": 0
}
```

---

## revokeTradingPermissions
Revoke multiple existing trading permissions granted to other parties.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/revoketradingpermissions`

**Request sample:**
```json
{
  "tradingPermissionIds": [0]
}
```

**Response sample (200):**
```json
{
  "errorText": "string",
  "ok": true
}
```

---

## signUpOrganizationMember
Used by B2B partners to create users for their own organizations.

> Supports inline `tradovateSubscriptionPlanId` and `entitlementIds`. Prefer `/user/createEvaluationUsers` for batch creation.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/signuporganizationmember`

**Request sample:**
```json
{
  "name": "string",
  "email": "string",
  "password": "stringst",
  "firstName": "string",
  "lastName": "string",
  "tradovateSubscriptionPlanId": 0,
  "entitlementIds": [0]
}
```

**Response sample (200):**
```json
{
  "errorText": "string",
  "errorCode": "DataError",
  "userId": 0,
  "emailVerified": true
}
```

---

## syncRequest
Used with WebSocket protocol. Returns all data associated with the user.

> Essential for efficient WebSocket API usage. Starts a subscription to real-time user data.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /user/syncrequest`

**Example:**
```js
const URL = 'wss://live.tradovateapi.com/v1/websocket'

const myWebSocket = new WebSocket(URL)

myWebSocket.onopen = function() {
    myWebSocket.send(`authorize\n0\n\n${accessToken}`)
}

const requestBody = {
    users: [12345]
}

myWebSocket.send(`user/syncrequest\n1\n\n${JSON.stringify(requestBody)}`)
// Starts a subscription to real-time user data.
```

**Request sample:**
```json
{
  "users": [0],
  "accounts": [0],
  "splitResponses": true
}
```

**Response sample (200):**
```json
{
  "users": [{}],
  "accounts": [{}],
  "accountRiskStatuses": [{}],
  "marginSnapshots": [{}],
  "userAccountAutoLiqs": [{}],
  "cashBalances": [{}],
  "currencies": [{}],
  "positions": [{}],
  "fillPairs": [{}],
  "orders": [{}],
  "contracts": [{}],
  "contractMaturities": [{}],
  "products": [{}],
  "exchanges": [{}],
  "spreadDefinitions": [{}],
  "commands": [{}],
  "commandReports": [{}],
  "executionReports": [{}],
  "orderVersions": [{}],
  "fills": [{}],
  "orderStrategies": [{}],
  "orderStrategyLinks": [{}],
  "userProperties": [{}],
  "properties": [{}],
  "userPlugins": [{}],
  "contractGroups": [{}],
  "orderStrategyTypes": [{}]
}
```

---

## addEntitlementSubscription
For use with Add-ons. Allows for purchase of entitlements such as Market Replay.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /userPlugin/addentitlementsubscription`

**Request sample:**
```json
{
  "entitlementId": 0,
  "creditCardId": 0,
  "accountId": 0,
  "userId": 0
}
```

---

## changePluginPermission
Change the permissions for a Trader plugin.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /userPlugin/changepluginpermission`

**Request sample:**
```json
{
  "userId": 0,
  "pluginName": "string",
  "approval": true
}
```

**Response sample (200):**
```json
{
  "errorText": "string",
  "ok": true
}
```

---

## userPluginCreate
Creates a new entity of UserPlugin.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /userPlugin/create`

**Request sample:**
```json
{
  "id": 0,
  "userId": 0,
  "timestamp": "2019-08-24T14:15:22Z",
  "planPrice": 0.1,
  "creditCardTransactionId": 0,
  "cashBalanceLogId": 0,
  "creditCardId": 0,
  "accountId": 0,
  "pluginName": "string",
  "approval": true,
  "entitlementId": 0,
  "startDate": { "year": 0, "month": 0, "day": 0 },
  "expirationDate": { "year": 0, "month": 0, "day": 0 },
  "paidAmount": 0.1,
  "autorenewal": true,
  "planCategories": "string"
}
```

> Also available: `GET /userPlugin/deps`, `/item`, `/items`, `/ldeps`, `/list` and `POST /userPlugin/update`

---

## userPropertyDependents / userPropertyItem / userPropertyItems / userPropertyLDependents

**Endpoints:**
- `GET /userProperty/deps` — all by User entity (`masterid`)
- `GET /userProperty/item` — single by id
- `GET /userProperty/items` — multiple by ids
- `GET /userProperty/ldeps` — all by multiple User entities (`masterids`)

**Response sample (200):**
```json
[
  {
    "id": 0,
    "userId": 0,
    "propertyId": 0,
    "value": "string"
  }
]
```

---

## userSessionItem / userSessionItems

**Endpoints:**
- `GET /userSession/item` — single by id
- `GET /userSession/items` — multiple by ids

**Response sample (200):**
```json
{
  "id": 0,
  "userId": 0,
  "startTime": "2019-08-24T14:15:22Z",
  "endTime": "2019-08-24T14:15:22Z",
  "ipAddress": "string",
  "appId": "string",
  "appVersion": "string",
  "clientAppId": 0
}
```

---

## userSessionStatsDependents / userSessionStatsItem / userSessionStatsItems / userSessionStatsLDependents / userSessionStatsList

**Endpoints:**
- `GET /userSessionStats/deps` — all by User entity (`masterid`)
- `GET /userSessionStats/item` — single by id
- `GET /userSessionStats/items` — multiple by ids
- `GET /userSessionStats/ldeps` — all by multiple User entities (`masterids`)
- `GET /userSessionStats/list` — all entities

**Response sample (200):**
```json
{
  "id": 0,
  "lastSessionTime": "2019-08-24T14:15:22Z",
  "failedPasswords": 0
}
```

---

# Chat

## closeChat
Close the chat context.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /chat/closechat`

**Request sample:**
```json
{
  "chatId": 0
}
```

**Response sample (200):**
```json
{
  "errorText": "string",
  "chat": {
    "id": 0,
    "userId": 0,
    "timestamp": "2019-08-24T14:15:22Z",
    "category": "Support",
    "assignedSupportId": 0,
    "closedById": 0,
    "closeTimestamp": "2019-08-24T14:15:22Z",
    "updatedTimestamp": "2019-08-24T14:15:22Z"
  }
}
```

---

## chatDependents / chatItem / chatItems / chatLDependents / chatList

**Endpoints:**
- `GET /chat/deps` — all by User entity (`masterid`)
- `GET /chat/item` — single by id
- `GET /chat/items` — multiple by ids
- `GET /chat/ldeps` — all by multiple User entities (`masterids`)
- `GET /chat/list` — all entities

**Response sample (200):**
```json
[
  {
    "id": 0,
    "userId": 0,
    "timestamp": "2019-08-24T14:15:22Z",
    "category": "Support",
    "assignedSupportId": 0,
    "closedById": 0,
    "closeTimestamp": "2019-08-24T14:15:22Z",
    "updatedTimestamp": "2019-08-24T14:15:22Z"
  }
]
```

---

## markAsReadChatMessage
Marks a chat message as read.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /chat/markasreadchatmessage`

**Request sample:**
```json
{
  "chatMessageId": 0
}
```

**Response sample (200):**
```json
{
  "errorText": "string",
  "chatMessage": {
    "id": 0,
    "timestamp": "2019-08-24T14:15:22Z",
    "chatId": 0,
    "senderId": 0,
    "senderName": "string",
    "text": "string",
    "readStatus": true
  }
}
```

---

## postChatMessage
Post a chat message to a given chat's history.

**Authorization:** `bearer_access_token`

**Endpoint:** `POST /chat/postchatmessage`

**Request Body schema:** `application/json`

| Field | Required | Type | Constraints |
|-------|----------|------|-------------|
| `userId` | optional | integer \<int64\> | > 0 |
| `category` | required | string | `"Support"` or `"TradeDesk"` |
| `text` | required | string | <= 1024 characters |

**Request sample:**
```json
{
  "userId": 0,
  "category": "Support",
  "text": "string"
}
```

**Response sample (200):**
```json
{
  "errorText": "string",
  "chatMessage": {
    "id": 0,
    "timestamp": "2019-08-24T14:15:22Z",
    "chatId": 0,
    "senderId": 0,
    "senderName": "string",
    "text": "string",
    "readStatus": true
  }
}
```

---

## chatMessageDependents / chatMessageItem / chatMessageItems / chatMessageLDependents

**Endpoints:**
- `GET /chatMessage/deps` — all by Chat entity (`masterid`)
- `GET /chatMessage/item` — single by id
- `GET /chatMessage/items` — multiple by ids
- `GET /chatMessage/ldeps` — all by multiple Chat entities (`masterids`)

**Response sample (200):**
```json
[
  {
    "id": 0,
    "timestamp": "2019-08-24T14:15:22Z",
    "chatId": 0,
    "senderId": 0,
    "senderName": "string",
    "text": "string",
    "readStatus": true
  }
]
```


