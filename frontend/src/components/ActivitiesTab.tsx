import { useState, useEffect } from 'react';
import Modal from './Modal';
import travelService from '../services/travel.service';
import type { Activity, CreateActivity, ActivityStatus } from '../models/travel.models';
import '../pages/TravelPlanPage.css';
import '../App.css';

const STATUSES: ActivityStatus[] = ['Planned', 'Reserved', 'Completed', 'Cancelled'];
const STATUS_LABELS: Record<ActivityStatus, string> = {
  Planned: 'Planirano', Reserved: 'Rezervisano', Completed: 'Zavrseno', Cancelled: 'Otkazano'
};

interface Props { planId: number; startDate: string; endDate: string; }

export default function ActivitiesTab({ planId, startDate, endDate }: Props) {
  const [activities, setActivities] = useState<Activity[]>([]);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<Activity | null>(null);
  const [view, setView] = useState<'list' | 'calendar'>('list');

  useEffect(() => {
    travelService.getActivities(planId).then(setActivities);
  }, [planId]);

  const grouped = activities.reduce<Record<string, Activity[]>>((acc, a) => {
    const key = a.date.slice(0, 10);
    if (!acc[key]) acc[key] = [];
    acc[key].push(a);
    return acc;
  }, {});

  const handleSubmit = async (data: CreateActivity) => {
    if (editing) {
      const updated = await travelService.updateActivity(planId, editing.id, data);
      setActivities(prev => prev.map(a => a.id === editing.id ? updated : a));
      setEditing(null);
    } else {
      const created = await travelService.createActivity(planId, data);
      setActivities(prev => [...prev, created].sort((a, b) => a.date.localeCompare(b.date)));
      setShowForm(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Obrisati aktivnost?')) return;
    await travelService.deleteActivity(planId, id);
    setActivities(prev => prev.filter(a => a.id !== id));
  };

  const fmt = (d: string) => new Date(d).toLocaleDateString('bs-BA', { weekday: 'long', day: 'numeric', month: 'long' });

  return (
    <div className="tab-section">
      <div className="tab-header">
        <h2>Aktivnosti</h2>
        <div style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
          <div className="view-toggle">
            <button className={view === 'list' ? 'active' : ''} onClick={() => setView('list')}>Lista</button>
            <button className={view === 'calendar' ? 'active' : ''} onClick={() => setView('calendar')}>Kalendar</button>
          </div>
          <button className="btn-primary" style={{ width: 'auto' }} onClick={() => setShowForm(true)}>+ Dodaj</button>
        </div>
      </div>

      {activities.length === 0 && <p className="text-muted">Nema aktivnosti.</p>}

      {view === 'list' && activities.length > 0 && (
        Object.entries(grouped).sort(([a], [b]) => a.localeCompare(b)).map(([date, acts]) => (
          <div key={date} className="day-group">
            <div className="day-group-header">{fmt(date)}</div>
            <div className="item-list">
              {acts.map(a => (
                <div key={a.id} className="item-card">
                  <div className="item-card-body">
                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '4px' }}>
                      <h3>{a.title}</h3>
                      <span className={`status-badge status-${a.status}`}>{STATUS_LABELS[a.status]}</span>
                    </div>
                    <div className="item-meta">
                      {a.time && <span>{a.time.slice(0, 5)}</span>}
                      {a.location && <span>{a.location}</span>}
                      {a.estimatedCost != null && a.estimatedCost > 0 && <span>{a.estimatedCost} €</span>}
                    </div>
                    {a.description && <p style={{ fontSize: '13px', marginTop: '6px' }}>{a.description}</p>}
                  </div>
                  <div className="item-actions">
                    <button className="btn-secondary btn-sm" onClick={() => setEditing(a)}>Uredi</button>
                    <button className="btn-danger btn-sm" onClick={() => handleDelete(a.id)}>Obrisi</button>
                  </div>
                </div>
              ))}
            </div>
          </div>
        ))
      )}

      {view === 'calendar' && (
        <CalendarView activities={activities} startDate={startDate} endDate={endDate} />
      )}

      {(showForm || editing) && (
        <Modal title={editing ? 'Uredi aktivnost' : 'Nova aktivnost'} onClose={() => { setShowForm(false); setEditing(null); }}>
          <ActivityForm
            initial={editing ?? undefined}
            onSubmit={handleSubmit}
            onCancel={() => { setShowForm(false); setEditing(null); }}
          />
        </Modal>
      )}
    </div>
  );
}

function CalendarView({ activities, startDate, endDate }: { activities: Activity[]; startDate: string; endDate: string }) {
  const byDate = activities.reduce<Record<string, Activity[]>>((acc, a) => {
    const key = a.date.slice(0, 10);
    if (!acc[key]) acc[key] = [];
    acc[key].push(a);
    return acc;
  }, {});

  const tripStart = new Date(startDate);
  const tripEnd = new Date(endDate);

  const months: Date[] = [];
  const cur = new Date(tripStart.getFullYear(), tripStart.getMonth(), 1);
  while (cur <= tripEnd) {
    months.push(new Date(cur));
    cur.setMonth(cur.getMonth() + 1);
  }

  return (
    <div className="calendar-view">
      {months.map(month => (
        <CalendarMonth
          key={month.toISOString()}
          month={month}
          byDate={byDate}
          tripStart={tripStart}
          tripEnd={tripEnd}
        />
      ))}
    </div>
  );
}

