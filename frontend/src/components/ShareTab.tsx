import { useState, useEffect } from 'react';
import { QRCodeSVG } from 'qrcode.react';
import travelService from '../services/travel.service';
import type { ShareLink, AccessLevel } from '../models/share.models';
import '../pages/TravelPlanPage.css';
import '../App.css';

interface Props { planId: number; }

export default function ShareTab({ planId }: Props) {
  const [links, setLinks] = useState<ShareLink[]>([]);
  const [accessLevel, setAccessLevel] = useState<AccessLevel>('VIEW');
  const [expiryDays, setExpiryDays] = useState(7);
  const [creating, setCreating] = useState(false);
  const [copied, setCopied] = useState<string | null>(null);
  const [showQr, setShowQr] = useState<string | null>(null);

  useEffect(() => {
    travelService.getShareLinks(planId).then(setLinks);
  }, [planId]);

  const handleCreate = async () => {
    setCreating(true);
    try {
      const link = await travelService.createShareLink(planId, { accessLevel, expiryDays });
      setLinks(prev => [link, ...prev]);
    } finally {
      setCreating(false);
    }
  };

  const handleDelete = async (token: string) => {
    if (!confirm('Opozati link?')) return;
    await travelService.deleteShareLink(planId, token);
    setLinks(prev => prev.filter(l => l.token !== token));
  };

  const getShareUrl = (token: string) => `${window.location.origin}/shared/${token}`;

  const handleCopy = (token: string) => {
    navigator.clipboard.writeText(getShareUrl(token));
    setCopied(token);
    setTimeout(() => setCopied(null), 2000);
  };

  const fmt = (d?: string) => d ? new Date(d).toLocaleDateString('bs-BA') : '—';

  return (
    <div className="tab-section">
      <div className="tab-header">
        <h2>Dijeljenje plana</h2>
      </div>

      <div className="card" style={{ marginBottom: '24px' }}>
        <h3 style={{ marginBottom: '16px' }}>Kreiraj novi link</h3>
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px', marginBottom: '16px' }}>
          <div className="form-group">
            <label>Nivo pristupa</label>
            <select value={accessLevel} onChange={e => setAccessLevel(e.target.value as AccessLevel)}>
              <option value="VIEW">VIEW — samo pregled</option>
              <option value="EDIT">EDIT — uredjivanje</option>
            </select>
          </div>
          <div className="form-group">
            <label>Istice za (dana)</label>
            <input type="number" value={expiryDays} onChange={e => setExpiryDays(Number(e.target.value))} min={1} max={365} />
          </div>
        </div>
        <button className="btn-primary" style={{ width: 'auto' }} onClick={handleCreate} disabled={creating}>
          {creating ? 'Kreiranje...' : 'Kreiraj link'}
        </button>
      </div>

      {links.length === 0 && <p className="text-muted">Nema aktivnih linkova za dijeljenje.</p>}

      <div className="share-links">
        {links.map(link => (
          <div key={link.token} className="share-link-card" style={{ flexDirection: 'column', alignItems: 'stretch' }}>
            <div style={{ display: 'flex', alignItems: 'flex-start', gap: '8px' }}>
              <div style={{ flex: 1, minWidth: 0 }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '6px' }}>
                  <span className={`access-badge access-${link.accessLevel}`}>{link.accessLevel}</span>
                  <span style={{ fontSize: '13px', color: 'var(--text-muted)' }}>Istice: {fmt(link.expiresAt)}</span>
                </div>
                <div className="share-token">{getShareUrl(link.token)}</div>
              </div>
              <div style={{ display: 'flex', gap: '6px', flexShrink: 0 }}>
                <button className="btn-secondary btn-sm" onClick={() => handleCopy(link.token)}>
                  {copied === link.token ? '✓ Kopirano' : 'Kopiraj'}
                </button>
                <button className="btn-secondary btn-sm" onClick={() => setShowQr(showQr === link.token ? null : link.token)}>
                  {showQr === link.token ? 'Sakrij QR' : 'QR kod'}
                </button>
                <button className="btn-danger btn-sm" onClick={() => handleDelete(link.token)}>Opozovi</button>
              </div>
            </div>
            {showQr === link.token && (
              <div style={{ paddingTop: '16px', display: 'flex', justifyContent: 'center', borderTop: '1px solid var(--border)', marginTop: '12px' }}>
                <QRCodeSVG value={getShareUrl(link.token)} size={160} />
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
