import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { QRCodeSVG } from 'qrcode.react';
import travelService from '../services/travel.service';
import type { TravelPlan, Activity, ActivityStatus, CreateDestination, CreateActivity, CreateChecklistItem } from '../models/travel.models';
import type { ShareLink } from '../models/share.models';
import Modal from '../components/Modal';
import MapTab from '../components/MapTab';
import '../App.css';
import './SharedPlanPage.css';

const STATUS_LABELS: Record<ActivityStatus, string> = {
  Planned: 'Planirano', Reserved: 'Rezervisano', Completed: 'Zavrseno', Cancelled: 'Otkazano'
};

export default function SharedPlanPage() {
  const { token } = useParams<{ token: string }>();
  const [plan, setPlan] = useState<TravelPlan | null>(null);
  const [tokenInfo, setTokenInfo] = useState<ShareLink | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [activeTab, setActiveTab] = useState(0);
  const [showQR, setShowQR] = useState(false);

  useEffect(() => {
    if (!token) return;
    Promise.all([
      travelService.getSharedPlan(token),
      travelService.getShareLinkInfo(token),
    ])
      .then(([p, info]) => { setPlan(p); setTokenInfo(info); })
      .catch(() => setError('Link je neispravan ili je istekao.'))
      .finally(() => setLoading(false));
  }, [token]);

  const isEdit = tokenInfo?.accessLevel === 'EDIT';
  const fmt = (d: string) => new Date(d).toLocaleDateString('bs-BA');

  if (loading) return <div className="shared-loading">Ucitavanje...</div>;
  if (error || !plan || !token) return <div className="shared-error">{error || 'Greska.'}</div>;

  const grouped = plan.activities.reduce<Record<string, Activity[]>>((acc, a) => {
    const key = a.date.slice(0, 10);
    if (!acc[key]) acc[key] = [];
    acc[key].push(a);
    return acc;
  }, {});

  return (
    <div className="shared-page">
      <div className="shared-header">
        <div className="shared-header-top">
          <div>
            <span className={`access-badge access-${tokenInfo?.accessLevel}`}>{tokenInfo?.accessLevel}</span>
            <h1>{plan.title}</h1>
            <p className="plan-dates">{fmt(plan.startDate)} – {fmt(plan.endDate)}</p>
            {plan.description && <p className="plan-desc" style={{ marginTop: '6px' }}>{plan.description}</p>}
          </div>
          <button className="btn-secondary" onClick={() => setShowQR(true)}>QR kod</button>
        </div>
      </div>

      <div className="shared-content">
        <div className="tabs">
          {['Destinacije', 'Aktivnosti', 'Ceklista', 'Karta'].map((tab, i) => (
            <button key={tab} className={`tab${activeTab === i ? ' active' : ''}`} onClick={() => setActiveTab(i)}>
              {tab}
            </button>
          ))}
        </div>

        {activeTab === 0 && (
          <SharedDestinationsTab plan={plan} setPlan={setPlan} token={token} isEdit={isEdit} />
        )}
        {activeTab === 1 && (
          <SharedActivitiesTab grouped={grouped} plan={plan} setPlan={setPlan} token={token} isEdit={isEdit} />
        )}
        {activeTab === 2 && (
          <SharedChecklistTab plan={plan} setPlan={setPlan} token={token} isEdit={isEdit} />
        )}
        {activeTab === 3 && <MapTab destinations={plan.destinations} />}
      </div>

      {showQR && (
        <Modal title="QR kod za dijeljenje" onClose={() => setShowQR(false)}>
          <div style={{ textAlign: 'center', padding: '16px' }}>
            <QRCodeSVG value={window.location.href} size={220} />
            <p style={{ marginTop: '16px', fontSize: '13px', color: 'var(--text-muted)', wordBreak: 'break-all' }}>
              {window.location.href}
            </p>
            <button className="btn-secondary" style={{ marginTop: '12px' }}
              onClick={() => navigator.clipboard.writeText(window.location.href)}>
              Kopiraj link
            </button>
          </div>
        </Modal>
      )}
    </div>
  );
}