function CalendarMonth({ month, byDate, tripStart, tripEnd }: {
  month: Date;
  byDate: Record<string, Activity[]>;
  tripStart: Date;
  tripEnd: Date;
}) {
  const year = month.getFullYear();
  const mon = month.getMonth();
  const firstDay = new Date(year, mon, 1);
  const lastDay = new Date(year, mon + 1, 0);

  let startDow = firstDay.getDay();
  if (startDow === 0) startDow = 7;
  startDow -= 1;

  const days: (Date | null)[] = [];
  for (let i = 0; i < startDow; i++) days.push(null);
  for (let d = 1; d <= lastDay.getDate(); d++) days.push(new Date(year, mon, d));
  while (days.length % 7 !== 0) days.push(null);

  const monthName = month.toLocaleDateString('bs-BA', { month: 'long', year: 'numeric' });

  return (
    <div className="cal-month">
      <div className="cal-month-header">{monthName}</div>
      <div className="cal-grid">
        {['Pon', 'Uto', 'Sri', 'Cet', 'Pet', 'Sub', 'Ned'].map(d => (
          <div key={d} className="cal-dow">{d}</div>
        ))}
        {days.map((day, i) => {
          if (!day) return <div key={`e-${i}`} className="cal-day cal-day-empty" />;
          const dateStr = `${year}-${String(mon + 1).padStart(2, '0')}-${String(day.getDate()).padStart(2, '0')}`;
          const inTrip = day >= tripStart && day <= tripEnd;
          const acts = byDate[dateStr] || [];
          return (
            <div key={dateStr} className={`cal-day${inTrip ? ' cal-day-trip' : ''}`}>
              <span className="cal-day-num">{day.getDate()}</span>
              {acts.map(a => (
                <div key={a.id} className={`cal-activity status-${a.status}`} title={`${a.time ? a.time.slice(0, 5) + ' ' : ''}${a.title}`}>
                  {a.time ? a.time.slice(0, 5) + ' ' : ''}{a.title}
                </div>
              ))}
            </div>
          );
        })}
      </div>
    </div>
  );
}

function ActivityForm({ initial, onSubmit, onCancel }: {
  initial?: Activity;
  onSubmit: (d: CreateActivity) => Promise<void>;
  onCancel: () => void;
}) {
  const [title, setTitle] = useState(initial?.title ?? '');
  const [date, setDate] = useState(initial?.date?.slice(0, 10) ?? '');
  const [time, setTime] = useState(initial?.time?.slice(0, 5) ?? '');
  const [location, setLocation] = useState(initial?.location ?? '');
  const [description, setDescription] = useState(initial?.description ?? '');
  const [estimatedCost, setEstimatedCost] = useState(initial?.estimatedCost?.toString() ?? '');
  const [status, setStatus] = useState<ActivityStatus>(initial?.status ?? 'Planned');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) { setError('Naziv je obavezan.'); return; }
    if (!date) { setError('Datum je obavezan.'); return; }
    if (estimatedCost !== '' && Number(estimatedCost) < 0) { setError('Trosak ne moze biti negativan.'); return; }
    setLoading(true);
    setError('');
    try {
      await onSubmit({
        title, date, status,
        time: time ? `${time}:00` : undefined,
        location: location || undefined,
        description: description || undefined,
        estimatedCost: estimatedCost !== '' ? Number(estimatedCost) : undefined,
      });
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      setError(msg || 'Greska.');
    } finally { setLoading(false); }
  };

  return (
    <form onSubmit={handleSubmit} noValidate>
      {error && <div className="alert alert-error">{error}</div>}
      <div className="form-group"><label>Naziv *</label><input value={title} onChange={e => setTitle(e.target.value)} placeholder="npr. Obilazak Eiffelovog tornja" /></div>
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
        <div className="form-group"><label>Datum *</label><input type="date" value={date} onChange={e => setDate(e.target.value)} /></div>
        <div className="form-group"><label>Vrijeme</label><input type="time" value={time} onChange={e => setTime(e.target.value)} /></div>
      </div>
      <div className="form-group"><label>Lokacija</label><input value={location} onChange={e => setLocation(e.target.value)} placeholder="Adresa ili naziv mjesta" /></div>
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
        <div className="form-group">
          <label>Status</label>
          <select value={status} onChange={e => setStatus(e.target.value as ActivityStatus)}>
            {STATUSES.map(s => <option key={s} value={s}>{STATUS_LABELS[s]}</option>)}
          </select>
        </div>
        <div className="form-group"><label>Procijenjeni trosak (€)</label><input type="number" value={estimatedCost} onChange={e => setEstimatedCost(e.target.value)} min="0" step="0.01" /></div>
      </div>
      <div className="form-group"><label>Opis</label><textarea value={description} onChange={e => setDescription(e.target.value)} rows={2} style={{ resize: 'vertical' }} /></div>
      <div style={{ display: 'flex', gap: '8px', justifyContent: 'flex-end' }}>
        <button type="button" className="btn-secondary" onClick={onCancel}>Odustani</button>
        <button type="submit" className="btn-primary" style={{ width: 'auto' }} disabled={loading}>{loading ? 'Cuvanje...' : 'Sacuvaj'}</button>
      </div>
    </form>
  );
}
