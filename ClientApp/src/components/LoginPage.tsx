import { useState, type FormEvent } from 'react';
import { login } from '../api/client';
import { FaranouLogo } from './ui/FaranouLogo';

export function LoginPage({ onSuccess }: { onSuccess: () => void }) {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [pending, setPending] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (pending) return;
    setError(null);
    setPending(true);
    try {
      await login(username.trim(), password);
      onSuccess();
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'ورود انجام نشد. دوباره تلاش کنید.');
    } finally {
      setPending(false);
    }
  }

  return (
    <main dir="rtl" className="min-h-screen bg-[#f3f6fb] text-[#17263b]">
      <div className="grid min-h-screen lg:grid-cols-[1.05fr_0.95fr]">
        <section className="relative hidden overflow-hidden bg-[#10243d] px-12 py-14 text-white lg:flex lg:flex-col lg:justify-between">
          <div className="absolute -left-24 -top-24 size-96 rounded-full border border-white/10" />
          <div className="absolute -bottom-40 -right-28 size-[34rem] rounded-full border border-[#d5ad63]/20" />
          <FaranouLogo size={44} />
          <div className="relative max-w-xl">
            <p className="mb-5 text-sm font-semibold tracking-wide text-[#e2bd7c]">سامانه یکپارچه بازرگانی خارجی</p>
            <h1 className="text-4xl font-extrabold leading-[1.7]">مدیریت هوشمند فرآیندهای بازرگانی و واردات</h1>
            <p className="mt-5 max-w-lg text-base leading-8 text-white/70">دسترسی امن، گردش کار شفاف و اطلاعات قابل اتکا در تمام مراحل عملیات بازرگانی.</p>
          </div>
          <p className="relative text-xs text-white/50">دسترسی امن به اطلاعات سازمانی شما</p>
        </section>

        <section className="flex items-center justify-center px-5 py-12 sm:px-10">
          <div className="w-full max-w-md">
            <div className="mb-10 lg:hidden"><FaranouLogo size={42} /></div>
            <div className="mb-8">
              <p className="text-sm font-semibold text-[#a47a37]">خوش آمدید</p>
              <h2 className="mt-2 text-3xl font-extrabold">ورود به سامانه</h2>
              <p className="mt-3 text-sm leading-7 text-slate-500">برای ادامه، نام کاربری و گذرواژه خود را وارد کنید.</p>
            </div>

            <form onSubmit={handleSubmit} className="space-y-5" noValidate>
              <div>
                <label htmlFor="username" className="mb-2 block text-sm font-semibold">نام کاربری</label>
                <input id="username" name="username" autoComplete="username" required value={username}
                  onChange={event => setUsername(event.target.value)} disabled={pending}
                  className="h-12 w-full rounded-xl border border-slate-300 bg-white px-4 outline-none transition focus:border-[#315d8c] focus:ring-4 focus:ring-blue-100 disabled:opacity-60"
                  placeholder="نام کاربری خود را وارد کنید" />
              </div>
              <div>
                <label htmlFor="password" className="mb-2 block text-sm font-semibold">گذرواژه</label>
                <div className="relative">
                  <input id="password" name="password" type={showPassword ? 'text' : 'password'} autoComplete="current-password" required value={password}
                    onChange={event => setPassword(event.target.value)} disabled={pending}
                    className="h-12 w-full rounded-xl border border-slate-300 bg-white px-4 pe-12 outline-none transition focus:border-[#315d8c] focus:ring-4 focus:ring-blue-100 disabled:opacity-60"
                    placeholder="گذرواژه خود را وارد کنید" />
                  <button type="button" onClick={() => setShowPassword(value => !value)} aria-label={showPassword ? 'مخفی کردن گذرواژه' : 'نمایش گذرواژه'}
                    className="absolute inset-y-0 start-3 rounded-md px-2 text-xs font-semibold text-slate-500 hover:text-slate-800 focus-visible:outline-2 focus-visible:outline-offset-2">
                    {showPassword ? 'مخفی' : 'نمایش'}
                  </button>
                </div>
              </div>
              {error ? <p role="alert" className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm leading-6 text-red-700">{error}</p> : null}
              <button type="submit" disabled={pending || !username.trim() || !password}
                className="flex h-12 w-full items-center justify-center rounded-xl bg-[#102f50] px-5 text-sm font-bold text-white shadow-sm transition hover:bg-[#173f68] focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[#315d8c] disabled:cursor-not-allowed disabled:opacity-50">
                {pending ? 'در حال بررسی اطلاعات…' : 'ورود امن به سامانه'}
              </button>
            </form>
            <p className="mt-8 text-center text-xs leading-6 text-slate-400">اطلاعات ورود شما به‌صورت امن در سرور بررسی می‌شود.</p>
          </div>
        </section>
      </div>
    </main>
  );
}
