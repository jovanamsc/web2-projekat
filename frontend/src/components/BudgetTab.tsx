import { useState, useEffect } from 'react';
import Modal from './Modal';
import expenseService from '../services/expense.service';
import type { Expense, CreateExpense, ExpenseCategory } from '../models/expense.models';
import '../pages/TravelPlanPage.css';
import '../App.css';

const CATEGORIES: ExpenseCategory[] = ['Transport', 'Accommodation', 'Food', 'Tickets', 'Shopping', 'Other'];
const CAT_LABELS: Record<ExpenseCategory, string> = {
  Transport: 'Prevoz', Accommodation: 'Smjestaj', Food: 'Hrana',
  Tickets: 'Ulaznice', Shopping: 'Kupovina', Other: 'Ostalo'
};

interface Props { planId: number; budget: number; activityCostTotal?: number; }

export default function BudgetTab({ planId, budget, activityCostTotal = 0 }: Props) {
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<Expense | null>(null);

  useEffect(() => {
    expenseService.getExpenses(planId).then(setExpenses);
  }, [planId]);

  const totalExpenses = expenses.reduce((sum, e) => sum + e.amount, 0);
  const totalAll = totalExpenses + activityCostTotal;
  const remaining = budget - totalAll;

  const handleSubmit = async (data: CreateExpense) => {
    if (editing) {
      const updated = await expenseService.updateExpense(planId, editing.id, data);
      setExpenses(prev => prev.map(e => e.id === editing.id ? updated : e));
      setEditing(null);
    } else {
      const created = await expenseService.createExpense(planId, data);
      setExpenses(prev => [created, ...prev]);
      setShowForm(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Obrisati trosak?')) return;
    await expenseService.deleteExpense(planId, id);
    setExpenses(prev => prev.filter(e => e.id !== id));
  };

  const fmt = (d: string) => new Date(d).toLocaleDateString('bs-BA');

  return (
    <div className="tab-section">
      <div className="budget-summary">
        <div className="budget-stat">
          <label>Planirani budzet</label>
          <span className="amount">{budget.toLocaleString('bs-BA')} €</span>
        </div>
        <div className="budget-stat">
          <label>Evidentirana potrosnja</label>
          <span className="amount">{totalExpenses.toLocaleString('bs-BA')} €</span>
        </div>
        {activityCostTotal > 0 && (
          <div className="budget-stat">
            <label>Procijenjeni troskovi aktivnosti</label>
            <span className="amount">{activityCostTotal.toLocaleString('bs-BA')} €</span>
          </div>
        )}
        <div className="budget-stat">
          <label>Ukupno</label>
          <span className="amount">{totalAll.toLocaleString('bs-BA')} €</span>
        </div>
        <div className="budget-stat">
          <label>Preostalo</label>
          <span className={`amount ${remaining < 0 ? 'negative' : 'positive'}`}>
            {remaining.toLocaleString('bs-BA')} €
          </span>
        </div>
      </div>

      <div className="tab-header">
        <h2>Troskovi</h2>
        <button className="btn-primary" style={{ width: 'auto' }} onClick={() => setShowForm(true)}>+ Dodaj trosak</button>
      </div>

      {expenses.length === 0 && <p className="text-muted">Nema evidentiranih troskova.</p>}

      <div className="item-list">
        {expenses.map(e => (
          <div key={e.id} className="item-card">
            <div className="item-card-body">
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <h3>{e.title}</h3>
                <span className="status-badge" style={{ background: '#f1f5f9', color: '#475569' }}>{CAT_LABELS[e.category]}</span>
              </div>
              <div className="item-meta">
                <span>{e.amount.toLocaleString('bs-BA')} €</span>
                <span>{fmt(e.date)}</span>
              </div>
              {e.description && <p style={{ fontSize: '13px', marginTop: '4px' }}>{e.description}</p>}
            </div>
            <div className="item-actions">
              <button className="btn-secondary btn-sm" onClick={() => setEditing(e)}>Uredi</button>
              <button className="btn-danger btn-sm" onClick={() => handleDelete(e.id)}>Obrisi</button>
            </div>
          </div>
        ))}
      </div>

      {(showForm || editing) && (
        <Modal title={editing ? 'Uredi trosak' : 'Novi trosak'} onClose={() => { setShowForm(false); setEditing(null); }}>
          <ExpenseForm
            initial={editing ?? undefined}
            onSubmit={handleSubmit}
            onCancel={() => { setShowForm(false); setEditing(null); }}
          />
        </Modal>
      )}
    </div>
  );
}

function ExpenseForm({ initial, onSubmit, onCancel }: {
  initial?: Expense;
  onSubmit: (d: CreateExpense) => Promise<void>;
  onCancel: () => void;
}) {
  const [title, setTitle] = useState(initial?.title ?? '');
  const [category, setCategory] = useState<ExpenseCategory>(initial?.category ?? 'Other');
  const [amount, setAmount] = useState(initial?.amount?.toString() ?? '');
  const [date, setDate] = useState(initial?.date?.slice(0, 10) ?? new Date().toISOString().slice(0, 10));
  const [description, setDescription] = useState(initial?.description ?? '');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) { setError('Naziv je obavezan.'); return; }
    if (!amount || Number(amount) < 0) { setError('Iznos mora biti pozitivan broj.'); return; }
    if (!date) { setError('Datum je obavezan.'); return; }
    setLoading(true);
    setError('');
    try {
      await onSubmit({ title, category, amount: Number(amount), date, description: description || undefined });
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      setError(msg || 'Greska.');
    } finally { setLoading(false); }
  };

  return (
    <form onSubmit={handleSubmit} noValidate>
      {error && <div className="alert alert-error">{error}</div>}
      <div className="form-group"><label>Naziv *</label><input value={title} onChange={e => setTitle(e.target.value)} placeholder="npr. Avionska karta" /></div>
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
        <div className="form-group">
          <label>Kategorija</label>
          <select value={category} onChange={e => setCategory(e.target.value as ExpenseCategory)}>
            {CATEGORIES.map(c => <option key={c} value={c}>{CAT_LABELS[c]}</option>)}
          </select>
        </div>
        <div className="form-group"><label>Iznos (€) *</label><input type="number" value={amount} onChange={e => setAmount(e.target.value)} min="0" step="0.01" /></div>
      </div>
      <div className="form-group"><label>Datum *</label><input type="date" value={date} onChange={e => setDate(e.target.value)} /></div>
      <div className="form-group"><label>Opis</label><textarea value={description} onChange={e => setDescription(e.target.value)} rows={2} style={{ resize: 'vertical' }} /></div>
      <div style={{ display: 'flex', gap: '8px', justifyContent: 'flex-end' }}>
        <button type="button" className="btn-secondary" onClick={onCancel}>Odustani</button>
        <button type="submit" className="btn-primary" style={{ width: 'auto' }} disabled={loading}>{loading ? 'Cuvanje...' : 'Sacuvaj'}</button>
      </div>
    </form>
  );
}