function SharedDestinationsTab({ plan, setPlan, token, isEdit }: {
  plan: TravelPlan; setPlan: (p: TravelPlan) => void; token: string; isEdit: boolean;
}) {
  const [showForm, setShowForm] = useState(false);
  const fmt = (d: string) => new Date(d).toLocaleDateString('bs-BA');

  const handleAdd = async (data: CreateDestination) => {
    const dest = await travelService.createSharedDestination(token, data);
    setPlan({ ...plan, destinations: [...plan.destinations, dest] });
    setShowForm(false);
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Obrisati destinaciju?')) return;
    await travelService.deleteSharedDestination(token, id);
    setPlan({ ...plan, destinations: plan.destinations.filter(d => d.id !== id) });
  };

  return (
    <div className="tab-section">
      {isEdit && (
        <div className="tab-header">
          <span />
          <button className="btn-primary" style={{ width: 'auto' }} onClick={() => setShowForm(true)}>+ Dodaj destinaciju</button>
        </div>
      )}
      {plan.destinations.length === 0 && <p className="text-muted">Nema destinacija.</p>}
      <div className="item-list">
        {plan.destinations.map(d => (
          <div key={d.id} className="item-card">
            <div className="item-card-body">
              <h3>{d.name}</h3>
              <div className="item-meta">
                <span>{d.location}</span>
                <span>{fmt(d.arrivalDate)} – {fmt(d.departureDate)}</span>
              </div>
              {d.description && <p style={{ fontSize: '13px', marginTop: '6px' }}>{d.description}</p>}
            </div>
            {isEdit && (
              <button className="btn-danger btn-sm" onClick={() => handleDelete(d.id)}>Obrisi</button>
            )}
          </div>
        ))}
      </div>
      {showForm && (
        <Modal title="Nova destinacija" onClose={() => setShowForm(false)}>
          <SimpleDestinationForm onSubmit={handleAdd} onCancel={() => setShowForm(false)} />
        </Modal>
      )}
    </div>
  );
}

function SharedActivitiesTab({ grouped, plan, setPlan, token, isEdit }: {
  grouped: Record<string, Activity[]>; plan: TravelPlan;
  setPlan: (p: TravelPlan) => void; token: string; isEdit: boolean;
}) {
  const [showForm, setShowForm] = useState(false);
  const fmt = (d: string) => new Date(d).toLocaleDateString('bs-BA', { weekday: 'long', day: 'numeric', month: 'long' });

  const handleAdd = async (data: CreateActivity) => {
    const activity = await travelService.createSharedActivity(token, data);
    setPlan({ ...plan, activities: [...plan.activities, activity] });
    setShowForm(false);
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Obrisati aktivnost?')) return;
    await travelService.deleteSharedActivity(token, id);
    setPlan({ ...plan, activities: plan.activities.filter(a => a.id !== id) });
  };

  return (
    <div className="tab-section">
      {isEdit && (
        <div className="tab-header">
          <span />
          <button className="btn-primary" style={{ width: 'auto' }} onClick={() => setShowForm(true)}>+ Dodaj aktivnost</button>
        </div>
      )}
      {plan.activities.length === 0 && <p className="text-muted">Nema aktivnosti.</p>}
      {Object.entries(grouped).sort(([a], [b]) => a.localeCompare(b)).map(([date, acts]) => (
        <div key={date} className="day-group">
          <div className="day-group-header">{fmt(date)}</div>
          <div className="item-list">
            {acts.map(a => (
              <div key={a.id} className="item-card">
                <div className="item-card-body">
                  <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <h3>{a.title}</h3>
                    <span className={`status-badge status-${a.status}`}>{STATUS_LABELS[a.status]}</span>
                  </div>
                  <div className="item-meta">
                    {a.time && <span>{a.time.slice(0, 5)}</span>}
                    {a.location && <span>{a.location}</span>}
                  </div>
                </div>
                {isEdit && <button className="btn-danger btn-sm" onClick={() => handleDelete(a.id)}>Obrisi</button>}
              </div>
            ))}
          </div>
        </div>
      ))}
      {showForm && (
        <Modal title="Nova aktivnost" onClose={() => setShowForm(false)}>
          <SimpleActivityForm onSubmit={handleAdd} onCancel={() => setShowForm(false)} />
        </Modal>
      )}
    </div>
  );
}

