# توسعه محلی

نسخه دقیق SDKها و ابزارها باید در `global.json`، `Directory.Packages.props` و `ClientApp/package.json` ثبت و در CI کنترل شود؛ پیش از تثبیت پروژه هیچ نسخه‌ای از فرانو به‌عنوان الزام قطعی تلقی نشود.

## پیش‌نیازهای هدف
- .NET SDK نسخه مصوب پروژه
- SQL Server نسخه پشتیبانی‌شده
- Node.js LTS سازگار با Vite
- Git و IDE دلخواه
- دسترسی اولیه برای restore وابستگی‌ها؛ اجرای runtime نباید به CDN نیاز داشته باشد.

## اصول پیکربندی
- اتصال دیتابیس از Environment/User Secrets/Secret Store تزریق شود.
- فایل‌های secret و تنظیمات شخصی در Git نباشند.
- Cloud و Dedicated Server از یک کدبیس و profileهای پیکربندی استفاده کنند.
- قبل از هر migration، SQL تولیدی، backup، سازگاری و rollback بررسی شود.

## گردش توسعه
پس از تثبیت Solution:
1. restore
2. تست‌های واحد
3. تست‌های integration با SQL Server
4. typecheck و تست React
5. build کلاینت
6. build و test سرور
7. بررسی انتشار آفلاین و نبود CDN
