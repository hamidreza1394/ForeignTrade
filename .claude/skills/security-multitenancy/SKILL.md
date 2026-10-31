---
name: security-multitenancy
description: بررسی و پیاده‌سازی امنیت و جداسازی Tenant و Company در ForeignTrade.
---
# Security & Multitenancy

- هویت کاربر را از principal معتبر سرور استخراج کن.
- Tenant/Company/Period از ورودی client قابل اعتماد نیستند.
- عضویت فعال و permission هر درخواست را بررسی کن.
- فیلترهای EF Core لایه دفاعی دوم‌اند، نه جایگزین مجوز.
- همه مسیرهای query/write/export/import/background job/raw SQL را بررسی کن.
- تست منفی cross-tenant، cross-company و period-ineligible بنویس.
- privileged cross-tenant operation باید explicit و audited باشد.
- خطاهای auth پاسخ امن بدهند؛ وجود حساب/منبع غیرمجاز را بی‌جهت افشا نکن.
