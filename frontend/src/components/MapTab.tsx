import { useEffect, useRef, useState } from 'react';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import type { Activity, Destination } from '../models/travel.models';
import travelService from '../services/travel.service';
import '../pages/TravelPlanPage.css';

// fix za Vite
delete (L.Icon.Default.prototype as unknown as Record<string, unknown>)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
});

interface GeoResult { id: number; lat: number; lon: number; }

interface Props {
  planId: number;
}

export default function MapTab({ planId }: Props) {
  const [destinations, setDestinations] = useState<Destination[]>([]);
  const [activities, setActivities] = useState<Activity[]>([]);

  useEffect(() => {
    travelService.getDestinations(planId).then(setDestinations);
    travelService.getActivities(planId).then(setActivities);
  }, [planId]);
  const mapRef = useRef<L.Map | null>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const markersRef = useRef<L.Marker[]>([]);
  const polylineRef = useRef<L.Polyline | null>(null);
  const [mode, setMode] = useState<'destinations' | 'activities'>('destinations');
  const [geoResults, setGeoResults] = useState<GeoResult[]>([]);
  const [geocoding, setGeocoding] = useState(false);
  const [notFound, setNotFound] = useState<string[]>([]);

  useEffect(() => {
    if (!containerRef.current || mapRef.current) return;
    mapRef.current = L.map(containerRef.current).setView([44.0, 17.5], 5);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors',
      maxZoom: 18,
    }).addTo(mapRef.current);
    return () => { mapRef.current?.remove(); mapRef.current = null; };
  }, []);

  const geocodeQuery = async (query: string, id: number): Promise<GeoResult | null> => {
    try {
      const base = import.meta.env.VITE_NOMINATIM_URL as string;
      const url = `${base}/search?q=${encodeURIComponent(query)}&format=json&limit=1`;
      const res = await fetch(url, { headers: { 'Accept-Language': 'bs,hr,sr,en' } });
      const data = await res.json();
      if (data.length === 0) return null;
      return { id, lat: parseFloat(data[0].lat), lon: parseFloat(data[0].lon) };
    } catch { return null; }
  };

  useEffect(() => {
    setGeoResults([]);
    setNotFound([]);

    if (mode === 'destinations') {
      if (destinations.length === 0) return;
      setGeocoding(true);
      const run = async () => {
        const failed: string[] = [];
        const results: GeoResult[] = [];
        for (const d of destinations) {
          await new Promise(r => setTimeout(r, 200));
          const geo = await geocodeQuery(d.location, d.id);
          if (geo) results.push(geo);
          else failed.push(d.name);
        }
        setGeoResults(results);
        setNotFound(failed);
        setGeocoding(false);
      };
      run();
    } else {
      const withLoc = activities.filter(a => a.location);
      if (withLoc.length === 0) return;
      setGeocoding(true);
      const run = async () => {
        const failed: string[] = [];
        const results: GeoResult[] = [];
        for (const a of withLoc) {
          await new Promise(r => setTimeout(r, 200));
          const geo = await geocodeQuery(a.location!, a.id);
          if (geo) results.push(geo);
          else failed.push(a.title);
        }
        setGeoResults(results);
        setNotFound(failed);
        setGeocoding(false);
      };
      run();
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [mode, destinations, activities]);

  useEffect(() => {
    const map = mapRef.current;
    if (!map) return;

    markersRef.current.forEach(m => m.remove());
    markersRef.current = [];
    polylineRef.current?.remove();
    polylineRef.current = null;

    const fmt = (d: string) => new Date(d).toLocaleDateString('bs-BA');
    const points: L.LatLng[] = [];

    if (mode === 'destinations') {
      for (const geo of geoResults) {
        const dest = destinations.find(d => d.id === geo.id);
        if (!dest) continue;
        const popup = `
          <div style="min-width:180px">
            <b style="font-size:14px">${dest.name}</b>
            <div style="color:#64748b;font-size:12px;margin-top:4px">${dest.location}</div>
            <div style="color:#64748b;font-size:12px">${fmt(dest.arrivalDate)} – ${fmt(dest.departureDate)}</div>
            ${dest.description ? `<div style="font-size:12px;margin-top:6px">${dest.description}</div>` : ''}
          </div>
        `;
        const marker = L.marker([geo.lat, geo.lon]).addTo(map).bindPopup(popup);
        markersRef.current.push(marker);
        points.push(L.latLng(geo.lat, geo.lon));
      }
    } else {
      const sorted = [...activities.filter(a => a.location)]
        .sort((a, b) => (a.date + (a.time || '00:00')).localeCompare(b.date + (b.time || '00:00')));

      for (const geo of geoResults) {
        const act = activities.find(a => a.id === geo.id);
        if (!act) continue;
        const popup = `
          <div style="min-width:180px">
            <b style="font-size:14px">${act.title}</b>
            ${act.time ? `<div style="color:#64748b;font-size:12px;margin-top:4px">${act.time.slice(0, 5)}</div>` : ''}
            <div style="color:#64748b;font-size:12px">${act.location}</div>
            <div style="color:#64748b;font-size:12px">${fmt(act.date)}</div>
            ${act.description ? `<div style="font-size:12px;margin-top:6px">${act.description}</div>` : ''}
          </div>
        `;
        const marker = L.marker([geo.lat, geo.lon]).addTo(map).bindPopup(popup);
        markersRef.current.push(marker);
        points.push(L.latLng(geo.lat, geo.lon));
      }

      const polylinePoints = sorted
        .map(a => geoResults.find(g => g.id === a.id))
        .filter((g): g is GeoResult => !!g)
        .map(g => L.latLng(g.lat, g.lon));

      if (polylinePoints.length > 1) {
        polylineRef.current = L.polyline(polylinePoints, { color: '#3b82f6', weight: 2, dashArray: '6 4' }).addTo(map);
      }
    }

    if (points.length > 0) {
      map.fitBounds(L.latLngBounds(points), { padding: [48, 48], maxZoom: 10 });
    }
  }, [geoResults, mode, destinations, activities]);

  const fmt = (d: string) => new Date(d).toLocaleDateString('bs-BA');
  const activitiesWithLoc = [...activities.filter(a => a.location)]
    .sort((a, b) => (a.date + (a.time || '')).localeCompare(b.date + (b.time || '')));

  return (
    <div className="tab-section">
      <div className="tab-header">
        <h2>Karta</h2>
        <div style={{ display: 'flex', gap: '12px', alignItems: 'center' }}>
          <div className="view-toggle">
            <button className={mode === 'destinations' ? 'active' : ''} onClick={() => setMode('destinations')}>Destinacije</button>
            <button className={mode === 'activities' ? 'active' : ''} onClick={() => setMode('activities')}>Aktivnosti</button>
          </div>
          {geocoding && <span className="text-muted" style={{ fontSize: '13px' }}>Ucitavanje...</span>}
        </div>
      </div>

      <div ref={containerRef} className="map-container" />

      {mode === 'destinations' && destinations.length === 0 && (
        <p className="text-muted">Nema destinacija za prikaz na karti.</p>
      )}
      {mode === 'activities' && activitiesWithLoc.length === 0 && (
        <p className="text-muted">Nema aktivnosti s lokacijom za prikaz na karti.</p>
      )}

      {notFound.length > 0 && (
        <p className="text-muted" style={{ fontSize: '13px', marginTop: '4px' }}>
          Nisu pronadjene na karti: {notFound.join(', ')}
        </p>
      )}

      {mode === 'destinations' && destinations.length > 0 && (
        <div className="dest-legend">
          {destinations.map((d, i) => (
            <div key={d.id} className="dest-legend-item">
              <span className="dest-legend-num">{i + 1}</span>
              <div>
                <span className="dest-legend-name">{d.name}</span>
                <span className="dest-legend-loc">{d.location} · {fmt(d.arrivalDate)} – {fmt(d.departureDate)}</span>
              </div>
            </div>
          ))}
        </div>
      )}

      {mode === 'activities' && activitiesWithLoc.length > 0 && (
        <div className="dest-legend">
          {activitiesWithLoc.map((a, i) => (
            <div key={a.id} className="dest-legend-item">
              <span className="dest-legend-num">{i + 1}</span>
              <div>
                <span className="dest-legend-name">{a.title}</span>
                <span className="dest-legend-loc">{a.location} · {fmt(a.date)}{a.time ? ' ' + a.time.slice(0, 5) : ''}</span>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
