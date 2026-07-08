import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { jsPDF } from 'jspdf';
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

  const formatDate = (d: string) => new Date(d).toLocaleDateString('sr-RS');

  const exportPdf = () => {
    if (!plan) return;
    const doc = new jsPDF();
    const fmt = (d: string) => new Date(d).toLocaleDateString('sr-RS');
    const STATUS: Record<string, string> = { Planned: 'Planirano', Reserved: 'Rezervisano', Completed: 'Zavrseno', Cancelled: 'Otkazano' };
    let y = 20;

    const line = (text: string, size = 11, bold = false) => {
      if (y > 270) { doc.addPage(); y = 20; }
      doc.setFontSize(size);
      doc.setFont('helvetica', bold ? 'bold' : 'normal');
      doc.text(text, 15, y);
      y += size * 0.5 + 3;
    };

    const sep = () => { if (y > 270) { doc.addPage(); y = 20; } doc.setDrawColor(200); doc.line(15, y, 195, y); y += 5; };

    line(plan.title, 20, true);
    line(`${fmt(plan.startDate)} - ${fmt(plan.endDate)}`, 11);
    if (plan.budget > 0) line(`Budzet: ${plan.budget.toLocaleString('sr-RS')} EUR`, 11);
    if (plan.description) line(plan.description, 10);
    y += 4;

    if (plan.destinations.length > 0) {
      sep();
      line('DESTINACIJE', 13, true);
      y += 2;
      plan.destinations.forEach(d => {
        line(`${d.name} - ${d.location}`, 11, true);
        line(`  ${fmt(d.arrivalDate)} - ${fmt(d.departureDate)}`, 10);
        if (d.description) line(`  ${d.description}`, 10);
        y += 2;
      });
    }

    if (plan.activities.length > 0) {
      sep();
      line('AKTIVNOSTI', 13, true);
      y += 2;
      const grouped = plan.activities
        .slice().sort((a, b) => a.date.localeCompare(b.date))
        .reduce<Record<string, typeof plan.activities>>((acc, a) => {
          const k = a.date.slice(0, 10);
          if (!acc[k]) acc[k] = [];
          acc[k].push(a);
          return acc;
        }, {});
      Object.entries(grouped).forEach(([date, acts]) => {
        line(fmt(date), 11, true);
        acts.forEach(a => {
          const time = a.time ? a.time.slice(0, 5) + ' ' : '';
          line(`  ${time}${a.title} [${STATUS[a.status]}]`, 10);
          if (a.location) line(`  Lokacija: ${a.location}`, 10);
          if (a.estimatedCost) line(`  Procijenjeno: ${a.estimatedCost} EUR`, 10);
        });
        y += 2;
      });
    }

    if (plan.checklistItems.length > 0) {
      sep();
      line('CEKLISTA', 13, true);
      y += 2;
      plan.checklistItems.forEach(item => {
        line(`  ${item.isCompleted ? '[x]' : '[ ]'} ${item.title}`, 10);
      });
    }

    doc.save(`${plan.title.replace(/\s+/g, '_')}.pdf`);
  };

  if (loading) return <Layout><p className="text-muted">Ucitavanje...</p></Layout>;
  if (!plan) return null;

  return (
    <Layout>
      <div className="plan-detail-header">
        <div className="plan-header-bg" />
        <div className="plan-header-overlay" />
        <div className="plan-header-content">
          <div>
            <button className="btn-back" onClick={() => navigate('/')}>← Nazad</button>
            <h1>{plan.title}</h1>
            <div className="plan-dates">
              <span className="plan-dates-chip">📅 {formatDate(plan.startDate)} – {formatDate(plan.endDate)}</span>
              {plan.budget > 0 && <span className="plan-dates-chip">💰 {plan.budget.toLocaleString('sr-RS')} €</span>}
            </div>
            {plan.description && <p className="plan-desc">{plan.description}</p>}
          </div>
          <div style={{ display: 'flex', gap: '8px' }}>
            <button className="btn-hero" onClick={exportPdf}>Izvezi PDF</button>
            <button className="btn-hero" onClick={() => setShowEdit(true)}>Uredi plan</button>
          </div>
        </div>
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
      {activeTab === 5 && <MapTab planId={planId} />}

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
