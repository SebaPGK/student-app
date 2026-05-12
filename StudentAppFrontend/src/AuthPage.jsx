import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import './AuthPage.css'

function AuthPage() {
  const [mode, setMode] = useState('login')
  const [form, setForm] = useState({ name: '', email: '', password: '', confirmPassword: '' })
  const [errors, setErrors] = useState({})
  const [loading, setLoading] = useState(false)
  const [apiError, setApiError] = useState('')
  const navigate = useNavigate()

  const update = (field) => (e) => {
    setForm((f) => ({ ...f, [field]: e.target.value }))
    setErrors((err) => ({ ...err, [field]: undefined }))
  }

  const validate = () => {
    const e = {}
    if (mode === 'register' && !form.name.trim()) e.name = 'Name is required'
    if (!form.email.trim()) e.email = 'Email is required'
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) e.email = 'Enter a valid email'
    if (!form.password) e.password = 'Password is required'
    else if (form.password.length < 8) e.password = 'At least 8 characters'
    if (mode === 'register' && form.password !== form.confirmPassword)
      e.confirmPassword = 'Passwords do not match'
    return e
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    const errs = validate()
    if (Object.keys(errs).length) { setErrors(errs); return }

    setLoading(true)
    setApiError('')

    try {
      const url = mode === 'login'
        ? 'http://localhost:5174/api/auth/login'
        : 'http://localhost:5174/api/auth/register'

      const body = mode === 'login'
        ? { email: form.email, password: form.password }
        : { username: form.name, email: form.email, password: form.password }

      const res = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body),
      })

      const text = await res.text()
      let data
      try { data = text ? JSON.parse(text) : {} } catch { data = text }

      if (!res.ok) {
        setApiError(typeof data === 'string' ? data : (data.message ?? 'Something went wrong.'))
        return
      }

      if (mode === 'login') {
        localStorage.setItem('token', data.token)
        localStorage.setItem('user', JSON.stringify(data.user))
        navigate('/dashboard')
      } else {
        setMode('login')
        setForm({ name: '', email: '', password: '', confirmPassword: '' })
      }
    } catch {
      setApiError('Could not reach the server. Is the backend running?')
    } finally {
      setLoading(false)
    }
  }

  const switchMode = (next) => {
    setMode(next)
    setErrors({})
  }

  return (
    <div id="auth-wrapper">
      <div id="auth-card">
        <div id="auth-header">
          <h1 id="auth-title">Student App</h1>
          <div id="auth-tabs" role="tablist">
            <button
              role="tab"
              aria-selected={mode === 'login'}
              className={mode === 'login' ? 'active' : ''}
              onClick={() => switchMode('login')}
              type="button"
            >
              Log in
            </button>
            <button
              role="tab"
              aria-selected={mode === 'register'}
              className={mode === 'register' ? 'active' : ''}
              onClick={() => switchMode('register')}
              type="button"
            >
              Register
            </button>
          </div>
        </div>

        <form id="auth-form" onSubmit={handleSubmit} noValidate>
          {mode === 'register' && (
            <Field
              id="name"
              label="Full name"
              type="text"
              value={form.name}
              onChange={update('name')}
              placeholder="Jane Smith"
              error={errors.name}
              autoComplete="name"
            />
          )}

          <Field
            id="email"
            label="Email"
            type="email"
            value={form.email}
            onChange={update('email')}
            placeholder="you@university.edu"
            error={errors.email}
            autoComplete="email"
          />

          <Field
            id="password"
            label="Password"
            type="password"
            value={form.password}
            onChange={update('password')}
            placeholder={mode === 'register' ? 'At least 8 characters' : '••••••••'}
            error={errors.password}
            autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
          />

          {mode === 'register' && (
            <Field
              id="confirmPassword"
              label="Confirm password"
              type="password"
              value={form.confirmPassword}
              onChange={update('confirmPassword')}
              placeholder="••••••••"
              error={errors.confirmPassword}
              autoComplete="new-password"
            />
          )}

          {mode === 'login' && (
            <div id="forgot">
              <button type="button" className="link-btn">Forgot password?</button>
            </div>
          )}

          {apiError && <p className="api-error" role="alert">{apiError}</p>}

          <button id="submit-btn" type="submit" disabled={loading}>
            {loading ? 'Please wait…' : (mode === 'login' ? 'Log in' : 'Create account')}
          </button>
        </form>

        <p id="auth-switch">
          {mode === 'login' ? "Don't have an account?" : 'Already have an account?'}
          {' '}
          <button
            type="button"
            className="link-btn"
            onClick={() => switchMode(mode === 'login' ? 'register' : 'login')}
          >
            {mode === 'login' ? 'Register' : 'Log in'}
          </button>
        </p>
      </div>
    </div>
  )
}

function Field({ id, label, type, value, onChange, placeholder, error, autoComplete }) {
  return (
    <div className={`field${error ? ' field--error' : ''}`}>
      <label htmlFor={id}>{label}</label>
      <input
        id={id}
        type={type}
        value={value}
        onChange={onChange}
        placeholder={placeholder}
        autoComplete={autoComplete}
        aria-invalid={!!error}
        aria-describedby={error ? `${id}-err` : undefined}
      />
      {error && <span className="field-error" id={`${id}-err`} role="alert">{error}</span>}
    </div>
  )
}

export default AuthPage
