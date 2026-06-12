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
    if (mode === 'register' && !form.name.trim()) e.name = 'Imię i nazwisko jest wymagane'
    if (!form.email.trim()) e.email = 'Email jest wymagany'
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) e.email = 'Podaj prawidłowy adres email'
    if (!form.password) e.password = 'Hasło jest wymagane'
    else if (form.password.length < 8) e.password = 'Co najmniej 8 znaków'
    if (mode === 'register' && form.password !== form.confirmPassword)
      e.confirmPassword = 'Hasła nie są zgodne'
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
        ? 'http://localhost:7071/api/auth/login'
        : 'http://localhost:7071/api/auth/register'

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
        setApiError(typeof data === 'string' ? data : (data.message ?? 'Coś poszło nie tak.'))
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
      setApiError('Nie można połączyć się z serwerem. Czy backend jest uruchomiony?')
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
              Zaloguj się
            </button>
            <button
              role="tab"
              aria-selected={mode === 'register'}
              className={mode === 'register' ? 'active' : ''}
              onClick={() => switchMode('register')}
              type="button"
            >
              Zarejestruj się
            </button>
          </div>
        </div>

        <form id="auth-form" onSubmit={handleSubmit} noValidate>
          {mode === 'register' && (
            <Field
              id="name"
              label="Imię i nazwisko"
              type="text"
              value={form.name}
              onChange={update('name')}
              placeholder="Jan Kowalski"
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
            placeholder="ty@uczelnia.edu"
            error={errors.email}
            autoComplete="email"
          />

          <Field
            id="password"
            label="Hasło"
            type="password"
            value={form.password}
            onChange={update('password')}
            placeholder={mode === 'register' ? 'Co najmniej 8 znaków' : '••••••••'}
            error={errors.password}
            autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
          />

          {mode === 'register' && (
            <Field
              id="confirmPassword"
              label="Potwierdź hasło"
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
              <button type="button" className="link-btn">Zapomniałeś hasła?</button>
            </div>
          )}

          {apiError && <p className="api-error" role="alert">{apiError}</p>}

          <button id="submit-btn" type="submit" disabled={loading}>
            {loading ? 'Proszę czekać…' : (mode === 'login' ? 'Zaloguj się' : 'Utwórz konto')}
          </button>
        </form>

        <p id="auth-switch">
          {mode === 'login' ? 'Nie masz konta?' : 'Masz już konto?'}
          {' '}
          <button
            type="button"
            className="link-btn"
            onClick={() => switchMode(mode === 'login' ? 'register' : 'login')}
          >
            {mode === 'login' ? 'Zarejestruj się' : 'Zaloguj się'}
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
