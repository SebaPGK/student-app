import "./Header.css";

function Header({ user, onAddTask, onLogout }) {
  return (
    <header className="header">
      <div className="logo-section">
        <div className="logo">📝</div>
        <h2>ToDo App</h2>
      </div>

      <div className="welcome-section">
        <h3>Witaj, {user.username}!</h3>
      </div>

      <div className="header-actions">
        <button className="add-task-btn" onClick={onAddTask}>
          + Dodaj zadanie
        </button>
        <button className="logout-btn" onClick={onLogout}>
          Wyloguj
        </button>
      </div>
    </header>
  );
}

export default Header;
