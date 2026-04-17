## Technical-Indicators-Lib#

Library for technical indicator development targeted toward equity index futures

---
Currently the scope is within webull, NT, and IBKR
---
## 🔐 Authentication & Session
- `login(email, password)`
- `get_mfa(email)` → request verification code
- `login_with_mfa(email, password, code)`
- `refresh_login()`
- `logout()`

## 👤 Account & Portfolio
- `get_account()`
- `get_portfolio()`
- `get_positions()`
- `get_account_id()`
- `get_capital_flow()`
- `get_funds()`

## 📊 Market Data
- `get_quote(stock)`
- `get_realtime(stock)`
- `get_bars(stock, interval='m1', count=100)`
- `get_historical(stock, interval='d1')`
- `get_option_quote(option_id)`
- `get_option_chain(stock)`
- `get_market_news()`
- `get_top_gainers()`
- `get_top_losers()`

## 📈 Orders & Trading
- `place_order(stock, price, qty, action, orderType)`
- `place_order_option(...)`
- `cancel_order(order_id)`
- `get_orders(status='ALL')`
- `get_active_orders()`
- `modify_order(order_id, ...)`

## 📉 Options Tools
- `get_options(stock)`
- `get_option_expiration_dates(stock)`
- `get_option_quote(option_id)`
- `place_order_option(...)`

## 🔎 Search & Instrument Info
- `get_ticker(stock)`
- `search(stock)`
- `get_instrument_id(stock)`
- `get_security(stock)`

## 🧪 Screener / Discovery
- `get_screener_results()`
- `get_watchlist()`
- `add_to_watchlist(stock)`
- `remove_from_watchlist(stock)`

## 🧰 Utility / Misc
- `get_trade_token()` → required before placing orders
- `get_device_id()`
- `get_uuid()`
- ---
