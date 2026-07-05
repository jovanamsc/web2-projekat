import { useState, useEffect, type FormEvent } from 'react';
import travelService from '../services/travel.service';
import type { ChecklistItem } from '../models/travel.models';
import '../pages/TravelPlanPage.css';
import '../App.css';

interface Props { planId: number; }

export default function ChecklistTab({ planId }: Props) {
  const [items, setItems] = useState<ChecklistItem[]>([]);
  const [newTitle, setNewTitle] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    travelService.getChecklist(planId).then(setItems);
  }, [planId]);

  const handleAdd = async (e: FormEvent) => {
    e.preventDefault();
    if (!newTitle.trim()) { setError('Unesite naziv stavke.'); return; }
    try {
      const item = await travelService.createChecklistItem(planId, { title: newTitle.trim() });
      setItems(prev => [...prev, item]);
      setNewTitle('');
      setError('');
    } catch {
      setError('Greska pri dodavanju.');
    }
  };

  const handleToggle = async (item: ChecklistItem) => {
    const updated = await travelService.updateChecklistItem(planId, item.id, { isCompleted: !item.isCompleted });
    setItems(prev => prev.map(i => i.id === item.id ? updated : i));
  };

  const handleDelete = async (id: number) => {
    await travelService.deleteChecklistItem(planId, id);
    setItems(prev => prev.filter(i => i.id !== id));
  };

  const completed = items.filter(i => i.isCompleted).length;

  return (
    <div className="tab-section">
      <div className="tab-header">
        <h2>Ceklista</h2>
        {items.length > 0 && (
          <span className="text-muted" style={{ fontSize: '14px' }}>{completed}/{items.length} zavrseno</span>
        )}
      </div>

      <form onSubmit={handleAdd} style={{ display: 'flex', gap: '8px', marginBottom: '16px' }}>
        <input
          value={newTitle}
          onChange={e => { setNewTitle(e.target.value); setError(''); }}
          placeholder="npr. Pasos, karta, osiguranje..."
        />
        <button type="submit" className="btn-primary" style={{ width: 'auto', whiteSpace: 'nowrap' }}>+ Dodaj</button>
      </form>
      {error && <p className="form-error" style={{ marginBottom: '8px' }}>{error}</p>}

      {items.length === 0 && <p className="text-muted">Ceklista je prazna.</p>}

      <div className="item-list">
        {items.map(item => (
          <div key={item.id} className={`checklist-item${item.isCompleted ? ' completed' : ''}`}>
            <input
              type="checkbox"
              checked={item.isCompleted}
              onChange={() => handleToggle(item)}
            />
            <span className="checklist-title">{item.title}</span>
            <button className="btn-danger btn-sm" onClick={() => handleDelete(item.id)}>✕</button>
          </div>
        ))}
      </div>
    </div>
  );
}
