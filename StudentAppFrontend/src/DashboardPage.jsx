function DashboardPage() {
  const user = JSON.parse(localStorage.getItem('user') ?? '{}')

  const handleLogout = () => {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    window.location.href = '/login'
  }

  return (
    <div id="dashboard-wrapper">
      <header id="dashboard-header">
        <h1>Student App</h1>
        <div id="dashboard-user">
          <span>{user.username ?? user.email}</span>
          <button type="button" onClick={handleLogout}>Log out</button>
        </div>
      </header>
      <main id="dashboard-main">
        <p>Welcome, {user.username ?? 'student'}!</p>
      </main>
    </div>
  )
}

export default DashboardPage
