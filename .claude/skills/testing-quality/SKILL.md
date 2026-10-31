---
name: testing-quality
description: تست‌محوری، تست امنیت و کیفیت تحویل ForeignTrade.
---
# Testing & Quality

- برای رفتار جدید ابتدا تست RED، سپس GREEN و refactor.
- Unit برای منطق؛ Integration برای SQL/API؛ E2E برای جریان حیاتی.
- تست‌های مجوز و جداسازی منفی اجباری‌اند.
- از mock بیش از حد پرهیز؛ fake واقعی را ترجیح بده.
- تست flaky را با retry پنهان نکن.
- full suite و build را طبق فرمان‌های واقعی repo اجرا کن.
- Pass/Fail/Not Run/Blocked را جدا گزارش بده.
