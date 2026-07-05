import { useState, useEffect } from 'react';
import Modal from './Modal';
import travelService from '../services/travel.service';
import type { Destination, CreateDestination } from '../models/travel.models';
import '../pages/TravelPlanPage.css';
import '../App.css';

interface Props { planId: number; }

export default function DestinationsTab({ planId }: Props) {
  const [destinations, setDestinations] = useState<Destination[]>([]);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<Destination | null>(null);
  const [error, setError] = useState('');

  useEffect(() => {
    travelService.getDestinations(planId).then(setDestinations);
  }, [planId]);

  const handleSubmit = async (data: CreateDestination) => {
    if (editing) {
      const updated = await travelService.updateDestination(planId, editing.id, data);
      setDestinations(prev => prev.map(d => d.id === editing.id ? updated : d));
      setEditing(null);
    } else {
      const created = await travelService.createDestination(planId, data);
      setDestinations(prev => [...prev, created]);
      setShowForm(false);
    }
    setError('');
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Obrisati destinaciju?')) return;
    await travelService.deleteDestination(planId, id);
    setDestinations(prev => prev.filter(d => d.id !== id));
  };

  const fmt = (d: string) => new Date(d).toLocaleDateString('bs-BA');

  return (
    <div className="tab-section">
      <div className="tab-header">
        <h2>Destinacije</h2>
        <button className="btn-primary" style={{ width: 'auto' }} onClick={() => setShowForm(true)}>+ Dodaj destinaciju</button>
      </div>

      {destinations.length === 0 && <p className="text-muted">Nema destinacija.</p>}

      <div className="item-list">
        {destinations.map(d => (
          <div key={d.id} className="item-card">
            <div className="item-card-body">
              <h3>{d.name}</h3>
              <div className="item-meta">
                <span>{d.location}</span>
                <span>{fmt(d.arrivalDate)} – {fmt(d.departureDate)}</span>
              </div>
              {d.description && <p style={{ fontSize: '13px', marginTop: '6px' }}>{d.description}</p>}
              {d.notes && <p style={{ fontSize: '13px', color: 'var(--text-muted)', marginTop: '4px' }}>{d.notes}</p>}
            </div>
            <div className="item-actions">
              <button className="btn-secondary btn-sm" onClick={() => setEditing(d)}>Uredi</button>
              <button className="btn-danger btn-sm" onClick={() => handleDelete(d.id)}>Obrisi</button>
            </div>
          </div>
        ))}
      </div>

      {(showForm || editing) && (
        <Modal title={editing ? 'Uredi destinaciju' : 'Nova destinacija'} onClose={() => { setShowForm(false); setEditing(null); setError(''); }}>
          <DestinationForm
            initial={editing ?? undefined}
            error={error}
            onSubmit={handleSubmit}
            onCancel={() => { setShowForm(false); setEditing(null); setError(''); }}
          />
        </Modal>
      )}
    </div>
  );
}

function DestinationForm({ initial, error, onSubmit, onCancel }: {
  initial?: Destination;
  error: string;
  onSubmit: (d: CreateDestination) => Promise<void>;
  onCancel: () => void;
}) {
  const [name, setName] = useState(initial?.name ?? '');
  const [location, setLocation] = useState(initial?.location ?? '');
  const [arrivalDate, setArrivalDate] = useState(initial?.arrivalDate?.slice(0, 10) ?? '');
  const [departureDate, setDepartureDate] = useState(initial?.departureDate?.slice(0, 10) ?? '');
  const [description, setDescription] = useState(initial?.description ?? '');
  const [notes, setNotes] = useState(initial?.notes ?? '');
  const [localError, setLocalError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) { setLocalError('Naziv je obavezan.'); return; }
    if (!location.trim()) { setLocalError('Lokacija je obavezna.'); return; }
    if (!arrivalDate) { setLocalError('Datum dolaska je obavezan.'); return; }
    if (!departureDate) { setLocalError('Datum odlaska je obavezan.'); return; }
    if (new Date(departureDate) < new Date(arrivalDate)) { setLocalError('Datum odlaska ne moze biti prije datuma dolaska.'); return; }
    setLoading(true);
    setLocalError('');
    try {
      await onSubmit({ name, location, arrivalDate, departureDate, description: description || undefined, notes: notes || undefined });
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      setLocalError(msg || 'Greska.');
    } finally { setLoading(false); }
  };

  return (
    <form onSubmit={handleSubmit} noValidate>
      {(localError || error) && <div className="alert alert-error">{localError || error}</div>}
      <div className="form-group"><label>Naziv *</label><input value={name} onChange={e => setName(e.target.value)} placeholder="npr. Pariz" /></div>
      <div className="form-group"><label>Lokacija *</label><input value={location} onChange={e => setLocation(e.target.value)} placeholder="Grad, Drzava" /></div>
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
        <div className="form-group"><label>Datum dolaska *</label><input type="date" value={arrivalDate} onChange={e => setArrivalDate(e.target.value)} /></div>
        <div className="form-group"><label>Datum odlaska *</label><input type="date" value={departureDate} onChange={e => setDepartureDate(e.target.value)} min={arrivalDate} /></div>
      </div>
      <div className="form-group"><label>Opis</label><textarea value={description} onChange={e => setDescription(e.target.value)} rows={2} style={{ resize: 'vertical' }} /></div>
      <div className="form-group"><label>Napomene</label><textarea value={notes} onChange={e => setNotes(e.target.value)} rows={2} style={{ resize: 'vertical' }} /></div>
      <div style={{ display: 'flex', gap: '8px', justifyContent: 'flex-end' }}>
        <button type="button" className="btn-secondary" onClick={onCancel}>Odustani</button>
        <button type="submit" className="btn-primary" style={{ width: 'auto' }} disabled={loading}>{loading ? 'Cuvanje...' : 'Sacuvaj'}</button>
      </div>
    </form>
  );
}
