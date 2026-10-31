---
name: react-patterns
description: الگوهای React 19 و TypeScript برای رابط ForeignTrade.
---
# React Patterns

- Functional components و hooks؛ hooks فقط در top-level.
- State مشتق‌شده را در render محاسبه کن، نه در effect.
- API access در `src/api/client.ts`؛ responseها در runtime parse شوند.
- هیچ token/session secret در localStorage یا sessionStorage ذخیره نشود.
- فرم‌ها label، validation، loading، error و disabled state داشته باشند.
- keyboard navigation، focus، semantic HTML و screen-reader announcements الزامی‌اند.
- Tailwind/shadcn با نسخه ثبت‌شده؛ dependency جدید نیازمند دلیل و review.
- RTL با logical CSS properties و bidi isolation برای داده لاتین.
- برای هر تغییر رفتار، تست React Testing Library/Vitest اضافه کن.
