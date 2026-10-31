# راهبرد تست

## هرم تست
- Unit: قواعد دامنه، محاسبات، وضعیت دوره و مجوزها.
- Integration: EF Core/SQL Server، API، cookie، antiforgery و transaction.
- Security: تست منفی Tenant/Company/Period و RBAC.
- Component: فرم ورود، انتخاب شرکت/دوره، حالت‌های خطا و دسترس‌پذیری.
- E2E: ورود تا انتخاب شرکت/دوره و ورود به خانه؛ سناریوهای چندشرکتی.

## الزامات
- منطق جدید: ابتدا تست شکست‌خورنده.
- دسترسی cross-tenant و cross-company حتماً تست منفی داشته باشد.
- تست‌های UI وضعیت loading/error/empty/disabled و keyboard navigation را پوشش دهند.
- تست‌ها با دیتابیس SQL Server واقعی یا container سازگار اجرا شوند؛ SQLite جایگزین خودکار تست SQL Server نیست.
- تست‌های واقعی چندمیزبانه/استقرار تا مرحله آماده‌سازی محیط جدا نگه داشته شوند.

## گزارش
نتایج را به Pass، Fail، Not Run و Blocked تفکیک کنید. هرگز از build/test اجرا‌نشده نتیجه‌گیری نکنید.
