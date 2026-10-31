---
name: ui-ux-rtl
description: طراحی و پیاده‌سازی رابط فارسی RTL و دسترس‌پذیر.
---
# UI/UX Persian RTL

- راست‌چین واقعی، نه صرفاً text-align:right؛ از logical CSS properties استفاده کن.
- label و error text برای تمام فیلدها؛ focus و keyboard navigation.
- نمایش تاریخ محلی با ذخیره UTC و timezone مشخص.
- اعداد/کدها/شناسه‌ها با bidi isolation.
- وضعیت‌های loading, empty, error, disabled, read-only را طراحی کن.
- از کنتراست مناسب و نشانگرهای چندگانه برای وضعیت استفاده کن.
- فونت و icon محلی؛ هیچ runtime CDN.
- موبایل و دسکتاپ را تست کن.
