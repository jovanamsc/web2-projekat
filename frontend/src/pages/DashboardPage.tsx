import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import Layout from '../components/Layout';
import Modal from '../components/Modal';
import TravelPlanForm from '../components/TravelPlanForm';
import travelService from '../services/travel.service';
import type { TravelPlan, CreateTravelPlan } from '../models/travel.models';
import '../App.css';
import './DashboardPage.css';

export default function DashboardPage() {
  const [plans, setPlans] = useState<TravelPlan[]>([]);
  const [loading, setLoading] = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    travelService.getAll()
      .then(setPlans)
      .finally(() => setLoading(false));
  }, []);

  const handleCreate = async (data: CreateTravelPlan) => {
    const plan = await travelService.create(data);
    setPlans(prev => [plan, ...prev]);
    setShowCreate(false);
  };

  const handleDelete = async (e: React.MouseEvent, id: number) => {
    e.stopPropagation();
    if (!confirm('Obrisati plan putovanja?')) return;
    await travelService.remove(id);
    setPlans(prev => prev.filter(p => p.id !== id));
  };

  const formatDate = (d: string) => new Date(d).toLocaleDateString('bs-BA');

  const getDuration = (start: string, end: string) => {
    const days = Math.ceil((new Date(end).getTime() - new Date(start).getTime()) / 86400000);
    return `${days} ${days === 1 ? 'dan' : 'dana'}`;
  };

  return (
    <Layout>
      <div className="dashboard-hero">
        <div className="dashboard-hero-bg" />
        <div className="dashboard-hero-overlay" />
        <div className="dashboard-hero-content">
          <div>
            <h1>Moja putovanja</h1>
            <p>Planiranje koje prati vas na svakom koraku</p>
          </div>
          <button className="btn-hero" onClick={() => setShowCreate(true)}>+ Novo putovanje</button>
        </div>
      </div>

      {loading && <p className="text-muted">Ucitavanje...</p>}

      {!loading && plans.length === 0 && (
        <div className="empty-state">
          <div className="empty-state-icon">✈</div>
          <p>Nema planiranih putovanja</p>
          <span>Kreirajte prvo putovanje i pocnite planirati.</span>
          <button className="btn-primary" style={{ width: 'auto' }} onClick={() => setShowCreate(true)}>
            + Novo putovanje
          </button>
        </div>
      )}

      <div className="card-grid">
        {plans.map(plan => (
          <div key={plan.id} className="plan-card" onClick={() => navigate(`/plans/${plan.id}`)}>
            <div className="plan-card-header">
              <h3>{plan.title}</h3>
              <button className="btn-danger btn-sm" onClick={e => handleDelete(e, plan.id)}>Obrisi</button>
            </div>
            {plan.description && <p className="plan-description">{plan.description}</p>}
            <div className="plan-meta">
              <span className="plan-meta-chip">📅 {formatDate(plan.startDate)} – {formatDate(plan.endDate)}</span>
              <span className="plan-meta-chip">⏱ {getDuration(plan.startDate, plan.endDate)}</span>
              {plan.budget > 0 && <span className="plan-meta-chip">💰 {plan.budget.toLocaleString('bs-BA')} €</span>}
              {plan.destinations?.length > 0 && <span className="plan-meta-chip">📍 {plan.destinations.length} {plan.destinations.length === 1 ? 'destinacija' : 'destinacije'}</span>}
            </div>
          </div>
        ))}
      </div>

      {showCreate && (
        <Modal title="Novo putovanje" onClose={() => setShowCreate(false)}>
          <TravelPlanForm
            onSubmit={handleCreate}
            onCancel={() => setShowCreate(false)}
            submitLabel="Kreiraj putovanje"
          />
        </Modal>
      )}
    </Layout>
  );
}
