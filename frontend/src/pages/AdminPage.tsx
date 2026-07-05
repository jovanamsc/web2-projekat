import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import userService from '../services/user.service';
import travelService from '../services/travel.service';
import type { User } from '../models/user.models';
import { useAuth } from '../context/AuthContext';
import '../App.css';
import './AdminPage.css';

export default function AdminPage() {
  const { user: currentUser } = useAuth();
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [deleting, setDeleting] = useState<number | null>(null);

  useEffect(() => {
    userService.getAll()
      .then(setUsers)
      .catch(() => setError('Greska pri ucitavanju korisnika.'))
      .finally(() => setLoading(false));
  }, []);

  const handleDelete = async (id: number, name: string) => {
    if (!confirm(`Obrisati korisnika "${name}" i sve njegove planove putovanja?`)) return;
    setDeleting(id);
    try {
      await travelService.deleteUserPlans(id);
      await userService.deleteUser(id);
      setUsers(prev => prev.filter(u => u.id !== id));
    } catch {
      alert('Greska pri brisanju korisnika.');
    } finally {
      setDeleting(null);
    }
  };

  const fmt = (d?: string) => d ? new Date(d).toLocaleDateString('bs-BA') : '—';

  return (
    <Layout>
      <div className="admin-page">
        <div className="admin-header">
          <h1>Upravljanje korisnicima</h1>
          <span className="user-count">{users.length} korisnika</span>
        </div>

        {error && <div className="alert alert-error">{error}</div>}

        {loading ? (
          <p className="text-muted">Ucitavanje...</p>
        ) : (
          <div className="users-table-wrapper">
            <table className="users-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Ime</th>
                  <th>Email</th>
                  <th>Uloga</th>
                  <th>Registrovan</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {users.map(u => (
                  <tr key={u.id} className={u.id === currentUser?.id ? 'current-user-row' : ''}>
                    <td className="user-id">#{u.id}</td>
                    <td>
                      <span className="user-name">{u.name}</span>
                      {u.id === currentUser?.id && <span className="you-badge">Vi</span>}
                    </td>
                    <td className="user-email">{u.email}</td>
                    <td>
                      <span className={`role-badge role-${u.role?.toLowerCase() ?? 'user'}`}>
                        {u.role === 'Admin' ? 'Admin' : 'Korisnik'}
                      </span>
                    </td>
                    <td className="user-date">{fmt(u.createdAt)}</td>
                    <td className="user-actions">
                      {u.id !== currentUser?.id && (
                        <button
                          className="btn-danger btn-sm"
                          onClick={() => handleDelete(u.id, u.name)}
                          disabled={deleting === u.id}
                        >
                          {deleting === u.id ? '...' : 'Obrisi'}
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {users.length === 0 && (
              <p className="text-muted" style={{ textAlign: 'center', padding: '32px' }}>Nema korisnika.</p>
            )}
          </div>
        )}
      </div>
    </Layout>
  );
}
