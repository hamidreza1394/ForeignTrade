import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { LoginPage } from './LoginPage';
import { login } from '../api/client';

vi.mock('../api/client', () => ({ login: vi.fn() }));

describe('LoginPage', () => {
  beforeEach(() => vi.clearAllMocks());

  it('submits username and password and calls success callback', async () => {
    const onSuccess = vi.fn();
    vi.mocked(login).mockResolvedValue(undefined);
    render(<LoginPage onSuccess={onSuccess} />);
    fireEvent.change(screen.getByLabelText('نام کاربری'), { target: { value: 'operator' } });
    fireEvent.change(screen.getByLabelText('گذرواژه'), { target: { value: 'secret' } });
    fireEvent.click(screen.getByRole('button', { name: 'ورود امن به سامانه' }));
    await waitFor(() => expect(login).toHaveBeenCalledWith('operator', 'secret'));
    expect(onSuccess).toHaveBeenCalledOnce();
  });

  it('allows the user to reveal and hide the password', () => {
    render(<LoginPage onSuccess={vi.fn()} />);
    const password = screen.getByLabelText('گذرواژه');
    expect(password).toHaveAttribute('type', 'password');
    fireEvent.click(screen.getByRole('button', { name: 'نمایش گذرواژه' }));
    expect(password).toHaveAttribute('type', 'text');
    fireEvent.click(screen.getByRole('button', { name: 'مخفی کردن گذرواژه' }));
    expect(password).toHaveAttribute('type', 'password');
  });

  it('shows a safe error and does not navigate after failed sign-in', async () => {
    const onSuccess = vi.fn();
    vi.mocked(login).mockRejectedValue(new Error('نام کاربری یا گذرواژه صحیح نیست.'));
    render(<LoginPage onSuccess={onSuccess} />);
    fireEvent.change(screen.getByLabelText('نام کاربری'), { target: { value: 'operator' } });
    fireEvent.change(screen.getByLabelText('گذرواژه'), { target: { value: 'wrong' } });
    fireEvent.click(screen.getByRole('button', { name: 'ورود امن به سامانه' }));
    expect(await screen.findByRole('alert')).toHaveTextContent('نام کاربری یا گذرواژه صحیح نیست.');
    expect(onSuccess).not.toHaveBeenCalled();
  });
});
