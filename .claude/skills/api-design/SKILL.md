---
name: api-design
description: طراحی قراردادهای REST/Minimal API برای ForeignTrade.
---
# API Design

- قرارداد endpoint را قبل از UI تعریف کن و DTO را مستقل از Entity نگه دار.
- مسیرها resource-oriented و versioned زیر `/api/v1` باشند.
- ورودی را با validation صریح بررسی کن؛ over-posting ممنوع.
- خطاها status code درست و پیام امن داشته باشند؛ stack trace/SQL details هرگز به client نرود.
- همه endpointهای خصوصی authentication و authorization صریح داشته باشند.
- mutationها با antiforgery در cookie-authenticated browser API محافظت شوند.
- endpointهای چندشرکتی باید فقط از فهرست مجاز server-side استفاده کنند.
- OpenAPI و قرارداد TypeScript runtime parser را همگام نگه دار.
- pagination، idempotency و concurrency را برای endpointهای متناسب طراحی کن.
