import { useState, type FormEvent } from 'react';
import type { CreateTravelPlan, TravelPlan } from '../models/travel.models';
import '../App.css';

interface Props {
  initial?: TravelPlan;
  onSubmit: (data: CreateTravelPlan) => Promise<void>;
  onCancel: () => void;
  submitLabel?: string;
}

export default function TravelPlanForm({ initial, onSubmit, onCancel, submitLabel = 'Sacuvaj' }: Props) {
  const [title, setTitle] = useState(initial?.title ?? '');
  const [description, setDescription] = useState(initial?.description ?? '');
  const [startDate, setStartDate] = useState(initial?.startDate?.slice(0, 10) ?? '');
  const [endDate, setEndDate] = useState(initial?.endDate?.slice(0, 10) ?? '');
  const [budget, setBudget] = useState(initial?.budget?.toString() ?? '');
  const [notes, setNotes] = useState(initial?.notes ?? '');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const validate = () => {
    if (!title.trim()) return 'Naziv je obavezan.';
    if (!startDate) return 'Datum pocetka je obavezan.';
    if (!endDate) return 'Datum zavrsetka je obavezan.';
    if (new Date(endDate) < new Date(startDate)) return 'Datum zavrsetka ne moze biti prije datuma pocetka.';
    if (budget !== '' && Number(budget) < 0) return 'Budzet ne moze biti negativan.';
    return null;
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const err = validate();
    if (err) { setError(err); return; }

    setLoading(true);
    setError('');
    try {
      await onSubmit({
        title,
        description: description || undefined,
        startDate,
        endDate,
        budget: budget !== '' ? Number(budget) : 0,
        notes: notes || undefined,
      });
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      setError(msg || 'Doslo je do greske.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} noValidate>
      {error && <div className="alert alert-error">{error}</div>}

      <div className="form-group">
        <label>Naziv putovanja *</label>
        <input value={title} onChange={e => setTitle(e.target.value)} placeholder="npr. Putovanje u Pariz" />
      </div>

      <div className="form-group">
        <label>Opis</label>
        <textarea value={description} onChange={e => setDescription(e.target.value)} rows={2} placeholder="Kratki opis putovanja" style={{ resize: 'vertical' }} />
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
        <div className="form-group">
          <label>Datum pocetka *</label>
          <input type="date" value={startDate} onChange={e => setStartDate(e.target.value)} />
        </div>
        <div className="form-group">
          <label>Datum zavrsetka *</label>
          <input type="date" value={endDate} onChange={e => setEndDate(e.target.value)} min={startDate} />
        </div>
      </div>

      <div className="form-group">
        <label>Planirani budzet (€)</label>
        <input type="number" value={budget} onChange={e => setBudget(e.target.value)} placeholder="0" min="0" step="0.01" />
      </div>

      <div className="form-group">
        <label>Napomene</label>
        <textarea value={notes} onChange={e => setNotes(e.target.value)} rows={2} placeholder="Dodatne napomene..." style={{ resize: 'vertical' }} />
      </div>

      <div style={{ display: 'flex', gap: '8px', justifyContent: 'flex-end', marginTop: '8px' }}>
        <button type="button" className="btn-secondary" onClick={onCancel}>Odustani</button>
        <button type="submit" className="btn-primary" style={{ width: 'auto' }} disabled={loading}>
          {loading ? 'Cuvanje...' : submitLabel}
        </button>
      </div>
    </form>
  );
}
