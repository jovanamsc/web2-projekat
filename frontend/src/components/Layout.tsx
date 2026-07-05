import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import type { ReactNode } from 'react';
import '../App.css';

export default function Layout({ children }: { children: ReactNode }) {
  const { user, logout, isAdmin } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="layout">
      <nav className="navbar">
        <Link to="/" className="navbar-brand">TravelPlanner</Link>
        <div className="navbar-actions">
          {isAdmin && <Link to="/admin" className="nav-link">Admin</Link>}
          <span className="nav-user">{user?.name}</span>
          <button className="btn-secondary" onClick={handleLogout}>Odjava</button>
        </div>
      </nav>
      <main className="main-content">
        {children}
      </main>
    </div>
  );
}