function SharedChecklistTab({ plan, setPlan, token, isEdit }: {
  plan: TravelPlan; setPlan: (p: TravelPlan) => void; token: string; isEdit: boolean;
}) {
  const [newTitle, setNewTitle] = useState('');

  const handleToggle = async (id: number, isCompleted: boolean) => {
    if (!isEdit) return;
    const updated = await travelService.updateSharedChecklistItem(token, id, { isCompleted: !isCompleted });
    setPlan({ ...plan, checklistItems: plan.checklistItems.map(i => i.id === id ? updated : i) });
  };

  const handleAdd = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newTitle.trim()) return;
    const item = await travelService.createSharedChecklistItem(token, { title: newTitle.trim() });
    setPlan({ ...plan, checklistItems: [...plan.checklistItems, item] });
    setNewTitle('');
  };

  const handleDelete = async (id: number) => {
    await travelService.deleteSharedChecklistItem(token, id);
    setPlan({ ...plan, checklistItems: plan.checklistItems.filter(i => i.id !== id) });
  };

  const completed = plan.checklistItems.filter(i => i.isCompleted).length;

  return (
    <div className="tab-section">
      <div className="tab-header">
        <span className="text-muted" style={{ fontSize: '14px' }}>
          {plan.checklistItems.length > 0 && `${completed}/${plan.checklistItems.length} zavrseno`}
        </span>
      </div>
      {isEdit && (
        <form onSubmit={handleAdd} style={{ display: 'flex', gap: '8px', marginBottom: '16px' }}>
          <input value={newTitle} onChange={e => setNewTitle(e.target.value)} placeholder="Nova stavka..." />
          <button type="submit" className="btn-primary" style={{ width: 'auto', whiteSpace: 'nowrap' }}>+ Dodaj</button>
        </form>
      )}
      {plan.checklistItems.length === 0 && <p className="text-muted">Ceklista je prazna.</p>}
      <div className="item-list">
        {plan.checklistItems.map(item => (
          <div key={item.id} className={`checklist-item${item.isCompleted ? ' completed' : ''}`}>
            <input type="checkbox" checked={item.isCompleted} onChange={() => handleToggle(item.id, item.isCompleted)} disabled={!isEdit} />
            <span className="checklist-title">{item.title}</span>
            {isEdit && <button className="btn-danger btn-sm" onClick={() => handleDelete(item.id)}>✕</button>}
          </div>
        ))}
      </div>
    </div>
  );
}

function SimpleDestinationForm({ onSubmit, onCancel }: { onSubmit: (d: CreateDestination) => Promise<void>; onCancel: () => void }) {
  const [name, setName] = useState('');
  const [location, setLocation] = useState('');
  const [arrivalDate, setArrivalDate] = useState('');
  const [departureDate, setDepartureDate] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim() || !location.trim() || !arrivalDate || !departureDate) { setError('Sva obavezna polja moraju biti popunjena.'); return; }
    if (new Date(departureDate) < new Date(arrivalDate)) { setError('Datum odlaska ne moze biti prije dolaska.'); return; }
    setLoading(true);
    try { await onSubmit({ name, location, arrivalDate, departureDate }); }
    catch { setError('Greska.'); }
    finally { setLoading(false); }
  };

  return (
    <form onSubmit={handleSubmit} noValidate>
      {error && <div className="alert alert-error">{error}</div>}
      <div className="form-group"><label>Naziv *</label><input value={name} onChange={e => setName(e.target.value)} /></div>
      <div className="form-group"><label>Lokacija *</label><input value={location} onChange={e => setLocation(e.target.value)} /></div>
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
        <div className="form-group"><label>Dolazak *</label><input type="date" value={arrivalDate} onChange={e => setArrivalDate(e.target.value)} /></div>
        <div className="form-group"><label>Odlazak *</label><input type="date" value={departureDate} onChange={e => setDepartureDate(e.target.value)} min={arrivalDate} /></div>
      </div>
      <div style={{ display: 'flex', gap: '8px', justifyContent: 'flex-end' }}>
        <button type="button" className="btn-secondary" onClick={onCancel}>Odustani</button>
        <button type="submit" className="btn-primary" style={{ width: 'auto' }} disabled={loading}>{loading ? 'Cuvanje...' : 'Sacuvaj'}</button>
      </div>
    </form>
  );
}

function SimpleActivityForm({ onSubmit, onCancel }: { onSubmit: (d: CreateActivity) => Promise<void>; onCancel: () => void }) {
  const [title, setTitle] = useState('');
  const [date, setDate] = useState('');
  const [time, setTime] = useState('');
  const [location, setLocation] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim() || !date) { setError('Naziv i datum su obavezni.'); return; }
    setLoading(true);
    try { await onSubmit({ title, date, status: 'Planned', time: time ? `${time}:00` : undefined, location: location || undefined }); }
    catch { setError('Greska.'); }
    finally { setLoading(false); }
  };

  return (
    <form onSubmit={handleSubmit} noValidate>
      {error && <div className="alert alert-error">{error}</div>}
      <div className="form-group"><label>Naziv *</label><input value={title} onChange={e => setTitle(e.target.value)} /></div>
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
        <div className="form-group"><label>Datum *</label><input type="date" value={date} onChange={e => setDate(e.target.value)} /></div>
        <div className="form-group"><label>Vrijeme</label><input type="time" value={time} onChange={e => setTime(e.target.value)} /></div>
      </div>
      <div className="form-group"><label>Lokacija</label><input value={location} onChange={e => setLocation(e.target.value)} /></div>
      <div style={{ display: 'flex', gap: '8px', justifyContent: 'flex-end' }}>
        <button type="button" className="btn-secondary" onClick={onCancel}>Odustani</button>
        <button type="submit" className="btn-primary" style={{ width: 'auto' }} disabled={loading}>{loading ? 'Cuvanje...' : 'Sacuvaj'}</button>
      </div>
    </form>
  );
}
