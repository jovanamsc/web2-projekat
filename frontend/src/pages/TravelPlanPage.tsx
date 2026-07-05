import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import Layout from '../components/Layout';
import Modal from '../components/Modal';
import TravelPlanForm from '../components/TravelPlanForm';
import DestinationsTab from '../components/DestinationsTab';
import ActivitiesTab from '../components/ActivitiesTab';
import ChecklistTab from '../components/ChecklistTab';
import BudgetTab from '../components/BudgetTab';
import ShareTab from '../components/ShareTab';
import MapTab from '../components/MapTab';
import travelService from '../services/travel.service';
import type { TravelPlan, CreateTravelPlan } from '../models/travel.models';
import '../App.css';
import './TravelPlanPage.css';

const TABS = ['Destinacije', 'Aktivnosti', 'Ceklista', 'Budzet', 'Dijeljenje', 'Karta'];

export default function TravelPlanPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const planId = Number(id);

  const [plan, setPlan] = useState<TravelPlan | null>(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState(0);
  const [showEdit, setShowEdit] = useState(false);

  useEffect(() => {
    travelService.getById(planId)
      .then(setPlan)
      .catch(() => navigate('/'))
      .finally(() => setLoading(false));
  }, [planId, navigate]);

  const handleUpdate = async (data: CreateTravelPlan) => {
    const updated = await travelService.update(planId, data);
    setPlan(updated);
    setShowEdit(false);
  };

  const formatDate = (d: string) => new Date(d).toLocaleDateString('bs-BA');

  if (loading) return <Layout><p className="text-muted">Ucitavanje...</p></Layout>;
  if (!plan) return null;

  return (
    <Layout>
      <div className="plan-detail-header">
        <div>
          <button className="btn-back" onClick={() => navigate('/')}>← Nazad</button>
          <h1>{plan.title}</h1>
          <p className="plan-dates">
            {formatDate(plan.startDate)} – {formatDate(plan.endDate)}
            {plan.budget > 0 && <span> · Budzet: {plan.budget.toLocaleString('bs-BA')} €</span>}
          </p>
          {plan.description && <p className="plan-desc">{plan.description}</p>}
        </div>
        <button className="btn-secondary" onClick={() => setShowEdit(true)}>Uredi plan</button>
      </div>

      <div className="tabs">
        {TABS.map((tab, i) => (
          <button key={tab} className={`tab${activeTab === i ? ' active' : ''}`} onClick={() => setActiveTab(i)}>
            {tab}
          </button>
        ))}
      </div>

      {activeTab === 0 && <DestinationsTab planId={planId} />}
      {activeTab === 1 && <ActivitiesTab planId={planId} startDate={plan.startDate} endDate={plan.endDate} />}
      {activeTab === 2 && <ChecklistTab planId={planId} />}
      {activeTab === 3 && <BudgetTab planId={planId} budget={plan.budget} />}
      {activeTab === 4 && <ShareTab planId={planId} />}
      {activeTab === 5 && <MapTab destinations={plan.destinations} activities={plan.activities} />}

      {showEdit && (
        <Modal title="Uredi putovanje" onClose={() => setShowEdit(false)}>
          <TravelPlanForm
            initial={plan}
            onSubmit={handleUpdate}
            onCancel={() => setShowEdit(false)}
            submitLabel="Sacuvaj izmjene"
          />
        </Modal>
      )}
    </Layout>
  );
}
