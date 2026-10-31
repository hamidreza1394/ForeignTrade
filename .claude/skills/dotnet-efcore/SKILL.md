---
name: dotnet-efcore
description: توسعه .NET، ASP.NET Core و EF Core برای ForeignTrade.
---
# .NET & EF Core

- نسخه target framework و package graph را از مخزن بخوان.
- Domain/Application/Infrastructure را بر اساس نیاز واقعی جدا نگه دار.
- async I/O و CancellationToken را رعایت کن.
- ورودی‌ها را validate و خروجی خطا را امن کن.
- برای پول decimal با precision مشخص؛ تاریخ UTC؛ concurrency token برای ویرایش همزمان.
- قبل از migration مدل، index، FK، SQL تولیدی، rollback و backup را بررسی کن.
- از EnsureCreated در کنار migration production استفاده نکن مگر تصمیم ثبت‌شده.
- queryهای N+1 و بارگذاری بی‌نیاز را کنترل کن.
